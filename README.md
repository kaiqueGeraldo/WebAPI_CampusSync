# API do Projeto CampusSync

Bem-vindo à documentação da API do **CampusSync**, um sistema de gerenciamento de faculdades, cursos, alunos, professores e mais. Esta API foi construída utilizando o framework **.NET** e serve como backend para o aplicativo Flutter.

## Índice

1. [Visão Geral](#visão-geral)
2. [Tecnologias Utilizadas](#tecnologias-utilizadas)
3. [Instalação](#instalação)
4. [Autenticação](#autenticação)
5. [EndPoints](#endpoints)
   - [Usuários](#usuários)
   - [Faculdades](#faculdades)
   - [Cursos](#cursos)
6. [Exemplos de Requisições](#exemplos-de-requisições)
7. [Testes e Qualidade de Código](#testes-e-qualidade-de-código)
8. [Licença](#licença)

---

## Visão Geral

A API **CampusSync** fornece funcionalidades para gerenciar faculdades, cursos, alunos, professores, turmas e matrículas. A API é projetada para ser consumida pelo aplicativo **CampusSync** em Flutter e pode ser utilizada para operações CRUD (Criar, Ler, Atualizar, Deletar) nas entidades do sistema.

---

## Tecnologias Utilizadas

- **.NET 8**: Framework utilizado para desenvolver a API.
- **Entity Framework Core**: ORM para interagir com o banco de dados.
- **SQL Server**: Banco de dados relacional para armazenar as informações.
- **JWT (JSON Web Tokens)**: Utilizado para autenticação de usuários.

---

## Instalação

### Passo a passo:

1. Clone o repositório:

  ```bash
   git clone https://github.com/kaiqueGeraldo/WebAPI_CampusSync.git
  ```

2. Navegue até o diretório do projeto:
  ```bash
   cd campussync-api
  ```

3. Instale as dependências do projeto:

  ```bash
   dotnet restore
  ```

4. Configure a string de conexão no arquivo appsettings.json.

5.Execute a API:
 
  ```bash
   dotnet run
  ```

A API estará rodando em http://localhost:5000 (ou a URL configurada no seu ambiente).

---

## Autenticação

A API utiliza JWT para autenticação. Para obter um token de acesso, faça uma requisição POST para o endpoint /api/Users/login com as credenciais do usuário.

Exemplo de Requisição:

```bash
POST /api/Users/login
{
  "cpf": "12345678901",
  "senha": "senha123"
}
```
A resposta incluirá um token JWT que deve ser incluído nos headers de todas as requisições subsequentes como:

```bash
Authorization: Bearer <seu_token_jwt>
```

---
## EndPoints

**Usuários**

**POST /api/Users/login**

Autentica um usuário e retorna um token JWT.

**Request body:**

```bash
{
  "cpf": "12345678901",
  "senha": "senha123"
}
```
Resposta:

```bash
{
  "token": "seu_token_jwt_aqui"
}
```

**GET /api/Users/{cpf}**

Recupera as informações do usuário.

**Parâmetros:**

cpf: O CPF do usuário.

**Resposta:**

```bash
{
  "cpf": "12345678901",
  "nome": "João Silva",
  "email": "joao.silva@example.com"
}
```

**Faculdades**

**GET /api/Faculdades**

Lista todas as faculdades.

**Resposta:**

```bash
[
  {
    "id": 1,
    "nome": "Faculdade de Tecnologia"
  },
  {
    "id": 2,
    "nome": "Universidade Federal"
  }
]
```

**POST /api/Faculdades**

Cria uma nova faculdade.

**Request body:**

```bash
{
  "nome": "Nova Faculdade"
}
```
**Resposta:**

```bash
{
  "id": 3,
  "nome": "Nova Faculdade"
}
```

**Cursos**

**POST /api/Cursos**

Cria um novo curso e associa a uma faculdade.

**Request body:**

```bash
{
  "nome": "Curso de Engenharia",
  "idFaculdade": 1
}
```

**Resposta:**

```bash
{
  "id": 1,
  "nome": "Curso de Engenharia",
  "idFaculdade": 1
}
```
---

## Exemplos de Requisições

**Autenticação**

```bash
curl -X POST http://localhost:5000/api/Users/login -H "Content-Type: application/json" -d '{"cpf":"12345678901","senha":"senha123"}'
```

**Obter Informações do Usuário**

```bash
curl -X GET http://localhost:5000/api/Users/12345678901 -H "Authorization: Bearer seu_token_jwt_aqui"
```

**Testes e Qualidade de Código**

A API é testada utilizando o xUnit. Para executar os testes, basta rodar o seguinte comando:

```bash
dotnet test
```

## Licença
Este projeto está licenciado sob a MIT License. Consulte o arquivo LICENSE para mais detalhes.
 
