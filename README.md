# Weather Chat API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![OpenAI](https://img.shields.io/badge/OpenAI-GPT--4o--mini-412991?logo=openai)](https://openai.com/)
[![UTCP](https://img.shields.io/badge/UTCP-1.1.0-blue)](https://dev.to/wandealves/utcp-um-protocolo-alternativo-ao-mcp-para-chamada-de-ferramentas-5dnn)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

API desenvolvida em .NET 10 que integra um chatbot com OpenAI GPT-4o-mini e ferramentas customizadas via UTCP (Universal Tool Call Protocol) para consulta de previsão do tempo.

Este projeto é parte de [UTCP: um protocolo alternativo ao MCP para chamada de ferramentas](https://dev.to/wandealves/utcp-um-protocolo-alternativo-ao-mcp-para-chamada-de-ferramentas-5dnn)

## Índice

- [Quick Start](#quick-start)
- [O que é UTCP?](#o-que-é-utcp)
- [Funcionalidades](#funcionalidades)
- [Requisitos](#requisitos)
- [Configuração](#configuração)
- [Endpoints](#endpoints)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Como Funciona](#como-funciona)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Desenvolvimento](#desenvolvimento)
- [Casos de Uso](#casos-de-uso)
- [Testando a API](#testando-a-api)
- [Troubleshooting](#troubleshooting)
- [Recursos Adicionais](#recursos-adicionais)
- [Licença](#licença)
- [Autor](#autor)
- [Contribuindo](#contribuindo)

## Quick Start

```bash
# 1. Clone o repositório
git clone <url-do-repositorio>
cd Weather/API

# 2. Configure sua chave OpenAI no appsettings.json
# Edite o arquivo e adicione sua chave em "OpenAI:ApiKey"

# 3. Restaure as dependências
dotnet restore

# 4. Execute o projeto
dotnet run

# 5. Acesse a documentação Swagger
# https://localhost:7247/swagger

# 6. Teste o endpoint de descoberta UTCP
curl https://localhost:7247/api/v1/utcp

# 7. Teste o chatbot
curl -X POST https://localhost:7247/api/v1/chats \
  -H "Content-Type: application/json" \
  -d "\"Qual é a previsão do tempo para São Paulo?\""
```

## O que é UTCP?

UTCP (Universal Tool Call Protocol) é um protocolo para descoberta e execução de ferramentas (tools) em sistemas distribuídos. Ele permite que aplicações exponham suas funcionalidades como ferramentas que podem ser descobertas e chamadas por outras aplicações, incluindo LLMs (Large Language Models).

### Benefícios do UTCP

- **Descoberta Automática**: Ferramentas são descobertas dinamicamente via endpoint de discovery
- **Desacoplamento**: Aplicações cliente não precisam conhecer a implementação das ferramentas
- **Padronização**: Schema JSON define claramente entradas, saídas e como chamar cada ferramenta
- **Interoperabilidade**: Diferentes aplicações e LLMs podem consumir as mesmas ferramentas
- **Versionamento**: Suporte a múltiplas versões de ferramentas e do protocolo
- **Flexibilidade**: Suporta diferentes métodos HTTP, autenticação e templates de chamada
- **Metadados Ricos**: Cada ferramenta inclui descrição, tags e estimativa de tamanho de resposta

### Arquitetura Conceitual

```
┌─────────────────────┐
│   LLM (GPT-4o-mini) │
│                     │
│  + Chat Service     │
└──────────┬──────────┘
           │
           │ Function Calling
           ▼
┌─────────────────────┐
│  UTCP Service       │
│                     │
│  + Tool Execution   │
└──────────┬──────────┘
           │
           │ UTCP Protocol
           ▼
┌─────────────────────┐        ┌─────────────────────┐
│  Discovery Endpoint │◄───────│  UtcpToolClient     │
│                     │        │                     │
│  /api/v1/utcp       │        │  + Discovery        │
│                     │        │  + Tool Calling     │
│  Returns: Manual    │        └─────────────────────┘
│  - Tools List       │
│  - Schemas          │                 ▲
│  - Call Templates   │                 │
└─────────────────────┘                 │
           │                            │
           │ Provides Metadata          │ Consumes Tools
           ▼                            │
┌─────────────────────┐                 │
│  Weather API        │─────────────────┘
│                     │
│  + Forecast         │
│  + Current Weather  │
└─────────────────────┘
```

## Funcionalidades

- **Chat inteligente**: Integração com OpenAI GPT-4o-mini para processamento de linguagem natural
- **Tool Calling**: Suporte a chamadas de ferramentas (function calling) da OpenAI
- **UTCP Server**: Exposição de ferramentas via protocolo UTCP
- **UTCP Client**: Cliente para descobrir e consumir ferramentas UTCP de outras APIs
- **Discovery Endpoint**: Endpoint `/api/v1/utcp` que retorna o manual UTCP com todas as ferramentas disponíveis
- **Weather Forecast**: API de previsão do tempo por cidade
- **Swagger/OpenAPI**: Documentação interativa da API

## Requisitos

- .NET 10.0 SDK
- Chave de API da OpenAI
- Editor de código (Visual Studio, VS Code, Rider, etc.)

## Configuração

### 1. Clonar o repositório

```bash
git clone <url-do-repositorio>
cd Weather/API
```

### 2. Configurar appsettings.json

Edite o arquivo `appsettings.json` e adicione sua chave da OpenAI:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OpenAI": {
    "ApiKey": "sua-chave-api-openai-aqui",
    "Model": "gpt-4o-mini"
  },
  "UTCP": {
    "ManualUrl": "https://api.example.com/api/v1/utcp",
    "ApiKey": "chave-opcional-se-necessario"
  }
}
```

### 3. Restaurar dependências

```bash
dotnet restore
```

### 4. Executar o projeto

```bash
dotnet run
```

A API estará disponível em `https://localhost:7247`

## Endpoints

### UTCP Discovery

#### GET /api/v1/utcp

Retorna o manual UTCP com todas as ferramentas disponíveis para descoberta e integração.

**Response:**
```json
{
  "manualVersion": "1.0.0",
  "utcpVersion": "1.1.0",
  "name": "Weather API",
  "description": "API para informações e previsões meteorológicas",
  "tools": [
    {
      "name": "get_current_weather",
      "description": "Obter o clima atual para uma localização específica",
      "inputs": { ... },
      "output": { ... },
      "toolCallTemplate": { ... }
    },
    {
      "name": "get_forecast",
      "description": "Obter a previsão do tempo para os próximos dias",
      "inputs": { ... },
      "output": { ... },
      "toolCallTemplate": { ... }
    }
  ]
}
```

### Chat

#### POST /api/v1/chats

Envia uma mensagem para o chatbot que pode usar ferramentas para responder.

**Request Body:**
```json
"Qual é a previsão do tempo para São Paulo?"
```

**Response:**
```json
{
  "response": "A previsão do tempo para São Paulo nos próximos 5 dias é..."
}
```

### Weather Forecast

#### GET /api/v1/weatherforecast/{city}

Retorna a previsão do tempo para uma cidade específica.

**Parâmetros:**
- `city` (string): Nome da cidade

**Response:**
```json
[
  {
    "date": "2025-12-24",
    "temperatureC": 25,
    "temperatureF": 77,
    "summary": "Warm",
    "city": "São Paulo"
  }
]
```

## Documentação Interativa

Acesse a documentação Swagger em modo de desenvolvimento:

- **Swagger UI**: `https://localhost:7247/swagger`
- **OpenAPI JSON**: `https://localhost:7247/openapi/v1.json`

## Estrutura do Projeto

```
Weather/
└── API/
    ├── Endpoints/
    │   ├── ChatEndpoint.cs          # Endpoint do chat
    │   ├── WeatherEndpoint.cs       # Endpoint de previsão do tempo
    │   └── DiscoveryEndpoint.cs     # Endpoint de descoberta UTCP
    ├── Services/
    │   ├── ChatService.cs           # Serviço de integração com OpenAI
    │   ├── ToolService.cs           # Definição de ferramentas (tools)
    │   ├── UtpcService.cs           # Serviço UTCP para executar ferramentas
    │   └── UtcpToolClient.cs        # Cliente para consumir ferramentas UTCP
    ├── Models/
    │   ├── OpenAISettings.cs        # Configuração da OpenAI
    │   ├── UTCPSettings.cs          # Configuração do UTCP
    │   ├── UtcpManual.cs            # Modelo do manual UTCP
    │   ├── Tool.cs                  # Modelo de ferramenta
    │   ├── ToolInputSchema.cs       # Schema de entrada da ferramenta
    │   ├── ToolOutputSchema.cs      # Schema de saída da ferramenta
    │   ├── PropertySchema.cs        # Schema de propriedades
    │   ├── HttpCallTemplate.cs      # Template de chamada HTTP
    │   └── AuthConfig.cs            # Configuração de autenticação
    ├── appsettings.json             # Configurações da aplicação
    └── Program.cs                   # Configuração e inicialização
```

## Como Funciona

### Arquitetura UTCP

Este projeto implementa tanto o **servidor UTCP** quanto o **cliente UTCP**:

#### Servidor UTCP
- Expõe ferramentas através do endpoint `/api/v1/utcp`
- Retorna um manual JSON com todas as ferramentas disponíveis
- Cada ferramenta inclui: nome, descrição, schema de entrada/saída e template de chamada HTTP

#### Cliente UTCP (UtcpToolClient)
- Descobre ferramentas de outros servidores UTCP via `DiscoverToolsAsync()`
- Lista ferramentas disponíveis com `ListTools()`
- Executa chamadas a ferramentas remotas com `CallToolAsync()`
- Suporta autenticação via API Key
- Substitui variáveis de ambiente e parâmetros nos templates HTTP

### Fluxo do Chat com Tools

1. **Usuário envia mensagem**: Via POST `/api/v1/chats`
2. **ChatService processa**: Envia para OpenAI com definição de ferramentas disponíveis
3. **OpenAI decide**: Se precisa usar uma ferramenta (tool call) ou responder diretamente
4. **Tool Call**: Se OpenAI solicitar, o UtpcService executa a ferramenta
5. **Resultado**: O resultado da ferramenta é enviado de volta para OpenAI
6. **Resposta final**: OpenAI gera a resposta final com base nos dados da ferramenta

### Fluxo de Descoberta e Chamada UTCP

1. **Descoberta**: Cliente chama `GET /api/v1/utcp` para obter o manual
2. **Parsing**: Manual é parseado e ferramentas são catalogadas
3. **Seleção**: Cliente escolhe uma ferramenta pelo nome
4. **Preparação**: Parâmetros são substituídos no template HTTP
5. **Execução**: Request HTTP é montado e enviado
6. **Resposta**: Resultado é retornado ao cliente

### Ferramentas Disponíveis no Manual UTCP

#### get_current_weather

Obtém o clima atual para uma localização específica.

**Parâmetros:**
- `location` (string, obrigatório): Nome da cidade ou coordenadas
- `units` (string, opcional): "celsius" ou "fahrenheit"

#### get_forecast

Obtém a previsão do tempo para os próximos dias.

**Parâmetros:**
- `location` (string, obrigatório): Nome da cidade
- `days` (integer, obrigatório): Número de dias (1-7)

### Exemplo de Uso do Cliente UTCP

```csharp
// Criar cliente
var client = new UtcpToolClient();

// Descobrir ferramentas
var manual = await client.DiscoverToolsAsync("https://api.example.com/api/v1/utcp");

// Listar ferramentas disponíveis
var tools = client.ListTools();
foreach (var tool in tools)
{
    Console.WriteLine($"{tool.Name}: {tool.Description}");
}

// Chamar ferramenta
var parameters = new Dictionary<string, object>
{
    ["location"] = "São Paulo",
    ["units"] = "celsius"
};

var result = await client.CallToolAsync("get_current_weather", parameters);
Console.WriteLine($"Resposta: {result.Content}");
```

## Tecnologias Utilizadas

- **.NET 10.0**: Framework principal
- **OpenAI SDK (2.8.0)**: Integração com GPT-4o-mini
- **ASP.NET Core**: Web API
- **Swagger/OpenAPI**: Documentação da API
- **Dependency Injection**: Gerenciamento de dependências

## Desenvolvimento

### Adicionar nova ferramenta ao servidor UTCP

1. Edite `Endpoints/DiscoveryEndpoint.cs` e adicione a nova ferramenta ao manual UTCP
2. Defina o schema de entrada (`Inputs`) e saída (`Output`)
3. Configure o `HttpCallTemplate` com URL, método HTTP e parâmetros
4. Edite `Services/ToolService.cs` e adicione a definição da ferramenta para o OpenAI
5. Edite `Services/UtpcService.cs` e implemente a lógica de execução
6. A ferramenta estará automaticamente disponível tanto para o ChatService quanto para descoberta via `/api/v1/utcp`

### Consumir ferramentas UTCP de outras APIs

```csharp
// Configurar cliente com variáveis de ambiente (opcional)
var envVars = new Dictionary<string, string>
{
    ["API_KEY"] = "sua-chave-aqui"
};

using var client = new UtcpToolClient(envVars);

// Descobrir ferramentas
var manual = await client.DiscoverToolsAsync("https://api.example.com/api/v1/utcp");

// Chamar ferramenta
var result = await client.CallToolAsync("nome_da_ferramenta", new Dictionary<string, object>
{
    ["parametro"] = "valor"
});

if (result.Success)
{
    var data = result.GetContent<MinhaClasse>();
    // Processar dados
}
```

### Customizar modelo OpenAI

Edite o arquivo `appsettings.json` e altere o valor de `Model`:

```json
"OpenAI": {
  "ApiKey": "sua-chave-api-openai-aqui",
  "Model": "gpt-4o"  // ou qualquer modelo suportado
}
```

### Configurar UTCP

Edite o arquivo `appsettings.json` para configurar o UTCP:

```json
"UTCP": {
  "ManualUrl": "https://api.example.com/api/v1/utcp",
  "ApiKey": "chave-opcional-para-autenticacao"
}
```

## Troubleshooting

### Erro: "O nome do tipo ou do namespace 'OpenAI' não pode ser encontrado"

Execute:
```bash
dotnet restore
dotnet build
```

### Erro de conexão HTTPS

Em desenvolvimento, confie no certificado local:
```bash
dotnet dev-certs https --trust
```

## Casos de Uso

### 1. Chatbot com Ferramentas Meteorológicas
Use o endpoint `/api/v1/chats` para criar conversas naturais que automaticamente acessam dados de clima quando necessário.

```bash
curl -X POST https://localhost:7247/api/v1/chats \
  -H "Content-Type: application/json" \
  -d "\"Qual é a previsão do tempo para São Paulo?\""
```

### 2. Integração UTCP com Outras Aplicações
Outras aplicações podem descobrir e consumir as ferramentas meteorológicas:

```csharp
var client = new UtcpToolClient();
await client.DiscoverToolsAsync("https://localhost:7247/api/v1/utcp");
var result = await client.CallToolAsync("get_current_weather", new Dictionary<string, object>
{
    ["location"] = "Rio de Janeiro",
    ["units"] = "celsius"
});
```

### 3. Agregação de Múltiplas APIs UTCP
Combine ferramentas de diferentes APIs UTCP em uma única aplicação:

```csharp
// Descobrir ferramentas de clima
var weatherClient = new UtcpToolClient();
await weatherClient.DiscoverToolsAsync("https://weather-api.com/api/v1/utcp");

// Descobrir ferramentas de notícias
var newsClient = new UtcpToolClient();
await newsClient.DiscoverToolsAsync("https://news-api.com/api/v1/utcp");

// Usar ambas no mesmo contexto
var weather = await weatherClient.CallToolAsync("get_forecast", params1);
var news = await newsClient.CallToolAsync("get_news", params2);
```

## Testando a API

### Testar Discovery Endpoint

```bash
curl https://localhost:7247/api/v1/utcp
```

### Testar Weather Forecast

```bash
curl https://localhost:7247/api/v1/weatherforecast/Tokyo
```

### Testar Chat com Tool Calling

```bash
curl -X POST https://localhost:7247/api/v1/chats \
  -H "Content-Type: application/json" \
  -d "\"Como está o clima em Londres hoje?\""
```

## Recursos Adicionais

### Documentação e Artigos

- [UTCP: um protocolo alternativo ao MCP para chamada de ferramentas](https://dev.to/wandealves/utcp-um-protocolo-alternativo-ao-mcp-para-chamada-de-ferramentas-5dnn) - Artigo introdutório sobre UTCP
- [OpenAI Function Calling](https://platform.openai.com/docs/guides/function-calling) - Documentação oficial da OpenAI sobre chamadas de funções
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/) - Documentação oficial do .NET

### Projetos Relacionados

- **MCP (Model Context Protocol)**: Protocolo similar desenvolvido pela Anthropic
- **OpenAPI/Swagger**: Padrão de documentação de APIs REST

### Comunidade

- Issues e discussões no GitHub
- Contribuições são bem-vindas via Pull Requests

## Licença

MIT

## Autor

Desenvolvido por [Wanderson Alves](https://dev.to/wandealves)

## Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

### Como Contribuir

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request
