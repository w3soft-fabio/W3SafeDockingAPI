using WebSafeDockingAPI.Models;
using WebSafeDockingAPI.Models.Common;
using WebSafeDockingAPI.Repositories;
using WebSafeDockingAPI.Utils;

namespace WebSafeDockingAPI.Services
{
    public class EmailContaService
    {
        private readonly IEmailContaRepository _repository;

        public EmailContaService(IEmailContaRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Busca uma conta de e-mail pelo ID.
        /// </summary>
        public async Task<EmailContaResponseDTO?> GetByIdAsync(int id)
        {
            var emailConta = await _repository.GetByIdAsync(id);
            return emailConta != null ? EmailContaResponseDTO.FromEmailConta(emailConta) : null;
        }

        /// <summary>
        /// Cria uma nova conta de e-mail.
        /// </summary>
        public async Task<EmailContaResponseDTO> CriarEmailContaAsync(EmailContaCreateUpdateDTO dto)
        {
            var novoEmailConta = dto.ToEntity();
            var emailContaCriada = await _repository.CreateAsync(novoEmailConta);
            return EmailContaResponseDTO.FromEmailConta(emailContaCriada);
        }

        /// <summary>
        /// Atualiza uma conta de e-mail existente.
        /// </summary>
        public async Task<EmailContaResponseDTO?> AtualizarEmailContaAsync(int id, EmailContaCreateUpdateDTO dto)
        {
            var emailContaExistente = await _repository.GetByIdAsync(id);

            if (emailContaExistente == null) return null;

            emailContaExistente.Apelido = dto.Apelido;
            emailContaExistente.Endereco = dto.Endereco;
            emailContaExistente.CredenciaisUsuario = dto.CredenciaisUsuario;
            emailContaExistente.CredenciaisSenha = dto.CredenciaisSenha;
            emailContaExistente.SmtpHost = dto.SmtpHost;
            emailContaExistente.SmtpPort = dto.SmtpPort;
            emailContaExistente.SmtpSSL = dto.SmtpSSL;

            var atualizado = await _repository.UpdateAsync(emailContaExistente);

            if (!atualizado) return null;

            return EmailContaResponseDTO.FromEmailConta(emailContaExistente);
        }

        /// <summary>
        /// Deleta uma conta de e-mail pelo ID.
        /// </summary>
        public async Task<bool> DeletarEmailContaAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca contas de e-mail por termo com paginacao estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<EmailContaResponseDTO>> SearchEmailContasAsync(
            PaginationRequest request)
        {
            var (items, totalCount) = await _repository.SearchAsync(
                request.Page,
                request.PageSize,
                request.Search,
                request.SortBy,
                request.SortOrder
            );

            return PaginationHelper.CreateResponse(items, totalCount, request);
        }
    }
}
