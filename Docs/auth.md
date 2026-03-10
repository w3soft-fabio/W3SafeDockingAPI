# Documentação Completa do Sistema de Autenticação JWT

## Índice
1. [Visão Geral](#visão-geral)
2. [Conceitos Fundamentais](#conceitos-fundamentais)
3. [Arquitetura do Sistema](#arquitetura-do-sistema)
4. [Fluxo de Autenticação](#fluxo-de-autenticação)
5. [Componentes da API](#componentes-da-api)
6. [Como o Frontend Deve Implementar](#como-o-frontend-deve-implementar)
7. [Segurança](#segurança)
8. [Exemplos Práticos](#exemplos-práticos)
9. [Troubleshooting](#troubleshooting)

---

## Visão Geral

Este sistema implementa autenticação **JWT (JSON Web Token)** com **Refresh Tokens** para fornecer segurança robusta e experiência de usuário contínua. É um sistema profissional que separa credenciais (login/senha) dos tokens de acesso, permitindo sessões seguras e de longa duração.

### O Que Foi Implementado?

- **Registro de Usuários** - Criação de novos usuários (apenas administradores)
- **Login** - Autenticação com username e senha
- **Access Token** - Token de curta duração (1 hora) para acessar recursos protegidos
- **Refresh Token** - Token de longa duração (7 dias) para renovar access tokens
- **Revoke (Logout)** - Invalidação de tokens para logout seguro
- **Hash de Senhas** - Senhas nunca são armazenadas em texto plano (usa BCrypt)
- **Autorização por Roles** - Diferentes níveis de acesso (Administrador, Usuario)
- **Multi-banco de dados** - Suporte a múltiplos bancos de dados por cliente

---

## Conceitos Fundamentais

### O Que é JWT?

**JWT (JSON Web Token)** é um padrão aberto (RFC 7519) que define uma maneira compacta e auto-contida de transmitir informações entre partes como um objeto JSON. Essas informações podem ser verificadas e confiáveis porque são assinadas digitalmente.

**Estrutura de um JWT:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

Um JWT é dividido em 3 partes separadas por pontos (`.`):

1. **Header (Cabeçalho)** - Tipo do token e algoritmo de assinatura
2. **Payload (Dados)** - Informações do usuário (claims)
3. **Signature (Assinatura)** - Verificação de integridade

### Por Que Usar Access Token + Refresh Token?

Imagine que você tem duas chaves para sua casa:

- **Access Token (Chave do Dia-a-dia)** 🔑
  - Você carrega no bolso
  - Expira rápido (1 hora)
  - Se alguém roubar, só funciona por pouco tempo
  - Usada para todas as operações normais

- **Refresh Token (Chave Mestra)** 🗝️
  - Guardada em lugar seguro
  - Dura muito mais tempo (7 dias)
  - Usada APENAS para gerar novas "chaves do dia-a-dia"
  - Se comprometida, pode ser invalidada

**Vantagem:** Se alguém interceptar seu Access Token, ele só funciona por 1 hora. E você pode renovar sem precisar fazer login novamente!

### O Que é BCrypt?

**BCrypt** é um algoritmo de hash de senha que:
- Transforma sua senha em uma string aleatória
- É impossível reverter (não dá pra descobrir a senha original)
- Adiciona "salt" automaticamente (cada hash é único)
- É resistente a ataques de força bruta

**Exemplo:**
```
Senha original: "MinhaSenha123"
Hash BCrypt: "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy"
```

---

## Arquitetura do Sistema

### Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────────┐
│                        FRONTEND                              │
│  (React, Angular, Vue, etc.)                                │
│                                                              │
│  - Armazena tokens                                          │
│  - Envia Access Token nas requisições                       │
│  - Renova token automaticamente quando expira               │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   │ HTTP/HTTPS
                   │
┌──────────────────▼──────────────────────────────────────────┐
│                    API - CONTROLLERS                         │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   Register   │  │    Login     │  │   Refresh    │     │
│  │  Controller  │  │  Controller  │  │  Controller  │     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
│         │                  │                  │              │
│  ┌──────▼───────┐         │         ┌────────▼────────┐    │
│  │    Revoke    │         │         │  Protected      │    │
│  │  Controller  │         │         │  Endpoints      │    │
│  └──────────────┘         │         └─────────────────┘    │
└────────────────────────────┼──────────────────────────────┘
                             │
┌────────────────────────────▼──────────────────────────────┐
│                      SERVICES                              │
│                                                            │
│  ┌──────────────────┐  ┌─────────────────────────────┐  │
│  │  LoginService    │  │  RefreshTokenService        │  │
│  │                  │  │                             │  │
│  │ - Autentica      │  │ - Renova Access Token       │  │
│  │ - Gera JWT       │  │ - Revoga Refresh Token      │  │
│  │ - Cria Claims    │  │ - Valida Refresh Token      │  │
│  └────────┬─────────┘  └────────┬────────────────────┘  │
│           │                      │                        │
│  ┌────────▼──────────────────────▼────────────────────┐  │
│  │          PasswordHasher Service                    │  │
│  │  - Hash de senhas (BCrypt)                        │  │
│  │  - Verificação de senhas                          │  │
│  └───────────────────────────────────────────────────┘  │
└────────────────────────────┬──────────────────────────────┘
                             │
┌────────────────────────────▼──────────────────────────────┐
│                    REPOSITORIES                            │
│                                                            │
│  ┌──────────────────┐  ┌─────────────────────────────┐  │
│  │ LoginRepository  │  │ RefreshTokenRepository      │  │
│  │                  │  │                             │  │
│  │ - Busca usuário  │  │ - Cria Refresh Token        │  │
│  │ - CRUD usuários  │  │ - Valida Refresh Token      │  │
│  └────────┬─────────┘  └────────┬────────────────────┘  │
└────────────┼──────────────────────┼────────────────────────┘
             │                      │
┌────────────▼──────────────────────▼────────────────────────┐
│                    BANCO DE DADOS                          │
│                                                            │
│  ┌──────────────┐           ┌──────────────────┐         │
│  │   usuarios   │           │  refresh_tokens  │         │
│  │              │           │                  │         │
│  │ - id         │           │ - id             │         │
│  │ - username   │───────────│ - usuarioId      │         │
│  │ - passwordH. │           │ - token          │         │
│  │ - role       │           │ - expiresAt      │         │
│  │ - ativo      │           │ - isRevoked      │         │
│  └──────────────┘           └──────────────────┘         │
└────────────────────────────────────────────────────────────┘
```

### Estrutura de Pastas

```
W3AssinaDiplomaAPI/
│
├── Controllers/
│   ├── Register/
│   │   └── RegisterController.cs      # Criação de usuários
│   ├── Login/
│   │   └── LoginController.cs         # Autenticação
│   ├── Refresh/
│   │   └── RefreshController.cs       # Renovação de tokens
│   └── Revoke/
│       └── RevokeController.cs        # Logout (revogação)
│
├── Services/
│   ├── Login/
│   │   └── LoginService.cs            # Lógica de autenticação
│   ├── RefreshToken/
│   │   ├── RefreshTokenService.cs     # Gestão de refresh tokens
│   │   └── IRefreshTokenService.cs    # Interface
│   └── Password/
│       └── PasswordHasher.cs          # Hash de senhas (BCrypt)
│
├── Repositories/
│   ├── Login/
│   │   ├── LoginRepository.cs         # Acesso a dados de usuários
│   │   └── ILoginRepository.cs
│   └── RefreshToken/
│       ├── RefreshTokenRepository.cs  # Acesso a dados de tokens
│       └── IRefreshTokenRepository.cs
│
├── Models/
│   ├── Usuario/
│   │   ├── Usuario.cs                 # Entidade de usuário
│   │   └── RegisterRequest.cs         # DTO de registro
│   ├── Login/
│   │   ├── Login.cs                   # DTO de login
│   │   └── LoginResponse.cs           # DTO de resposta
│   └── RefreshToken/
│       ├── RefreshToken.cs            # Entidade de refresh token
│       └── RefreshTokenRequest.cs     # DTO de requisição
│
└── Program.cs                         # Configuração JWT e injeção de dependências
```

---

## Fluxo de Autenticação

### 1. Registro de Novo Usuário

**Endpoint:** `POST /register`
**Autenticação:** Requer token de Administrador

```mermaid
sequenceDiagram
    participant Admin as Administrador
    participant API as RegisterController
    participant Hash as PasswordHasher
    participant DB as Banco de Dados

    Admin->>API: POST /register<br/>{username, password, role}
    API->>API: Valida dados (ModelState)
    API->>DB: Verifica se username já existe
    alt Username já existe
        API-->>Admin: 400 Bad Request<br/>"Username já está em uso"
    else Username disponível
        API->>Hash: HashPassword(password)
        Hash-->>API: passwordHash
        API->>DB: Salva novo usuário
        DB-->>API: Sucesso
        API-->>Admin: 200 OK<br/>{id, username, role}
    end
```

**O Que Acontece Passo a Passo:**

1. **Administrador envia requisição** com dados do novo usuário
2. **Validação automática** (Data Annotations) verifica se os dados são válidos
3. **Verifica username único** - não pode ter dois usuários com mesmo username
4. **Cria hash da senha** usando BCrypt (senha nunca é salva em texto plano)
5. **Salva no banco de dados** com a senha hasheada
6. **Retorna sucesso** com dados do usuário (sem expor a senha)

**Arquivo:** [RegisterController.cs](Controllers/Register/RegisterController.cs)

```csharp
// Linha 50: Hash da senha
var passwordHash = _passwordHasher.HashPassword(request.Password);

// Linha 53-61: Criação do novo usuário
var novoUsuario = new Usuario
{
    Username = request.Username,
    PasswordHash = passwordHash,  // ⚠️ NUNCA salve senha em texto plano!
    Role = request.Role,
    NumeroBancoDados = numeroBancoDados,
    DataCriacao = DateTime.UtcNow,
    Ativo = true
};
```

---

### 2. Login (Autenticação)

**Endpoint:** `POST /login/{numeroBanco}`
**Autenticação:** Não requer token (público)

```mermaid
sequenceDiagram
    participant User as Usuário
    participant API as LoginController
    participant Service as LoginService
    participant Hash as PasswordHasher
    participant DB as Banco de Dados

    User->>API: POST /login/1<br/>{username, password}
    API->>Service: AuthenticateAsync()
    Service->>DB: Busca usuário por username
    alt Usuário não encontrado
        Service-->>API: null
        API-->>User: 401 Unauthorized
    else Usuário encontrado
        Service->>Hash: VerifyPassword()
        alt Senha incorreta
            Service-->>API: null
            API-->>User: 401 Unauthorized
        else Senha correta
            Service->>Service: Gera Access Token (JWT)
            Service->>DB: Cria Refresh Token
            Service-->>API: LoginResponse
            API-->>User: 200 OK<br/>{token, refreshToken, expires}
        end
    end
```

**O Que Acontece Passo a Passo:**

1. **Usuário envia credenciais** (username e password)
2. **Busca usuário no banco** de dados pelo username
3. **Verifica senha** comparando o hash (BCrypt)
4. **Se credenciais válidas:**
   - Gera **Access Token (JWT)** com duração de 1 hora
   - Gera **Refresh Token** com duração de 7 dias
   - Salva Refresh Token no banco de dados
5. **Retorna resposta** com ambos os tokens

**Arquivo:** [LoginService.cs](Services/Login/LoginService.cs)

```csharp
// Linha 34-38: Busca e valida usuário
var usuario = _loginRepository.GetUsuarioByUsername(request.Username);

if (usuario == null || !_passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash))
    return null;  // ❌ Credenciais inválidas

// Linha 52-59: Cria Claims (informações que vão dentro do token)
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, usuario.Username),
    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
    new Claim("role", usuario.Role),
    new Claim("numeroBancoDados", usuario.NumeroBancoDados.ToString()),
    new Claim("app", "W3AssinaDiplomaAPI")
};

// Linha 62-72: Cria o token JWT
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(claims),  // 📝 Dados do usuário
    Expires = expires,                      // ⏰ Expira em 1 hora
    Issuer = jwtIssuer,                    // 🏢 Quem emitiu o token
    Audience = jwtAudience,                 // 👥 Para quem é o token
    SigningCredentials = new SigningCredentials(...) // 🔐 Assinatura
};
```

**Resposta de Login:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6g7h8i9j0...",
  "expires": "2025-11-10T15:30:00Z",
  "numeroBancoDados": 1,
  "username": "joao.silva",
  "role": "Usuario"
}
```

---

### 3. Acessando Recursos Protegidos

**Endpoint:** Qualquer endpoint com `[Authorize]`
**Autenticação:** Requer Access Token válido

```mermaid
sequenceDiagram
    participant Client as Frontend
    participant API as Endpoint Protegido
    participant JWT as JWT Middleware
    participant DB as Banco de Dados

    Client->>API: GET /api/recurso<br/>Authorization: Bearer {token}
    API->>JWT: Valida token
    JWT->>JWT: Verifica assinatura
    JWT->>JWT: Verifica expiração
    JWT->>JWT: Verifica Issuer/Audience
    alt Token inválido ou expirado
        JWT-->>Client: 401 Unauthorized
    else Token válido
        JWT->>API: Autoriza requisição
        API->>DB: Busca dados
        DB-->>API: Retorna dados
        API-->>Client: 200 OK + Dados
    end
```

**O Que Acontece Passo a Passo:**

1. **Cliente envia requisição** com Access Token no header `Authorization`
2. **Middleware JWT intercepta** a requisição automaticamente
3. **Valida o token:**
   - Verifica assinatura digital
   - Verifica se não expirou
   - Verifica Issuer e Audience
4. **Se válido:** Extrai as Claims e autoriza o acesso
5. **Se inválido:** Retorna 401 Unauthorized

**Configuração:** [Program.cs](Program.cs:81-109)

```csharp
// Linha 81-109: Configuração de autenticação JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,  // ✅ Valida assinatura
        IssuerSigningKey = new SymmetricSecurityKey(key),

        ValidateIssuer = true,            // ✅ Valida emissor
        ValidIssuer = jwtIssuer,

        ValidateAudience = true,          // ✅ Valida destinatário
        ValidAudience = jwtAudience,

        ClockSkew = TimeSpan.Zero,        // ⏰ Sem tolerância de tempo
        ValidateLifetime = true            // ✅ Valida expiração
    };
});
```

---

### 4. Renovação de Token (Refresh)

**Endpoint:** `POST /api/refresh`
**Autenticação:** Não requer Access Token, mas requer Refresh Token válido

```mermaid
sequenceDiagram
    participant Client as Frontend
    participant API as RefreshController
    participant Service as RefreshTokenService
    participant DB as Banco de Dados

    Note over Client: Access Token expirou!
    Client->>API: POST /api/refresh<br/>{refreshToken, numeroBancoDados}
    API->>Service: RefreshAccessTokenAsync()
    Service->>DB: Busca refresh token
    alt Refresh token inválido ou expirado
        Service-->>API: null
        API-->>Client: 401 Unauthorized<br/>"Faça login novamente"
    else Refresh token válido
        Service->>DB: Revoga refresh token usado
        Service->>Service: Gera novo Access Token
        Service->>DB: Gera novo Refresh Token
        Service-->>API: LoginResponse
        API-->>Client: 200 OK<br/>{novoToken, novoRefreshToken}
    end
```

**O Que Acontece Passo a Passo:**

1. **Access Token expira** (após 1 hora)
2. **Frontend detecta** expiração (antes ou após erro 401)
3. **Envia Refresh Token** para renovar
4. **Valida Refresh Token:**
   - Verifica se existe no banco
   - Verifica se não foi revogado
   - Verifica se não expirou (7 dias)
5. **Rotação de tokens** (Token Rotation):
   - Revoga o Refresh Token usado (segurança)
   - Gera novo Access Token (1 hora)
   - Gera novo Refresh Token (7 dias)
6. **Retorna novos tokens**

**Arquivo:** [RefreshTokenService.cs](Services/RefreshToken/RefreshTokenService.cs:27-92)

```csharp
// Linha 30: Busca refresh token no banco
var storedToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshToken, numeroBancoDados);

// Linha 33-34: Valida se está ativo (não revogado e não expirado)
if (storedToken == null || !storedToken.IsActive)
    return null;

// Linha 43: IMPORTANTE - Revoga token usado (rotação)
await _refreshTokenRepository.RevokeRefreshTokenAsync(refreshToken, numeroBancoDados);

// Linha 80: Gera novo refresh token
var newRefreshToken = await _refreshTokenRepository.CreateRefreshTokenAsync(usuario.Id, numeroBancoDados);
```

**Por Que Revogar o Token Usado?**

Imagine que alguém roubou seu Refresh Token. Com rotação:
- Você usa o token e recebe um novo
- O token antigo é invalidado
- Se o ladrão tentar usar, não funciona mais!
- E você é notificado de atividade suspeita

---

### 5. Logout (Revogação de Token)

**Endpoint:** `POST /api/revoke`
**Autenticação:** Não requer Access Token

```mermaid
sequenceDiagram
    participant Client as Frontend
    participant API as RevokeController
    participant Service as RefreshTokenService
    participant DB as Banco de Dados

    Client->>API: POST /api/revoke<br/>{refreshToken, numeroBancoDados}
    API->>Service: RevokeTokenAsync()
    Service->>DB: Marca refresh token como revogado
    alt Token não encontrado
        Service-->>API: false
        API-->>Client: 404 Not Found
    else Token revogado com sucesso
        Service-->>API: true
        API-->>Client: 200 OK<br/>"Logout realizado"
        Client->>Client: Remove tokens do storage
    end
```

**O Que Acontece Passo a Passo:**

1. **Usuário clica em "Logout"**
2. **Frontend envia Refresh Token** para revogar
3. **Backend marca token como revogado** no banco
4. **Frontend remove todos os tokens** do localStorage/sessionStorage
5. **Usuário precisa fazer login novamente** para obter novos tokens

**Arquivo:** [RevokeController.cs](Controllers/Revoke/RevokeController.cs:44-47)

```csharp
// Marca o token como revogado no banco de dados
var success = await _refreshTokenService.RevokeTokenAsync(
    request.RefreshToken,
    request.NumeroBancoDados
);
```

---

## Componentes da API

### Models (Modelos de Dados)

#### 1. Usuario.cs
**Representa um usuário no sistema**

```csharp
[Table("usuarios")]
public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; }

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; }  // ⚠️ Hash BCrypt, NUNCA senha em texto plano

    [Required]
    [MaxLength(50)]
    public string Role { get; set; }  // "Administrador" ou "Usuario"

    [Required]
    public int NumeroBancoDados { get; set; }

    public DateTime DataCriacao { get; set; }

    public bool Ativo { get; set; }
}
```

**Campos Importantes:**
- `PasswordHash` - Hash BCrypt da senha (não reversível)
- `Role` - Define permissões ("Administrador", "Usuario")
- `NumeroBancoDados` - Identifica o banco de dados do cliente
- `Ativo` - Permite desativar usuários sem deletar

---

#### 2. RefreshToken.cs
**Representa um refresh token no sistema**

```csharp
[Table("refresh_tokens")]
public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Token { get; set; }  // String aleatória única

    [Required]
    public int UsuarioId { get; set; }  // Relaciona com Usuario

    [Required]
    public DateTime ExpiresAt { get; set; }  // Expira em 7 dias

    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }  // Quando foi revogado (nullable)

    public bool IsRevoked { get; set; }

    // Propriedades computadas (não mapeadas no banco)
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    [NotMapped]
    public bool IsActive => !IsRevoked && !IsExpired;  // Válido se não revogado e não expirado
}
```

**Campos Importantes:**
- `Token` - String aleatória única (diferente do JWT)
- `IsRevoked` - Marca token como inválido (logout)
- `IsActive` - Propriedade calculada que verifica se está válido

---

#### 3. LoginResponse.cs
**Resposta enviada após login ou refresh bem-sucedido**

```csharp
public class LoginResponse
{
    public string Token { get; set; }          // Access Token (JWT)
    public string RefreshToken { get; set; }    // Refresh Token
    public DateTime Expires { get; set; }       // Quando o Access Token expira
    public int NumeroBancoDados { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
}
```

**Uso:** Frontend armazena estes dados após login

---

### Services (Lógica de Negócio)

#### 1. LoginService.cs
**Responsável por autenticar usuários e gerar tokens**

**Principais Métodos:**

```csharp
public async Task<LoginResponse?> AuthenticateAsync(Login request, int numeroBanco)
```

**O que faz:**
1. Busca usuário por username
2. Verifica senha usando BCrypt
3. Gera Access Token (JWT) com Claims
4. Gera Refresh Token
5. Retorna LoginResponse ou null se falhar

**Claims Gerados no Token:**
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, usuario.Username),              // Nome do usuário
    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), // ID do usuário
    new Claim("role", usuario.Role),                           // Role (Administrador/Usuario)
    new Claim("numeroBancoDados", usuario.NumeroBancoDados.ToString()), // Banco de dados
    new Claim("app", "W3AssinaDiplomaAPI")                    // Identificador da aplicação
};
```

**Como Acessar Claims em um Controller:**
```csharp
// Pega o username do token
var username = User.FindFirst(ClaimTypes.Name)?.Value;

// Pega a role do token
var role = User.FindFirst("role")?.Value;

// Pega o ID do usuário
var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
```

---

#### 2. RefreshTokenService.cs
**Responsável por renovar tokens e revogar**

**Principais Métodos:**

```csharp
// Renova Access Token usando Refresh Token
public async Task<LoginResponse?> RefreshAccessTokenAsync(string refreshToken, int numeroBancoDados)

// Revoga um Refresh Token (logout)
public async Task<bool> RevokeTokenAsync(string refreshToken, int numeroBancoDados)
```

**Processo de Renovação:**
1. Valida Refresh Token
2. Busca usuário associado
3. Revoga Refresh Token usado (rotação)
4. Gera novo Access Token
5. Gera novo Refresh Token
6. Retorna ambos

---

#### 3. PasswordHasher.cs
**Responsável por hash e verificação de senhas**

**Métodos:**

```csharp
// Cria hash da senha
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password);
}

// Verifica se senha corresponde ao hash
public bool VerifyPassword(string password, string hashedPassword)
{
    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}
```

**Exemplo de Uso:**
```csharp
// No registro
var hash = _passwordHasher.HashPassword("MinhaSenha123");
// Resultado: "$2a$11$abcd1234..."

// No login
bool senhaCorreta = _passwordHasher.VerifyPassword("MinhaSenha123", hash);
// Resultado: true
```

---

### Controllers (Endpoints da API)

#### 1. RegisterController.cs
**Cria novos usuários (apenas administradores)**

```csharp
[Authorize(Roles = "Administrador")]  // ⚠️ Requer ser administrador
[HttpPost]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
```

**Endpoint:** `POST /register`

**Requisição:**
```json
{
  "username": "joao.silva",
  "password": "SenhaSegura123",
  "role": "Usuario",
  "numeroBancoDados": 1
}
```

**Resposta (200 OK):**
```json
{
  "message": "Usuário criado com sucesso",
  "usuario": {
    "id": 5,
    "username": "joao.silva",
    "role": "Usuario",
    "numeroBancoDados": 1
  }
}
```

---

#### 2. LoginController.cs
**Autentica usuários e retorna tokens**

```csharp
[HttpPost]
[Route("{numeroBanco:int}")]
public async Task<IActionResult> Login([FromRoute] int numeroBanco, [FromBody] Login request)
```

**Endpoint:** `POST /login/{numeroBanco}`

**Requisição:**
```json
{
  "username": "joao.silva",
  "password": "SenhaSegura123"
}
```

**Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWI...",
  "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
  "expires": "2025-11-10T15:30:00Z",
  "numeroBancoDados": 1,
  "username": "joao.silva",
  "role": "Usuario"
}
```

---

#### 3. RefreshController.cs
**Renova Access Token usando Refresh Token**

```csharp
[HttpPost]
public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
```

**Endpoint:** `POST /api/refresh`

**Requisição:**
```json
{
  "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
  "numeroBancoDados": 1
}
```

**Resposta (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",  // Novo Access Token
  "refreshToken": "b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7",  // Novo Refresh Token
  "expires": "2025-11-10T16:30:00Z",
  "numeroBancoDados": 1,
  "username": "joao.silva",
  "role": "Usuario"
}
```

---

#### 4. RevokeController.cs
**Revoga Refresh Token (logout)**

```csharp
[HttpPost]
public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
```

**Endpoint:** `POST /api/revoke`

**Requisição:**
```json
{
  "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
  "numeroBancoDados": 1
}
```

**Resposta (200 OK):**
```json
{
  "success": true,
  "message": "Logout realizado com sucesso"
}
```

---

## Como o Frontend Deve Implementar

### Estrutura de Armazenamento

O frontend precisa armazenar 3 informações após o login:

```javascript
// Salvar após login bem-sucedido
localStorage.setItem('accessToken', response.token);
localStorage.setItem('refreshToken', response.refreshToken);
localStorage.setItem('tokenExpires', response.expires);
```

**Onde Armazenar?**

| Método | Segurança | Persistência | Recomendação |
|--------|-----------|--------------|--------------|
| **localStorage** | ⚠️ Vulnerável a XSS | Persiste após fechar navegador | ✅ Recomendado se não há XSS |
| **sessionStorage** | ⚠️ Vulnerável a XSS | Apaga ao fechar aba | ⚠️ Perde sessão facilmente |
| **httpOnly Cookie** | ✅ Seguro contra XSS | Configurável | ✅ Mais seguro, mas complexo |
| **Variável em memória** | ✅ Seguro | ❌ Apaga ao recarregar | ❌ Má experiência |

**Recomendação:** Use `localStorage` para a maioria dos casos, mas garanta proteção contra XSS no seu frontend.

---

### Implementação em JavaScript/TypeScript

#### 1. Serviço de Autenticação Completo

```javascript
// auth.service.js
class AuthService {
  constructor() {
    this.API_URL = 'https://api.exemplo.com';
    this.numeroBancoDados = 1;  // Configure conforme necessário
  }

  // =============================
  // LOGIN
  // =============================
  async login(username, password) {
    try {
      const response = await fetch(`${this.API_URL}/login/${this.numeroBancoDados}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ username, password })
      });

      if (!response.ok) {
        throw new Error('Credenciais inválidas');
      }

      const data = await response.json();

      // 💾 Salva tokens no localStorage
      this.saveTokens(data);

      return data;
    } catch (error) {
      console.error('Erro no login:', error);
      throw error;
    }
  }

  // =============================
  // SALVAR TOKENS
  // =============================
  saveTokens(loginResponse) {
    localStorage.setItem('accessToken', loginResponse.token);
    localStorage.setItem('refreshToken', loginResponse.refreshToken);
    localStorage.setItem('tokenExpires', loginResponse.expires);
    localStorage.setItem('username', loginResponse.username);
    localStorage.setItem('role', loginResponse.role);
  }

  // =============================
  // OBTER ACCESS TOKEN
  // =============================
  getAccessToken() {
    return localStorage.getItem('accessToken');
  }

  // =============================
  // OBTER REFRESH TOKEN
  // =============================
  getRefreshToken() {
    return localStorage.getItem('refreshToken');
  }

  // =============================
  // VERIFICAR SE ESTÁ AUTENTICADO
  // =============================
  isAuthenticated() {
    const token = this.getAccessToken();
    const refreshToken = this.getRefreshToken();
    return !!token && !!refreshToken;
  }

  // =============================
  // VERIFICAR SE TOKEN EXPIROU
  // =============================
  isTokenExpired() {
    const expires = localStorage.getItem('tokenExpires');
    if (!expires) return true;

    const expirationDate = new Date(expires);
    const now = new Date();

    // Adiciona margem de 5 minutos para renovar antes de expirar
    const bufferTime = 5 * 60 * 1000; // 5 minutos em milissegundos
    return now.getTime() >= (expirationDate.getTime() - bufferTime);
  }

  // =============================
  // RENOVAR TOKEN
  // =============================
  async refreshAccessToken() {
    try {
      const refreshToken = this.getRefreshToken();

      if (!refreshToken) {
        throw new Error('Refresh token não encontrado');
      }

      const response = await fetch(`${this.API_URL}/api/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          refreshToken: refreshToken,
          numeroBancoDados: this.numeroBancoDados
        })
      });

      if (!response.ok) {
        // Refresh token inválido ou expirado - precisa fazer login novamente
        this.logout();
        throw new Error('Sessão expirada. Faça login novamente.');
      }

      const data = await response.json();

      // 💾 Salva novos tokens
      this.saveTokens(data);

      return data.token;
    } catch (error) {
      console.error('Erro ao renovar token:', error);
      this.logout();
      throw error;
    }
  }

  // =============================
  // LOGOUT
  // =============================
  async logout() {
    try {
      const refreshToken = this.getRefreshToken();

      if (refreshToken) {
        // Revoga o refresh token no servidor
        await fetch(`${this.API_URL}/api/revoke`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            refreshToken: refreshToken,
            numeroBancoDados: this.numeroBancoDados
          })
        });
      }
    } catch (error) {
      console.error('Erro ao revogar token:', error);
    } finally {
      // Remove tokens do localStorage (sempre executa)
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('tokenExpires');
      localStorage.removeItem('username');
      localStorage.removeItem('role');
    }
  }

  // =============================
  // OBTER DADOS DO USUÁRIO
  // =============================
  getUserData() {
    return {
      username: localStorage.getItem('username'),
      role: localStorage.getItem('role')
    };
  }

  // =============================
  // VERIFICAR SE É ADMINISTRADOR
  // =============================
  isAdmin() {
    return localStorage.getItem('role') === 'Administrador';
  }
}

// Exporta instância única (Singleton)
export const authService = new AuthService();
```

---

#### 2. HTTP Interceptor (Adiciona Token Automaticamente)

**Para Fetch API:**

```javascript
// http.interceptor.js
import { authService } from './auth.service';

// Função para fazer requisições autenticadas
export async function fetchWithAuth(url, options = {}) {
  // 1️⃣ Verifica se token expirou
  if (authService.isTokenExpired()) {
    console.log('Token expirado, renovando...');
    try {
      await authService.refreshAccessToken();
    } catch (error) {
      // Redireciona para login se não conseguir renovar
      window.location.href = '/login';
      throw error;
    }
  }

  // 2️⃣ Adiciona token no header
  const token = authService.getAccessToken();
  const authOptions = {
    ...options,
    headers: {
      ...options.headers,
      'Authorization': `Bearer ${token}`,  // ⚠️ Formato: "Bearer {token}"
      'Content-Type': 'application/json'
    }
  };

  // 3️⃣ Faz a requisição
  const response = await fetch(url, authOptions);

  // 4️⃣ Se retornar 401, tenta renovar token e refazer requisição
  if (response.status === 401) {
    console.log('Recebeu 401, tentando renovar token...');

    try {
      await authService.refreshAccessToken();

      // Refaz a requisição com novo token
      const newToken = authService.getAccessToken();
      authOptions.headers['Authorization'] = `Bearer ${newToken}`;
      return fetch(url, authOptions);
    } catch (error) {
      // Se falhar novamente, desloga
      authService.logout();
      window.location.href = '/login';
      throw error;
    }
  }

  return response;
}
```

**Uso:**
```javascript
// Em vez de usar fetch diretamente
const response = await fetch('/api/alunos');

// Use fetchWithAuth
const response = await fetchWithAuth('/api/alunos');
```

---

**Para Axios:**

```javascript
// axios.interceptor.js
import axios from 'axios';
import { authService } from './auth.service';

// Cria instância do axios
const api = axios.create({
  baseURL: 'https://api.exemplo.com'
});

// 📤 Request Interceptor (adiciona token antes de enviar)
api.interceptors.request.use(
  async (config) => {
    // Verifica se token expirou
    if (authService.isTokenExpired()) {
      console.log('Token expirado, renovando...');
      await authService.refreshAccessToken();
    }

    // Adiciona token no header
    const token = authService.getAccessToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// 📥 Response Interceptor (trata erro 401)
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Se receber 401 e não tiver tentado renovar ainda
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Tenta renovar token
        await authService.refreshAccessToken();

        // Refaz requisição com novo token
        const token = authService.getAccessToken();
        originalRequest.headers.Authorization = `Bearer ${token}`;
        return api(originalRequest);
      } catch (refreshError) {
        // Se falhar, desloga
        authService.logout();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

**Uso:**
```javascript
import api from './axios.interceptor';

// Todas as requisições automaticamente incluem o token
const response = await api.get('/api/alunos');
const data = await api.post('/api/alunos', { nome: 'João' });
```

---

#### 3. Componente de Login (React)

```jsx
// LoginPage.jsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from './auth.service';

function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      // Faz login
      await authService.login(username, password);

      // Redireciona para home
      navigate('/home');
    } catch (err) {
      setError('Usuário ou senha inválidos');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <form onSubmit={handleSubmit}>
        <h2>Login</h2>

        {error && <div className="error">{error}</div>}

        <div>
          <label>Usuário:</label>
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>

        <div>
          <label>Senha:</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        <button type="submit" disabled={loading}>
          {loading ? 'Entrando...' : 'Entrar'}
        </button>
      </form>
    </div>
  );
}

export default LoginPage;
```

---

#### 4. Rota Protegida (React)

```jsx
// ProtectedRoute.jsx
import React from 'react';
import { Navigate } from 'react-router-dom';
import { authService } from './auth.service';

function ProtectedRoute({ children, requireAdmin = false }) {
  // Verifica se está autenticado
  if (!authService.isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  // Se requer admin, verifica role
  if (requireAdmin && !authService.isAdmin()) {
    return <Navigate to="/acesso-negado" replace />;
  }

  return children;
}

export default ProtectedRoute;
```

**Uso no Router:**
```jsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
import LoginPage from './LoginPage';
import HomePage from './HomePage';
import AdminPage from './AdminPage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        {/* Rota protegida - qualquer usuário autenticado */}
        <Route
          path="/home"
          element={
            <ProtectedRoute>
              <HomePage />
            </ProtectedRoute>
          }
        />

        {/* Rota protegida - apenas administradores */}
        <Route
          path="/admin"
          element={
            <ProtectedRoute requireAdmin={true}>
              <AdminPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </BrowserRouter>
  );
}
```

---

#### 5. Componente de Logout

```jsx
// LogoutButton.jsx
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from './auth.service';

function LogoutButton() {
  const navigate = useNavigate();

  const handleLogout = async () => {
    await authService.logout();
    navigate('/login');
  };

  return (
    <button onClick={handleLogout} className="btn-logout">
      Sair
    </button>
  );
}

export default LogoutButton;
```

---

### Fluxograma Completo do Frontend

```
┌─────────────────────────────────────────────────────────────┐
│                     USUÁRIO ACESSA APP                       │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
         ┌─────────────────────────┐
         │ Tem tokens salvos?      │
         └────────┬────────────────┘
                  │
         ┌────────┴────────┐
         │                 │
        SIM               NÃO
         │                 │
         ▼                 ▼
┌────────────────┐  ┌─────────────┐
│ Token expirou? │  │ Redireciona │
└────────┬───────┘  │ para LOGIN  │
         │          └─────────────┘
    ┌────┴────┐
    │         │
   SIM       NÃO
    │         │
    ▼         ▼
┌─────────┐ ┌────────────────┐
│ Renova  │ │ Acessa recurso │
│ token   │ │ normalmente    │
└────┬────┘ └────────────────┘
     │
     ▼
┌────────────────┐
│ Renovação      │
│ bem-sucedida?  │
└────────┬───────┘
         │
    ┌────┴────┐
    │         │
   SIM       NÃO
    │         │
    ▼         ▼
┌─────────┐ ┌─────────────┐
│ Acessa  │ │ Desloga e   │
│ recurso │ │ vai p/LOGIN │
└─────────┘ └─────────────┘
```

---

### Boas Práticas do Frontend

#### ✅ FAÇA:

1. **Sempre use HTTPS em produção**
   ```javascript
   // ❌ Nunca em produção
   const API_URL = 'http://api.exemplo.com';

   // ✅ Sempre em produção
   const API_URL = 'https://api.exemplo.com';
   ```

2. **Remova tokens ao fazer logout**
   ```javascript
   async logout() {
     await this.revokeToken();
     localStorage.clear();  // Limpa TUDO
   }
   ```

3. **Trate erros de autenticação**
   ```javascript
   try {
     await fetchWithAuth('/api/alunos');
   } catch (error) {
     if (error.status === 401) {
       alert('Sessão expirada. Faça login novamente.');
       navigate('/login');
     }
   }
   ```

4. **Renove token proativamente**
   ```javascript
   // Renova 5 minutos ANTES de expirar
   const bufferTime = 5 * 60 * 1000;
   if (now >= expirationTime - bufferTime) {
     await refreshToken();
   }
   ```

5. **Proteja rotas sensíveis**
   ```jsx
   <Route path="/admin" element={
     <ProtectedRoute requireAdmin>
       <AdminPage />
     </ProtectedRoute>
   } />
   ```

#### ❌ NÃO FAÇA:

1. **Nunca armazene senhas**
   ```javascript
   // ❌ NUNCA FAÇA ISSO!
   localStorage.setItem('password', password);
   ```

2. **Nunca exponha tokens em URLs**
   ```javascript
   // ❌ NUNCA!
   window.location.href = `/dashboard?token=${token}`;

   // ✅ Use headers
   headers: { 'Authorization': `Bearer ${token}` }
   ```

3. **Nunca ignore erros 401**
   ```javascript
   // ❌ Ruim
   fetch('/api/alunos').then(r => r.json());

   // ✅ Bom
   const response = await fetch('/api/alunos');
   if (response.status === 401) {
     handleUnauthorized();
   }
   ```

4. **Nunca use token expirado**
   ```javascript
   // ✅ Sempre verifique antes
   if (isTokenExpired()) {
     await refreshToken();
   }
   ```

---

## Segurança

### Medidas Implementadas

#### 1. Hash de Senhas com BCrypt

**Problema:** Armazenar senhas em texto plano é extremamente inseguro.

**Solução:** BCrypt transforma a senha em um hash irreversível.

```csharp
// ❌ NUNCA faça isso
usuario.Senha = "MinhaSenha123";  // Visível no banco!

// ✅ SEMPRE use hash
var hash = _passwordHasher.HashPassword("MinhaSenha123");
usuario.PasswordHash = hash;  // Hash: "$2a$11$abc123..."
```

**Benefícios do BCrypt:**
- Adiciona "salt" automaticamente (cada hash é único)
- Resistente a ataques de força bruta (lento por design)
- Impossível reverter o hash para obter a senha

---

#### 2. Tokens JWT Assinados

**Problema:** Alguém pode tentar falsificar um token.

**Solução:** JWT é assinado com chave secreta que só o servidor conhece.

```csharp
// Assinatura do token
SigningCredentials = new SigningCredentials(
    new SymmetricSecurityKey(key),  // Chave secreta
    SecurityAlgorithms.HmacSha256Signature  // Algoritmo HMAC-SHA256
)
```

**Como funciona:**
1. Servidor gera token e assina com chave secreta
2. Cliente recebe token
3. Cliente envia token de volta
4. Servidor verifica assinatura (só ele tem a chave)
5. Se assinatura válida, confia no token

**Tentativa de Falsificação:**
```
Token original:  eyJ...abc (assinado)
Token falsificado: eyJ...xyz (assinatura inválida)
❌ Servidor rejeita!
```

---

#### 3. Validação de Issuer e Audience

**Problema:** Token pode ser usado em outra aplicação.

**Solução:** Valida quem emitiu (Issuer) e para quem é (Audience).

```csharp
ValidateIssuer = true,
ValidIssuer = "W3AssinaDiplomaAPI",  // Quem emitiu

ValidateAudience = true,
ValidAudience = "W3AssinaDiplomaAPI",  // Para quem é
```

**Cenário:**
- Token emitido para "AppA" não funciona no "AppB"
- Token emitido por "ServidorX" não funciona no "ServidorY"

---

#### 4. Expiração de Tokens

**Access Token:** 1 hora
**Refresh Token:** 7 dias

**Por que expira?**
- Se token for roubado, só funciona por tempo limitado
- Força renovação periódica
- Reduz janela de ataque

```csharp
Expires = DateTime.UtcNow.AddHours(1),  // Access Token
ClockSkew = TimeSpan.Zero,  // Sem tolerância (expira no segundo exato)
ValidateLifetime = true  // Valida expiração
```

---

#### 5. Rotação de Refresh Tokens

**Problema:** Refresh Token roubado pode ser usado indefinidamente.

**Solução:** Cada vez que usa Refresh Token, ele é invalidado e um novo é gerado.

```
Login
  ↓
RefreshToken1 (válido)
  ↓
Usa RefreshToken1 → Gera RefreshToken2
  ↓
RefreshToken1 (REVOGADO ❌)
RefreshToken2 (válido ✅)
```

**Se alguém roubar:**
1. Usuário legítimo usa RefreshToken1
2. RefreshToken1 é revogado
3. Ladrão tenta usar RefreshToken1
4. ❌ Rejeitado! Token revogado
5. Sistema detecta atividade suspeita

Código: [RefreshTokenService.cs:43](Services/RefreshToken/RefreshTokenService.cs:43)
```csharp
// Revoga token usado
await _refreshTokenRepository.RevokeRefreshTokenAsync(refreshToken, numeroBancoDados);
```

---

#### 6. HTTPS em Produção

**Problema:** Tokens podem ser interceptados no tráfego HTTP.

**Solução:** Exige HTTPS em produção.

```csharp
options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
```

**O que muda:**
- **Desenvolvimento:** Aceita HTTP (localhost)
- **Produção:** Exige HTTPS (criptografado)

---

#### 7. CORS Configurado

**Problema:** Qualquer site pode fazer requisições para sua API.

**Solução:** Whitelist de origens permitidas.

```csharp
var allowedOrigins = new[] {
    "http://localhost:3000",
    "https://app.exemplo.com"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)  // ⚠️ Apenas estas origens
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

---

#### 8. Autorização por Roles

**Problema:** Qualquer usuário pode acessar endpoints administrativos.

**Solução:** Atributo `[Authorize]` com Roles.

```csharp
// Apenas administradores podem registrar usuários
[Authorize(Roles = "Administrador")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)

// Qualquer usuário autenticado
[Authorize]
public async Task<IActionResult> GetAlunos()

// Endpoint público (sem autenticação)
public async Task<IActionResult> Login([FromBody] Login request)
```

---

### Vulnerabilidades Prevenidas

| Ataque | Como foi Prevenido |
|--------|-------------------|
| **SQL Injection** | Entity Framework usa queries parametrizadas |
| **XSS** | JWT não armazena HTML, apenas JSON |
| **CSRF** | JWT em header (não em cookie automático) |
| **Força Bruta** | BCrypt é lento (11 rounds), dificulta tentativas |
| **Rainbow Tables** | BCrypt adiciona salt único em cada hash |
| **Token Replay** | Tokens expiram, rotação de Refresh Tokens |
| **Man-in-the-Middle** | HTTPS obrigatório em produção |
| **Token Falsificado** | Assinatura HMAC-SHA256 verificada |
| **Privilege Escalation** | Validação de Roles no backend |
| **CORS** | Whitelist de origens permitidas |

---

### Configuração da Chave JWT (IMPORTANTE!)

A chave JWT **NÃO DEVE** estar no `appsettings.json` em produção!

**Configurar com User Secrets (Desenvolvimento):**

```bash
cd W3AssinaDiplomaAPI
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "sua-chave-super-secreta-com-pelo-menos-32-caracteres"
```

**Configurar com Variáveis de Ambiente (Produção):**

```bash
# Linux/Mac
export Jwt__Key="sua-chave-super-secreta"

# Windows
set Jwt__Key=sua-chave-super-secreta

# Docker
docker run -e Jwt__Key="sua-chave" ...
```

**Gerar Chave Segura:**

```bash
# Gera chave aleatória de 64 caracteres
openssl rand -base64 48
```

---

## Exemplos Práticos

### Exemplo 1: Registro de Novo Usuário

**Cenário:** Administrador quer criar conta para novo funcionário.

**Passo a Passo:**

1. **Administrador faz login**
   ```http
   POST /login/1
   {
     "username": "admin",
     "password": "SenhaAdmin123"
   }
   ```

2. **Recebe tokens**
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "refreshToken": "a1b2c3...",
     "role": "Administrador"
   }
   ```

3. **Registra novo usuário**
   ```http
   POST /register
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

   {
     "username": "maria.santos",
     "password": "SenhaMaria456",
     "role": "Usuario",
     "numeroBancoDados": 1
   }
   ```

4. **Usuário criado com sucesso**
   ```json
   {
     "message": "Usuário criado com sucesso",
     "usuario": {
       "id": 5,
       "username": "maria.santos",
       "role": "Usuario"
     }
   }
   ```

---

### Exemplo 2: Fluxo Completo de Login e Acesso

```javascript
// 1. LOGIN
const loginResponse = await fetch('https://api.exemplo.com/login/1', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'joao.silva',
    password: 'SenhaJoao123'
  })
});

const loginData = await loginResponse.json();
// {
//   "token": "eyJhbGc...",
//   "refreshToken": "a1b2c3...",
//   "expires": "2025-11-10T15:30:00Z"
// }

// 2. SALVA TOKENS
localStorage.setItem('accessToken', loginData.token);
localStorage.setItem('refreshToken', loginData.refreshToken);
localStorage.setItem('tokenExpires', loginData.expires);

// 3. ACESSA RECURSO PROTEGIDO
const alunosResponse = await fetch('https://api.exemplo.com/api/alunos', {
  headers: {
    'Authorization': `Bearer ${loginData.token}`
  }
});

const alunos = await alunosResponse.json();
console.log(alunos);  // Lista de alunos

// 4. TOKEN EXPIRA (após 1 hora)
// Frontend detecta expiração

// 5. RENOVA TOKEN
const refreshResponse = await fetch('https://api.exemplo.com/api/refresh', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    refreshToken: loginData.refreshToken,
    numeroBancoDados: 1
  })
});

const newTokens = await refreshResponse.json();
// {
//   "token": "eyJhbGc...",  // NOVO Access Token
//   "refreshToken": "x1y2z3...",  // NOVO Refresh Token
//   "expires": "2025-11-10T16:30:00Z"
// }

// 6. ATUALIZA TOKENS SALVOS
localStorage.setItem('accessToken', newTokens.token);
localStorage.setItem('refreshToken', newTokens.refreshToken);
localStorage.setItem('tokenExpires', newTokens.expires);

// 7. CONTINUA USANDO APP NORMALMENTE
```

---

### Exemplo 3: Tratamento de Erro 401

```javascript
async function buscarAlunos() {
  try {
    // Tenta buscar alunos
    const response = await fetch('https://api.exemplo.com/api/alunos', {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      }
    });

    // Se retornar 401 (Unauthorized)
    if (response.status === 401) {
      console.log('Token expirado, renovando...');

      // Tenta renovar token
      const refreshResponse = await fetch('https://api.exemplo.com/api/refresh', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          refreshToken: localStorage.getItem('refreshToken'),
          numeroBancoDados: 1
        })
      });

      // Se renovação falhar, redireciona para login
      if (!refreshResponse.ok) {
        alert('Sessão expirada. Faça login novamente.');
        window.location.href = '/login';
        return;
      }

      // Salva novos tokens
      const newTokens = await refreshResponse.json();
      localStorage.setItem('accessToken', newTokens.token);
      localStorage.setItem('refreshToken', newTokens.refreshToken);

      // REFAZ a requisição original com novo token
      const retryResponse = await fetch('https://api.exemplo.com/api/alunos', {
        headers: {
          'Authorization': `Bearer ${newTokens.token}`
        }
      });

      return await retryResponse.json();
    }

    // Resposta OK
    return await response.json();
  } catch (error) {
    console.error('Erro ao buscar alunos:', error);
    throw error;
  }
}
```

---

### Exemplo 4: Logout Completo

```javascript
async function logout() {
  try {
    // 1. Revoga refresh token no servidor
    await fetch('https://api.exemplo.com/api/revoke', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        refreshToken: localStorage.getItem('refreshToken'),
        numeroBancoDados: 1
      })
    });

    console.log('Token revogado no servidor');
  } catch (error) {
    console.error('Erro ao revogar token:', error);
  } finally {
    // 2. Remove tokens do localStorage (SEMPRE executa)
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpires');
    localStorage.removeItem('username');
    localStorage.removeItem('role');

    // 3. Redireciona para login
    window.location.href = '/login';
  }
}
```

---

## Troubleshooting

### Problema 1: "401 Unauthorized" ao acessar endpoint

**Possíveis Causas:**

1. **Token não foi enviado**
   ```javascript
   // ❌ Esqueceu de enviar token
   fetch('/api/alunos')

   // ✅ Envia token no header
   fetch('/api/alunos', {
     headers: { 'Authorization': `Bearer ${token}` }
   })
   ```

2. **Formato do header incorreto**
   ```javascript
   // ❌ Formato errado
   headers: { 'Authorization': token }

   // ✅ Formato correto (com "Bearer ")
   headers: { 'Authorization': `Bearer ${token}` }
   ```

3. **Token expirado**
   - Verifique campo `expires` da resposta de login
   - Renove o token usando Refresh Token

4. **Chave JWT não configurada**
   ```bash
   dotnet user-secrets set "Jwt:Key" "sua-chave-aqui"
   ```

---

### Problema 2: "Refresh token inválido ou expirado"

**Possíveis Causas:**

1. **Refresh Token expirou (7 dias)**
   - Usuário precisa fazer login novamente
   - Não há como renovar Refresh Token expirado

2. **Refresh Token foi revogado**
   - Acontece após logout
   - Usuário precisa fazer login novamente

3. **Token não existe no banco**
   - Verifique se banco de dados está correto
   - Verifique `numeroBancoDados` na requisição

---

### Problema 3: "Username já está em uso"

**Causa:** Tentando registrar usuário com username que já existe.

**Solução:** Use outro username único.

---

### Problema 4: CORS Error

**Erro:** `Access to fetch at 'https://api.exemplo.com' from origin 'http://localhost:3000' has been blocked by CORS policy`

**Causa:** Origem do frontend não está na whitelist.

**Solução:** Adicione origem no `appsettings.json`

```json
"AllowedOrigins": [
  "http://localhost:3000",
  "http://localhost:4200",
  "https://seu-dominio.com"
]
```

---

### Problema 5: Token não está sendo validado

**Verifique:**

1. **Middleware na ordem correta** ([Program.cs:162-163](Program.cs:162-163))
   ```csharp
   app.UseAuthentication();  // ⚠️ ANTES
   app.UseAuthorization();   // ⚠️ DEPOIS
   ```

2. **Atributo [Authorize] no controller**
   ```csharp
   [Authorize]  // ⚠️ Não esqueça!
   public async Task<IActionResult> GetAlunos()
   ```

3. **Issuer e Audience correspondem**
   - Verifique `appsettings.json`
   - Devem ser iguais no token e na validação

---

### Problema 6: Senha não está sendo validada corretamente

**Verifique:**

1. **Usando BCrypt.Verify (não comparação direta)**
   ```csharp
   // ❌ Nunca compare diretamente
   if (request.Password == usuario.PasswordHash)

   // ✅ Use VerifyPassword
   if (_passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash))
   ```

2. **Hash foi salvo corretamente**
   - Verifique no banco se campo `passwordHash` está preenchido
   - Hash BCrypt começa com `$2a$` ou `$2b$`

---

### Debug: Como Decodificar um JWT

**Online:** https://jwt.io

**JavaScript:**
```javascript
function decodeJWT(token) {
  const parts = token.split('.');
  const payload = parts[1];
  const decoded = atob(payload);  // Base64 decode
  return JSON.parse(decoded);
}

const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
const payload = decodeJWT(token);
console.log(payload);
// {
//   "nameid": "5",
//   "unique_name": "joao.silva",
//   "role": "Usuario",
//   "numeroBancoDados": "1",
//   "exp": 1699632600
// }
```

---

## Resumo Rápido

### Para Iniciantes: O Básico

1. **JWT = Token de acesso** (como um ingresso temporário)
2. **Refresh Token = Renovador de JWT** (como renovar ingresso sem ir na bilheteria)
3. **BCrypt = Proteção de senha** (transforma senha em código impossível de reverter)
4. **401 Unauthorized = Token inválido** (precisa renovar ou fazer login)

---

### Fluxo Simplificado

```
1. LOGIN → Recebe Access Token + Refresh Token
2. USA Access Token → Para todas as requisições
3. Token EXPIRA → Usa Refresh Token para renovar
4. LOGOUT → Revoga Refresh Token
```

---

### Checklist de Implementação Frontend

- [ ] Criar serviço de autenticação
- [ ] Implementar login e salvar tokens
- [ ] Adicionar token em todas as requisições
- [ ] Implementar renovação automática de token
- [ ] Tratar erro 401 (token expirado)
- [ ] Implementar logout (remover tokens)
- [ ] Proteger rotas sensíveis
- [ ] Adicionar loading states
- [ ] Tratar erros de rede

---

### Endpoints Principais

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/register` | Cria novo usuário | Admin |
| POST | `/login/{numeroBanco}` | Faz login | Público |
| POST | `/api/refresh` | Renova token | Refresh Token |
| POST | `/api/revoke` | Logout | Refresh Token |

---

### Configurações Importantes

```json
// appsettings.json
{
  "Jwt": {
    "Key": "[Configure via User Secrets]",
    "Issuer": "W3AssinaDiplomaAPI",
    "Audience": "W3AssinaDiplomaAPI",
    "ExpirationHours": 1,
    "RefreshTokenExpirationDays": 7
  }
}
```

---

## Conclusão

Este sistema de autenticação implementa as melhores práticas de segurança da indústria:

- **JWT com assinatura HMAC-SHA256**
- **Refresh Tokens com rotação**
- **Hash BCrypt para senhas**
- **Validação completa de tokens**
- **CORS configurado**
- **HTTPS em produção**
- **Autorização por roles**

O sistema é **seguro, escalável e fácil de usar** tanto para desenvolvedores backend quanto frontend.

---

**Autor:** Documentação gerada para W3AssinaDiplomaAPI
**Data:** 2025-11-10
**Versão:** 1.0
