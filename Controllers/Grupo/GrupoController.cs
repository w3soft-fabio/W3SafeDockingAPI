using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/grupos")]
    [Authorize]
    public class GrupoController : ControllerBase
    {
        private readonly GrupoService _service;

        public GrupoController(GrupoService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/grupos/getGrupos
        /// Retorna lista paginada de grupos com busca e ordenacao.
        /// </summary>
        [HttpGet("getGrupos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<GrupoResponseDTO>>> GetGrupos(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchGruposAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/grupos/getGrupo/{id}
        /// Retorna um grupo especifico pelo ID.
        /// </summary>
        [HttpGet("getGrupo/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GrupoResponseDTO>> GetGrupo(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID invalido",
                    Detail = "O ID do grupo deve ser um numero positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var grupo = await _service.GetByIdAsync(id);
            if (grupo == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Grupo nao encontrado",
                    Detail = $"Nenhum grupo encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(grupo);
        }

        /// <summary>
        /// POST: api/grupos/setGrupo
        /// Cria um novo grupo.
        /// </summary>
        [HttpPost("setGrupo")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GrupoResponseDTO>> SetGrupo([FromBody] GrupoCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados invalidos",
                    Detail = "Os dados do grupo sao obrigatorios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novoGrupo = await _service.CriarGrupoAsync(dto);

            return CreatedAtAction(
                nameof(GetGrupo),
                new { id = novoGrupo.GrupoId },
                novoGrupo
            );
        }

        /// <summary>
        /// PUT: api/grupos/atualizarGrupo/{id}
        /// Atualiza um grupo existente.
        /// </summary>
        [HttpPut("atualizarGrupo/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GrupoResponseDTO>> AtualizarGrupo(
            int id,
            [FromBody] GrupoCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados invalidos",
                    Detail = "O ID deve ser positivo e os dados do grupo sao obrigatorios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var grupoAtualizado = await _service.AtualizarGrupoAsync(id, dto);
            if (grupoAtualizado == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Grupo nao encontrado",
                    Detail = $"Nenhum grupo encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(grupoAtualizado);
        }

        /// <summary>
        /// DELETE: api/grupos/deletarGrupo/{id}
        /// Remove um grupo pelo ID.
        /// </summary>
        [HttpDelete("deletarGrupo/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarGrupo(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID invalido",
                    Detail = "O ID do grupo deve ser um numero positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarGrupoAsync(id);

            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Grupo nao encontrado",
                    Detail = $"Nenhum grupo encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
