using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using WordSoul.Api.Hubs;
using WordSoul.Api.Services;
using WordSoul.Application.Common;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services;
using WordSoul.Application.Services.SRS;
using WordSoul.Infrastructure.BackgroundServices;
using WordSoul.Infrastructure.Common;


//using WordSoul.Infrastructure.BackgroundServices;
using WordSoul.Infrastructure.Persistence;
using WordSoul.Infrastructure.Persistence.Repositories;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("Appsettings/appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"Appsettings/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
// builder.Services.AddOpenApi();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "WordSoul API";
        document.Info.Version = "v1";
        document.Info.Description = "Hệ thống API hỗ trợ học từ vựng và quản lý thú ảo.";

        // Cấu hình bảo mật
        var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description = "Nhập Token JWT (chỉ phần chuỗi, không bao gồm 'Bearer ')."
        };

        document.Components ??= new Microsoft.OpenApi.Models.OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", scheme);

        // Áp dụng Security Requirement cho tất cả các endpoint (Yêu cầu ổ khóa xuất hiện)
        document.SecurityRequirements.Add(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

// Cấu hình Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
});

builder.Services.AddDbContext<WordSoulDbContext>(options =>
   options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm dịch vụ CORS
var allowedOrigins = builder.Configuration["AllowedOrigins"]?
    .Split(",", StringSplitOptions.RemoveEmptyEntries)
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };
    });

// Thêm dịch vụ SignalR
builder.Services.AddSignalR(options =>
{
    // Tùy chọn debug trong môi trường phát triển
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors = true;
    }
});

// Add in-memory caching service
builder.Services.AddMemoryCache();
builder.Services.AddLogging();

// Register repository and service (giữ nguyên như code bạn gửi)
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IVocabularySetService, VocabularySetService>();
builder.Services.AddScoped<IVocabularySetRepository, VocabularySetRepository>();
builder.Services.AddScoped<ILearningSessionService, LearningSessionService>();
builder.Services.AddScoped<ILearningSessionRepository, LearningSessionRepository>();
builder.Services.AddScoped<IAnswerRecordRepository, AnswerRecordRepository>();
builder.Services.AddScoped<IUserVocabularyProgressRepository, UserVocabularyProgressRepository>();
builder.Services.AddScoped<IUserVocabularyProgressService, UserVocabularyProgressService>();
builder.Services.AddScoped<ISetRewardPetRepository, SetRewardPetRepository>();
builder.Services.AddScoped<ISetRewardPetService, SetRewardPetService>();
builder.Services.AddScoped<IUserOwnedPetRepository, UserOwnedPetRepository>();
builder.Services.AddScoped<IUserOwnedPetService, UserOwnedPetService>();
builder.Services.AddScoped<IUserVocabularySetRepository, UserVocabularySetRepository>();
builder.Services.AddScoped<IUserVocabularySetService, UserVocabularySetService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<ISetVocabularyRepository, SetVocabularyRepository>();
builder.Services.AddScoped<ISetVocabularyService, SetVocabularyService>();
builder.Services.AddScoped<ISessionVocabularyRepository, SessionVocabularyRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();
builder.Services.AddScoped<IUserAchievementService, UserAchievementService>();
// Upload Assests
builder.Services.AddScoped<IUploadAssetsService, UploadAssetsService>();

// SRS
builder.Services.AddScoped<ISRSService, SRSService>();
builder.Services.AddScoped<IVocabularyReviewHistoryRepository, VocabularyReviewHistoryRepository>();

//Background Service
builder.Services.AddHostedService<NotificationBackgroundService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

// Register SRSAlgorithm
builder.Services.AddScoped<SRSAlgorithm>();

builder.Services.AddScoped<ITimeProvider, SystemTimeProvider>();
builder.Services.AddScoped<IDailyQuestService, DailyQuestService>();
builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();

// Configure Cloudinary
builder.Services.AddSingleton<Cloudinary>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var cloudinarySettings = configuration.GetSection("Cloudinary");
    var account = new Account(
        cloudinarySettings["CloudName"],
        cloudinarySettings["ApiKey"],
        cloudinarySettings["ApiSecret"]);
    return new Cloudinary(account);
});


var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<WordSoulDbContext>();
//    if (dbContext.Database.IsRelational() )
//    {
//        dbContext.Database.Migrate();
//    }    
//}    



//Hub SignalR
app.MapHub<NotificationHub>("/notificationHub");

//Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("WordSoul API");
    options.WithTheme(ScalarTheme.Purple);
});


// Thêm middleware Serilog để ghi log các yêu cầu HTTP
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// 🚀 Dùng CORS
app.UseCors("AllowFrontend");



app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WordSoulDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Đang chạy database migration...");
        db.Database.Migrate();
        logger.LogInformation("Migration hoàn thành.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi khi chạy migration.");
        throw; // Dừng app nếu migrate thất bại
    }
}


app.Run();
