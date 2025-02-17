using Amazon.Runtime;
using Amazon.S3;
using CcsSportScheduler_API.Models.Background;
using CcsSportScheduler_Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "CCS SportScheduler",
        Description = "Dostupni API za Ccs SportScheduler!",
    });
    c.OperationFilter<SwaggerFileOperationFilter>(); // Dodato za podršku IFormFile
    c.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" }); // Dodato za mapiranje IFormFile
});

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("WebApiDatabase");
builder.Services.AddDbContext<SportSchedulerContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.26-mysql")));

builder.Services.AddHostedService<BackgroundRefresh>();

var awsOption = builder.Configuration.GetAWSOptions("service2");
awsOption.Credentials = new BasicAWSCredentials(builder.Configuration["AWS:AccessKey"], builder.Configuration["AWS:SecretKey"]);
builder.Services.AddDefaultAWSOptions(awsOption);
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Register HttpClient with BaseAddress from configuration
builder.Services.AddHttpClient("MyHttpClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseAddress"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = "swagger"; // Postavi Swagger UI na /swagger
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapFallbackToFile("index.html"); // Serviraj React aplikaciju ako putanja nije pronađena
});

app.Run();