using System.Reflection;
using AuthApi.Auth;
using AuthApi.Auth.Options;
using AuthApi.Data;
using AuthApi.Helpers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//TODO: Add validation
var optionModels = builder.Services.AddOptionModels(
    Assembly.GetExecutingAssembly(), builder.Configuration);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Default") ??
                       throw new InvalidOperationException("Connection string 'Default' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var jwtOptions = optionModels.First(option => option.GetType() == typeof(JwtOptions));
var passwordOptions = optionModels.First(option => option.GetType() == typeof(AppPasswordOptions));
builder.Services.AddIdentityServices((JwtOptions)jwtOptions, (AppPasswordOptions)passwordOptions);

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();