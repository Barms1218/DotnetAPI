using System.Text;
using DotNetAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors((options) =>
{
    options.AddPolicy("DevCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000") // Origins I want to allow
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    options.AddPolicy("ProdCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("https://myProductionSite.com") // Origins I want to allow
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Need to make sure token key is the same as the one used to make the token
// Accesses the key from the appsettings file without accessing AuthController
string? tokenKeyString = builder.Configuration.GetSection("AppSettings:TokenKey").Value;

SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(tokenKeyString != null ? tokenKeyString : ""));


TokenValidationParameters parameters = new TokenValidationParameters()
{
    IssuerSigningKey = securityKey, // Property that specifies the key that was used to sign the JWT
    ValidateIssuer = false, // Determines whether the API should validate the issuer of the token
    ValidateIssuerSigningKey = false, // Indicates whether the API should validate that the signing key in the token matches the issuer signing key
    ValidateAudience = false // Checks whether the token's audience(expected recipients) matches an expected value
};

// Use this unless there's a security concern
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = parameters; // Tell the API to use the validation parameters created on line 44
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors"); // For when the app is deployed
    app.UseHttpsRedirection(); // Only use if not in development mode
}

app.MapControllers();

app.UseAuthentication(); // Always put above UseAuthorization, need to be Authenticated first

app.UseAuthorization();

app.Run();

