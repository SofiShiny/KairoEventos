namespace Entradas.API.DTOs;

/// <summary>
/// Envoltorio genérico para respuestas de la API, compatible con el frontend unificado
/// </summary>
/// <typeparam name="T">Tipo de los datos devueltos</typeparam>
public class ApiResponse<T>
{
    public T Data { get; set; }
    public string Message { get; set; }
    public bool Success { get; set; }

    public ApiResponse(T data, string message = null, bool success = true)
    {
        Data = data;
        Message = message;
        Success = success;
    }

    public static ApiResponse<T> Ok(T data, string message = null) => new ApiResponse<T>(data, message, true);
    public static ApiResponse<T> Error(string message, T data = default) => new ApiResponse<T>(data, message, false);
}
