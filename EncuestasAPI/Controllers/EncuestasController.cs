using Microsoft.AspNetCore.Mvc;
using EncuestasAPI.Models;
using EncuestasAPI.Services;
using System.Text.Json;

namespace EncuestasAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncuestasController : ControllerBase
    {
        private readonly IEncuestaService _encuestaService;
        private readonly ILogger<EncuestasController> _logger;

        public EncuestasController(IEncuestaService encuestaService, ILogger<EncuestasController> logger)
        {
            _encuestaService = encuestaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una encuesta y sus preguntas/opciones por tipo de encuesta
        /// </summary>
        /// <param name="id">ID del tipo de encuesta (1, 2 o 3)</param>
        /// <returns>Encuesta completa con preguntas y opciones</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Encuesta>> ObtenerEncuestaPorTipo(int id)
        {
            try
            {
                if (id < 1 || id > 3)
                {
                    return BadRequest("El ID del tipo de encuesta debe ser 1, 2 o 3");
                }

                var encuesta = await _encuestaService.ObtenerEncuestaPorTipoAsync(id);
                
                if (encuesta == null)
                {
                    return NotFound($"No se encontró encuesta para el tipo {id}");
                }

                return Ok(encuesta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener encuesta por tipo {TipoId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtiene el resumen de resultados de una encuesta en formato JSON
        /// </summary>
        /// <param name="id">ID de la encuesta (1, 2 o 3)</param>
        /// <returns>Resumen de resultados en JSON</returns>
        [HttpGet("resumen/{id}")]
        public async Task<ActionResult<object>> ObtenerResumenEncuesta(int id)
        {
            try
            {
                if (id < 1 || id > 3)
                {
                    return BadRequest("El ID de la encuesta debe ser 1, 2 o 3");
                }

                var resumenJson = await _encuestaService.ObtenerResumenEncuestaAsync(id);
                
                if (string.IsNullOrWhiteSpace(resumenJson) || resumenJson == "{}")
                {
                    return NotFound($"No se encontraron resultados para la encuesta {id}");
                }

                // Parsear el JSON para retornarlo como objeto
                var resumen = JsonSerializer.Deserialize<object>(resumenJson);
                return Ok(resumen);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear JSON del resumen de encuesta {EncuestaId}", id);
                return StatusCode(500, "Error al procesar el resumen de la encuesta");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resumen de encuesta {EncuestaId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Guarda las respuestas de un usuario
        /// </summary>
        /// <param name="request">Datos del usuario y sus respuestas</param>
        /// <returns>Confirmación de guardado</returns>
        [HttpPost("responder")]
        public async Task<ActionResult> ResponderEncuesta([FromBody] RespuestaUsuarioRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UsuarioID))
                {
                    return BadRequest("El UsuarioID es requerido");
                }

                if (request.Respuestas == null || !request.Respuestas.Any())
                {
                    return BadRequest("Se requiere al menos una respuesta");
                }

                // Validar que todos los campos requeridos estén presentes
                foreach (var respuesta in request.Respuestas)
                {
                    if (respuesta.OpcionID <= 0)
                    {
                        return BadRequest("OpcionID debe ser mayor a 0");
                    }

                    if (respuesta.Seleccionado < 0 || respuesta.Seleccionado > 1)
                    {
                        return BadRequest("Seleccionado debe ser 0 o 1");
                    }
                }

                var resultado = await _encuestaService.GuardarRespuestasUsuarioAsync(request);

                if (!resultado)
                {
                    return StatusCode(500, "Error al guardar las respuestas");
                }

                return Ok(new { 
                    mensaje = "Respuestas guardadas exitosamente",
                    usuario = request.UsuarioID,
                    respuestas_guardadas = request.Respuestas.Count,
                    fecha = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar respuestas del usuario {UsuarioId}", request.UsuarioID);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
