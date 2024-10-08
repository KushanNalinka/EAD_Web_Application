   using EADWebApplication.Helpers;
using EADWebApplication.Models;
using EADWebApplication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS to allow any origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.AllowAnyOrigin() // Allow requests from any origin
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
// Add MongoDB settings
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<VendorService>();
builder.Services.AddSingleton<ProductService>();  // Register ProductService
builder.Services.AddSingleton<OrderService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddSingleton<ShortNotificationService>();
builder.Services.AddSingleton<ProductNotificationService>();
builder.Services.AddSingleton<CancelNotificationService>();
builder.Services.AddSingleton<CategoryService>();
// Add JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<JwtHelper>();

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Enable CORS
app.UseCors("AllowAnyOrigin");

app.UseStaticFiles(); // To serve images
app.UseHttpsRedirection();

//app.UseAuthorization();

app.UseAuthentication();  // Ensure JWT authentication is used before authorization
app.UseAuthorization();   // Ensure authorization happens after authentication


app.MapControllers();


app.Run();
