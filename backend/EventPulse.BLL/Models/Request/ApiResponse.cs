namespace EventPulse.API.Models.Request
{
    public class ApiResponse<T>(bool success, int statusCode, string message, T? data = default)
    {
        public bool Success { get; set; } = success;

        public int StatusCode { get; set; } = statusCode;

        public string Message { get; set; } = message;
        
        public T? Data { get; set; } = data;
    }
}