using Usuarios.Infrastructure.Persistencia;

namespace Usuarios.Test.Infrastructure.Persistencia;

public class TestFactoryUsuarioContext
{
    [Fact]
    public void Test_FactoryUsuarioContext_ParametrosNormales_TipoDeDatoEsperadoUsuariosContext()
    {
        var factory = new FactoryUsuarioContext();

        var context = factory.CreateDbContext(["test"]);

        Assert.IsType<UsuariosContext>(context);
    }
}