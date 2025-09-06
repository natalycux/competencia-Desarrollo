namespace EncuestasAPI.Models
{
    public class RespuestaUsuarioRequest
    {
        public string UsuarioID { get; set; } = string.Empty;
        public List<RespuestaItem> Respuestas { get; set; } = new List<RespuestaItem>();
    }

    public class RespuestaItem
    {
        public int OpcionID { get; set; }
        public int Seleccionado { get; set; }
    }

    public class RespuestaUsuario
    {
        public int RespuestaID { get; set; }
        public string UsuarioID { get; set; } = string.Empty;
        public int OpcionID { get; set; }
        public int Seleccionado { get; set; }
        public DateTime FechaRespuesta { get; set; }
    }
}
