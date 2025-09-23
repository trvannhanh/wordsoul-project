using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using WordSoulApi.Data;
using WordSoulApi.Hubs;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Implementations;
using WordSoulApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("Appsettings/appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"Appsettings/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
// builder.Services.AddOpenApi();

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

// 🔑 Dual provider (Dev = SQL Server, Prod = Postgres)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<WordSoulDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    var pgConn = Environment.GetEnvironmentVariable("DefaultConnection");
    builder.Services.AddDbContext<WordSoulDbContext>(options =>
        options.UseNpgsql(pgConn));
}

// Thêm dịch vụ CORS
var frontendUrl = Environment.GetEnvironmentVariable("FrontendUrl") ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrl)
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

//Thêm dịch vụ SignalR
builder.Services.AddSignalR();

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
builder.Services.AddScoped<IUploadAssetsService, UploadAssetsService>();
builder.Services.AddHostedService<NotificationBackgroundService>();

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

//Hub SignalR
app.MapHub<NotificationHub>("/notificationHub");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    // app.MapOpenApi();
}

app.UseSerilogRequestLogging();

// 🚀 Dùng CORS
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 🚀 Railway auto migrate database
if (!app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<WordSoulDbContext>();
        db.Database.Migrate();
    }
}

// 🚀 Railway bind PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add("http://0.0.0.0:" + port);

app.Run();
