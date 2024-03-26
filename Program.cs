using AzureChatAPI;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var diagnosticSource = app.Services.GetRequiredService<DiagnosticListener>();
using var badRequestListener = new BadRequestEventListener(diagnosticSource, (badRequestExceptionFeature) =>
{
    app.Logger.LogError(badRequestExceptionFeature.Error, "Bad request received");
});

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder =>
{
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
});

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();
    
app.MapFallbackToFile("/index.html");

app.Run();
