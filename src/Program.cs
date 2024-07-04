using System.Reflection;
using AuthApi.Admin.Filters;
using AuthApi.Auth.Services;
using AuthApi.Data;
using AuthApi.Helpers;
using AuthApi.Program;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//TODO: Add validation
var optionModels = builder.Services.AddOptionModels(
    Assembly.GetExecutingAssembly(), builder.Configuration
);

builder.Services.AddControllers(options => { options.Filters.Add(new AdminAuthorizeFilter()); });

var connectionString = builder.Configuration.GetConnectionString("Default") ??
                       throw new InvalidOperationException("Connection string 'Default' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentityServices(optionModels);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerConfigs());

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    await app.SeedDatabaseAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();