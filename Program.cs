using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using tradetrackr.api.Data;
using tradetrackr.api.Models;
using tradetrackr.api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure database connection
var connectionString = builder.Configuration["TradetrackrDb:ConnectionStrings"]
    ?? builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("Database connection string not found.");

var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<JobStatus>();
dataSourceBuilder.MapEnum<InvoiceStatus>();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<TradeTrackrDbContext>(options =>
    options.UseNpgsql(dataSource));

// Register custom services
builder.Services.AddScoped<ITaxCalculationService, TaxCalculationService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Configure CORS with environment-specific origins
var corsOrigins = builder.Configuration["CORS_ORIGINS"]?.Split(',')
    ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TradeTrackr API",
        Version = "v1",
        Description = "API for TradeTrackr app"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your access token below without the 'Bearer' prefix"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
           new List<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure Auth0 settings
var auth0Domain = builder.Configuration["Auth0:Domain"]
    ?? builder.Configuration["AUTH0_DOMAIN"]
    ?? throw new InvalidOperationException("Auth0 domain not configured.");

var auth0Audience = builder.Configuration["Auth0:Audience"]
    ?? builder.Configuration["AUTH0_AUDIENCE"]
    ?? throw new InvalidOperationException("Auth0 audience not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = auth0Domain;
        options.Audience = auth0Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = auth0Domain,
            ValidateAudience = true,
            ValidAudience = auth0Audience,
            ValidateLifetime = true
        };

        options.MapInboundClaims = false;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Enable Swagger in production for Render deployment
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TradeTrackr API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => "OK");

app.Run();