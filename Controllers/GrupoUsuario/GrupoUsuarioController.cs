using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/gruposUsuarios")]
    [Authorize]
    public class GrupoUsuarioController : ControllerBase
    {
        private readonly GrupoUsuarioService _service;

        public GrupoUsuarioController(GrupoUsuarioService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/gruposUsuarios/getGruposUsuarios
        /// Retorna lista paginada de vinculos grupo-usuario com busca e ordenacao.
        /// </summary>
        [HttpGet("getGruposUsuarios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<GrupoUsuarioResponseDTO>>> GetGruposUsuarios(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchGruposUsuariosAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/gruposUsuarios/getGrupoUsuario/{grupoId}/{usuarioId}
        /// Retorna um vinculo grupo-usuario pela chave composta.
        /// </summary>
        [HttpGet("getGrupoUsuario/{grupoId:int}/{usuarioId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GrupoUsuarioResponseDTO>> GetGrupoUsuario(
            int grupoId,
            int usuarioId)
        {
            if (grupoId <= 0 || usuarioId <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Parametros invalidos",
                    Detail = "grupoId e usuarioId devem ser positivos.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var entidade = await _service.GetByIdAsync(grupoId, usuarioId);
            if (entidade == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Vinculo nao encontrado",
                    Detail = $"Nenhum vinculo encontrado para grupo {grupoId} e usuario {usuarioId}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(entidade);
        }

        /// <summary>
        /// POST: api/gruposUsuarios/setGrupoUsuario
        /// Cria um novo vinculo grupo-usuario.
        /// </summary>
        [HttpPost("setGrupoUsuario")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GrupoUsuarioResponseDTO>> SetGrupoUsuario(
            [FromBody] GrupoUsuarioCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados invalidos",
                    Detail = "Os dados do vinculo sao obrigatorios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novaEntidade = await _service.CriarGrupoUsuarioAsync(dto);

            return CreatedAtAction(
                nameof(GetGrupoUsuario),
                new { grupoId = novaEntidade.GrupoId, usuarioId = novaEntidade.UsuarioId },
                novaEntidade
            );
        }

        /// <summary>
        /// PUT: api/gruposUsuarios/atualizarGrupoUsuario/{grupoId}/{usuarioId}
        /// Atualiza um vinculo grupo-usuario existente.
        /// </summary>
        [HttpPut("atualizarGrupoUsuario/{grupoId:int}/{usuarioId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GrupoUsuarioResponseDTO>> AtualizarGrupoUsuario(
            int grupoId,
            int usuarioId,
            [FromBody] GrupoUsuarioCreateUpdateDTO dto)
        {
            if (grupoId <= 0 || usuarioId <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados invalidos",
                    Detail = "grupoId e usuarioId devem ser positivos e o corpo e obrigatorio.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var entidadeAtualizada = await _service.AtualizarGrupoUsuarioAsync(grupoId, usuarioId, dto);
            if (entidadeAtualizada == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Vinculo nao encontrado",
                    Detail = $"Nenhum vinculo encontrado para grupo {grupoId} e usuario {usuarioId}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(entidadeAtualizada);
        }

        /// <summary>
        /// DELETE: api/gruposUsuarios/deletarGrupoUsuario/{grupoId}/{usuarioId}
        /// Remove um vinculo grupo-usuario pela chave composta.
        /// </summary>
        [HttpDelete("deletarGrupoUsuario/{grupoId:int}/{usuarioId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarGrupoUsuario(int grupoId, int usuarioId)
        {
            if (grupoId <= 0 || usuarioId <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Parametros invalidos",
                    Detail = "grupoId e usuarioId devem ser positivos.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarGrupoUsuarioAsync(grupoId, usuarioId);
            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Vinculo nao encontrado",
                    Detail = $"Nenhum vinculo encontrado para grupo {grupoId} e usuario {usuarioId}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
