using System.Diagnostics;
using Xunit;

namespace Asientos.Pruebas.Sistema;

/// <summary>
/// Tests de compilación del sistema completo
/// Valida Requirements 10.1, 10.2, 10.3, 10.4, 10.5, 10.6
/// </summary>
public class CompilacionTests
{
    private readonly string _solutionPath;
    private readonly string _projectsBasePath;

    public CompilacionTests()
    {
        // Navegar desde el directorio de tests hasta la raíz de los proyectos
        var currentDir = Directory.GetCurrentDirectory();
        
        // Buscar el directorio que contiene los proyectos
        var searchDir = currentDir;
        while (searchDir != null && !Directory.Exists(Path.Combine(searchDir, "Asientos.API")))
        {
            searchDir = Directory.GetParent(searchDir)?.FullName;
        }
        
        _projectsBasePath = searchDir ?? currentDir;
        _solutionPath = _projectsBasePath;
    }

    [Fact]
    public void Sistema_Debe_Compilar_Sin_Errores()
    {
        // Requirement 10.1: THE System SHALL compilar sin errores
        
        // Compilar cada proyecto individualmente
        var projects = new[] { "Asientos.Dominio", "Asientos.Aplicacion", "Asientos.Infraestructura", "Asientos.API" };
        
        foreach (var project in projects)
        {
            var projectPath = Path.Combine(_projectsBasePath, project, $"{project}.csproj");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectPath}\" --configuration Release --no-incremental",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            Assert.NotNull(process);

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Assert.True(
                process.ExitCode == 0,
                $"La compilación de {project} falló con código {process.ExitCode}.\nOutput: {output}\nError: {error}"
            );
        }
    }

    [Fact]
    public void Compilacion_Debe_Generar_Asientos_Dominio_Dll()
    {
        // Requirement 10.2: THE System SHALL generar Asientos.Dominio.dll
        
        var dllPath = Path.Combine(
            _projectsBasePath,
            "Asientos.Dominio",
            "bin",
            "Release",
            "net8.0",
            "Asientos.Dominio.dll"
        );

        // Compilar primero si no existe
        if (!File.Exists(dllPath))
        {
            CompileProject("Asientos.Dominio");
        }

        Assert.True(
            File.Exists(dllPath),
            $"No se generó Asientos.Dominio.dll en {dllPath}"
        );
    }

    [Fact]
    public void Compilacion_Debe_Generar_Asientos_Aplicacion_Dll()
    {
        // Requirement 10.3: THE System SHALL generar Asientos.Aplicacion.dll
        
        var dllPath = Path.Combine(
            _projectsBasePath,
            "Asientos.Aplicacion",
            "bin",
            "Release",
            "net8.0",
            "Asientos.Aplicacion.dll"
        );

        // Compilar primero si no existe
        if (!File.Exists(dllPath))
        {
            CompileProject("Asientos.Aplicacion");
        }

        Assert.True(
            File.Exists(dllPath),
            $"No se generó Asientos.Aplicacion.dll en {dllPath}"
        );
    }

    [Fact]
    public void Compilacion_Debe_Generar_Asientos_Infraestructura_Dll()
    {
        // Requirement 10.4: THE System SHALL generar Asientos.Infraestructura.dll
        
        var dllPath = Path.Combine(
            _projectsBasePath,
            "Asientos.Infraestructura",
            "bin",
            "Release",
            "net8.0",
            "Asientos.Infraestructura.dll"
        );

        // Compilar primero si no existe
        if (!File.Exists(dllPath))
        {
            CompileProject("Asientos.Infraestructura");
        }

        Assert.True(
            File.Exists(dllPath),
            $"No se generó Asientos.Infraestructura.dll en {dllPath}"
        );
    }

    [Fact]
    public void Compilacion_Debe_Generar_Asientos_API_Dll()
    {
        // Requirement 10.5: THE System SHALL generar Asientos.API.dll
        
        var dllPath = Path.Combine(
            _projectsBasePath,
            "Asientos.API",
            "bin",
            "Release",
            "net8.0",
            "Asientos.API.dll"
        );

        // Compilar primero si no existe
        if (!File.Exists(dllPath))
        {
            CompileProject("Asientos.API");
        }

        Assert.True(
            File.Exists(dllPath),
            $"No se generó Asientos.API.dll en {dllPath}"
        );
    }

    [Fact]
    public void Compilacion_Debe_Completarse_En_Menos_De_10_Segundos()
    {
        // Requirement 10.6: THE System SHALL completar la compilación en menos de 10 segundos
        
        var stopwatch = Stopwatch.StartNew();

        // Compilar cada proyecto individualmente
        var projects = new[] { "Asientos.Dominio", "Asientos.Aplicacion", "Asientos.Infraestructura", "Asientos.API" };
        
        foreach (var project in projects)
        {
            var projectPath = Path.Combine(_projectsBasePath, project, $"{project}.csproj");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectPath}\" --configuration Release --no-incremental",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();
        }

        stopwatch.Stop();

        var compilationTime = stopwatch.Elapsed.TotalSeconds;

        Assert.True(
            compilationTime < 10,
            $"La compilación tomó {compilationTime:F2} segundos, excediendo el límite de 10 segundos"
        );
    }

    private void CompileProject(string projectName)
    {
        var projectPath = Path.Combine(_projectsBasePath, projectName);
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build --configuration Release",
            WorkingDirectory = projectPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        process?.WaitForExit();
    }
}
