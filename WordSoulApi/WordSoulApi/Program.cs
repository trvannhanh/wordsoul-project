using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using WordSoulApi.Data;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Implementations;
using WordSoulApi.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile("Appsettings/appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"Appsettings/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<WordSoulDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Thêm dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.WithOrigins("http://localhost:5173", "http://localhost:3000")
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
// Quiz Question
builder.Services.AddScoped<IQuizQuestionService, QuizQuestionService>();
builder.Services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
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
// User Owned Pet
builder.Services.AddScoped<IUserOwnedPetRepository, UserOwnedPetRepository>();
builder.Services.AddScoped<IUserOwnedPetService, UserOwnedPetService>();
// User Vocabulary Set
builder.Services.AddScoped<IUserVocabularySetRepository, UserVocabularySetRepository>();
builder.Services.AddScoped<IUserVocabularySetService, UserVocabularySetService>();
// Upload Assests
builder.Services.AddScoped<IUploadAssetsService, UploadAssetsService>();


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


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

// Sử dụng CORS trước các middleware khác
app.UseCors("AllowLocalhost");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
