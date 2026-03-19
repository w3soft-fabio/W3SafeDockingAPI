using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    /// <summary>
    /// Controller de autenticacao.
    /// Contem endpoints de login, refresh, revoke e primeiro acesso.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// POST: api/auth/login
        /// Autentica o usuario com CPF e senha.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var resultado = await _authService.LoginAsync(request);

            if (resultado == null)
            {
                return Unauthorized(new
                {
                    erro = "CPF ou senha invalidos."
                });
            }

            return Ok(resultado);
        }

        /// <summary>
        /// POST: api/auth/refresh
        /// Renova o Access Token usando o Refresh Token.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var resultado = await _authService.RefreshAsync(request.RefreshToken);

            if (resultado == null)
            {
                return Unauthorized(new
                {
                    erro = "Refresh Token invalido ou expirado. Faca login novamente."
                });
            }

            return Ok(resultado);
        }

        /// <summary>
        /// POST: api/auth/revoke
        /// Revoga (invalida) o Refresh Token do usuario.
        /// </summary>
        [HttpPost("revoke")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
        {
            var sucesso = await _authService.RevogarTokenAsync(request.RefreshToken);

            if (!sucesso)
            {
                return BadRequest(new
                {
                    erro = "Refresh Token invalido ou ja revogado."
                });
            }

            return Ok(new
            {
                mensagem = "Logout realizado com sucesso."
            });
        }

        /// <summary>
        /// POST: api/auth/definir-primeira-senha
        /// Define a senha inicial usando token de primeiro acesso.
        /// </summary>
        [HttpPost("definir-primeira-senha")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DefinirPrimeiraSenha([FromBody] DefinirPrimeiraSenhaRequest request)
        {
            var sucesso = await _authService.DefinirPrimeiraSenhaAsync(request);

            if (!sucesso)
            {
                return BadRequest(new
                {
                    erro = "Token de primeiro acesso invalido, expirado ou ja utilizado."
                });
            }

            return Ok(new
            {
                mensagem = "Senha definida com sucesso."
            });
        }

        /// <summary>
        /// POST: api/auth/reenviar-primeiro-acesso
        /// Gera e envia um novo link de primeiro acesso.
        /// </summary>
        [HttpPost("reenviar-primeiro-acesso")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReenviarPrimeiroAcesso([FromBody] ReenviarPrimeiroAcessoRequest request)
        {
            var sucesso = await _authService.ReenviarPrimeiroAcessoAsync(request.Cpf);

            if (!sucesso)
            {
                return BadRequest(new
                {
                    erro = "Nao foi possivel reenviar o link de primeiro acesso para o CPF informado."
                });
            }

            return Ok(new
            {
                mensagem = "Link de primeiro acesso reenviado com sucesso."
            });
        }

        /// <summary>
        /// POST: api/auth/esqueci-senha
        /// Solicita envio de link de recuperacao via CPF.
        /// Retorna sempre mensagem generica para nao expor existencia de conta.
        /// </summary>
        [HttpPost("esqueci-senha")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaRequest request)
        {
            await _authService.EsqueciSenhaAsync(request.Cpf);

            return Ok(new
            {
                mensagem = "Se os dados estiverem corretos, enviaremos um link para redefinicao de senha."
            });
        }

        /// <summary>
        /// POST: api/auth/redefinir-senha
        /// Redefine a senha usando token de recuperacao.
        /// </summary>
        [HttpPost("redefinir-senha")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequest request)
        {
            var sucesso = await _authService.RedefinirSenhaAsync(request);
            if (!sucesso)
            {
                return BadRequest(new
                {
                    erro = "Token de recuperacao invalido, expirado ou ja utilizado."
                });
            }

            return Ok(new
            {
                mensagem = "Senha redefinida com sucesso."
            });
        }
    }
}
