using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;
using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using WordSoul.Api.Errors;
using WordSoul.Api.Extensions;
using WordSoul.Api.Hubs;
using WordSoul.Api.Routing;
using WordSoul.Api.Services;
using WordSoul.Application.Common;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services;
using WordSoul.Application.Services.SRS;
using WordSoul.Infrastructure.BackgroundServices;
using WordSoul.Infrastructure.Common;
using WordSoul.Infrastructure.ExternalServices;
using WordSoul.Infrastructure.Persistence;
using WordSoul.Infrastructure.Persistence.Repositories;
using WordSoul.Infrastructure.RateLimiting;
using WordSoul.Infrastructure.Services;
using WordSoul.Api.Middlewares;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("Appsettings/appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"Appsettings/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// builder.Services.AddOpenApi();

// HttpClient factory (dùng cho Google OAuth và các external HTTP calls)
builder.Services.AddHttpClient();

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
    ?? ["http://localhost:5173", "http://localhost:3000"];

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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments(ApiRoutes.NotificationHub) ||
                     path.StartsWithSegments(ApiRoutes.BattleHub)))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
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

// Problem Details (RFC 7807) + Global Exception Handling
builder.Services.AddWordSoulProblemDetails();

// Add in-memory caching service
builder.Services.AddMemoryCache();
builder.Services.AddLogging();

// Add Redis cache (IDistributedCache)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "wordsoul:";
});

// Register IConnectionMultiplexer (shared Redis connection) for RedisRateLimiter.
// Reuses the same connection string — no second connection is opened.
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379"));

// Register Redis-backed distributed rate limiter for cost-sensitive policies
// (ai-vocabulary, audio-generation). Registered as Singleton — stateless, thread-safe.
builder.Services.AddSingleton<RedisRateLimiter>();

// Register Redis Cache Service
builder.Services.AddSingleton<IVocabularyAiCacheService, VocabularyAiCacheService>();

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

builder.Services.AddScoped<IPetBuffService, PetBuffService>();
builder.Services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<IFcmService, FcmService>();

// Upload Assests
builder.Services.AddScoped<IUploadAssetsService, UploadAssetsService>();

// SRS
builder.Services.AddScoped<ISRSService, SRSService>();
builder.Services.AddScoped<IPronunciationPracticeService, PronunciationPracticeService>();
builder.Services.AddScoped<IVocabularyReviewHistoryRepository, VocabularyReviewHistoryRepository>();

//Background Service
builder.Services.AddHostedService<NotificationBackgroundService>();
builder.Services.AddHostedService<EmailReminderBackgroundService>();
builder.Services.AddHostedService<SmartTimingAnalyzerWorker>();

// System Logs
builder.Services.AddSingleton<SystemLogQueue>();
builder.Services.AddHostedService<SystemLogBackgroundWorker>();
builder.Services.AddHostedService<LogCleanupBackgroundWorker>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

// Register SRSAlgorithm
builder.Services.AddScoped<SRSAlgorithm>();

builder.Services.AddScoped<ITimeProvider, SystemTimeProvider>();
builder.Services.AddScoped<IDailyQuestService, DailyQuestService>();
builder.Services.AddScoped<IUserInventoryService, UserInventoryService>();

// Pronunciation Practice
builder.Services.AddSingleton<IAzurePronunciationService, AzurePronunciationService>();
builder.Services.AddScoped<IPronunciationAttemptRepository, PronunciationAttemptRepository>();

// Gym Leader Progression
builder.Services.AddScoped<IGymLeaderService, GymLeaderService>();
builder.Services.AddScoped<IArenaBattleService, ArenaBattleService>();

// User Groups
builder.Services.AddScoped<IUserGroupService, UserGroupService>();

// Admin Dashboard
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// PvP Matchmaking – Singleton vì queue phải dùng chung giữa requests
builder.Services.AddSingleton<IMatchmakingQueueService, MatchmakingQueueService>();
builder.Services.AddSingleton<IMatchmakingNotifier, MatchmakingNotifier>();
builder.Services.AddHostedService<MatchmakingWorker>();

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

// Configure Firebase Admin
var firebaseJson = builder.Configuration["Firebase:ServiceAccountJson"];

if (!string.IsNullOrEmpty(firebaseJson))
{
    // Đọc từ biến môi trường (Azure App Service)
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromJson(firebaseJson)
    });
    Log.Information("Firebase Admin initialized from Environment Variables.");
}
else
{
    // Fallback đọc từ file cục bộ (Local Development)
    var firebaseKeyPath = Path.Combine(builder.Environment.ContentRootPath, "Appsettings/vocamon-7932b-firebase-adminsdk-fbsvc-a967a14219.json");
    if (File.Exists(firebaseKeyPath))
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(firebaseKeyPath)
        });
        Log.Information("Firebase Admin initialized from local JSON file.");
    }
    else
    {
        Log.Warning("Firebase Service Account JSON not found. Push notifications will not work.");
    }
}

// External AI & Media Services
builder.Services.AddHttpClient<IGeminiAiService, GeminiAiService>();
builder.Services.AddHttpClient<IUnsplashService, UnsplashService>();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddSingleton<IAzureSpeechService, AzureSpeechService>();


// ── Rate Limiting ────────────────────────────────────────────────────────────
// Registered AFTER Redis and BEFORE builder.Build().
// All 7 policies are config-driven via RateLimitingOptions (appsettings.json).
builder.Services.AddWordSoulRateLimiting(builder.Configuration);

var app = builder.Build();

//Configure the HTTP request pipeline.
// 1. OpenAPI/Scalar (không cần CORS)
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("WordSoul API");
    options.WithTheme(ScalarTheme.Purple);
});
app.MapGet(ApiRoutes.Root, () => Results.Redirect(ApiRoutes.ScalarReference))
   .DisableRateLimiting();

// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  MIDDLEWARE PIPELINE ORDER — WordSoulApi                               ║
// ║                                                                        ║
// ║  1. GlobalExceptionMiddleware  — outermost, catches ALL downstream     ║
// ║     exceptions including 429s that bubble up unexpectedly.             ║
// ║  2. StatusCodePages            — canonical body for empty 4xx/5xx       ║
// ║  3. SerilogRequestLogging      — log after exception handler wraps it  ║
// ║  4. RequestResponseLogging     — detailed body logging                 ║
// ║  5. ActivityTrackingMiddleware — user last-seen telemetry              ║
// ║  6. CORS                       — must precede HTTPS redirect so        ║
// ║     OPTIONS preflight is not redirected                                ║
// ║  7. HTTPS redirect             — production only                       ║
// ║  8. UseRateLimiter             — AFTER exception handler (so 429       ║
// ║     responses are logged) but BEFORE authentication.                   ║
// ║     WHY before auth: the "auth-endpoints" policy partitions by IP and  ║
// ║     acts on login/register before any identity is resolved.            ║
// ║     The "authenticated-user" policy uses claims extracted by           ║
// ║     UseAuthentication, but because the policy factory runs lazily on   ║
// ║     each request, the JWT has already been parsed by the time the      ║
// ║     limiter inspects HttpContext.User. Placing UseRateLimiter here     ║
// ║     ensures the 429 response bypasses UseAuthorization (no 401        ║
// ║     interference) and that the GlobalExceptionMiddleware can log it.   ║
// ║  9. UseAuthentication + UseAuthorization                               ║
// ╚══════════════════════════════════════════════════════════════════════════╝

// 1. Global Exception Handler (must be FIRST to catch everything downstream)
app.UseWordSoulProblemDetails();

// 3. HTTP request logging
app.UseSerilogRequestLogging();

// 4. Detailed Request/Response logging
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// 5. User activity tracking
app.UseMiddleware<ActivityTrackingMiddleware>();

// 6. CORS must be before HTTPS redirect
// On Azure App Service, HTTPS is terminated at the load balancer, so
// UseHttpsRedirection would redirect OPTIONS preflight before CORS processes it.
app.UseCors("AllowFrontend");

// 7. HTTPS redirect (production/staging only — not when developing locally)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 8. Rate Limiting — placed AFTER exception middleware (so 429 responses are
//    properly logged) and BEFORE authentication (so IP-based auth endpoint
//    protection works without requiring a valid JWT first).
app.UseRateLimiter();

// 9. Auth
app.UseAuthentication();
app.UseAuthorization();

// 10. Controllers — no global rate limiting policy here; individual endpoints
//    are decorated with [EnableRateLimiting("policy-name")] attributes.
app.MapControllers();

// 11. Health check endpoints — explicitly exempt from rate limiting.
//     These are called by Azure App Service probes and must never be blocked.
app.MapGet("/health",  () => Results.Ok(new { status = "healthy" }))
   .DisableRateLimiting();
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }))
   .DisableRateLimiting();

// 12. SignalR Hubs — MUST use DisableRateLimiting on the negotiate path.
//     Rate limiting the /negotiate endpoint breaks SignalR connection establishment
//     because the client retries negotiation aggressively and will hit limits
//     before the real-time connection can be established.
app.MapHub<NotificationHub>(ApiRoutes.NotificationHub)
   .RequireCors("AllowFrontend")
   .DisableRateLimiting();  // ← critical: negotiate + hub traffic must not be rate-limited
app.MapHub<BattleHub>(ApiRoutes.BattleHub)
   .RequireCors("AllowFrontend")
   .DisableRateLimiting();  // ← critical: same reason

// 13. Migration
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
        throw;
    }
}

app.Run();
