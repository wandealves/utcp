# Weather Chat API

API desenvolvida em .NET 10 que integra um chatbot com OpenAI GPT-4o-mini e ferramentas customizadas via UTCP (Universal Tool Call Protocol) para consulta de previsão do tempo.

Esse projet é parte de [UTCP: um protocolo alternativo ao MCP para chamada de ferramentas](https://dev.to/wandealves/utcp-um-protocolo-alternativo-ao-mcp-para-chamada-de-ferramentas-5dnn)

## Funcionalidades

- **Chat inteligente**: Integração com OpenAI GPT-4o-mini para processamento de linguagem natural
- **Tool Calling**: Suporte a chamadas de ferramentas (function calling) da OpenAI
- **UTCP Service**: Execução de ferramentas via protocolo UTCP
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
  "OpenAISettings": {
    "ApiKey": "sua-chave-api-openai-aqui",
    "Model": "gpt-4o-mini"
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
API/
├── Endpoints/
│   ├── ChatEndpoint.cs          # Endpoint do chat
│   └── WeatherEndpoint.cs       # Endpoint de previsão do tempo
├── Services/
│   ├── ChatService.cs           # Serviço de integração com OpenAI
│   ├── ToolService.cs           # Definição de ferramentas (tools)
│   └── UtpcService.cs           # Serviço UTCP para executar ferramentas
├── Models/
│   └── OpenAISettings.cs        # Modelo de configuração da OpenAI
├── appsettings.json             # Configurações da aplicação
└── Program.cs                   # Configuração e inicialização
```

## Como Funciona

### Fluxo do Chat com Tools

1. **Usuário envia mensagem**: Via POST `/api/v1/chats`
2. **ChatService processa**: Envia para OpenAI com definição de ferramentas disponíveis
3. **OpenAI decide**: Se precisa usar uma ferramenta (tool call) ou responder diretamente
4. **Tool Call**: Se OpenAI solicitar, o UTCP Service executa a ferramenta
5. **Resultado**: O resultado da ferramenta é enviado de volta para OpenAI
6. **Resposta final**: OpenAI gera a resposta final com base nos dados da ferramenta

### Ferramentas Disponíveis

#### get_weather

Obtém a previsão do tempo de uma cidade.

**Parâmetros:**
- `city` (string): Nome da cidade

**Exemplo de uso pelo chat:**
```
Usuário: "Como está o tempo em Lisboa?"
Bot: [chama get_weather(city: "Lisboa")]
Bot: "Em Lisboa, a previsão para os próximos dias é..."
```

## Tecnologias Utilizadas

- **.NET 10.0**: Framework principal
- **OpenAI SDK (2.8.0)**: Integração com GPT-4o-mini
- **ASP.NET Core**: Web API
- **Swagger/OpenAPI**: Documentação da API
- **Dependency Injection**: Gerenciamento de dependências

## Desenvolvimento

### Adicionar nova ferramenta

1. Edite `Services/ToolService.cs` e adicione a definição da nova ferramenta
2. Edite `Services/UtpcService.cs` e implemente a lógica de execução
3. A ferramenta estará automaticamente disponível para o ChatService

### Customizar modelo OpenAI

Edite o arquivo `appsettings.json` e altere o valor de `Model`:

```json
"OpenAISettings": {
  "Model": "gpt-4o"  // ou qualquer modelo suportado
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

## Licença

MIT

## Contribuindo

Wanderson
