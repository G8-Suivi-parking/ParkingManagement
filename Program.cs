using Microsoft.EntityFrameworkCore;
using ParkingManagement.API.Data;
using ParkingManagement.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Services
// ============================================

builder.Services.AddHttpClient<ParkingImageService>();

builder.Services.AddControllers();

// Calibration
builder.Services.AddSingleton<ParkingCalibrationService>();

// Occupancy
// IMPORTANT : Scoped car il utilise ApplicationDbContext
builder.Services.AddScoped<ParkingOccupancyService>();

// VLM
builder.Services.AddHttpClient<VlmService>();

// ============================================
// Swagger
// ============================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================
// PostgreSQL / Entity Framework
// ============================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// ============================================
// OpenAPI
// ============================================

builder.Services.AddOpenApi();

var app = builder.Build();

// ============================================
// Swagger uniquement en développement
// ============================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

// ============================================
// HTTPS
// ============================================

app.UseHttpsRedirection();

// ============================================
// Controllers
// ============================================

app.MapControllers();

app.Run();