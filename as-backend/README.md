# API de Gerenciamento de Usuários

## Descrição

API REST para gerenciamento de usuários implementada em ASP.NET Core (Minimal APIs) seguindo Clean Architecture. Permite operações CRUD, validações com FluentValidation, persistência com Entity Framework Core + SQLite, e patterns Repository e Service.

## Tecnologias Utilizadas

- .NET 8.0
- C# 12
- ASP.NET Core Minimal APIs
- Entity Framework Core 8 + SQLite
- FluentValidation 11.3

## Como Executar o Projeto

### Pré-requisitos

- .NET SDK 8.0 ou superior

### Passos

1. Clone o repositório

```bash
git clone https://github.com/<seu-usuario>/api-usuarios-as-murillo-menini.git
cd api-usuarios-as-murillo-menini/APIUsuarios
```

2. Restaurar pacotes e aplicar migrations

```bash
dotnet restore
dotnet tool install --global dotnet-ef --version 8.0.0
dotnet ef migrations add InitialCreate
dotnet ef database update
```

3. Executar a aplicação

```bash
dotnet run
```

A API estará disponível em `http://localhost:5000` (ou porta indicada no console). Use Swagger em `/swagger`.
