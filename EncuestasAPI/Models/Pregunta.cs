namespace EncuestasAPI.Models
{
    public class Pregunta
    {
        public int PreguntaID { get; set; }
        public string TextoPregunta { get; set; } = string.Empty;
        public int EncuestaID { get; set; }
        public List<Opcion> Opciones { get; set; } = new List<Opcion>();
    }
}
