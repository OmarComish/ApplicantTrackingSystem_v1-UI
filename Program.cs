using Microsoft.EntityFrameworkCore;
using ATS.API.Data;
using ATS.API.Services;
using ATS.API.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title =" Applicant Tracking System API",
            Version ="v1",
            Description ="A prototype Applicant tracking system API",
            Contact = new OpenApiContact
            {
                Name ="Codaflem Malawi",
                Email ="support@codaflem.io"
            }
        });
    }
);

// Database
builder.Services.AddDbContext<AtsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service registrations
builder.Services.AddScoped<IJobPostingService, JobPostingService>();
builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<IShortlistingService, ShortlistingService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// External API clients
builder.Services.AddHttpClient<IOpenCatsClient, OpenCatsClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:OpenCats:BaseUrl"]);
});

builder.Services.AddHttpClient<IApideckClient, ApideckClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:Apideck:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["ExternalApis:Apideck:ApiKey"]}");
});

builder.Services.AddHttpClient<IKnitClient, KnitClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:Knit:BaseUrl"]);
});

builder.Services.AddHttpClient<IMergeClient, MergeClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalApis:Merge:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["ExternalApis:Merge:ApiKey"]}");
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsProduction() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
