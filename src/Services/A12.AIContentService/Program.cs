using A12.Infrastructure;
using A12.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddA12WebConfig(builder.Environment.ContentRootPath);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddA12Database(builder.Configuration);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapA12DatabaseHealth();
app.MapControllers();

app.Run();
