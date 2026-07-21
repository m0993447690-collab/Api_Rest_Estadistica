using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Proyecto_GolMundial.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
    //builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EstadisticasDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// HttpClient para el servicio UTNGolCoin
builder.Services.AddHttpClient<Proyecto_GolMundial.Services.UtnGolCoinClient>();

// 1. Agregar el servicio de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirRedLocal", policy =>
    {
        // Permite peticiones desde cualquier origen, con cualquier método y cabecera
        policy.AllowAnyOrigin() 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seeding logic on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<EstadisticasDbContext>();
        
        string? seedFilePath = null;
        var pathsToTry = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "seed-utn-golmundial-2026.json"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Docs", "seed-utn-golmundial-2026.json"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Docs", "seed-utn-golmundial-2026.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "Docs", "seed-utn-golmundial-2026.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "Docs", "seed-utn-golmundial-2026.json"),
            @"c:\Users\m0993\Desktop\Proyecto_GolMundial\Docs\seed-utn-golmundial-2026.json"
        };
        
        foreach (var path in pathsToTry)
        {
            if (File.Exists(path))
            {
                seedFilePath = path;
                break;
            }
        }
        
        if (seedFilePath != null)
        {
            logger.LogInformation("Found seed file at: {SeedFilePath}", seedFilePath);
            await DbSeeder.SeedAsync(context, seedFilePath);
            logger.LogInformation("Database seeding completed successfully.");
        }
        else
        {
            logger.LogWarning("Seed data file seed-utn-golmundial-2026.json not found in any checked paths.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

// 2. Aplicar la política de CORS
app.UseCors("PermitirRedLocal");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
