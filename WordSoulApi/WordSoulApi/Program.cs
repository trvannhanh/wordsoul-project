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

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Cấu hình Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Information() // Ghi log từ mức Information trở lên
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // Giảm log từ Microsoft
        .WriteTo.Console() // Ghi log ra console
        .WriteTo.File(
            path: "logs/log-.txt", // Lưu log vào file, tạo file mới mỗi ngày
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"); // Định dạng log
});

builder.Services.AddDbContext<WordSoulDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Thêm dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://wordsoul-frontend-cqb5awdca4c7cceg.southeastasia-01.azurewebsites.net")
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

// Add logging service
builder.Services.AddLogging();

// Register repository and service
// Vocabulary
builder.Services.AddScoped<IVocabularyRepository, VocabularyRepository>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
// Auth & User
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Pet 
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IPetRepository, PetRepository>();

// Vocabulary Set
builder.Services.AddScoped<IVocabularySetService, VocabularySetService>();
builder.Services.AddScoped<IVocabularySetRepository, VocabularySetRepository>();
// Learning Session
builder.Services.AddScoped<ILearningSessionService, LearningSessionService>();
builder.Services.AddScoped<ILearningSessionRepository, LearningSessionRepository>();
// Answer Record 
builder.Services.AddScoped<IAnswerRecordRepository, AnswerRecordRepository>();
// User Vocabulary Progress
builder.Services.AddScoped<IUserVocabularyProgressRepository, UserVocabularyProgressRepository>();
builder.Services.AddScoped<IUserVocabularyProgressService, UserVocabularyProgressService>();
// Set Reward Pet
builder.Services.AddScoped<ISetRewardPetRepository, SetRewardPetRepository>();
builder.Services.AddScoped<ISetRewardPetService, SetRewardPetService>();
// User Owned Pet
builder.Services.AddScoped<IUserOwnedPetRepository, UserOwnedPetRepository>();
builder.Services.AddScoped<IUserOwnedPetService, UserOwnedPetService>();
// User Vocabulary Set
builder.Services.AddScoped<IUserVocabularySetRepository, UserVocabularySetRepository>();
builder.Services.AddScoped<IUserVocabularySetService, UserVocabularySetService>();
// Notification
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// ActivityLog
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
// SetVocabulary 
builder.Services.AddScoped<ISetVocabularyRepository, SetVocabularyRepository>();
builder.Services.AddScoped<ISetVocabularyService, SetVocabularyService>();
// SessionVocabulary
builder.Services.AddScoped<ISessionVocabularyRepository, SessionVocabularyRepository>();
// Item
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();


// Achievement
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
// UserAchievement
builder.Services.AddScoped<IUserAchievementRepository, UserAchievementRepository>();
// Upload Assests
builder.Services.AddScoped<IUploadAssetsService, UploadAssetsService>();

//Background Service
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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WordSoulDbContext>();
    if (dbContext.Database.IsRelational() )
    {
        dbContext.Database.Migrate();
    }    
}    


//Hub SignalR
app.MapHub<NotificationHub>("/notificationHub");

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

// Thêm middleware Serilog để ghi log các yêu cầu HTTP
app.UseSerilogRequestLogging();

// Sử dụng CORS trước các middleware khác
app.UseCors("AllowLocalhost");

app.UseHttpsRedirection();

app.UseAuthentication();    
app.UseAuthorization();

app.MapControllers();

app.Run();
