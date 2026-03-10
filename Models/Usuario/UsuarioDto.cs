using System.ComponentModel.DataAnnotations;

namespace WebSafeDockingAPI.Models
{
    /// <summary>
    /// DTO para CREATE e UPDATE de Usuario.
    /// </summary>
    public class UsuarioCreateUpdateDTO
    {
        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string? Nome { get; set; }

        [StringLength(14, ErrorMessage = "O CPF deve ter no máximo 14 caracteres.")]
        public string? Cpf { get; set; }

        [StringLength(50, ErrorMessage = "O nível de acesso deve ter no máximo 50 caracteres.")]
        [RegularExpression(
            "^(administrador|operador_portuario|supervisor_operacional|tecnico|visualizacao)$",
            ErrorMessage = "O nível de acesso deve ser: administrador, operador_portuario, supervisor_operacional, tecnico ou visualizacao.")]
        public string? NivelAcesso { get; set; }

        [StringLength(255, ErrorMessage = "O hash da senha deve ter no máximo 255 caracteres.")]
        public string? SenhaHash { get; set; }

        [StringLength(15, ErrorMessage = "O telefone deve ter no máximo 15 caracteres.")]
        public string? Telefone { get; set; }

        [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        public string? Email { get; set; }

        public bool? Ativo { get; set; }

        /// <summary>
        /// Converte o DTO para a entidade Usuario.
        /// </summary>
        public Usuario ToEntity()
        {
            return new Usuario
            {
                Id = this.Id,
                Nome = this.Nome,
                Cpf = this.Cpf,
                NivelAcesso = this.NivelAcesso,
                SenhaHash = this.SenhaHash,
                Telefone = this.Telefone,
                Email = this.Email,
                Ativo = this.Ativo
            };
        }
    }

    /// <summary>
    /// DTO para RESPONSE de Usuario.
    /// </summary>
    public class UsuarioResponseDTO
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Cpf { get; set; }
        public string? NivelAcesso { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? RazaoSocial { get; set; }
        public bool? Ativo { get; set; }

        /// <summary>
        /// Converte a entidade Usuario para UsuarioResponseDTO.
        /// Note: senhaHash é omitido intencionalmente por segurança.
        /// </summary>
        public static UsuarioResponseDTO FromUsuario(Usuario usuario)
        {
            return new UsuarioResponseDTO
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Cpf = usuario.Cpf,
                NivelAcesso = usuario.NivelAcesso,
                Telefone = usuario.Telefone,
                Email = usuario.Email,
                Ativo = usuario.Ativo
            };
        }
    }
}
