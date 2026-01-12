using Reportes.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure all services
builder.ConfigureServices();

var app = builder.Build();

// Configure the HTTP request pipeline
app.ConfigurePipeline();

app.Run();

// Hacer la clase Program accesible para tests de integración
public partial class Program { }
