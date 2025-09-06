namespace EncuestasAPI.Models
{
    public class Opcion
    {
        public int OpcionID { get; set; }
        public string TextoOpcion { get; set; } = string.Empty;
        public int PreguntaID { get; set; }
    }
}
