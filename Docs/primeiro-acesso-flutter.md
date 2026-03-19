# Fluxo de Primeiro Acesso (Backend + Flutter)

Este documento descreve o fluxo de primeiro acesso implementado na API para cadastro de usuario sem envio de senha em texto por e-mail.

## Visao geral

1. Admin cria usuario em `POST /api/usuarios/setUsuario`.
2. API salva o usuario com:
   - `Ativo = false`
   - `SenhaHash = ""`
3. API gera token de primeiro acesso (uso unico, com expiracao).
4. API envia e-mail com link para definir senha.
5. Usuario abre o link no app/web Flutter e define a primeira senha.
6. API ativa o usuario (`Ativo = true`) e invalida o token.

## Configuracoes necessarias

### 1) SMTP

Cadastre ao menos uma conta SMTP no modulo `EmailConta` (a API usa a conta de menor `ContaID` como padrao).

Campos usados:
- `endereco`
- `credenciaisUsuario`
- `credenciaisSenha`
- `smtpHost`
- `smtpPort`
- `smtpSSL` (`True` ou `False`)

### 2) appsettings.json

```json
"FirstAccess": {
  "BaseUrl": "http://localhost:3000",
  "Path": "/primeiro-acesso",
  "TokenExpirationMinutes": 60
}
```

- `BaseUrl`: URL base do frontend Flutter Web (ou dominio que recebe o link).
- `Path`: rota da tela de primeiro acesso.
- `TokenExpirationMinutes`: validade do link.

## Banco de dados

Criar tabela de tokens de primeiro acesso:

```sql
CREATE TABLE primeiro_acesso_tokens (
    id INT NOT NULL AUTO_INCREMENT,
    usuarioId INT NOT NULL,
    tokenHash VARCHAR(128) NOT NULL,
    criadoEm DATETIME NOT NULL,
    expiraEm DATETIME NOT NULL,
    usadoEm DATETIME NULL,
    revogado TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    INDEX idx_primeiro_acesso_usuario (usuarioId),
    UNIQUE KEY uk_primeiro_acesso_token_hash (tokenHash),
    CONSTRAINT fk_primeiro_acesso_usuario
        FOREIGN KEY (usuarioId) REFERENCES usuarios(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
```

## Endpoints novos de Auth

### 1) Definir primeira senha

`POST /api/auth/definir-primeira-senha`

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
  "mensagem": "Senha definida com sucesso."
}
```

Falha (`400`):

```json
{
  "erro": "Token de primeiro acesso invalido, expirado ou ja utilizado."
}
```

### 2) Reenviar link de primeiro acesso

`POST /api/auth/reenviar-primeiro-acesso`

Request:

```json
{
  "cpf": "123.456.789-00"
}
```

Sucesso (`200`):

```json
{
  "mensagem": "Link de primeiro acesso reenviado com sucesso."
}
```

Falha (`400`):

```json
{
  "erro": "Nao foi possivel reenviar o link de primeiro acesso para o CPF informado."
}
```

## Impacto no cadastro de usuario

Endpoint existente:
- `POST /api/usuarios/setUsuario`

Comportamento atual:
- cria usuario inativo;
- ignora senha inicial para login;
- dispara e-mail de primeiro acesso.

## Implementacao no Flutter (Dio)

### 1) Tela de primeiro acesso

Quando abrir a rota (ex.: `/primeiro-acesso?token=...`), extraia o token da URL e envie:

```dart
final response = await dio.post(
  '/api/auth/definir-primeira-senha',
  data: {
    'token': tokenDaUrl,
    'novaSenha': senhaDigitada,
  },
);
```

Tratamento:
- `200`: mostrar sucesso e redirecionar para login.
- `400`: mostrar mensagem de token expirado/invalido e botao de reenvio.

### 2) Reenvio quando token expirar

```dart
final response = await dio.post(
  '/api/auth/reenviar-primeiro-acesso',
  data: {
    'cpf': cpfInformadoPeloUsuario,
  },
);
```

Tratamento:
- `200`: avisar que novo link foi enviado por e-mail.
- `400`: avisar que nao foi possivel reenviar.

## Fluxo recomendado no app

1. Usuario abre link de primeiro acesso.
2. Digita nova senha.
3. Se sucesso, vai para login.
4. Se token expirado, mostra CTA "Reenviar link".
5. Usuario informa CPF e solicita novo link.

## Prompt pronto para implementar no Flutter

Copie e cole o prompt abaixo na sua ferramenta de IA para gerar a implementacao no app Flutter:

```text
Quero implementar no Flutter o fluxo de primeiro acesso da minha API.
Siga exatamente este passo a passo, com codigo completo e organizado:

Contexto de backend:
- Endpoint definir senha: POST /api/auth/definir-primeira-senha
  body: { "token": "string", "novaSenha": "string" }
  200: { "mensagem": "Senha definida com sucesso." }
  400: { "erro": "Token de primeiro acesso invalido, expirado ou ja utilizado." }

- Endpoint reenvio de link: POST /api/auth/reenviar-primeiro-acesso
  body: { "cpf": "string" }
  200: { "mensagem": "Link de primeiro acesso reenviado com sucesso." }
  400: { "erro": "Nao foi possivel reenviar o link de primeiro acesso para o CPF informado." }

Objetivo no Flutter:
1) Criar rota de primeiro acesso:
   - Exemplo: /primeiro-acesso?token=...
   - Ler token da URL (web).

2) Criar tela "PrimeiroAcessoPage":
   - Campos:
     - Nova senha
     - Confirmar senha
   - Validacoes:
     - minimo 8 caracteres
     - confirmar senha igual
   - Botao "Definir senha"
   - Estado de loading e erro amigavel

3) Implementar servico de API com Dio:
   - metodo definirPrimeiraSenha(token, novaSenha)
   - metodo reenviarPrimeiroAcesso(cpf)
   - retornar objetos tipados de sucesso/erro
   - tratar timeout e falha de rede

4) Fluxo de submissao da senha:
   - chamar definirPrimeiraSenha
   - em 200: mostrar snackbar/dialog de sucesso e redirecionar para /login
   - em 400: mostrar mensagem de token invalido/expirado e exibir secao de reenvio

5) Secao de reenvio de link:
   - campo CPF com mascara (###.###.###-##)
   - botao "Reenviar link"
   - em 200: feedback "novo link enviado"
   - em 400: feedback de erro
   - bloquear spam com cooldown local de 60s no botao

6) UX minima:
   - mensagens claras em portugues
   - loading em botoes
   - layout responsivo para web e mobile
   - nao usar codigo complexo desnecessario

7) Estrutura de arquivos sugerida:
   - lib/features/auth/data/auth_api.dart
   - lib/features/auth/models/first_access_models.dart
   - lib/features/auth/presentation/primeiro_acesso_page.dart
   - lib/features/auth/presentation/widgets/reenviar_link_section.dart
   - lib/core/network/dio_client.dart (se necessario)

8) Entregaveis:
   - codigo completo dos arquivos
   - trecho de configuracao de rotas
   - exemplo de uso no main.dart
   - checklist de testes manuais

9) Testes manuais que devem funcionar:
   - token valido define senha e redireciona para login
   - token expirado mostra secao de reenvio
   - reenvio com CPF valido retorna sucesso
   - erros de rede exibem mensagem amigavel
```

## Checklist de validacao no Flutter

- Rota de primeiro acesso carregando com `token` na URL.
- Tela valida senha e confirmacao antes de chamar API.
- Chamada `POST /api/auth/definir-primeira-senha` funcionando.
- Fluxo de sucesso redireciona para login.
- Fluxo de token expirado mostra area de reenvio.
- Chamada `POST /api/auth/reenviar-primeiro-acesso` funcionando.
- Cooldown de reenvio aplicado para evitar spam.
- Mensagens de erro/sucesso legiveis em portugues.
