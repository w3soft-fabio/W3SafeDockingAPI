using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Services;

namespace WebSafeDockingAPI.Controllers
{
    /// <summary>
    /// Controller de Autenticação.
    /// Contém os endpoints de login, refresh e logout (revoke).
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

        // =====================================================
        //  POST: api/auth/login
        //  Autentica o usuário com CPF e senha
        // =====================================================

        /// <summary>
        /// Realiza o login do usuário.
        /// Recebe CPF e senha, retorna Access Token + Refresh Token.
        /// Não requer autenticação prévia (endpoint público).
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
                    erro = "CPF ou senha inválidos."
                });
            }

            return Ok(resultado);
        }

        // =====================================================
        //  POST: api/auth/refresh
        //  Renova o Access Token usando o Refresh Token
        // =====================================================

        /// <summary>
        /// Renova o Access Token.
        /// Quando o Access Token expira (após 1 hora), o frontend envia o Refresh Token
        /// para obter um novo Access Token sem precisar fazer login novamente.
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
                    erro = "Refresh Token inválido ou expirado. Faça login novamente."
                });
            }

            return Ok(resultado);
        }

        // =====================================================
        //  POST: api/auth/revoke (LOGOUT)
        //  Invalida o Refresh Token do usuário
        // =====================================================

        /// <summary>
        /// Revoga (invalida) o Refresh Token do usuário.
        /// Usado para fazer logout. Após revogar, o usuário precisará
        /// fazer login novamente para obter novos tokens.
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
                    erro = "Refresh Token inválido ou já revogado."
                });
            }

            return Ok(new
            {
                mensagem = "Logout realizado com sucesso."
            });
        }
    }
}
