using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    [ApiController]
    [Route("api/emailcontas")]
    [Authorize]
    public class EmailContaController : ControllerBase
    {
        private readonly EmailContaService _service;

        public EmailContaController(EmailContaService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET: api/emailcontas/getEmailContas
        /// Retorna lista paginada de contas de e-mail com busca e ordenacao.
        /// </summary>
        [HttpGet("getEmailContas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<EmailContaResponseDTO>>> GetEmailContas(
            [FromQuery] PaginationRequest request)
        {
            var result = await _service.SearchEmailContasAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/emailcontas/getEmailConta/{id}
        /// Retorna uma conta de e-mail especifica pelo ID.
        /// </summary>
        [HttpGet("getEmailConta/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmailContaResponseDTO>> GetEmailConta(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID invalido",
                    Detail = "O ID da conta deve ser um numero positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var emailConta = await _service.GetByIdAsync(id);
            if (emailConta == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "EmailConta nao encontrada",
                    Detail = $"Nenhuma EmailConta encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(emailConta);
        }

        /// <summary>
        /// POST: api/emailcontas/setEmailConta
        /// Cria uma nova conta de e-mail.
        /// </summary>
        [HttpPost("setEmailConta")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EmailContaResponseDTO>> SetEmailConta(
            [FromBody] EmailContaCreateUpdateDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados invalidos",
                    Detail = "Os dados da conta sao obrigatorios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var novoEmailConta = await _service.CriarEmailContaAsync(dto);

            return CreatedAtAction(
                nameof(GetEmailConta),
                new { id = novoEmailConta.ContaID },
                novoEmailConta
            );
        }

        /// <summary>
        /// PUT: api/emailcontas/atualizarEmailConta/{id}
        /// Atualiza uma conta de e-mail existente.
        /// </summary>
        [HttpPut("atualizarEmailConta/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmailContaResponseDTO>> AtualizarEmailConta(
            int id,
            [FromBody] EmailContaCreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Dados invalidos",
                    Detail = "O ID deve ser positivo e os dados da conta sao obrigatorios.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var emailContaAtualizada = await _service.AtualizarEmailContaAsync(id, dto);
            if (emailContaAtualizada == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "EmailConta nao encontrada",
                    Detail = $"Nenhuma EmailConta encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(emailContaAtualizada);
        }

        /// <summary>
        /// DELETE: api/emailcontas/deletarEmailConta/{id}
        /// Remove uma conta de e-mail pelo ID.
        /// </summary>
        [HttpDelete("deletarEmailConta/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletarEmailConta(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "ID invalido",
                    Detail = "O ID da conta deve ser um numero positivo.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var sucesso = await _service.DeletarEmailContaAsync(id);
            if (!sucesso)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "EmailConta nao encontrada",
                    Detail = $"Nenhuma EmailConta encontrada com o ID {id}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
    }
}
