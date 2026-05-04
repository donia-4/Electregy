using System;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PeakWise.API.ExceptionHandling;
using PeakWise.API.Middlewares;
using PeakWise.Application;
using PeakWise.Application.DTOs.Auth;
using PeakWise.Application.ExternalServices.Services.CafeMangment;
using PeakWise.Application.ExternalServices.Services.SmartAssistant;
using PeakWise.Application.Features;
using PeakWise.Application.Features.Devices;
using PeakWise.Application.Interfaces;
using PeakWise.Domain.Common;
using PeakWise.Infrastructure;
using PeakWise.Infrastructure.Service;
using PeakWise.Shared.Responses;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your valid JWT token below."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
// 1. Database & Redis Configuration
var connectionString = builder.Configuration.GetConnectionString("ProdCS");
var redisConnection = builder.Configuration.GetConnectionString("Redis");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ==========================================
// 2. Identity Configuration (וחו ÌÏÇכ ההא UserManager)
// ==========================================
builder.Services.AddIdentity<AppUser, PeakWise.Domain.Entities.Role>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ==========================================
// 3. JWT & Authentication Configuration
// ==========================================
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // we will make it true in production 
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"])),
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;
            var token = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) && ((path.StartsWithSegments("/chatbot")|| path.StartsWithSegments("/consumption"))))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ==========================================
// 4. Application Services & Repositories
// ==========================================
builder.Services.AddScoped<ResponseHandler>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenStoreService, TokenStoreService>();
builder.Services.AddScoped<IConsumptionService, ConsumptionService>();
builder.Services.AddSingleton<MockSimulatorState>();
builder.Services.AddHostedService<PeakWise.Application.Workers.DataIngestionWorker>();
builder.Services.AddScoped<ISmartAssistantService, SamrtAssistantService>();

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddSingleton<TokenManager>();
builder.Services.AddHttpClient<ICafeMangmentService, CafeMangmentService>(client =>
{
    client.BaseAddress = new Uri("https://ignoredmember-peakwise.hf.space/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// =======================================================================
// 5. Global Exception Handling with ProblemDetails And Custom Middleware
// =======================================================================
builder.Services.AddProblemDetails();
builder.Services.AddTransient<StopwatchRequestMiddleware>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
        });
});
// ==========================================
// 6. FluentValidation Registration
// ==========================================
builder.Services.AddValidatorsFromAssemblyContaining<CreateDeviceValidator>();

// Hangfire Configuration
builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("ProdCS")));

builder.Services.AddHangfireServer();

var app = builder.Build();

// ==========================================
// 7. Database Seeding 
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<PeakWise.Domain.Entities.Role>>();

        await PeakWise.Infrastructure.Seeder.RoleSeeder.SeedAsync(roleManager);
        await PeakWise.Infrastructure.Seeder.UserSeeder.SeedAsync(userManager);

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseExceptionHandler();
app.UseMiddleware<StopwatchRequestMiddleware>();
app.UseHangfireDashboard();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<PeakWise.API.Hubs.ChatbotHub>("/chatbot").RequireAuthorization();

app.MapHub<PeakWise.API.Hubs.ConsumptionHub>("/consumption").RequireAuthorization();
using (var scope = app.Services.CreateScope())
{
    // Recurring Job Registration
    RecurringJob.AddOrUpdate<IConsumptionService>(
        "refresh-all-charts",
        service => service.AggregateAllUsersChartDataAsync(),
        Cron.Hourly);
}
app.MapControllers();

app.Run();