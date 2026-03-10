using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _service;

        public UsuarioController(UsuarioService service)
        {
            _service = service;
        }

        /// <summary>
        /// 📋 GET: api/usuarios/getUsuarios
        /// Retorna lista paginada de usuários com busca e ordenação.
        /// </summary>
        [HttpGet("getUsuarios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponse<UsuarioResponseDTO>>> GetUsuarios(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchUsuariosAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// 🔍 GET: api/usuarios/getUsuario/{id}
        /// Retorna um usuário específico pelo ID.
        /// </summary>
        [HttpGet("getUsuario/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioResponseDTO>> GetUsuario(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID do usuário deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var usuario = await _service.GetByIdAsync(id);
            if (usuario == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Usuário não encontrado",
                    Detail = $"Nenhum usuário encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(usuario);
        }

        /// <summary>
        /// ✅ POST: api/usuarios/setUsuario
        /// Cria um novo usuário.
        /// </summary>
        [HttpPost("setUsuario")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UsuarioResponseDTO>> SetUsuario(
            [FromBody] UsuarioCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "Os dados do usuário são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novoUsuario = await _service.CriarUsuarioAsync(dto);

            return CreatedAtAction(
                nameof(GetUsuario),
                new { id = novoUsuario.Id },
                novoUsuario
            );
        }

        /// <summary>
        /// ✏️ PUT: api/usuarios/atualizarUsuario/{id}
        /// Atualiza um usuário existente.
        /// </summary>
        [HttpPut("atualizarUsuario/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioResponseDTO>> AtualizarUsuario(
            int id,
            [FromBody] UsuarioCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados inválidos",
                    Detail = "O ID deve ser positivo e os dados do usuário são obrigatórios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var usuarioAtualizado = await _service.AtualizarUsuarioAsync(id, dto);
            if (usuarioAtualizado == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Usuário não encontrado",
                    Detail = $"Nenhum usuário encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(usuarioAtualizado);
        }

        /// <summary>
        /// 🗑️ DELETE: api/usuarios/deletarUsuario/{id}
        /// Remove um usuário pelo ID.
        /// </summary>
        [HttpDelete("deletarUsuario/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarUsuario(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID inválido",
                    Detail = "O ID do usuário deve ser um número positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarUsuarioAsync(id);

            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Usuário não encontrado",
                    Detail = $"Nenhum usuário encontrado com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
