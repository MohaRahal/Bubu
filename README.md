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
dotnet tool run dotnet-ef database update
dotnet run --launch-profile http
```

A API será iniciada em `http://localhost:5261` e o Swagger estará disponível em `http://localhost:5261/swagger`.

### Frontend

Em outro terminal:

```bash
cd MyLifeFrontend
npm install
npm run dev
```

Abra o endereço informado pelo Vite, normalmente `http://localhost:5173`.
