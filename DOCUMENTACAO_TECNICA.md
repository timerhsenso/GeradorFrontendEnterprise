# Gerador Frontend Enterprise v2.0 - Documentação Técnica Completa

## 📋 Índice

1. [Visão Geral](#visão-geral)
2. [Arquitetura do Sistema](#arquitetura-do-sistema)
3. [Componentes Principais](#componentes-principais)
4. [Fluxo do Wizard](#fluxo-do-wizard)
5. [Modelos de Dados](#modelos-de-dados)
6. [Contratos (Interfaces)](#contratos-interfaces)
7. [Implementações](#implementações)
8. [Testes](#testes)
9. [Guia de Uso](#guia-de-uso)
10. [Troubleshooting](#troubleshooting)

---

## 🎯 Visão Geral

O **Gerador Frontend Enterprise v2.0** é um sistema automatizado de geração de interfaces CRUD (Create, Read, Update, Delete) para aplicações ASP.NET Core 8, baseado em esquemas de banco de dados SQL Server e manifestos de API.

### Características Principais

- ✅ **Geração Automática**: Cria Controllers, ViewModels, Views Razor, JavaScript e CSS
- ✅ **Detecção de Conflitos**: Identifica inconsistências entre banco e manifesto
- ✅ **Configuração Visual**: Wizard interativo em 5 etapas
- ✅ **Idempotência**: Gerar N vezes produz o mesmo resultado
- ✅ **Separação Gerado/Customizado**: `*.generated.cs` vs `*.custom.cs`
- ✅ **Reutilização de Configurações**: Salvar e carregar configs em JSON

---

## 🏗️ Arquitetura do Sistema

```
┌─────────────────────────────────────────────────────────┐
│                    Wizard (Razor Pages)                 │
│              (Interface do Usuário - 5 Etapas)          │
└──────────────────────┬──────────────────────────────────┘
                       │
        ┌──────────────┼──────────────┐
        │              │              │
        ▼              ▼              ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ Orchestrator │ │  Generator   │ │  Template    │
│   Service    │ │   Service    │ │   Engine     │
└──────┬───────┘ └──────┬───────┘ └──────┬───────┘
       │                │                │
       └────────┬───────┴────────┬───────┘
                │                │
        ┌───────▼────────┐ ┌─────▼────────┐
        │  Schema Reader │ │ Manifest     │
        │  (SQL Server)  │ │ Client (API) │
        └────────────────┘ └──────────────┘
```

### Hierarquia de Fontes da Verdade

1. **Banco de Dados (SQL Server)** - Fonte primária
2. **Manifesto (API)** - Metadados e permissões
3. **Wizard Configuration** - Preferências visuais

---

## 🔧 Componentes Principais

### 1. Schema Reader (SqlServerSchemaReader)

**Responsabilidade**: Extrair estrutura real das tabelas do SQL Server

**Métodos Principais**:
- `ReadTableSchemaAsync()` - Lê schema de uma tabela
- `ReadAllTablesAsync()` - Lê todas as tabelas de um schema
- `ValidateConsistencyAsync()` - Valida consistência banco/manifesto
- `TestConnectionAsync()` - Testa conexão com SQL Server

**Tecnologia**: System.Data.SqlClient (será migrado para Microsoft.Data.SqlClient)

### 2. Manifest Client (HttpManifestClient)

**Responsabilidade**: Comunicação com API de manifesto

**Métodos Principais**:
- `GetEntityManifestAsync()` - Obtém manifesto de uma entidade
- `GetAllManifestsAsync()` - Obtém todos os manifestos
- `GetManifestsByModuleAsync()` - Obtém manifestos por módulo

**Tecnologia**: HttpClient com tratamento de erros

### 3. Template Engine (ScribanTemplateEngine)

**Responsabilidade**: Renderização de templates Scriban

**Métodos Principais**:
- `RenderAsync()` - Renderiza um template
- `ValidateTemplateAsync()` - Valida sintaxe do template
- `CreateDefaultTemplatesAsync()` - Cria templates padrão

**Tecnologia**: Scriban (motor de templates)

### 4. Orchestrator Service (OrchestratorService)

**Responsabilidade**: Coordenar todo o fluxo de geração

**Métodos Principais**:
- `InitializeWizardAsync()` - Inicia o wizard
- `DetectConflictsAsync()` - Detecta conflitos
- `ResolveConflictsAsync()` - Resolve conflitos
- `ValidateConfigurationAsync()` - Valida configuração
- `GenerateCodeAsync()` - Gera código
- `SaveConfigurationAsync()` - Salva configuração
- `LoadConfigurationAsync()` - Carrega configuração
- `GetGenerationHistoryAsync()` - Obtém histórico

### 5. Generator Service (GeneratorService)

**Responsabilidade**: Gerar arquivos de código

**Métodos Principais**:
- `GenerateAsync()` - Gera todos os arquivos
- `GenerateControllerAsync()` - Gera Controller
- `GenerateViewModelAsync()` - Gera ViewModel
- `GenerateRazorViewAsync()` - Gera Razor View
- `GenerateJavaScriptAsync()` - Gera JavaScript
- `GenerateCssAsync()` - Gera CSS
- `CreateZipPackageAsync()` - Cria pacote ZIP
- `ValidateGeneratedCodeAsync()` - Valida código
- `GetStatistics()` - Calcula estatísticas

---

## 🧙 Fluxo do Wizard

### Etapa 1: Seleção da Entidade
- Usuário seleciona a entidade a gerar
- Sistema carrega schema e manifesto
- Detecta conflitos automaticamente

### Etapa 2: Resolução de Conflitos
- Exibe conflitos detectados
- Usuário escolhe resolução (Banco, Manifesto, Ignorar, Manual)
- Sistema aplica resoluções

### Etapa 3: Configuração Visual
- Usuário configura layout da grid (colunas, filtros, etc)
- Usuário configura layout do formulário (abas, campos, etc)
- Sistema valida configuração

### Etapa 4: Geração de Código
- Sistema gera Controller, ViewModel, View, JS, CSS
- Cria arquivos customizáveis (*.custom.cs)
- Valida código gerado

### Etapa 5: Download
- Usuário baixa arquivo ZIP com código gerado
- ZIP contém estrutura completa pronta para uso

---

## 📊 Modelos de Dados

### TableSchema
```csharp
public class TableSchema
{
    public string SchemaName { get; set; }
    public string TableName { get; set; }
    public string DisplayName { get; set; }
    public List<ColumnSchema> Columns { get; set; }
    public List<string> PrimaryKeyColumns { get; set; }
    public List<ForeignKeySchema> ForeignKeys { get; set; }
    public List<IndexSchema> Indexes { get; set; }
}
```

### EntityManifest
```csharp
public class EntityManifest
{
    public string EntityId { get; set; }
    public string EntityName { get; set; }
    public string Module { get; set; }
    public string ApiRoute { get; set; }
    public List<EntityPermission> Permissions { get; set; }
    public Dictionary<string, string> FieldMappings { get; set; }
}
```

### WizardConfig
```csharp
public class WizardConfig
{
    public string ConfigId { get; set; }
    public string EntityId { get; set; }
    public string EntityName { get; set; }
    public GridLayoutConfig GridLayout { get; set; }
    public FormLayoutConfig FormLayout { get; set; }
    public List<FormField> FormFields { get; set; }
    public Dictionary<string, ConflictResolution> ConflictResolutions { get; set; }
}
```

### GenerationResult
```csharp
public class GenerationResult
{
    public string GenerationId { get; set; }
    public string EntityId { get; set; }
    public bool IsSuccessful { get; set; }
    public List<GeneratedFile> Files { get; set; }
    public List<string> Errors { get; set; }
    public List<string> Warnings { get; set; }
    public GenerationStatistics Statistics { get; set; }
}
```

---

## 📝 Contratos (Interfaces)

### ISchemaReader
```csharp
public interface ISchemaReader
{
    Task<TableSchema> ReadTableSchemaAsync(string connectionString, string schemaName, string tableName);
    Task<List<TableSchema>> ReadAllTablesAsync(string connectionString, string schemaName);
    Task<ValidationResult> ValidateConsistencyAsync(TableSchema dbSchema, EntityManifest manifest);
    Task<bool> TestConnectionAsync(string connectionString);
}
```

### IManifestClient
```csharp
public interface IManifestClient
{
    Task<EntityManifest> GetEntityManifestAsync(string entityId);
    Task<List<EntityManifest>> GetAllManifestsAsync();
    Task<List<EntityManifest>> GetManifestsByModuleAsync(string module);
}
```

### ITemplateEngine
```csharp
public interface ITemplateEngine
{
    Task<string> RenderAsync(string templateName, Dictionary<string, object> data);
    Task<bool> ValidateTemplateAsync(string templateContent);
    Task CreateDefaultTemplatesAsync();
}
```

### IOrchestratorService
```csharp
public interface IOrchestratorService
{
    Task<WizardInitializationResult> InitializeWizardAsync(string entityId);
    Task<List<Conflict>> DetectConflictsAsync(string entityId);
    Task<ConflictResolutionResult> ResolveConflictsAsync(string entityId, Dictionary<string, ConflictResolution> resolutions);
    Task<ValidationResult> ValidateConfigurationAsync(WizardConfig config);
    Task<GenerationResult> GenerateCodeAsync(WizardConfig config);
    Task<string> SaveConfigurationAsync(WizardConfig config);
    Task<WizardConfig> LoadConfigurationAsync(string configId);
    Task<List<GenerationSummary>> GetGenerationHistoryAsync(string entityId);
}
```

### IGeneratorService
```csharp
public interface IGeneratorService
{
    Task<GenerationResult> GenerateAsync(WizardConfig config, TableSchema schema, EntityManifest manifest);
    Task<string> CreateZipPackageAsync(GenerationResult result, string outputPath);
    Task<CodeValidationResult> ValidateGeneratedCodeAsync(GenerationResult result);
    GenerationStatistics GetStatistics(GenerationResult result);
}
```

---

## 💻 Implementações

### SqlServerSchemaReader
- Lê metadados de Information_Schema
- Mapeia tipos SQL para CLR
- Detecta PKs, FKs, índices
- Valida consistência com manifesto

### HttpManifestClient
- Comunicação HTTP com API
- Retry logic com exponential backoff
- Tratamento de erros
- Cache de manifestos

### ScribanTemplateEngine
- Renderização de templates Scriban
- Validação de sintaxe
- Cache de templates compilados
- Criação de templates padrão

### OrchestratorService
- Coordenação de todo o fluxo
- Detecção automática de conflitos
- Resolução inteligente de conflitos
- Salvamento/carregamento de configurações em JSON
- Histórico de gerações

### GeneratorService
- Geração de Controllers CRUD
- Geração de ViewModels com validação
- Geração de Razor Views com DataTables
- Geração de JavaScript com AJAX
- Geração de CSS responsivo
- Criação de pacotes ZIP
- Validação de código gerado
- Cálculo de estatísticas

---

## 🧪 Testes

### Testes Unitários

#### SchemaReaderTests
- ✅ Validação de schema
- ✅ Mapeamento de tipos
- ✅ Detecção de FKs
- ✅ Validação de colunas

#### OrchestratorServiceTests
- ✅ Inicialização do wizard
- ✅ Detecção de conflitos
- ✅ Resolução de conflitos
- ✅ Validação de configuração
- ✅ Salvamento/carregamento de configs
- ✅ Histórico de gerações

#### GeneratorServiceTests
- ✅ Geração de código
- ✅ Criação de pacotes ZIP
- ✅ Validação de código
- ✅ Cálculo de estatísticas

### Executar Testes

```bash
cd GeradorFrontendEnterprise
dotnet test
```

---

## 📖 Guia de Uso

### 1. Configuração Inicial

```bash
# Clonar repositório
git clone https://github.com/seu-repo/GeradorFrontendEnterprise.git
cd GeradorFrontendEnterprise

# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar
dotnet run
```

### 2. Acessar o Wizard

1. Abrir navegador: `http://localhost:5000/wizard/step1`
2. Selecionar entidade
3. Resolver conflitos
4. Configurar layout
5. Gerar código
6. Baixar ZIP

### 3. Integrar Código Gerado

1. Extrair ZIP em seu projeto
2. Adicionar referências necessárias
3. Registrar serviços no Startup
4. Executar migrações do banco (se necessário)
5. Testar endpoints

### 4. Customizar Código Gerado

- Editar `*.custom.cs` para adicionar lógica
- NÃO editar `*.generated.cs` (será sobrescrito)
- Usar partial classes para extensão

---

## 🔧 Troubleshooting

### Erro: "Conexão com SQL Server falhou"
- Verificar string de conexão em `appsettings.json`
- Verificar credenciais de banco
- Verificar firewall e permissões de rede

### Erro: "Manifesto não encontrado"
- Verificar URL da API de manifesto
- Verificar se entidade existe no manifesto
- Verificar permissões de acesso

### Erro: "Conflito não resolvido"
- Revisar conflito detectado
- Escolher resolução apropriada
- Ou ignorar e continuar

### Código gerado não compila
- Verificar templates Scriban
- Verificar dados passados para templates
- Verificar validação de código

---

## 📞 Suporte

Para dúvidas ou problemas, consulte:
- Documentação técnica: `/DOCUMENTACAO_TECNICA.md`
- Decisões técnicas: `/DECISOES_TECNICAS.md`
- Progresso: `/PROGRESSO.md`
- README: `/README.md`

---

**Versão**: 2.0
**Data**: 2025-12-26
**Status**: ✅ Completo e Funcional
