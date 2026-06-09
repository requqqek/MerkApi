using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MerkApi.Data;
using MerkApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=diplom.db"));

// Сервисы
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<CryptoService>();
builder.Services.AddSingleton<JwtService>();

var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? "my_super_secret_key_12345_minimum_32_chars_long!!!";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAndroid", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    var passwords = scope.ServiceProvider.GetRequiredService<PasswordService>();
    var crypto = scope.ServiceProvider.GetRequiredService<CryptoService>();
    DbSeeder.Seed(dbContext, passwords, crypto);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAndroid");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();