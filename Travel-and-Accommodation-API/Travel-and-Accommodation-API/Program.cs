using AspNetCoreRateLimit;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Travel_and_Accommodation_API;
using Microsoft.AspNetCore.Mvc.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureAutoMapper();

builder.Services.ConfigureRepositories();

builder.Services.ConfigureDataAccessServices();

builder.Services.ConfigureValidations();

builder.Services.ConfigureRateLimiting();

builder.Services.ConfigureJwtAuthentication(builder.Configuration);

builder.Services.ConfigureIdentity();

builder.Services.ConfigureDatabase();

builder.Services.ConfigureEmailAndImageServices();

builder.Services.ConfigureMemoryCache();

builder.Services.ConfigureControllers();

builder.Services.ConfigureSwagger();

builder.Services.ConfigureUnitOfWork();

builder.Services.ConfigureSecurity();



builder.Host.UseSerilog((context, config) =>
    config.WriteTo.Console());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIpRateLimiting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();