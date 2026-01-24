using System.Text;
using ETNA.Api.Data.DbContext;
using ETNA.Api.Data.Interfaces;
using ETNA.Api.Data.Repositories;
using ETNA.Api.Services.Implementations;
using ETNA.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString));

// Repositories
builder.Services.AddScoped<IWorkshopOwnerRepository, WorkshopOwnerRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleVisitRepository, VehicleVisitRepository>();
builder.Services.AddScoped<IWorkshopStaffRepository, WorkshopStaffRepository>();

// Services
builder.Services.AddHttpClient(); // Required for Authkey SMS service
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, AuthkeySmsService>();
builder.Services.AddSingleton<IOtpService, OtpService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<IWorkshopOwnerService, WorkshopOwnerService>(); // TODO: Create this service
// builder.Services.AddScoped<IWorkshopStaffService, WorkshopStaffService>(); // TODO: Create this service
builder.Services.AddScoped<IFileUploadService, LocalFileUploadService>();
// builder.Services.AddScoped<IVerificationService, VerificationService>(); // TODO: Create this service
builder.Services.AddScoped<IWorkshopRegistrationService, WorkshopRegistrationService>();

// OCR Service (Gemini)
builder.Services.AddHttpClient<IOcrService, GeminiOcrService>();



// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// OpenAPI/Swagger
builder.Services.AddOpenApi();

// CORS (for frontend integration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001","https://esoft-one.vercel.app") // Next.js default port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Create uploads directory if it doesn't exist
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// CORS must be before other middleware
app.UseCors("AllowFrontend");

// Only use HTTPS redirect in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Serve static files from uploads folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
