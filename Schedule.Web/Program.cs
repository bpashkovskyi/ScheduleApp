using FastEndpoints;
using FastEndpoints.Swagger;
using Schedule.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

// Add authorization services
builder.Services.AddAuthorization();

// Add custom services
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<ILessonMapper, LessonMapper>();
builder.Services.AddHttpClient();

// Add CORS for static files
builder.Services.AddCors(corsOptions =>
{
    corsOptions.AddDefaultPolicy(corsPolicy =>
    {
        corsPolicy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseCors();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Use FastEndpoints (this must come BEFORE MapFallbackToFile)
app.UseFastEndpoints();

// Add Swagger UI for FastEndpoints
app.UseSwaggerGen();

// Map fallback to index.html AFTER FastEndpoints
app.MapFallbackToFile("index.html");

app.Run(); 