using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using Usuarios.API.Middlewares;
using Usuarios.Infrastructure.RequestHttp;

namespace Usuarios.Test.Infrastructure.RequestHttp;

public class TestRequestHttp
{
    private HttpResponseMessage response;
    private Mock<HttpMessageHandler> message;
    private HttpClient httpClient;
    private WebRequestHttp requestHttp;
    private FormUrlEncodedContent content;
    private Dictionary<string, string> headers;
    private Mock<ILogger<WebRequestHttp>> logger;

    public TestRequestHttp()
    {
        response = new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "test", "test" }
            })
        };
        message = new();
        httpClient = new (message.Object);
        logger = new();
        requestHttp = new (httpClient,logger.Object);
        content = new(new Dictionary<string, string>() { { "test", "test" } });
        headers = new() { { "test", "test" } };
    }

    [Fact]
    public async void Test_GetAsync_PasarUnStatusCorrecto_ObtieneLaMismaRespuesta()
    {
        message.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),ItExpr.IsAny<CancellationToken>()).ReturnsAsync(response);

        var respuesta = await requestHttp.GetAsync("http://localhost:8080/admin/realms/myrealm/users");

        Assert.Contains("test", respuesta);
    }

    [Fact]
    public async void Test_PutAsync_PasarUnStatusCorrecto_ObtieneLaMismaRespuesta()
    {
        message.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(response);

        var respuesta = await requestHttp.PutAsync("http://localhost:8080/admin/realms/myrealm/users",content,headers);

        Assert.Contains("test", respuesta);
    }

    [Fact]
    public async void Test_PostAsync_PasarUnStatusCorrecto_ObtieneLaRespuestaEsperada()
    {
        message.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(response);

        var respuesta = await requestHttp.PostAsync("http://localhost:8080/admin/realms/myrealm/users", content, headers);

        Assert.Contains("test", respuesta);
    }

    [Fact]
    public async void Test_DeleteAsync_PasarUnStatusCorrecto_ObtieneLaMismaRespuesta()
    {
        message.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(response);

        var respuesta = await requestHttp.DeleteAsync("http://localhost:8080/admin/realms/myrealm/users", headers);

        Assert.Contains("test", respuesta);
    }
}