using BankAccountService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using BankAccountService.Core.Resources.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("TestDb") ?? throw new InvalidOperationException("Connection string 'TestDb' not found.");

ConfigurationManager configuration = builder.Configuration;

builder.Services.AddDbContext<BankAccountServiceDbContext>(x => x.UseSqlServer(connectionString, builder =>
{
    builder.EnableRetryOnFailure(1, TimeSpan.FromSeconds(5), null);
}));

builder.Services.AddLocalization();
builder.Services.AddControllers();

// Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var langCodes = Languages.GetAll();
var supportedCultures = new List<CultureInfo>(langCodes.Count);

foreach (var langCode in langCodes)
{
    supportedCultures.Add(new CultureInfo(langCode));
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(Languages.EN),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseAuthorization();

app.MapControllers();

app.Run();
