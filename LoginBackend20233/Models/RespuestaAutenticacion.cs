// RespuestaAutenticacion.cs

namespace LoginBackend20233.Models
{
    public class RespuestaAutenticacion
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracion { get; set; }
    }
}
