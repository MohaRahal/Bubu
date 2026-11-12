# Bubu

Bubu é uma aplicação de organização pessoal para acompanhar rotina, objetivos e finanças em um único lugar.

O projeto está em desenvolvimento. Atualmente possui landing page, cadastro, login com JWT, dashboard e dados de atividades, metas e movimentações financeiras.

## Tecnologias

- Frontend: React, Vite, CSS Modules, Rive e Recharts
- Backend: ASP.NET Core 8 e Entity Framework Core
- Banco de dados: MySQL
- Autenticação: JWT armazenado em cookie HttpOnly

## Estrutura

```text
MyLife/
├── MyLifeApi/       # API e acesso ao banco
└── MyLifeFrontend/  # Interface web
```

## Como executar

Você precisa ter o .NET 8, Node.js e MySQL instalados.

### Backend

```bash
cd MyLifeApi
dotnet restore
dotnet tool restore
dotnet user-secrets set "ConnectionStrings:MyLifeDatabase" "Server=localhost;Database=mylife;User=root;Password=SUA_SENHA"
dotnet user-secrets set "Jwt:Key" "UMA_CHAVE_SECRETA_FORTE_COM_PELO_MENOS_32_CARACTERES"
dotnet user-secrets set "Brevo:ApiKey" "SUA_CHAVE_DA_API_BREVO"
dotnet user-secrets set "Brevo:SenderEmail" "EMAIL_REMETENTE_VERIFICADO_NA_BREVO"
dotnet user-secrets set "Brevo:AssetsBaseUrl" "https://URL_PUBLICA_DA_SUA_API"
dotnet user-secrets set "Brevo:Enabled" "true"
dotnet user-secrets set "PasswordReset:HashKey" "UMA_CHAVE_ALEATORIA_EXCLUSIVA_COM_PELO_MENOS_32_CARACTERES"
dotnet tool run dotnet-ef database update
dotnet run --launch-profile http
```

A API será iniciada em `http://localhost:5261` e o Swagger estará disponível em `http://localhost:5261/swagger`.

O perfil HTTP escuta em todas as interfaces da máquina. Para acessar o frontend a partir de outro dispositivo na mesma rede, mantenha API e Vite executando na máquina anfitriã e abra `http://<IP-DA-MAQUINA>:5173`. O Vite encaminha `/api` e `/avatars` para a API local; libere as portas 5173 e, se precisar acessar a API/Swagger diretamente, 5261 no firewall do Windows. O cookie de autenticação é emitido para o endereço usado no navegador, então faça login nesse mesmo endereço.

O remetente precisa estar verificado na Brevo. A chave da API fica em user secrets no desenvolvimento e deve ser fornecida pela variável de ambiente `Brevo__ApiKey` em produção; ela não deve ser adicionada ao `appsettings.json` nem versionada. `Brevo:AssetsBaseUrl` deve apontar para a URL pública da API, sem `/` no final, para que os mascotes em `wwwroot/email-assets` apareçam nos e-mails. Os HTMLs enviados ficam em `MyLifeApi/EmailTemplates`.

A recuperação de senha utiliza um código de seis dígitos com validade e limite de tentativas configuráveis na seção `PasswordReset`. Apenas o hash HMAC do código é persistido. Em produção, forneça a chave exclusiva pela variável `PasswordReset__HashKey` e nunca reutilize a chave JWT.

### Frontend

Em outro terminal:

```bash
cd MyLifeFrontend
npm install
npm run dev
```

Abra o endereço informado pelo Vite, normalmente `http://localhost:5173`.
