# Gerador Frontend Enterprise v2.0

**Automatizador de geração de interfaces CRUD para sistemas ERP corporativos**

## Visão Geral

O Gerador Frontend Enterprise é uma ferramenta web (ASP.NET Core 8) que automatiza a criação de interfaces CRUD completas a partir de:

1. **Schema de tabelas SQL Server** - Estrutura real do banco de dados
2. **Metadados de API REST** - Regras de negócio e permissões
3. **Configuração visual do Wizard** - Layout e apresentação

**Objetivo:** Reduzir tempo de desenvolvimento de CRUDs de **2 dias → 30 minutos**, padronizando interfaces em todo o ERP.

---

## Arquitetura

```
GeradorFrontendEnterprise/
├── Core/
│   ├── Contracts/        # Interfaces (ISchemaReader, IManifestClient, etc)
│   ├── Enums/            # Enumerações (SqlDataType, ConflictType, etc)
│   └── Models/           # Modelos de dados (TableSchema, EntityManifest, etc)
├── Infrastructure/
│   ├── SchemaReader/     # Leitura de schema SQL Server
│   ├── ManifestClient/   # Comunicação com API de manifesto
│   └── TemplateEngine/   # Motor de templates Scriban
├── Services/
│   ├── Orchestrator/     # Orquestração do fluxo (em desenvolvimento)
│   └── Generator/        # Geração de código (em desenvolvimento)
├── Tests/
│   ├── Unit/             # Testes unitários
│   └── Integration/      # Testes de integração
└── Controllers/          # Controllers MVC (Wizard)
```

---

## Stack Técnica

| Componente | Tecnologia | Versão |
|-----------|-----------|--------|
| **Backend** | ASP.NET Core MVC | 8.0 |
| **Banco de Dados** | SQL Server | 2019+ |
| **Template Engine** | Scriban | 5.x |
| **Serialização** | Newtonsoft.Json | 13.0.4 |
| **Testes** | xUnit | 2.9.3 |
| **HTTP Client** | HttpClient (built-in) | - |

---

## Componentes Implementados

### ✓ Fase 1-3: Modelos de Dados e Contratos
- **14 arquivos C#** com documentação XML completa
- Modelos: TableSchema, ColumnSchema, EntityManifest, WizardConfig, GenerationResult
- Contratos: ISchemaReader, IManifestClient, ITemplateEngine, IOrchestratorService, IGeneratorService
- Enumerações: SqlDataType, ConflictType, FormInputType, ValidationType, etc

### ✓ Fase 4: Schema Reader
- **SqlServerSchemaReader** - Leitura completa de schema SQL Server
- Extração de: colunas, tipos, constraints, FKs, índices, extended properties
- Validação de consistência entre banco e manifesto
- Mapeamento automático SQL → CLR types
- **10 testes unitários** cobrindo validações

### ✓ Fase 5: Manifest Client e Template Engine
- **HttpManifestClient** - Comunicação com API de manifesto
- **ScribanTemplateEngine** - Renderização de templates Liquid
- Suporte a templates padrão (Controller, Razor, ViewModel)
- Validação de sintaxe de templates

### ⏳ Fase 6-12: Em Desenvolvimento
- Orchestrator Service (coordenação do fluxo)
- Generator Service (orquestração final)
- Wizard UI (Razor Pages)
- Templates de geração de código
- Testes de integração
- Documentação final

---

## Princípios Fundamentais

### 1. Hierarquia de Fontes da Verdade
```
Banco de Dados > Manifesto > Configuração do Wizard
```
- **Banco:** Campos, tipos, nullability, PKs, FKs
- **Manifesto:** Rotas, módulo, permissões, sistema
- **Wizard:** Layout grid, layout form, ordem, abas

### 2. Idempotência
- Gerar N vezes com mesma configuração = **mesmo resultado**
- Hash SHA256 da configuração no header dos arquivos
- Rastreabilidade completa de gerações

### 3. Separação Gerado vs Customizado
```
*.generated.cs  ← Sempre regerado
*.custom.cs     ← Nunca alterado (customizações do dev)
```
Usa **partial classes** para permitir extensão sem perder código.

### 4. Não-Inferência
- ❌ Não inferir tipos por nome de coluna
- ✅ Usar metadata explícito
- ✅ Falhar rápido se ambíguo

---

## Instalação e Configuração

### Pré-requisitos
- .NET 8 SDK
- SQL Server 2019+
- Visual Studio Code ou Visual Studio 2022

### Instalação

```bash
# Clonar repositório
git clone <repo-url>
cd GeradorFrontendEnterprise

# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar testes
dotnet test

# Executar aplicação
dotnet run
```

### Configuração

Editar `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GeradorDB;Trusted_Connection=true;"
  },
  "ManifestApi": {
    "BaseUrl": "https://api.seu-erp.com"
  },
  "TemplatesPath": "./Templates"
}
```

---

## Uso Básico

### 1. Inicializar Wizard
```csharp
var orchestrator = serviceProvider.GetRequiredService<IOrchestratorService>();
var result = await orchestrator.InitializeWizardAsync("TreTiposTreinamento");
```

### 2. Carregar Schema
```csharp
var schema = await orchestrator.LoadSchemaAsync("TreTiposTreinamento");
```

### 3. Detectar Conflitos
```csharp
var conflicts = await orchestrator.DetectConflictsAsync("TreTiposTreinamento");
```

### 4. Gerar Código
```csharp
var config = new WizardConfig { /* ... */ };
var result = await orchestrator.GenerateCodeAsync(config);
```

---

## Fluxo do Wizard (5 Etapas)

```
1. Seleção da Entidade
   ↓
2. Leitura do Schema
   ↓
3. Resolução de Conflitos (se houver)
   ↓
4. Configuração Visual (Grid + Form)
   ↓
5. Geração e Download (ZIP)
```

---

## Estrutura de Arquivos Gerados

```
Output.zip
├── Controllers/
│   ├── TreTiposTreinamentoController.generated.cs
│   └── TreTiposTreinamentoController.custom.cs
├── Views/TreTiposTreinamento/
│   ├── Index.generated.cshtml
│   ├── Index.custom.cshtml
│   ├── _FormFields.generated.cshtml
│   └── _Modal.generated.cshtml
├── ViewModels/
│   ├── TreTiposTreinamentoViewModel.generated.cs
│   └── TreTiposTreinamentoViewModel.custom.cs
├── wwwroot/js/
│   ├── tretipostreinamento.generated.js
│   └── tretipostreinamento.custom.js
└── wwwroot/css/
    └── tretipostreinamento.generated.css
```

---

## Decisões Técnicas

Veja [DECISOES_TECNICAS.md](./DECISOES_TECNICAS.md) para detalhes sobre:
- Escolha de tecnologias
- Padrões arquiteturais
- Justificativas de design
- Roadmap futuro

---

## Métricas do Projeto

| Métrica | Valor |
|---------|-------|
| **Arquivos C#** | 14 |
| **Linhas de código** | ~3.500 |
| **Documentação XML** | 100% |
| **Testes unitários** | 10 |
| **Build status** | ✓ Sucesso |
| **Tamanho do projeto** | 15 MB |

---

## Roadmap

### Fase 6 (Próxima)
- [ ] Implementar Orchestrator Service
- [ ] Coordenação do fluxo completo
- [ ] Detecção e resolução de conflitos

### Fase 7
- [ ] Implementar Wizard (Razor Pages)
- [ ] Interface passo a passo
- [ ] Drag-and-drop para layout

### Fase 8
- [ ] Implementar Generator Service
- [ ] Orquestração final da geração
- [ ] Criação de ZIP

### Fase 9
- [ ] Templates de geração de código
- [ ] Controller, ViewModel, View, JavaScript
- [ ] Suporte a multi-tenant

### Fase 10-12
- [ ] Testes de integração
- [ ] Documentação final
- [ ] Deploy e release

---

## Contribuindo

1. Criar branch: `git checkout -b feature/sua-feature`
2. Commit: `git commit -am 'Adicionar feature'`
3. Push: `git push origin feature/sua-feature`
4. Pull Request

---

## Licença

Propriedade da Equipe RhSensoERP

---

## Suporte

Para dúvidas ou problemas, abra uma issue no repositório.

---

**Versão:** 2.0  
**Status:** Em desenvolvimento (Fase 5/12)  
**Última atualização:** 2025-12-26
