using Microsoft.Data.SqlClient;
using EncuestasAPI.Models;
using System.Data;

namespace EncuestasAPI.Services
{
    public interface IEncuestaService
    {
        Task<Encuesta?> ObtenerEncuestaPorTipoAsync(int tipoEncuestaId);
        Task<string> ObtenerResumenEncuestaAsync(int encuestaId);
        Task<bool> GuardarRespuestasUsuarioAsync(RespuestaUsuarioRequest request);
    }

    public class EncuestaService : IEncuestaService
    {
        private readonly string _connectionString;

        public EncuestaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("ConnectionString");
        }

        public async Task<Encuesta?> ObtenerEncuestaPorTipoAsync(int tipoEncuestaId)
        {
            Encuesta? encuesta = null;
            var preguntas = new Dictionary<int, Pregunta>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ObtenerEncuestaPorTipo", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@TipoEncuestaID", tipoEncuestaId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // Primera vez que leemos la encuesta
                if (encuesta == null)
                {
                    encuesta = new Encuesta
                    {
                        EncuestaID = reader.GetInt32("EncuestaID"),
                        Titulo = reader.GetString("Titulo"),
                        Descripcion = reader.GetString("Descripcion"),
                        TipoEncuestaID = reader.GetInt32("TipoEncuestaID")
                    };
                }

                var preguntaId = reader.GetInt32("PreguntaID");
                
                // Agregar pregunta si no existe
                if (!preguntas.ContainsKey(preguntaId))
                {
                    var pregunta = new Pregunta
                    {
                        PreguntaID = preguntaId,
                        TextoPregunta = reader.GetString("TextoPregunta"),
                        EncuestaID = encuesta.EncuestaID
                    };
                    preguntas[preguntaId] = pregunta;
                    encuesta.Preguntas.Add(pregunta);
                }

                // Agregar opción a la pregunta
                if (!reader.IsDBNull("OpcionID"))
                {
                    var opcion = new Opcion
                    {
                        OpcionID = reader.GetInt32("OpcionID"),
                        TextoOpcion = reader.GetString("TextoOpcion"),
                        PreguntaID = preguntaId
                    };
                    preguntas[preguntaId].Opciones.Add(opcion);
                }
            }

            return encuesta;
        }

        public async Task<string> ObtenerResumenEncuestaAsync(int encuestaId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_ResumenEncuestaJson", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@EncuestaID", encuestaId);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            
            return result?.ToString() ?? "{}";
        }

        public async Task<bool> GuardarRespuestasUsuarioAsync(RespuestaUsuarioRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            
            try
            {
                foreach (var respuesta in request.Respuestas)
                {
                    var insertCommand = new SqlCommand(
                        "INSERT INTO RespuestasUsuario (UsuarioID, OpcionID, Seleccionado, FechaRespuesta) " +
                        "VALUES (@UsuarioID, @OpcionID, @Seleccionado, @FechaRespuesta)", 
                        connection, transaction);

                    insertCommand.Parameters.AddWithValue("@UsuarioID", request.UsuarioID);
                    insertCommand.Parameters.AddWithValue("@OpcionID", respuesta.OpcionID);
                    insertCommand.Parameters.AddWithValue("@Seleccionado", respuesta.Seleccionado);
                    insertCommand.Parameters.AddWithValue("@FechaRespuesta", DateTime.Now);

                    await insertCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}
