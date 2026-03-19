using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/recursos")]
    [Authorize]
    public class RecursoController : ControllerBase
    {
        private readonly RecursoService _service;

        public RecursoController(RecursoService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/recursos/getRecursos
        /// Retorna lista paginada de recursos com busca e ordenacao.
        /// </summary>
        [HttpGet("getRecursos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<RecursoResponseDTO>>> GetRecursos(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchRecursosAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/recursos/getRecurso/{recursoChave}
        /// Retorna um recurso especifico pela chave.
        /// </summary>
        [HttpGet("getRecurso/{recursoChave}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecursoResponseDTO>> GetRecurso(string recursoChave)
        {
            if (string.IsNullOrWhiteSpace(recursoChave))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Parametro invalido",
                    Detail = "A chave do recurso deve ser informada.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var recurso = await _service.GetByIdAsync(recursoChave);
            if (recurso == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Recurso nao encontrado",
                    Detail = $"Nenhum recurso encontrado com a chave '{recursoChave}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(recurso);
        }

        /// <summary>
        /// POST: api/recursos/setRecurso
        /// Cria um novo recurso.
        /// </summary>
        [HttpPost("setRecurso")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecursoResponseDTO>> SetRecurso(
            [FromBody] RecursoCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados invalidos",
                    Detail = "Os dados do recurso sao obrigatorios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novoRecurso = await _service.CriarRecursoAsync(dto);

            return CreatedAtAction(
                nameof(GetRecurso),
                new { recursoChave = novoRecurso.RecursoChave },
                novoRecurso
            );
        }

        /// <summary>
        /// PUT: api/recursos/atualizarRecurso/{recursoChave}
        /// Atualiza um recurso existente.
        /// </summary>
        [HttpPut("atualizarRecurso/{recursoChave}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecursoResponseDTO>> AtualizarRecurso(
            string recursoChave,
            [FromBody] RecursoCreateUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(recursoChave) || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados invalidos",
                    Detail = "A chave do recurso deve ser informada e o corpo e obrigatorio.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var recursoAtualizado = await _service.AtualizarRecursoAsync(recursoChave, dto);
            if (recursoAtualizado == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Recurso nao encontrado",
                    Detail = $"Nenhum recurso encontrado com a chave '{recursoChave}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(recursoAtualizado);
        }

        /// <summary>
        /// DELETE: api/recursos/deletarRecurso/{recursoChave}
        /// Remove um recurso pela chave.
        /// </summary>
        [HttpDelete("deletarRecurso/{recursoChave}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarRecurso(string recursoChave)
        {
            if (string.IsNullOrWhiteSpace(recursoChave))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Parametro invalido",
                    Detail = "A chave do recurso deve ser informada.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarRecursoAsync(recursoChave);
            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Recurso nao encontrado",
                    Detail = $"Nenhum recurso encontrado com a chave '{recursoChave}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
