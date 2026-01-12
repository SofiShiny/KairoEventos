using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Usuarios.API.Middlewares;
using Usuarios.Application.Exceptions;

namespace Usuarios.Test.API.Middlewares;

public class TestExceptionMiddleware
{
    private DefaultHttpContext _context;
    private ExceptionMiddleware _middleware;
    private Exception _exception;
    private Mock<ILogger<ExceptionMiddleware>> logger;

    public TestExceptionMiddleware()
    {
        logger = new();
        Task Request(HttpContext httpContext) => throw _exception!;
        _context = new DefaultHttpContext();
        _middleware = new ExceptionMiddleware(Request,logger.Object);
    }

    [Fact]
    public async Task Test_ExceptionMiddleware_SeLanzaUnaValidatorException_409()
    {
        _exception = new ValidatorException("test");

        await _middleware.InvokeAsync(_context);

        Assert.Equal(409,_context.Response.StatusCode);
    }

    [Fact]
    public async Task Test_ExceptionMiddleware_SeLanzaUnaAutoMapperMappingException_422()
    {
        _exception = new AutoMapperMappingException("test");

        await _middleware.InvokeAsync(_context);

        Assert.Equal(422, _context.Response.StatusCode);
    }

    [Fact]
    public async Task Test_ExceptionMiddleware_SeLanzaUnaMongoException_500()
    {
        _exception = new MongoException("test");

        await _middleware.InvokeAsync(_context);

        Assert.Equal(500, _context.Response.StatusCode);
    }

    [Fact]
    public async Task Test_ExceptionMiddleware_SeLanzaUnaDbUpdateException_504()
    {
        _exception = new DbUpdateException("test");

        await _middleware.InvokeAsync(_context);

        Assert.Equal(504, _context.Response.StatusCode);
    }

    [Fact]
    public async Task Test_ExceptionMiddleware_SeLanzaUnaHttpRequestException_502()
    {
        _exception = new HttpRequestException("test");

        await _middleware.InvokeAsync(_context);

        Assert.Equal(502, _context.Response.StatusCode);
    }

    [Fact]
    public async Task Test_ExceptionMiddleware_SeLanzaUnaAutenticationException_417()
    {
        _exception = new AutenticacionException("test");

        await _middleware.InvokeAsync(_context);

        Assert.Equal(417, _context.Response.StatusCode);
    }

    [Fact]
    public async Task Test_ExceptionMiddleware_SeLanzaUnaRegistroNoEncotradoException_400()
    {
        _exception = new RegistroNoEncontradoException("test");

        await _middleware.InvokeAsync(_context);

        Assert.Equal(400, _context.Response.StatusCode);
    }
}