# 🚀 Guia Rápido - Gerador Frontend Enterprise v2.0

## ⚡ Início Rápido (5 minutos)

### 1. Compilar o Projeto
```bash
cd GeradorFrontendEnterprise
dotnet build
```

### 2. Executar a Aplicação
```bash
dotnet run
```

### 3. Acessar o Wizard
Abrir navegador em: `http://localhost:5000/wizard/step1`

### 4. Seguir as 5 Etapas
1. **Passo 1**: Selecione a entidade
2. **Passo 2**: Resolva conflitos (se houver)
3. **Passo 3**: Configure o layout visual
4. **Passo 4**: Gere o código
5. **Passo 5**: Baixe o arquivo ZIP

## 📁 Estrutura do Projeto

```
GeradorFrontendEnterprise/
├── Core/                          # Modelos e contratos
│   ├── Enums/                     # Enumerações
│   ├── Models/                    # Modelos de dados
│   └── Contracts/                 # Interfaces
├── Infrastructure/                # Implementações
│   ├── SchemaReader/              # Leitura de SQL Server
│   ├── ManifestClient/            # Cliente HTTP
│   └── TemplateEngine/            # Motor Scriban
├── Services/                      # Serviços
│   ├── Orchestrator/              # Orquestração
│   └── Generator/                 # Geração de código
├── Controllers/                   # Controllers MVC
├── Views/                         # Views Razor
├── Templates/                     # Templates Scriban
├── Tests/                         # Testes unitários
└── README.md                      # Documentação
```

## 🔧 Configuração

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=YourDB;User Id=sa;Password=YourPassword;"
  },
  "ManifestApi": {
    "BaseUrl": "https://api.example.com",
    "ApiKey": "your-api-key"
  }
}
```

## 📊 Componentes Principais

| Componente | Responsabilidade |
|-----------|-----------------|
| **SchemaReader** | Lê estrutura do SQL Server |
| **ManifestClient** | Obtém metadados da API |
| **TemplateEngine** | Renderiza templates Scriban |
| **OrchestratorService** | Coordena todo o fluxo |
| **GeneratorService** | Gera arquivos de código |

## 🧪 Executar Testes

```bash
dotnet test
```

## 📦 Arquivos Gerados

O ZIP contém:
- `*.generated.cs` - Código gerado (não editar)
- `*.custom.cs` - Código customizável (editar aqui)
- `*.cshtml` - Views Razor
- `*.js` - JavaScript com AJAX
- `*.css` - Estilos CSS

## ⚙️ Customização

### Editar Templates
Editar arquivos em `/Templates/`:
- `ControllerTemplate.scriban`
- `ViewModelTemplate.scriban`
- `RazorViewTemplate.scriban`
- `JavaScriptTemplate.scriban`

### Adicionar Lógica
Editar `*.custom.cs`:
```csharp
// Adicione sua lógica aqui
public partial class MyEntityController
{
    public async Task<IActionResult> CustomMethod()
    {
        // Sua implementação
    }
}
```

## 🐛 Troubleshooting

| Problema | Solução |
|---------|---------|
| Conexão com BD falha | Verificar `appsettings.json` |
| Manifesto não encontrado | Verificar URL da API |
| Código não compila | Verificar templates Scriban |
| Conflito não resolvido | Escolher resolução apropriada |

## 📚 Documentação Completa

- `DOCUMENTACAO_TECNICA.md` - Documentação técnica detalhada
- `DECISOES_TECNICAS.md` - Justificativas de design
- `README.md` - Visão geral do projeto

## 🎯 Próximos Passos

1. ✅ Compilar projeto
2. ✅ Executar aplicação
3. ✅ Acessar wizard
4. ✅ Gerar código para uma entidade
5. ✅ Integrar código em seu projeto
6. ✅ Customizar conforme necessário

## 💡 Dicas

- Salve configurações para reutilizar depois
- Use a mesma configuração para múltiplas gerações
- Customize `*.custom.cs` para adicionar lógica
- Não edite `*.generated.cs` (será sobrescrito)
- Use partial classes para extensão

## 📞 Suporte

Para dúvidas:
1. Consulte `DOCUMENTACAO_TECNICA.md`
2. Verifique `DECISOES_TECNICAS.md`
3. Revise exemplos em `/Templates/`

---

**Versão**: 2.0
**Status**: ✅ Pronto para Uso
**Data**: 2025-12-26
