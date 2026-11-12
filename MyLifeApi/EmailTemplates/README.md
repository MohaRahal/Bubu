# Templates de e-mail

Os arquivos HTML desta pasta são enviados pela API da Brevo. O `_layout.html` contém a estrutura visual compartilhada e `{{Content}}` recebe o conteúdo do template específico.

Variáveis disponíveis:

- `wallet-income.html` e `wallet-expense.html`: `UserName`, `AccountName`, `Description`, `Amount`, `TransactionDate`, `MascotUrl`, `BrandIconUrl`, `PreviewText` e `CurrentYear`.
- `password-changed.html`: `UserName`, `ChangedAt`, `MascotUrl`, `BrandIconUrl`, `PreviewText` e `CurrentYear`.
- `password-reset-code.html`: `UserName`, `Code`, `LifetimeMinutes`, `MascotUrl`, `BrandIconUrl`, `PreviewText` e `CurrentYear`.

Os valores dinâmicos são codificados como HTML pelo backend. Para adicionar um novo template, crie o HTML nesta pasta e informe todas as variáveis ao chamar o renderizador.

Para gerar prévias locais completas com dados fictícios, execute:

```powershell
.\generate-previews.ps1
```

Os arquivos serão criados em `Previews`. Essa pasta é apenas para conferência e não é incluída na publicação da API.
