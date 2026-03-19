# Fluxo de Esqueci Senha (Backend + Flutter)

Este documento descreve o fluxo de recuperacao de senha implementado na API usando apenas CPF para solicitacao.

## Visao geral

1. Usuario informa CPF em `POST /api/auth/esqueci-senha`.
2. API responde sempre `200` com mensagem generica.
3. Se o CPF existir, API gera token de recuperacao (uso unico, com expiracao).
4. API envia e-mail com link de redefinicao.
5. Usuario abre o link e define nova senha em `POST /api/auth/redefinir-senha`.
6. API atualiza a senha, invalida o token e revoga refresh tokens ativos.

## Configuracoes necessarias

### 1) SMTP

Cadastre ao menos uma conta SMTP no modulo `EmailConta`.
A API usa a conta com menor `ContaID` como padrao.

Campos usados:
- `endereco`
- `credenciaisUsuario`
- `credenciaisSenha`
- `smtpHost`
- `smtpPort`
- `smtpSSL` (`True` ou `False`)

### 2) appsettings.json

```json
"PasswordReset": {
  "BaseUrl": "https://www.w3soft3.com.br/w3SafeDocking/#",
  "Path": "/redefinir-senha",
  "TokenExpirationMinutes": 30
}
```

- `BaseUrl`: URL base do app web que recebe o link.
- `Path`: rota da tela de redefinicao.
- `TokenExpirationMinutes`: validade do token de recuperacao.

## Banco de dados

Criar tabela de tokens de recuperacao:

```sql
CREATE TABLE recuperacao_senha_tokens (
    id INT NOT NULL AUTO_INCREMENT,
    usuarioId INT NOT NULL,
    tokenHash VARCHAR(128) NOT NULL,
    criadoEm DATETIME NOT NULL,
    expiraEm DATETIME NOT NULL,
    usadoEm DATETIME NULL,
    revogado TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    INDEX idx_recuperacao_usuario (usuarioId),
    UNIQUE KEY uk_recuperacao_token_hash (tokenHash),
    CONSTRAINT fk_recuperacao_usuario
        FOREIGN KEY (usuarioId) REFERENCES usuarios(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
```

## Endpoints de recuperacao

### 1) Esqueci senha (CPF)

`POST /api/auth/esqueci-senha`

Request:

```json
{
  "cpf": "123.456.789-00"
}
```

Response (`200` sempre):

```json
{
  "mensagem": "Se os dados estiverem corretos, enviaremos um link para redefinicao de senha."
}
```

Observacao:
- Essa resposta e sempre a mesma para nao expor se o CPF existe ou nao.

### 2) Redefinir senha

`POST /api/auth/redefinir-senha`

Request:

```json
{
  "token": "TOKEN_RECEBIDO_NO_LINK",
  "novaSenha": "SenhaForte123!"
}
```

Sucesso (`200`):

```json
{
  "mensagem": "Senha redefinida com sucesso."
}
```

Falha (`400`):

```json
{
  "erro": "Token de recuperacao invalido, expirado ou ja utilizado."
}
```

## Implementacao no Flutter (Dio)

### 1) Tela EsqueciSenhaPage

Campos:
- CPF

Comportamento:
- Usuario informa CPF e clica em "Enviar link".
- Chama `POST /api/auth/esqueci-senha`.
- Exibe a mensagem de retorno sem diferenciar existencia de conta.

Exemplo:

```dart
final response = await dio.post(
  '/api/auth/esqueci-senha',
  data: {
    'cpf': cpfInformado,
  },
);
```

### 2) Tela RedefinirSenhaPage

Rota exemplo:
- `/redefinir-senha?token=...`

Campos:
- Nova senha
- Confirmar senha

Validacoes:
- minimo 8 caracteres
- confirmar senha igual

Exemplo:

```dart
final response = await dio.post(
  '/api/auth/redefinir-senha',
  data: {
    'token': tokenDaUrl,
    'novaSenha': senhaDigitada,
  },
);
```

Tratamento:
- `200`: mostrar sucesso e redirecionar para login.
- `400`: mostrar erro de token invalido/expirado e opcao de voltar para "Esqueci senha".

## Fluxo recomendado no app

1. Usuario toca em "Esqueci minha senha" na tela de login.
2. Digita CPF e solicita link.
3. Recebe e-mail e abre link de redefinicao.
4. Define nova senha.
5. Retorna para login.

## Prompt pronto para implementar no Flutter

Copie e cole o prompt abaixo na sua ferramenta de IA para gerar a implementacao no app Flutter:

```text
Quero implementar no Flutter o fluxo de esqueci senha da minha API usando apenas CPF.
Siga exatamente este passo a passo e gere codigo completo, simples e legivel.

Contexto de backend:
- POST /api/auth/esqueci-senha
  body: { "cpf": "string" }
  response 200 (sempre): { "mensagem": "Se os dados estiverem corretos, enviaremos um link para redefinicao de senha." }

- POST /api/auth/redefinir-senha
  body: { "token": "string", "novaSenha": "string" }
  response 200: { "mensagem": "Senha redefinida com sucesso." }
  response 400: { "erro": "Token de recuperacao invalido, expirado ou ja utilizado." }

Objetivo:
1) Criar tela EsqueciSenhaPage:
   - campo CPF com mascara
   - botao "Enviar link"
   - loading no botao
   - exibicao da mensagem de retorno

2) Criar tela RedefinirSenhaPage:
   - ler token da URL (ex.: /redefinir-senha?token=...)
   - campos nova senha e confirmar senha
   - validacao minima de 8 caracteres
   - validacao de confirmacao de senha
   - botao "Redefinir senha"

3) Criar AuthApi com Dio:
   - Future<String> esqueciSenha(String cpf)
   - Future<String> redefinirSenha({required String token, required String novaSenha})
   - tratar timeout, sem internet e erros 400

4) Fluxo UX:
   - em esqueci-senha: sempre mostrar mensagem de sucesso retornada pela API
   - em redefinir-senha 200: mostrar sucesso e redirecionar para /login
   - em redefinir-senha 400: mostrar erro amigavel de token expirado/invalido

5) Estrutura sugerida:
   - lib/features/auth/data/auth_api.dart
   - lib/features/auth/models/password_reset_models.dart
   - lib/features/auth/presentation/esqueci_senha_page.dart
   - lib/features/auth/presentation/redefinir_senha_page.dart
   - lib/core/network/dio_client.dart (se necessario)

6) Entregaveis:
   - codigo completo dos arquivos
   - configuracao de rotas
   - exemplo de chamada no main.dart
   - checklist de testes manuais
```

## Checklist de validacao no Flutter

- Tela EsqueciSenhaPage envia CPF para `POST /api/auth/esqueci-senha`.
- Mensagem de retorno da API e exibida corretamente.
- Rota `/redefinir-senha?token=...` abre a tela de redefinicao.
- Tela valida senha e confirmacao antes de enviar.
- Chamada `POST /api/auth/redefinir-senha` funciona com token valido.
- Com token expirado/invalido, app mostra erro amigavel.
- Fluxo de sucesso redireciona para login.
