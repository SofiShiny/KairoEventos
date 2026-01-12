using Comunidad.Domain.Entidades;
using MongoDB.Driver;

namespace Comunidad.Infrastructure.Persistencia;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<Foro> Foros => _database.GetCollection<Foro>("Foros");
    public IMongoCollection<Comentario> Comentarios => _database.GetCollection<Comentario>("Comentarios");
}
