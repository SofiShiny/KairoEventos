var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

// Repositorio en memoria "volátil"
var db = new ExternalServiciosDb();

app.MapGet("/api/servicios/{tipo}", (string tipo) => {
    var filtered = db.Servicios.Where(s => s.Tipo.ToLower() == tipo.ToLower()).ToList();
    return Results.Ok(new { servicios = filtered });
});

app.MapGet("/api/servicios", () => {
    return Results.Ok(db.Servicios);
});

app.MapPost("/api/servicios/update", (UpdateServicioRequest request) => {
    var servicio = db.Servicios.FirstOrDefault(s => s.IdServicioExterno == request.IdServicioExterno);
    if (servicio == null) return Results.NotFound();
    
    servicio.Precio = request.Precio;
    servicio.Disponible = request.Disponible;
    
    return Results.Ok(servicio);
});

app.Run("http://localhost:5091");

public record UpdateServicioRequest(string IdServicioExterno, decimal Precio, bool Disponible);

public class ExternalServicio {
    public string IdServicioExterno { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public string Tipo { get; set; } = "";
    public bool Disponible { get; set; }
    public string Moneda { get; set; } = "USD";
}

public class ExternalServiciosDb {
    public List<ExternalServicio> Servicios { get; set; } = new() {
        new() { IdServicioExterno = "1", Nombre = "Bus", Precio = 10.5m, Tipo = "transporte", Disponible = true },
        new() { IdServicioExterno = "2", Nombre = "Taxi", Precio = 20.0m, Tipo = "transporte", Disponible = false },
        new() { IdServicioExterno = "3", Nombre = "Van", Precio = 15.0m, Tipo = "transporte", Disponible = true },
        
        new() { IdServicioExterno = "10", Nombre = "Camiseta", Precio = 8.5m, Tipo = "merchandising", Disponible = true },
        new() { IdServicioExterno = "11", Nombre = "Gorra", Precio = 5.0m, Tipo = "merchandising", Disponible = true },
        new() { IdServicioExterno = "12", Nombre = "Taza", Precio = 3.5m, Tipo = "merchandising", Disponible = false },
        
        new() { IdServicioExterno = "20", Nombre = "Buffet", Precio = 50.0m, Tipo = "catering", Disponible = true },
        new() { IdServicioExterno = "21", Nombre = "Coffee Break", Precio = 25.0m, Tipo = "catering", Disponible = true },
        new() { IdServicioExterno = "22", Nombre = "Cena Formal", Precio = 70.0m, Tipo = "catering", Disponible = true }
    };
}
