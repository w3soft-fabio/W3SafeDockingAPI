using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/gruposRecursos")]
    [Authorize]
    public class GrupoRecursoController : ControllerBase
    {
        private readonly GrupoRecursoService _service;

        public GrupoRecursoController(GrupoRecursoService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/gruposRecursos/getGruposRecursos
        /// Retorna lista paginada de vinculos grupo-recurso com busca e ordenacao.
        /// </summary>
        [HttpGet("getGruposRecursos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<GrupoRecursoResponseDTO>>> GetGruposRecursos(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchGruposRecursosAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/gruposRecursos/getGrupoRecurso/{grupoId}/{recursoChave}
        /// Retorna um vinculo grupo-recurso pela chave composta.
        /// </summary>
        [HttpGet("getGrupoRecurso/{grupoId:int}/{recursoChave}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GrupoRecursoResponseDTO>> GetGrupoRecurso(
            int grupoId,
            string recursoChave)
        {
            if (grupoId <= 0 || string.IsNullOrWhiteSpace(recursoChave))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Parametros invalidos",
                    Detail = "grupoId deve ser positivo e recursoChave deve ser informada.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var entidade = await _service.GetByIdAsync(grupoId, recursoChave);
            if (entidade == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Vinculo nao encontrado",
                    Detail = $"Nenhum vinculo encontrado para grupo {grupoId} e recurso '{recursoChave}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(entidade);
        }

        /// <summary>
        /// GET: api/gruposRecursos/checarRecursoVinculado/{usuarioId}/{recursoChave}
        /// Verifica se um usuario possui acesso ao recurso atraves dos grupos vinculados.
        /// </summary>
        [HttpGet("checarRecursoVinculado/{usuarioId:int}/{recursoChave}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ChecarRecursoVinculadoResponseDTO>> ChecarRecursoVinculado(
            int usuarioId,
            string recursoChave)
        {
            if (usuarioId <= 0 || string.IsNullOrWhiteSpace(recursoChave))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Parametros invalidos",
                    Detail = "usuarioId deve ser positivo e recursoChave deve ser informada.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var possuiAcesso = await _service.ChecarRecursoVinculadoAsync(usuarioId, recursoChave);

            return Ok(new ChecarRecursoVinculadoResponseDTO
            {
                UsuarioId = usuarioId,
                RecursoChave = recursoChave.Trim(),
                PossuiAcesso = possuiAcesso
            });
        }

        /// <summary>
        /// POST: api/gruposRecursos/setGrupoRecurso
        /// Cria um novo vinculo grupo-recurso.
        /// </summary>
        [HttpPost("setGrupoRecurso")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GrupoRecursoResponseDTO>> SetGrupoRecurso(
            [FromBody] GrupoRecursoCreateUpdateDTO dto)
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

            var novaEntidade = await _service.CriarGrupoRecursoAsync(dto);

            return CreatedAtAction(
                nameof(GetGrupoRecurso),
                new { grupoId = novaEntidade.GrupoId, recursoChave = novaEntidade.RecursoChave },
                novaEntidade
            );
        }

        /// <summary>
        /// PUT: api/gruposRecursos/atualizarGrupoRecurso/{grupoId}/{recursoChave}
        /// Atualiza um vinculo grupo-recurso existente.
        /// </summary>
        [HttpPut("atualizarGrupoRecurso/{grupoId:int}/{recursoChave}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GrupoRecursoResponseDTO>> AtualizarGrupoRecurso(
            int grupoId,
            string recursoChave,
            [FromBody] GrupoRecursoCreateUpdateDTO dto)
        {
            if (grupoId <= 0 || string.IsNullOrWhiteSpace(recursoChave) || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados invalidos",
                    Detail = "grupoId deve ser positivo, recursoChave deve ser informada e o corpo e obrigatorio.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var entidadeAtualizada = await _service.AtualizarGrupoRecursoAsync(grupoId, recursoChave, dto);
            if (entidadeAtualizada == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Vinculo nao encontrado",
                    Detail = $"Nenhum vinculo encontrado para grupo {grupoId} e recurso '{recursoChave}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(entidadeAtualizada);
        }

        /// <summary>
        /// DELETE: api/gruposRecursos/deletarGrupoRecurso/{grupoId}/{recursoChave}
        /// Remove um vinculo grupo-recurso pela chave composta.
        /// </summary>
        [HttpDelete("deletarGrupoRecurso/{grupoId:int}/{recursoChave}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarGrupoRecurso(int grupoId, string recursoChave)
        {
            if (grupoId <= 0 || string.IsNullOrWhiteSpace(recursoChave))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Parametros invalidos",
                    Detail = "grupoId deve ser positivo e recursoChave deve ser informada.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarGrupoRecursoAsync(grupoId, recursoChave);
            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Vinculo nao encontrado",
                    Detail = $"Nenhum vinculo encontrado para grupo {grupoId} e recurso '{recursoChave}'.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
