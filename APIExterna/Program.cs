using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var wireMockServer = WireMockServer.Start(5091); 

wireMockServer.Given(Request.Create().WithPath("/api/servicios/transporte").UsingGet())
        .RespondWith(Response.Create() .WithStatusCode(200) .WithHeader("Content-Type", "application/json")
            .WithBody(@"{ ""servicios"": 
                    [ { ""IdServicioExterno"": ""1"", ""Nombre"": ""Bus"", ""Precio"": 10.5 , ""Moneda"": ""USD"" }, 
                      { ""IdServicioExterno"": ""2"", ""Nombre"": ""Taxi"", ""Precio"": 20.0 , ""Moneda"": ""USD"" }, 
                      { ""IdServicioExterno"": ""3"", ""Nombre"": ""Van"", ""Precio"": 15.0 , ""Moneda"": ""USD"" } ] }"));

wireMockServer.Given(Request.Create().WithPath("/api/servicios/merchandising").UsingGet())
    .RespondWith(Response.Create()
        .WithStatusCode(200)
        .WithHeader("Content-Type", "application/json")
        .WithBody(@"{ ""servicios"": 
                [ { ""IdServicioExterno"": ""10"", ""Nombre"": ""Camiseta"", ""Precio"": 8.5 , ""Moneda"": ""USD"" }, 
                  { ""IdServicioExterno"": ""11"", ""Nombre"": ""Gorra"", ""Precio"":  5.0 , ""Moneda"": ""USD"" }, 
                  { ""IdServicioExterno"": ""12"", ""Nombre"": ""Taza"", ""Precio"": 3.5 , ""Moneda"": ""USD"" } ] }"));

wireMockServer.Given(Request.Create().WithPath("/api/servicios/catering").UsingGet())
    .RespondWith(Response.Create()
        .WithStatusCode(200)
        .WithHeader("Content-Type", "application/json")
        .WithBody(@"{ ""servicios"": 
                [ { ""IdServicioExterno"": ""20"", ""Nombre"": ""Buffet"", ""Precio"": 50.0, ""Moneda"": ""USD"" }, 
                  { ""IdServicioExterno"": ""21"", ""Nombre"": ""Coffee Break"", ""Precio"": 25.0 , ""Moneda"": ""USD"" }, 
                  { ""IdServicioExterno"": ""22"", ""Nombre"": ""Cena Formal"", ""Precio"": 70.0 , ""Moneda"": ""USD"" } ] }"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
