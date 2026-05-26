using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.DTOs.User;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Tests.Services;

public class AuthServiceTests
{
    // ─── Configuration ─────────────────────────────────────────────────────────
    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // HS512 key — must be > 512 bits (> 64 bytes), using 67 chars = 536 bits
                ["AppSettings:Token"]    = "test-auth-service-key-for-hs512-must-be-65-or-more-characters-long!",
                ["AppSettings:Issuer"]   = "TestIssuer",
                ["AppSettings:Audience"] = "TestAudience"
            })
            .Build();

    // ─── Deps ──────────────────────────────────────────────────────────────────
    private record Deps(
        Mock<IUnitOfWork>                Uow,
        Mock<IAuthRepository>            AuthRepo,
        Mock<IAchievementRepository>     AchievementRepo,
        Mock<IUserAchievementRepository> UserAchievementRepo,
        Mock<IPetRepository>             PetRepo,
        Mock<IUserOwnedPetRepository>    UserOwnedPetRepo,
        Mock<IActivityLogService>        ActivityLog,
        Mock<IDailyQuestService>         DailyQuest,
        Mock<IGoogleOAuthService>        GoogleOAuth);

    // ─── Factory ───────────────────────────────────────────────────────────────
    private static (AuthService service, Deps deps) CreateService()
    {
        var uow                 = new Mock<IUnitOfWork>();
        var authRepo            = new Mock<IAuthRepository>();
        var achievementRepo     = new Mock<IAchievementRepository>();
        var userAchievementRepo = new Mock<IUserAchievementRepository>();
        var petRepo             = new Mock<IPetRepository>();
        var userOwnedPetRepo    = new Mock<IUserOwnedPetRepository>();
        var activityLog         = new Mock<IActivityLogService>();
        var dailyQuest          = new Mock<IDailyQuestService>();
        var googleOAuth         = new Mock<IGoogleOAuthService>();

        uow.Setup(u => u.Auth).Returns(authRepo.Object);
        uow.Setup(u => u.Achievement).Returns(achievementRepo.Object);
        uow.Setup(u => u.UserAchievement).Returns(userAchievementRepo.Object);
        uow.Setup(u => u.Pet).Returns(petRepo.Object);
        uow.Setup(u => u.UserOwnedPet).Returns(userOwnedPetRepo.Object);
        uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        // Default for InitializeNewUserAsync (called in Register & GoogleLogin new-user path)
        achievementRepo
            .Setup(r => r.GetAchievementsAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Achievement>());
        userAchievementRepo
            .Setup(r => r.BulkCreateUserAchievementAsync(
                It.IsAny<IEnumerable<UserAchievement>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        petRepo
            .Setup(r => r.GetPetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pet?)null);

        // Default for CreateTokenResponse (called by every success path)
        authRepo
            .Setup(r => r.UpdateUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // DailyQuest — called without CancellationToken (uses optional default)
        dailyQuest
            .Setup(d => d.GenerateDailyQuestsForUserAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new AuthService(
            uow.Object,
            BuildConfig(),
            activityLog.Object,
            dailyQuest.Object,
            new Mock<ILogger<AuthService>>().Object,
            googleOAuth.Object);

        return (service, new Deps(
            uow, authRepo, achievementRepo, userAchievementRepo,
            petRepo, userOwnedPetRepo, activityLog, dailyQuest, googleOAuth));
    }

    // ─── Helper ────────────────────────────────────────────────────────────────
    private static User MakeUser(int id = 1, string email = "u@test.com",
        string username = "user1", string? password = null)
    {
        var u = new User { Id = id, Email = email, Username = username };
        if (password is not null)
            u.PasswordHash = new PasswordHasher<User>().HashPassword(null!, password);
        return u;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LoginAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_UserNotFound_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.AuthRepo
            .Setup(r => r.LoginUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await service.LoginAsync(new LoginDto { Username = "ghost", Password = "any" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_NullPasswordHash_ReturnsNull()
    {
        var (service, deps) = CreateService();
        var user = new User { Id = 1, Email = "u@test.com", Username = "user1", PasswordHash = null };
        deps.AuthRepo
            .Setup(r => r.LoginUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.LoginAsync(new LoginDto { Username = "user1", Password = "any" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsNull()
    {
        var (service, deps) = CreateService();
        var user = MakeUser(password: "correct");
        deps.AuthRepo
            .Setup(r => r.LoginUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.LoginAsync(new LoginDto { Username = "user1", Password = "wrong" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var (service, deps) = CreateService();
        var user = MakeUser(password: "pass123");
        deps.AuthRepo
            .Setup(r => r.LoginUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.LoginAsync(new LoginDto { Username = "user1", Password = "pass123" });

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // RegisterAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_UsernameExists_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.AuthRepo
            .Setup(r => r.UserExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.RegisterAsync(
            new RegisterDto { Username = "taken", Email = "e@e.com", Password = "p" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Register_EmailExists_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.AuthRepo
            .Setup(r => r.UserExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        deps.AuthRepo
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await service.RegisterAsync(
            new RegisterDto { Username = "user1", Email = "taken@e.com", Password = "p" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task Register_Success_ReturnsUserDto()
    {
        var (service, deps) = CreateService();
        var created = new User { Id = 42, Email = "new@e.com", Username = "newuser" };
        deps.AuthRepo
            .Setup(r => r.UserExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        deps.AuthRepo
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        deps.AuthRepo
            .Setup(r => r.RegisterUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await service.RegisterAsync(
            new RegisterDto { Username = "newuser", Email = "new@e.com", Password = "pass" });

        result.Should().NotBeNull();
        result!.Id.Should().Be(42);
        result.Username.Should().Be("newuser");
        result.Email.Should().Be("new@e.com");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // RefreshTokenAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_UserNotFound_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.AuthRepo
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await service.RefreshTokenAsync(
            new RefreshTokenRequestDto { Id = 99, RefreshToken = "tok" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshToken_TokenMismatch_ReturnsNull()
    {
        var (service, deps) = CreateService();
        var user = new User
        {
            Id = 1, Email = "u@test.com",
            RefreshToken = "correct_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };
        deps.AuthRepo
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.RefreshTokenAsync(
            new RefreshTokenRequestDto { Id = 1, RefreshToken = "wrong_token" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshToken_TokenExpired_ReturnsNull()
    {
        var (service, deps) = CreateService();
        var user = new User
        {
            Id = 1, Email = "u@test.com",
            RefreshToken = "expired_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)   // past
        };
        deps.AuthRepo
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.RefreshTokenAsync(
            new RefreshTokenRequestDto { Id = 1, RefreshToken = "expired_token" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshToken_ValidToken_ReturnsNewToken()
    {
        var (service, deps) = CreateService();
        var user = new User
        {
            Id = 1, Email = "u@test.com",
            RefreshToken = "valid_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };
        deps.AuthRepo
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await service.RefreshTokenAsync(
            new RefreshTokenRequestDto { Id = 1, RefreshToken = "valid_token" });

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GoogleLoginAsync
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GoogleLogin_ExchangeCodeFails_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.GoogleOAuth
            .Setup(g => g.ExchangeCodeForUserInfoAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GoogleUserInfoDto?)null);

        var result = await service.GoogleLoginAsync("bad_code");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GoogleLogin_EmailNotVerified_ReturnsNull()
    {
        var (service, deps) = CreateService();
        deps.GoogleOAuth
            .Setup(g => g.ExchangeCodeForUserInfoAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfoDto
            {
                Sub = "sub1", Email = "g@g.com", EmailVerified = false
            });
        deps.AuthRepo
            .Setup(r => r.GetUserByExternalLoginAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await service.GoogleLoginAsync("code");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GoogleLogin_ExistingProviderUser_ReturnsToken()
    {
        var (service, deps) = CreateService();
        var existingUser = new User { Id = 5, Email = "g@g.com", Username = "guser" };
        deps.GoogleOAuth
            .Setup(g => g.ExchangeCodeForUserInfoAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfoDto
            {
                Sub = "sub1", Email = "g@g.com", EmailVerified = true
            });
        deps.AuthRepo
            .Setup(r => r.GetUserByExternalLoginAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var result = await service.GoogleLoginAsync("code");

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GoogleLogin_UserFoundByEmail_LinksAccountAndReturnsToken()
    {
        var (service, deps) = CreateService();
        var emailUser = new User { Id = 6, Email = "g@g.com", Username = "emailuser" };
        deps.GoogleOAuth
            .Setup(g => g.ExchangeCodeForUserInfoAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfoDto
            {
                Sub = "sub1", Email = "g@g.com", EmailVerified = true
            });
        deps.AuthRepo
            .Setup(r => r.GetUserByExternalLoginAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        deps.AuthRepo
            .Setup(r => r.GetUserByEmailAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emailUser);

        var result = await service.GoogleLoginAsync("code");

        result.Should().NotBeNull();
        emailUser.ExternalLoginProvider.Should().Be("Google");
        emailUser.ExternalLoginProviderKey.Should().Be("sub1");
    }

    [Fact]
    public async Task GoogleLogin_NewUser_CreatesUserAndReturnsToken()
    {
        var (service, deps) = CreateService();
        var createdUser = new User { Id = 7, Email = "new@g.com", Username = "new_google_user" };
        deps.GoogleOAuth
            .Setup(g => g.ExchangeCodeForUserInfoAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfoDto
            {
                Sub = "newsub", Email = "new@g.com", EmailVerified = true, Name = "New User"
            });
        deps.AuthRepo
            .Setup(r => r.GetUserByExternalLoginAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        deps.AuthRepo
            .Setup(r => r.GetUserByEmailAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        deps.AuthRepo
            .Setup(r => r.RegisterUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        var result = await service.GoogleLoginAsync("code");

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
    }
}
