namespace EncuestasAPI.Models
{
    public class Encuesta
    {
        public int EncuestaID { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int TipoEncuestaID { get; set; }
        public List<Pregunta> Preguntas { get; set; } = new List<Pregunta>();
    }
}
