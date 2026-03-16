# Guia de Implementação de CRUD v2.0 - W3AssinaDiplomaAPI

Este documento serve como referência para implementação de novas rotas CRUD seguindo o padrão arquitetural do projeto.

## 📋 Visão Geral

O projeto segue uma arquitetura em camadas com o seguinte fluxo:
```
Controller → Service → Repository → Entity Framework → Database
```

**Principais características:**
- Paginação estilo Supabase em todas as listagens
- DTOs separados para CREATE/UPDATE e RESPONSE
- Validação de chaves estrangeiras no Service
- Uso de PaginationHelper para consistência
- ProblemDetails para tratamento de erros

---

## 🎯 Passo a Passo para Implementação

### **Passo 1: Análise do SQL e Criação do Modelo**

#### 1.1 Receber o SQL de Criação
Obtenha o código SQL de criação da tabela que será usada como fonte de dados.

**Exemplo:**
```sql
CREATE TABLE cursos (
    id INT(11) NOT NULL AUTO_INCREMENT,
    instituicaoId INT(11) NOT NULL,
    nomeCurso VARCHAR(255) NOT NULL,
    codigoCursoEmec INT(11) DEFAULT 0,
    modalidade VARCHAR(100) NOT NULL,
    grauConferido VARCHAR(100) NOT NULL,
    logradouro VARCHAR(255) NOT NULL,
    bairro VARCHAR(100) NOT NULL,
    municipioId INT(11) NOT NULL,
    nomeMunicipio VARCHAR(50) NOT NULL,
    municipioUf VARCHAR(2) NOT NULL,
    cep CHAR(8) NOT NULL,
    autorizacaoTipo VARCHAR(50) NOT NULL,
    autorizacaoNumero VARCHAR(50) NOT NULL,
    autorizacaoData DATE NOT NULL,
    reconhecimentoTipo VARCHAR(50) NOT NULL,
    reconhecimentoNumero VARCHAR(50) NOT NULL,
    reconhecimentoData DATE NOT NULL,
    numeroProcessoSemCodigo VARCHAR(50) DEFAULT NULL,
    tipoProcessoSemCodigo VARCHAR(50) DEFAULT NULL,
    dataCadastroSemCodigo DATE DEFAULT NULL,
    dataProtocoloSemCodigo DATE DEFAULT NULL,
    dataCriacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    dataAtualizacao DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    CONSTRAINT fk_curso_instituicao
        FOREIGN KEY (instituicaoId)
        REFERENCES instituicao_ensino(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
```

#### 1.2 Identificar Chaves Estrangeiras
⚠️ **IMPORTANTE**: Identifique todas as chaves estrangeiras no SQL. Para cada chave estrangeira identificada:
- **Você DEVE incluir no contexto do prompt a classe do modelo referenciado**
- Exemplo: Se o SQL tem `FOREIGN KEY (instituicaoId) REFERENCES instituicao_ensino(id)`, inclua a classe `InstituicaoEnsino` no contexto do prompt. Essa classe estaria localizada em `/Models/{NomeDaClasse}/{NomeDaClasse}.cs`, ou seja, nesse exemplo a classe estaria localizada em `/Models/InstituicaoEnsino/InstituicaoEnsino.cs`.

---

### **Passo 2: Criar o Modelo da Entidade**

#### 2.1 Localização
Crie o arquivo em: `/Models/{NomeDaClasse}/{NomeDaClasse}.cs`

#### 2.2 Estrutura do Modelo
```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace W3AssinaDiplomaAPI.Models
{
    [Table("nome_da_tabela")]
    public class NomeDaClasse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        // ⚠️ Para chaves estrangeiras NOT NULL:
        [Column("colunaEstrangeiraId")]
        [ForeignKey("PropriedadeNavegacao")]
        public int ColunaEstrangeiraId { get; set; }

        // Propriedade de navegação (NOT NULL = não nullable)
        public ClasseReferenciada PropriedadeNavegacao { get; set; } = null!;

        // ⚠️ Para chaves estrangeiras NULLABLE:
        [Column("colunaEstrangeiraId")]
        [ForeignKey("PropriedadeNavegacao")]
        public int? ColunaEstrangeiraId { get; set; }

        // Propriedade de navegação (NULL = nullable)
        public ClasseReferenciada? PropriedadeNavegacao { get; set; }

        // Demais propriedades seguindo o padrão:
        [Required]
        [StringLength(255)]
        [Column("nomeCampo")]
        public string NomeCampo { get; set; } = string.Empty;

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
```

### IMPORTANTE! ORIENTAÇÃO PARA O USO DE NOMES DE PROPRIEDADES E COLUNAS - [Column("nome_coluna")]
**SOMENTE USAR A ANOTATION `[Column("nome_coluna")]` quando o nome da coluna de criação no código SQL for declarada usando snake_case**, uma vez que o Entity Framework considera os nomes das propriedades em PascalCase.

#### 2.3 Regras de Mapeamento

| SQL | C# | Annotations |
|-----|-----|-------------|
| `INT NOT NULL` | `int` | `[Required]` |
| `INT DEFAULT NULL` | `int?` | - |
| `VARCHAR(X) NOT NULL` | `string` | `[Required]`, `[StringLength(X)]` |
| `VARCHAR(X) DEFAULT NULL` | `string?` | `[StringLength(X)]` |
| `DATE NOT NULL` | `DateTime` | `[Required]` |
| `DATE DEFAULT NULL` | `DateTime?` | - |
| `DATETIME DEFAULT CURRENT_TIMESTAMP` | `DateTime` | Inicializar com `DateTime.Now` |

#### 2.4 Exemplo Completo
**Arquivo:** `/Models/Alunos/Aluno.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace W3AssinaDiplomaAPI.Models
{
    [Table("aluno")]
    public class Aluno
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("alunoID")]
        public int AlunoID { get; set; }

        // Chave estrangeira (NOT NULL)
        [Column("turmaID")]
        [ForeignKey("Turma")]
        public int TurmaID { get; set; }

        // Propriedade de navegação (NOT NULL = não nullable)
        public Turma Turma { get; set; } = null!;

        [Required]
        [StringLength(255)]
        [Column("nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(1)]
        [Column("sexo")]
        public string Sexo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("nacionalidade")]
        public string Nacionalidade { get; set; } = string.Empty;

        [StringLength(9)]
        [Column("cep")]
        public string? Cep { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("logradouro")]
        public string? Logradouro { get; set; } = string.Empty;

        [Required]
        [Column("codigoMunicipio")]
        public int CodigoMunicipio { get; set; } = 0;

        [Required]
        [StringLength(150)]
        [Column("nomeMunicipio")]
        public string NomeMunicipio { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        [Column("uf")]
        public string Uf { get; set; } = string.Empty;

        [Required]
        [StringLength(14)]
        [Column("cpf")]
        public string Cpf { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("rgNumero")]
        public string RgNumero { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("rgOrgaoExpedidor")]
        public string RgOrgaoExpedidor { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        [Column("rgUf")]
        public string RgUf { get; set; } = string.Empty;

        [Required]
        [Column("dataNascimento")]
        public DateTime DataNascimento { get; set; }

        [StringLength(255)]
        [Column("filiacaoMaeNome")]
        public string? FiliacaoMaeNome { get; set; }

        [StringLength(255)]
        [Column("filiacaoPaiNome")]
        public string? FiliacaoPaiNome { get; set; }

        [Required]
        [Column("dataMatricula")]
        public DateTime DataMatricula { get; set; }

        [Column("dataConclusaoCurso")]
        public DateTime DataConclusaoCurso { get; set; }

        [Column("dataColacaoGrau")]
        public DateTime DataColacaoGrau { get; set; }

        [Column("dataExpedicaoDiploma")]
        public DateTime? DataExpedicaoDiploma { get; set; }

        [Required]
        [StringLength(50)]
        [Column("situacaoVinculo")]
        public string SituacaoVinculo { get; set; } = string.Empty;

        [Column("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
```

#### 2.5 Registrar no DbContext
Após criar o modelo, adicione-o em `/DataDbContext/DataDbContext.cs`:

```csharp
public class DataDbContext : DbContext
{
    // ... código existente ...

    public DbSet<NomeDaClasse> NomeDasClasses { get; set; }
}
```

---

### **Passo 3: Criar os DTOs (Data Transfer Objects)**

#### 3.1 Localização
Crie o arquivo em: `/Models/{NomeDaClasse}/{NomeDaClasse}Dto.cs`

#### 3.2 Estrutura dos DTOs
Crie **DOIS** DTOs:

1. **CreateUpdateDTO**: Para receber dados em POST e PUT
2. **ResponseDTO**: Para retornar dados em GET

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace W3AssinaDiplomaAPI.Models
{
    // DTO para CREATE e UPDATE
    public class {NomeDaClasse}CreateUpdateDTO
    {
        // Incluir TODAS as propriedades exceto:
        // - Id (gerado automaticamente)
        // - CreatedAt (gerado automaticamente)
        // - UpdatedAt (gerado automaticamente)
        // - Propriedades de navegação (objetos relacionados)

        [Required(ErrorMessage = "O campo X é obrigatório.")]
        [StringLength(255, ErrorMessage = "O campo X deve ter no máximo 255 caracteres.")]
        public string NomeCampo { get; set; } = string.Empty;

        // Método para converter DTO em Entidade
        public {NomeDaClasse} ToEntity()
        {
            return new {NomeDaClasse}
            {
                // Mapear todas as propriedades
                NomeCampo = this.NomeCampo,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
    }

    // DTO para RESPONSE
    public class {NomeDaClasse}ResponseDTO
    {
        // Incluir TODAS as propriedades do modelo
        // EXCETO propriedades de navegação
        public int Id { get; set; }
        public string NomeCampo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Método estático de conversão
        public static {NomeDaClasse}ResponseDTO From{NomeDaClasse}({NomeDaClasse} entidade)
        {
            return new {NomeDaClasse}ResponseDTO
            {
                // Mapear todas as propriedades
                Id = entidade.Id,
                NomeCampo = entidade.NomeCampo,
                CreatedAt = entidade.CreatedAt,
                UpdatedAt = entidade.UpdatedAt
            };
        }
    }
}
```

#### 3.3 Exemplo Completo
**Arquivo:** `/Models/Alunos/AlunoDto.cs`

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace W3AssinaDiplomaAPI.Models
{
    // DTO para CREATE e UPDATE
    public class AlunoCreateUpdateDTO
    {
        [Required(ErrorMessage = "O ID da turma é obrigatório.")]
        public int TurmaID { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(255, ErrorMessage = "O nome deve ter no máximo 255 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O sexo é obrigatório.")]
        [StringLength(1, ErrorMessage = "O sexo deve ter 1 caractere.")]
        [RegularExpression("^[MF]$", ErrorMessage = "O sexo deve ser 'M' ou 'F'.")]
        public string Sexo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nacionalidade é obrigatória.")]
        [StringLength(100, ErrorMessage = "A nacionalidade deve ter no máximo 100 caracteres.")]
        public string Nacionalidade { get; set; } = string.Empty;

        public string? Cep { get; set; }
        public string? Logradouro { get; set; }

        [Required(ErrorMessage = "O código do município é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O código do município deve ser um número positivo.")]
        public int CodigoMunicipio { get; set; } = 0;

        [Required(ErrorMessage = "O nome do município é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome do município deve ter no máximo 150 caracteres.")]
        public string NomeMunicipio { get; set; } = string.Empty;

        [Required(ErrorMessage = "A UF é obrigatória.")]
        [StringLength(2, ErrorMessage = "A UF deve ter 2 caracteres.")]
        public string Uf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [StringLength(14, ErrorMessage = "O CPF deve ter no máximo 14 caracteres.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O número do RG é obrigatório.")]
        [StringLength(20, ErrorMessage = "O número do RG deve ter no máximo 20 caracteres.")]
        public string RgNumero { get; set; } = string.Empty;

        [Required(ErrorMessage = "O órgão expedidor do RG é obrigatório.")]
        [StringLength(20, ErrorMessage = "O órgão expedidor deve ter no máximo 20 caracteres.")]
        public string RgOrgaoExpedidor { get; set; } = string.Empty;

        [Required(ErrorMessage = "A UF do RG é obrigatória.")]
        [StringLength(2, ErrorMessage = "A UF do RG deve ter 2 caracteres.")]
        public string RgUf { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        public DateTime DataNascimento { get; set; }

        [StringLength(255, ErrorMessage = "O nome da mãe deve ter no máximo 255 caracteres.")]
        public string? FiliacaoMaeNome { get; set; }

        [StringLength(255, ErrorMessage = "O nome do pai deve ter no máximo 255 caracteres.")]
        public string? FiliacaoPaiNome { get; set; }

        [Required(ErrorMessage = "A data de matrícula é obrigatória.")]
        public DateTime DataMatricula { get; set; }

        public DateTime DataConclusaoCurso { get; set; }
        public DateTime DataColacaoGrau { get; set; }
        public DateTime? DataExpedicaoDiploma { get; set; }

        [Required(ErrorMessage = "A situação do vínculo é obrigatória.")]
        [StringLength(50, ErrorMessage = "A situação do vínculo deve ter no máximo 50 caracteres.")]
        public string SituacaoVinculo { get; set; } = string.Empty;

        /// <summary>
        /// Converte o DTO para a entidade Aluno.
        /// </summary>
        public Aluno ToEntity()
        {
            return new Aluno
            {
                TurmaID = this.TurmaID,
                Nome = this.Nome,
                Sexo = this.Sexo,
                Nacionalidade = this.Nacionalidade,
                Cep = this.Cep ?? string.Empty,
                Logradouro = this.Logradouro ?? string.Empty,
                CodigoMunicipio = this.CodigoMunicipio,
                NomeMunicipio = this.NomeMunicipio,
                Uf = this.Uf,
                Cpf = this.Cpf,
                RgNumero = this.RgNumero,
                RgOrgaoExpedidor = this.RgOrgaoExpedidor,
                RgUf = this.RgUf,
                DataNascimento = this.DataNascimento,
                FiliacaoMaeNome = this.FiliacaoMaeNome,
                FiliacaoPaiNome = this.FiliacaoPaiNome,
                DataMatricula = this.DataMatricula,
                DataConclusaoCurso = this.DataConclusaoCurso,
                DataColacaoGrau = this.DataColacaoGrau,
                DataExpedicaoDiploma = this.DataExpedicaoDiploma,
                SituacaoVinculo = this.SituacaoVinculo,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }
    }

    // DTO para RESPONSE
    public class AlunoResponseDTO
    {
        public int AlunoID { get; set; }
        public int TurmaID { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string Nacionalidade { get; set; } = string.Empty;
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public int CodigoMunicipio { get; set; }
        public string NomeMunicipio { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string RgNumero { get; set; } = string.Empty;
        public string RgOrgaoExpedidor { get; set; } = string.Empty;
        public string RgUf { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string? FiliacaoMaeNome { get; set; }
        public string? FiliacaoPaiNome { get; set; }
        public DateTime DataMatricula { get; set; }
        public DateTime DataConclusaoCurso { get; set; }
        public DateTime DataColacaoGrau { get; set; }
        public DateTime? DataExpedicaoDiploma { get; set; }
        public string SituacaoVinculo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Converte a entidade Aluno para AlunoResponseDTO.
        /// </summary>
        public static AlunoResponseDTO FromAluno(Aluno aluno)
        {
            return new AlunoResponseDTO
            {
                AlunoID = aluno.AlunoID,
                TurmaID = aluno.TurmaID,
                Nome = aluno.Nome,
                Sexo = aluno.Sexo,
                Nacionalidade = aluno.Nacionalidade,
                Cep = aluno.Cep,
                Logradouro = aluno.Logradouro,
                CodigoMunicipio = aluno.CodigoMunicipio,
                NomeMunicipio = aluno.NomeMunicipio,
                Uf = aluno.Uf,
                Cpf = aluno.Cpf,
                RgNumero = aluno.RgNumero,
                RgOrgaoExpedidor = aluno.RgOrgaoExpedidor,
                RgUf = aluno.RgUf,
                DataNascimento = aluno.DataNascimento,
                FiliacaoMaeNome = aluno.FiliacaoMaeNome,
                FiliacaoPaiNome = aluno.FiliacaoPaiNome,
                DataMatricula = aluno.DataMatricula,
                DataConclusaoCurso = aluno.DataConclusaoCurso,
                DataColacaoGrau = aluno.DataColacaoGrau,
                DataExpedicaoDiploma = aluno.DataExpedicaoDiploma,
                SituacaoVinculo = aluno.SituacaoVinculo,
                CreatedAt = aluno.CreatedAt,
                UpdatedAt = aluno.UpdatedAt
            };
        }
    }
}
```

---

### **Passo 3.5: Entender Paginação com PaginationRequest**

#### 3.5.1 O que é PaginationRequest?
`PaginationRequest` é um DTO comum usado para todas as rotas que implementam paginação estilo Supabase.

**Localização**: `/Models/Common/PaginationDto.cs`

#### 3.5.2 Estrutura do PaginationRequest
```csharp
public class PaginationRequest
{
    // Número da página (começa em 1, padrão: 1)
    public int Page { get; set; } = 1;

    // Quantidade de itens por página (máximo 100, padrão: 10)
    public int PageSize { get; set; } = 10;

    // Termo de busca opcional
    public string? Search { get; set; }

    // Campo para ordenação (padrão: "Nome")
    public string SortBy { get; set; } = "Nome";

    // Ordem: "asc" ou "desc" (padrão: "asc")
    public string SortOrder { get; set; } = "asc";
}
```

#### 3.5.3 Estrutura do PaginatedResponse
```csharp
public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
```

#### 3.5.4 PaginationHelper - Criação de Respostas Paginadas
O projeto usa um helper para criar respostas paginadas de forma consistente.

**Localização**: `/Utils/PaginationHelper.cs`

```csharp
public static class PaginationHelper
{
    public static PaginatedResponse<T> CreateResponse<T>(
        List<T> items,
        int totalCount,
        PaginationRequest request)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PaginatedResponse<T>
        {
            Data = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalRecords = totalCount,
            TotalPages = totalPages,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };
    }
}
```

#### 3.5.5 Como Funciona a Paginação?
1. **Cliente faz requisição** com parâmetros query string:
   ```
   GET /api/alunos/getAlunos/1?search=João&page=1&pageSize=10&sortBy=nome&sortOrder=asc
   ```

2. **Controller recebe** `PaginationRequest` via `[FromQuery]`

3. **Service processa** e chama método `SearchAsync` do Repository

4. **Repository executa**:
   - Filtra por termo de busca (se fornecido)
   - Conta total de registros
   - Aplica ordenação
   - Aplica paginação (Skip/Take)
   - Retorna tupla: `(List<ResponseDTO>, int TotalCount)`

5. **Service usa PaginationHelper** para criar resposta:
   ```csharp
   return PaginationHelper.CreateResponse(items, totalCount, request);
   ```

#### 3.5.6 Benefícios da Paginação
- ✅ Reduz uso de memória
- ✅ Melhora performance
- ✅ Evita carregar dados desnecessários
- ✅ Permite busca e ordenação flexíveis
- ✅ Padrão consistente em toda API
- ✅ Helper centraliza lógica de cálculo de metadados

---

### **Passo 4: Criar a Interface do Repository**

#### 4.1 Localização
Crie o arquivo em: `/Repositories/{NomeDaClasse}/I{NomeDaClasse}Repository.cs`

#### 4.2 Estrutura da Interface
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using W3AssinaDiplomaAPI.Models;
using W3AssinaDiplomaAPI.Models.Common;

namespace W3AssinaDiplomaAPI.Repositories
{
    public interface I{NomeDaClasse}Repository
    {
        Task<{NomeDaClasse}?> GetByIdAsync(int id);
        Task<{NomeDaClasse}> CreateAsync({NomeDaClasse} entidade);
        Task<bool> UpdateAsync({NomeDaClasse} entidade);
        Task<bool> DeleteAsync(int id);

        // Método para paginação e busca
        Task<(List<{NomeDaClasse}ResponseDTO> Items, int TotalCount)> SearchAsync(
            int usuarioID,
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
```

#### 4.3 Notas Importantes
- ⚠️ `GetByIdAsync` retorna a entidade `{NomeDaClasse}?` completa
- ⚠️ `UpdateAsync` recebe a entidade completa já modificada
- ⚠️ `SearchAsync` retorna uma tupla com lista de DTOs e total de registros
- ⚠️ `SearchAsync` recebe `usuarioID` para filtrar registros por usuário
- ⚠️ `searchTerm` é opcional (nullable) - quando null/vazio, retorna todos os registros

#### 4.4 Exemplo Completo
**Arquivo:** `/Repositories/Alunos/IAlunoRepository.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using W3AssinaDiplomaAPI.Models;
using W3AssinaDiplomaAPI.Models.Common;

namespace W3AssinaDiplomaAPI.Repositories
{
    public interface IAlunoRepository
    {
        Task<Aluno?> GetByIdAsync(int alunoID);
        Task<Aluno> CreateAsync(Aluno aluno);
        Task<bool> UpdateAsync(Aluno aluno);
        Task<bool> DeleteAsync(int alunoID);
        Task<(List<AlunoResponseDTO> Items, int TotalCount)> SearchAsync(
            int usuarioID,
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder);
    }
}
```

---

### **Passo 5: Implementar o Repository**

#### 5.1 Localização
Crie o arquivo em: `/Repositories/{NomeDaClasse}/{NomeDaClasse}Repository.cs`

#### 5.2 Estrutura da Implementação
```csharp
using Microsoft.EntityFrameworkCore;
using W3AssinaDiplomaAPI;
using W3AssinaDiplomaAPI.Data;
using W3AssinaDiplomaAPI.Models;

namespace W3AssinaDiplomaAPI.Repositories
{
    public class {NomeDaClasse}Repository : I{NomeDaClasse}Repository
    {
        private readonly DataDbConnection _dataDbConnection;

        public {NomeDaClasse}Repository(DataDbConnection dataDbConnection)
        {
            _dataDbConnection = dataDbConnection;
        }

        private async Task<T> ComContextoAsync<T>(Func<DataDbContext, Task<T>> operacao)
        {
            try
            {
                using var contexto = _dataDbConnection.CriarContexto();
                return await operacao(contexto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao acessar {typeof({NomeDaClasse}).Name}: {ex.Message} - {ex.StackTrace}");
                throw;
            }
        }

        public async Task<{NomeDaClasse}?> GetByIdAsync(int id)
        {
            return await ComContextoAsync(
                _contexto => _contexto.{NomeDasClasses}
                    .FirstOrDefaultAsync(e => e.Id == id));
        }

        public async Task<{NomeDaClasse}> CreateAsync({NomeDaClasse} entidade)
        {
            return await ComContextoAsync(async context =>
            {
                context.{NomeDasClasses}.Add(entidade);
                await context.SaveChangesAsync();
                return entidade;
            });
        }

        public async Task<bool> UpdateAsync({NomeDaClasse} entidade)
        {
            return await ComContextoAsync(async context =>
            {
                context.{NomeDasClasses}.Update(entidade);
                await context.SaveChangesAsync();
                return true;
            });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await ComContextoAsync(async context =>
            {
                var entidade = await context.{NomeDasClasses}.FindAsync(id);
                if (entidade == null) return false;

                context.{NomeDasClasses}.Remove(entidade);
                await context.SaveChangesAsync();
                return true;
            });
        }

        /// <summary>
        /// Busca entidades por termo de busca com paginação e ordenação.
        /// </summary>
        public async Task<(List<{NomeDaClasse}ResponseDTO> Items, int TotalCount)> SearchAsync(
            int usuarioID,
            int page,
            int pageSize,
            string? searchTerm,
            string sortBy,
            string sortOrder)
        {
            return await ComContextoAsync(async _contexto =>
            {
                var query = _contexto.{NomeDasClasses}.AsQueryable();

                // Aplicar filtro de busca (se fornecido)
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(e =>
                        e.NomeCampo.Contains(searchTerm)
                        // Adicione mais campos conforme necessário
                    );
                }

                // Obter total de registros antes da paginação
                var totalCount = await query.CountAsync();

                // Aplicar ordenação dinâmica
                query = ApplySorting(query, sortBy, sortOrder);

                // Aplicar paginação e converter para DTO
                var entidades = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var items = entidades.Select({NomeDaClasse}ResponseDTO.From{NomeDaClasse}).ToList();

                return (items, totalCount);
            });
        }

        /// <summary>
        /// Aplica ordenação dinâmica à query.
        /// </summary>
        private IQueryable<{NomeDaClasse}> ApplySorting(
            IQueryable<{NomeDaClasse}> query,
            string sortBy,
            string sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";

            return sortBy?.ToLower() switch
            {
                "nome" or "nomecampo" => isDescending
                    ? query.OrderByDescending(e => e.NomeCampo)
                    : query.OrderBy(e => e.NomeCampo),
                // Adicione mais casos conforme necessário
                _ => query.OrderBy(e => e.NomeCampo) // Ordenação padrão
            };
        }
    }
}
```

#### 5.3 Notas Importantes sobre SearchAsync
- ⚠️ **Filtro de busca**: Adapte o `Where` para os campos específicos da sua entidade
- ⚠️ **Ordenação**: Implemente `ApplySorting` com os campos que deseja permitir ordenação
- ⚠️ **Conversão para DTO**: Use `.Select()` e o método estático `From{NomeDaClasse}` para converter
- ⚠️ **Performance**: `AsQueryable()` garante que tudo seja executado no banco de dados

#### 5.4 Exemplo Completo de SearchAsync
**Arquivo:** `/Repositories/Alunos/AlunoRepository.cs` (linhas 147-191)

```csharp
public async Task<(List<AlunoResponseDTO> Items, int TotalCount)> SearchAsync(
    int usuarioID,
    int page,
    int pageSize,
    string? searchTerm,
    string sortBy,
    string sortOrder)
{
    return await ComContextoAsync(async _contexto =>
    {
        var query = _contexto.Alunos.AsQueryable();

        // Aplicar filtro de busca
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Remover caracteres não numéricos para busca por CPF/RG
            var searchTermDigitsOnly = Regex.Replace(searchTerm, @"[^\d]", "");

            query = query.Where(a =>
                // Busca por nome (case-insensitive)
                a.Nome.Contains(searchTerm) ||
                // Busca por CPF (apenas dígitos)
                (!string.IsNullOrEmpty(searchTermDigitsOnly) &&
                 a.Cpf.Replace(".", "").Replace("-", "").Contains(searchTermDigitsOnly)) ||
                // Busca por RG (apenas dígitos)
                (!string.IsNullOrEmpty(searchTermDigitsOnly) &&
                 a.RgNumero.Replace(".", "").Replace("-", "").Contains(searchTermDigitsOnly))
            );
        }

        // Obter total de registros antes da paginação
        var totalCount = await query.CountAsync();

        // Aplicar ordenação dinâmica
        query = ApplySorting(query, sortBy, sortOrder);

        // Aplicar paginação e converter para DTO
        var alunos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = alunos.Select(AlunoResponseDTO.FromAluno).ToList();

        return (items, totalCount);
    });
}
```

#### 5.5 Exemplo Completo de ApplySorting
**Arquivo:** `/Repositories/Alunos/AlunoRepository.cs` (linhas 231-245)

```csharp
private IQueryable<Aluno> ApplySorting(IQueryable<Aluno> query, string sortBy, string sortOrder)
{
    var isDescending = sortOrder?.ToLower() == "desc";

    return sortBy?.ToLower() switch
    {
        "nome" => isDescending ? query.OrderByDescending(a => a.Nome) : query.OrderBy(a => a.Nome),
        "cpf" => isDescending ? query.OrderByDescending(a => a.Cpf) : query.OrderBy(a => a.Cpf),
        "rgnumero" or "rg" => isDescending ? query.OrderByDescending(a => a.RgNumero) : query.OrderBy(a => a.RgNumero),
        "datamatricula" => isDescending ? query.OrderByDescending(a => a.DataMatricula) : query.OrderBy(a => a.DataMatricula),
        "datanascimento" => isDescending ? query.OrderByDescending(a => a.DataNascimento) : query.OrderBy(a => a.DataNascimento),
        "situacaovinculo" => isDescending ? query.OrderByDescending(a => a.SituacaoVinculo) : query.OrderBy(a => a.SituacaoVinculo),
        _ => query.OrderBy(a => a.Nome) // Ordenação padrão por Nome
    };
}
```

---

### **Passo 6: Criar o Service**

#### 6.1 Localização
Crie o arquivo em: `/Services/{NomeDaClasse}/{NomeDaClasse}Service.cs`

#### 6.2 ⚠️ IMPORTANTE: Injeção de Dependências para Chaves Estrangeiras

**Se o modelo possui chaves estrangeiras**, você DEVE:

1. **Incluir no contexto do prompt as interfaces dos repositories das entidades referenciadas**
   - Exemplo: Para `Aluno` que referencia `Turma`, inclua `ITurmaRepository`
   - Localização: `/Repositories/{NomeDaClasseReferenciada}/I{NomeDaClasseReferenciada}Repository.cs`

2. **Injetar essas interfaces no construtor do Service**
   - Uma interface para cada chave estrangeira

3. **Validar a existência das entidades referenciadas** nos métodos `Criar{NomeDaClasse}Async` e `Atualizar{NomeDaClasse}Async`

#### 6.3 Estrutura do Service

```csharp
using W3AssinaDiplomaAPI.Exceptions;
using W3AssinaDiplomaAPI.Models;
using W3AssinaDiplomaAPI.Models.Common;
using W3AssinaDiplomaAPI.Repositories;
using W3AssinaDiplomaAPI.Utils;

namespace W3AssinaDiplomaAPI.Services
{
    public class {NomeDaClasse}Service
    {
        private readonly I{NomeDaClasse}Repository _repository;
        // ⚠️ Adicione um repository para CADA chave estrangeira:
        private readonly I{ClasseReferenciada}Repository _{classeReferenciada}Repository;

        public {NomeDaClasse}Service(
            I{NomeDaClasse}Repository repository,
            I{ClasseReferenciada}Repository {classeReferenciada}Repository)
        {
            _repository = repository;
            _{classeReferenciada}Repository = {classeReferenciada}Repository;
        }

        public async Task<{NomeDaClasse}ResponseDTO?> GetByIdAsync(int id)
        {
            var entidade = await _repository.GetByIdAsync(id);
            return entidade != null ? {NomeDaClasse}ResponseDTO.From{NomeDaClasse}(entidade) : null;
        }

        public async Task<{NomeDaClasse}ResponseDTO> Criar{NomeDaClasse}Async(
            int usuarioID,
            {NomeDaClasse}CreateUpdateDTO dto)
        {
            // ⚠️ Validar TODAS as chaves estrangeiras:
            var entidadeReferenciada = await _{classeReferenciada}Repository
                .GetByIdAsync(dto.ChaveEstrangeiraId);

            if (entidadeReferenciada == null)
            {
                throw new ValidationException(
                    $"Entidade referenciada com ID {dto.ChaveEstrangeiraId} não encontrada.");
            }

            // Converter DTO para entidade
            var novaEntidade = dto.ToEntity();

            // Salvar no banco
            var entidadeCriada = await _repository.CreateAsync(novaEntidade);

            return {NomeDaClasse}ResponseDTO.From{NomeDaClasse}(entidadeCriada);
        }

        public async Task<{NomeDaClasse}ResponseDTO?> Atualizar{NomeDaClasse}Async(
            int id,
            {NomeDaClasse}CreateUpdateDTO dto)
        {
            // Buscar entidade existente
            var entidadeExistente = await _repository.GetByIdAsync(id);

            if (entidadeExistente == null) return null;

            // ⚠️ Validar chave estrangeira:
            var entidadeReferenciada = await _{classeReferenciada}Repository
                .GetByIdAsync(dto.ChaveEstrangeiraId);

            if (entidadeReferenciada == null)
            {
                throw new ValidationException(
                    $"Entidade referenciada com ID {dto.ChaveEstrangeiraId} não encontrada.");
            }

            // Atualizar TODAS as propriedades
            entidadeExistente.ChaveEstrangeiraId = dto.ChaveEstrangeiraId;
            entidadeExistente.NomeCampo = dto.NomeCampo;
            // ... demais propriedades
            entidadeExistente.UpdatedAt = DateTime.Now;

            // Atualizar no banco
            var atualizado = await _repository.UpdateAsync(entidadeExistente);

            if (!atualizado) return null;

            // Retornar DTO da entidade atualizada
            return {NomeDaClasse}ResponseDTO.From{NomeDaClasse}(entidadeExistente);
        }

        public async Task<bool> Deletar{NomeDaClasse}Async(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca entidades por termo de busca com paginação estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<{NomeDaClasse}ResponseDTO>> Search{NomeDasClasses}Async(
            int usuarioID,
            PaginationRequest request)
        {
            var (items, totalCount) = await _repository.SearchAsync(
                usuarioID,
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
```

#### 6.4 Notas Importantes sobre Search{NomeDasClasses}Async
- ⚠️ Recebe `PaginationRequest` como parâmetro
- ⚠️ Chama `SearchAsync` do Repository
- ⚠️ Usa `PaginationHelper.CreateResponse` para criar resposta paginada
- ⚠️ Retorna `PaginatedResponse<ResponseDTO>` completo

#### 6.5 Exemplo Completo
**Arquivo:** `/Services/Alunos/AlunoService.cs`

```csharp
using W3AssinaDiplomaAPI.Exceptions;
using W3AssinaDiplomaAPI.Models;
using W3AssinaDiplomaAPI.Models.Common;
using W3AssinaDiplomaAPI.Repositories;
using W3AssinaDiplomaAPI.Utils;

namespace W3AssinaDiplomaAPI.Services
{
    public class AlunoService
    {
        private readonly IAlunoRepository _repository;
        private readonly ITurmaRepository _turmaRepository;

        public AlunoService(
            IAlunoRepository repository,
            ITurmaRepository turmaRepository)
        {
            _repository = repository;
            _turmaRepository = turmaRepository;
        }

        public async Task<AlunoResponseDTO?> GetByIdAsync(int alunoID)
        {
            var aluno = await _repository.GetByIdAsync(alunoID);
            return aluno != null ? AlunoResponseDTO.FromAluno(aluno) : null;
        }

        public async Task<AlunoResponseDTO> CriarAlunoAsync(
            int usuarioID,
            AlunoCreateUpdateDTO dto)
        {
            // Validar se a turma existe
            var turma = await _turmaRepository.GetByIdAsync(dto.TurmaID);

            if (turma == null)
            {
                throw new ValidationException(
                    $"Turma com ID {dto.TurmaID} não encontrada."
                );
            }

            // Converter DTO para entidade
            var novoAluno = dto.ToEntity();
            novoAluno.UsuarioID = usuarioID;

            // Salvar no banco
            var alunoCriado = await _repository.CreateAsync(novoAluno);

            return AlunoResponseDTO.FromAluno(alunoCriado);
        }

        public async Task<AlunoResponseDTO?> AtualizarAlunoAsync(
            int alunoID,
            AlunoCreateUpdateDTO dto)
        {
            // Buscar aluno existente
            var alunoExistente = await _repository.GetByIdAsync(alunoID);

            if (alunoExistente == null) return null;

            // Validar se a turma existe
            var turma = await _turmaRepository.GetByIdAsync(dto.TurmaID);

            if (turma == null)
            {
                throw new ValidationException(
                    $"Turma com ID {dto.TurmaID} não encontrada."
                );
            }

            // Atualizar propriedades básicas
            alunoExistente.TurmaID = dto.TurmaID;
            alunoExistente.Nome = dto.Nome;
            alunoExistente.Sexo = dto.Sexo;
            alunoExistente.Nacionalidade = dto.Nacionalidade;
            alunoExistente.Cep = dto.Cep;
            alunoExistente.Logradouro = dto.Logradouro;
            alunoExistente.CodigoMunicipio = dto.CodigoMunicipio;
            alunoExistente.NomeMunicipio = dto.NomeMunicipio;
            alunoExistente.Uf = dto.Uf;
            alunoExistente.Cpf = dto.Cpf;
            alunoExistente.RgNumero = dto.RgNumero;
            alunoExistente.RgOrgaoExpedidor = dto.RgOrgaoExpedidor;
            alunoExistente.RgUf = dto.RgUf;
            alunoExistente.DataNascimento = dto.DataNascimento;
            alunoExistente.FiliacaoMaeNome = dto.FiliacaoMaeNome;
            alunoExistente.FiliacaoPaiNome = dto.FiliacaoPaiNome;
            alunoExistente.DataMatricula = dto.DataMatricula;
            alunoExistente.DataConclusaoCurso = dto.DataConclusaoCurso;
            alunoExistente.DataColacaoGrau = dto.DataColacaoGrau;
            alunoExistente.DataExpedicaoDiploma = dto.DataExpedicaoDiploma;
            alunoExistente.SituacaoVinculo = dto.SituacaoVinculo;
            alunoExistente.UpdatedAt = DateTime.Now;

            // Atualizar no banco
            var atualizado = await _repository.UpdateAsync(alunoExistente);

            if (!atualizado) return null;

            // Retornar DTO do aluno atualizado
            return AlunoResponseDTO.FromAluno(alunoExistente);
        }

        public async Task<bool> DeletarAlunoAsync(int id) =>
            await _repository.DeleteAsync(id);

        /// <summary>
        /// Busca alunos por nome, CPF ou RG com paginação estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<AlunoResponseDTO>> SearchAlunosAsync(
            int usuarioID,
            PaginationRequest request)
        {
            var (items, totalCount) = await _repository.SearchAsync(
                usuarioID,
                request.Page,
                request.PageSize,
                request.Search,
                request.SortBy,
                request.SortOrder
            );

            return PaginationHelper.CreateResponse(items, totalCount, request);
        }

        /// <summary>
        /// Busca alunos por turmaId com paginação estilo Supabase.
        /// </summary>
        public async Task<PaginatedResponse<AlunoResponseDTO>> GetAlunosByTurmaAsync(
            int turmaId,
            PaginationRequest request)
        {
            var (items, totalCount) = await _repository.GetByTurmaIdAsync(
                turmaId,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortOrder
            );

            return PaginationHelper.CreateResponse(items, totalCount, request);
        }
    }
}
```

---

### **Passo 7: Criar o Controller**

#### 7.1 Localização
Crie o arquivo em: `/Controllers/{NomeDaClasse}/{NomeDaClasse}Controller.cs`

#### 7.2 Estrutura do Controller

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using W3AssinaDiplomaAPI.Models;
using W3AssinaDiplomaAPI.Models.Common;
using W3AssinaDiplomaAPI.Services;

namespace W3AssinaDiplomaAPI.Controllers
{
    [ApiController]
    [Route("api/{nomeRotaMinuscula}")]
    public class {NomeDaClasse}Controller : ControllerBase
    {
        private readonly {NomeDaClasse}Service _service;

        public {NomeDaClasse}Controller({NomeDaClasse}Service service)
        {
            _service = service;
        }

        /// <summary>
        /// 📋 GET: api/{nomeRota}/get{NomeRota}/{usuarioID}
        /// </summary>
        [Authorize]
        [HttpGet("get{NomeRota}/{usuarioID}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaginatedResponse<{NomeDaClasse}ResponseDTO>>> Get{NomeRota}(
            int usuarioID,
            [FromQuery] PaginationRequest request)
        {
            if (usuarioID <= 0)
            {
                return BadRequest(ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Parâmetros inválidos.",
                    detail: "O ID do usuário deve ser maior que zero."));
            }

            var result = await _service.Search{NomeDasClasses}Async(usuarioID, request);
            return Ok(result);
        }

        /// <summary>
        /// 🔍 GET: api/{nomeRota}/get{NomeRotaSingular}/{id}
        /// </summary>
        [Authorize]
        [HttpGet("get{NomeRotaSingular}/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<{NomeDaClasse}ResponseDTO>> Get{NomeRotaSingular}(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Parâmetros inválidos.",
                    detail: "O ID deve ser maior que zero."));
            }

            var entidade = await _service.GetByIdAsync(id);
            if (entidade == null)
            {
                return NotFound(ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Entidade não encontrada.",
                    detail: $"Nenhuma entidade com ID {id} foi encontrada."));
            }

            return Ok(entidade);
        }

        /// <summary>
        /// ✅ POST: api/{nomeRota}/set{NomeRotaSingular}/{usuarioID}
        /// </summary>
        [Authorize]
        [HttpPost("set{NomeRotaSingular}/{usuarioID}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<{NomeDaClasse}ResponseDTO>> Set{NomeRotaSingular}(
            int usuarioID,
            [FromBody] {NomeDaClasse}CreateUpdateDTO dto)
        {
            if (usuarioID <= 0 || dto == null)
            {
                return BadRequest(ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Dados inválidos.",
                    detail: "O ID do usuário deve ser maior que zero e os dados não podem ser nulos."));
            }

            var novaEntidade = await _service.Criar{NomeDaClasse}Async(usuarioID, dto);

            return CreatedAtAction(
                nameof(Get{NomeRotaSingular}),
                new { id = novaEntidade.Id },
                novaEntidade
            );
        }

        /// <summary>
        /// ✏️ PUT: api/{nomeRota}/atualizar{NomeRotaSingular}/{id}
        /// </summary>
        [Authorize]
        [HttpPut("atualizar{NomeRotaSingular}/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<{NomeDaClasse}ResponseDTO>> Atualizar{NomeRotaSingular}(
            int id,
            [FromBody] {NomeDaClasse}CreateUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest(ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Dados inválidos.",
                    detail: "O ID deve ser maior que zero e os dados não podem ser nulos."));
            }

            var entidadeAtualizada = await _service.Atualizar{NomeDaClasse}Async(id, dto);
            if (entidadeAtualizada == null)
            {
                return NotFound(ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Entidade não encontrada.",
                    detail: $"Nenhuma entidade com ID {id} foi encontrada."));
            }
            return Ok(entidadeAtualizada);
        }

        /// <summary>
        /// 🗑️ DELETE: api/{nomeRota}/deletar{NomeRotaSingular}/{id}
        /// </summary>
        [Authorize]
        [HttpDelete("deletar{NomeRotaSingular}/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deletar{NomeRotaSingular}(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Parâmetros inválidos.",
                    detail: "O ID deve ser maior que zero."));
            }

            var sucesso = await _service.Deletar{NomeDaClasse}Async(id);

            if (!sucesso)
            {
                return NotFound(ProblemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Entidade não encontrada.",
                    detail: $"Nenhuma entidade com ID {id} foi encontrada."));
            }

            return NoContent();
        }
    }
}
```

#### 7.3 Notas Importantes sobre o Controller
- ⚠️ **Rota Get{NomeRota}**:
  - Retorna `PaginatedResponse<{NomeDaClasse}ResponseDTO>` ao invés de `List`
  - Recebe `PaginationRequest` via `[FromQuery]`
  - Recebe `usuarioID` como parâmetro de rota para filtrar registros
  - Chama `Search{NomeDasClasses}Async` do Service
  - Suporta busca opcional via parâmetro `search`
  - Suporta ordenação via parâmetros `sortBy` e `sortOrder`
  - Quando `search` é vazio/null, retorna todos os registros paginados

- ⚠️ **Rotas Get{NomeRotaSingular}, Atualizar e Deletar**:
  - Não requerem `usuarioID` - usam apenas o `id` da entidade
  - As operações são feitas diretamente pelo ID da entidade

- ⚠️ **Tratamento de Erros**:
  - Usa `ProblemDetailsFactory.CreateProblemDetails` para erros
  - Retorna códigos HTTP apropriados (200, 201, 204, 400, 404)
  - Mensagens de erro descritivas

#### 7.4 Exemplos de Requisições
```http
# Listar todos (paginado) - requer usuarioID
GET /api/alunos/getAlunos/1?page=1&pageSize=10

# Buscar por termo
GET /api/alunos/getAlunos/1?search=João&page=1&pageSize=10

# Buscar e ordenar
GET /api/alunos/getAlunos/1?search=Silva&sortBy=nome&sortOrder=desc&page=1&pageSize=20

# Buscar por ID específico (sem usuarioID)
GET /api/alunos/getAluno/123

# Criar novo aluno (requer usuarioID)
POST /api/alunos/setAluno/1
{
  "turmaID": 5,
  "nome": "João Silva",
  "sexo": "M",
  ...
}

# Atualizar aluno (sem usuarioID)
PUT /api/alunos/atualizarAluno/123
{
  "turmaID": 5,
  "nome": "João Silva Santos",
  ...
}

# Deletar aluno (sem usuarioID)
DELETE /api/alunos/deletarAluno/123
```

#### 7.5 Exemplo Completo
Veja o arquivo de exemplo: `/Controllers/Alunos/AlunoController.cs`

---

### **Passo 8: Registrar Injeção de Dependências**

#### 8.1 Localização
Edite o arquivo: `/Program.cs`

#### 8.2 Adicionar Registros
Após as configurações existentes e antes de `builder.Services.AddControllers()`, adicione:

```csharp
// Repository e Service para {NomeDaClasse}
builder.Services.AddScoped<I{NomeDaClasse}Repository, {NomeDaClasse}Repository>();
builder.Services.AddScoped<{NomeDaClasse}Service>();
```

#### 8.3 Exemplo
```csharp
builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<AlunoService>();
```

---

## ✅ Checklist de Implementação

Use esta checklist para garantir que todos os passos foram seguidos:

- [ ] **Passo 1**: SQL analisado e chaves estrangeiras identificadas
- [ ] **Passo 1**: Classes dos modelos referenciados incluídas no contexto (se houver FK)
- [ ] **Passo 2**: Modelo criado em `/Models/{NomeDaClasse}/{NomeDaClasse}.cs`
- [ ] **Passo 2**: Modelo registrado no `DataDbContext.cs`
- [ ] **Passo 2**: Propriedades de navegação configuradas corretamente
- [ ] **Passo 3**: DTOs criados em `/Models/{NomeDaClasse}/{NomeDaClasse}Dto.cs`
- [ ] **Passo 3**: Método `From{NomeDaClasse}` implementado no ResponseDTO
- [ ] **Passo 3**: Método `ToEntity()` implementado no CreateUpdateDTO
- [ ] **Passo 3.5**: Paginação compreendida (`PaginationRequest` e `PaginatedResponse`)
- [ ] **Passo 4**: Interface criada em `/Repositories/{NomeDaClasse}/I{NomeDaClasse}Repository.cs`
- [ ] **Passo 4**: Método `SearchAsync` adicionado à interface do Repository
- [ ] **Passo 5**: Repository implementado em `/Repositories/{NomeDaClasse}/{NomeDaClasse}Repository.cs`
- [ ] **Passo 5**: Método `SearchAsync` implementado no Repository com filtros e ordenação
- [ ] **Passo 5**: Método `ApplySorting` implementado no Repository
- [ ] **Passo 5**: Método `GetByIdAsync` retorna a entidade completa
- [ ] **Passo 5**: Método `UpdateAsync` recebe entidade completa já modificada
- [ ] **Passo 5**: Método `SearchAsync` usa `usuarioID` para filtrar registros
- [ ] **Passo 6**: Interfaces dos repositories de FKs incluídas no contexto (se houver FK)
- [ ] **Passo 6**: Service criado em `/Services/{NomeDaClasse}/{NomeDaClasse}Service.cs`
- [ ] **Passo 6**: Validações de FK implementadas no Service (se houver FK)
- [ ] **Passo 6**: Método `Search{NomeDasClasses}Async` implementado no Service
- [ ] **Passo 6**: Service usa `PaginationHelper.CreateResponse` para criar resposta paginada
- [ ] **Passo 7**: Controller criado em `/Controllers/{NomeDaClasse}/{NomeDaClasse}Controller.cs`
- [ ] **Passo 7**: Rota `Get{NomeRota}` implementada com paginação (`PaginationRequest` e `PaginatedResponse`)
- [ ] **Passo 7**: Controller usa `ProblemDetailsFactory.CreateProblemDetails` para erros
- [ ] **Passo 8**: Injeções de dependência registradas em `Program.cs`
- [ ] **Teste**: Compilação bem-sucedida
- [ ] **Teste**: Endpoints testados via Swagger
- [ ] **Teste**: Paginação testada (com e sem parâmetro `search`)
- [ ] **Teste**: Ordenação testada com diferentes campos

---

## ✅ Convenções de Nomenclatura

| Elemento | Padrão | Exemplo |
|----------|--------|---------|
| **Tabela SQL** | snake_case | `aluno`, `aluno_turma` |
| **Classe Model** | PascalCase (singular) | `Aluno`, `AlunoTurma` |
| **Propriedade Model** | PascalCase | `Nome`, `TurmaID` |
| **DbSet** | PascalCase (plural) | `Alunos`, `AlunoTurmas` |
| **Interface** | I + PascalCase | `IAlunoRepository` |
| **Repository** | PascalCase | `AlunoRepository` |
| **Service** | PascalCase | `AlunoService` |
| **Controller** | PascalCase | `AlunoController` |
| **Route** | lowercase | `api/alunos` |
| **Endpoint** | camelCase | `getAlunos`, `setAluno` |

---

## ⚠️ Pontos de Atenção

### Chaves Estrangeiras
- ⚠️ **SEMPRE** identifique as chaves estrangeiras no SQL
- ⚠️ **SEMPRE** inclua as classes referenciadas no contexto do prompt
- ⚠️ **SEMPRE** injete os repositories das FKs no Service
- ⚠️ **SEMPRE** valide a existência das entidades referenciadas

### Nullability
- Campos `NOT NULL` → propriedades não-nullable
- Campos `DEFAULT NULL` → propriedades nullable (`?`)
- Propriedades de navegação seguem a nullability da FK

### DateTime
- `DATETIME DEFAULT CURRENT_TIMESTAMP` → inicializar com `DateTime.Now` no Model
- Atualizar `UpdatedAt` manualmente no método `Update` do Repository

### Validações
- Sempre validar `usuarioID > 0` nas rotas que requerem (listagem e criação)
- Sempre validar `id > 0`
- Sempre validar se DTO não é `null`
- Usar `NotFoundException` quando entidade não existe
- Usar `ValidationException` quando validação de negócio falha

### Repository
- `GetByIdAsync` retorna a entidade completa
- `UpdateAsync` recebe a entidade completa já modificada
- `SearchAsync` retorna tupla `(List<ResponseDTO>, int TotalCount)`
- `SearchAsync` recebe `usuarioID` para filtrar registros por usuário

### Service
- Usa `PaginationHelper.CreateResponse` para criar respostas paginadas
- Valida chaves estrangeiras antes de criar/atualizar
- Converte entidades para DTOs usando métodos estáticos

### Controller
- Usa `ProblemDetailsFactory.CreateProblemDetails` para tratamento de erros
- Recebe `PaginationRequest` via `[FromQuery]`
- Retorna `PaginatedResponse<ResponseDTO>` em listagens
- Documentação XML em cada endpoint

---

## 📁 Arquivos de Referência

Os seguintes arquivos servem como exemplos completos de implementação:

- **Model**: `/Models/Alunos/Aluno.cs`
- **DTOs**: `/Models/Alunos/AlunoDto.cs`
- **Interface Repository**: `/Repositories/Alunos/IAlunoRepository.cs`
- **Repository**: `/Repositories/Alunos/AlunoRepository.cs`
- **Service**: `/Services/Alunos/AlunoService.cs`
- **Controller**: `/Controllers/Alunos/AlunoController.cs`
- **DbContext**: `/DataDbContext/DataDbContext.cs`
- **Program**: `/Program.cs`
- **PaginationHelper**: `/Utils/PaginationHelper.cs`
- **PaginationDto**: `/Models/Common/PaginationDto.cs`

---

## ⚙️ Tecnologias Utilizadas

- **.NET 8.0**
- **Entity Framework Core** (MySQL)
- **JWT Authentication**
- **Swagger/OpenAPI**
- **Dependency Injection**

---

## 📜 Notas Finais

Este documento deve ser usado como referência ao implementar novos CRUDs no projeto. Siga rigorosamente os padrões estabelecidos para manter a consistência e qualidade do código.

**Versão**: 2.1
**Última atualização**: Janeiro 2026
**Projeto**: W3AssinaDiplomaAPI
**Baseado em**: Implementação real da classe Aluno