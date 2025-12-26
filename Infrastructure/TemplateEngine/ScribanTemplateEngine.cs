namespace GeradorFrontendEnterprise.Infrastructure.TemplateEngine;

using GeradorFrontendEnterprise.Core.Contracts;
using Scriban;

/// <summary>
/// Implementação do motor de templates usando Scriban.
/// Renderiza templates Liquid para geração de código.
/// </summary>
public class ScribanTemplateEngine : ITemplateEngine
{
    private readonly ILogger<ScribanTemplateEngine> _logger;
    private readonly string _templatesPath;
    private readonly Dictionary<string, Template> _templateCache;

    public ScribanTemplateEngine(
        ILogger<ScribanTemplateEngine> logger,
        string templatesPath = "Templates")
    {
        _logger = logger;
        _templatesPath = templatesPath;
        _templateCache = new Dictionary<string, Template>();

        // Criar diretório de templates se não existir
        if (!Directory.Exists(_templatesPath))
        {
            Directory.CreateDirectory(_templatesPath);
            _logger.LogInformation("Diretório de templates criado: {Path}", _templatesPath);
        }
    }

    /// <summary>
    /// Renderiza um template com os dados fornecidos.
    /// </summary>
    public async Task<string> RenderAsync(string templateName, object data)
    {
        _logger.LogInformation("Renderizando template: {TemplateName}", templateName);

        try
        {
            var templatePath = Path.Combine(_templatesPath, templateName);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {templatePath}");
            }

            var templateContent = await File.ReadAllTextAsync(templatePath);
            return await RenderFromStringAsync(templateContent, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renderizar template {TemplateName}", templateName);
            throw;
        }
    }

    /// <summary>
    /// Renderiza um template a partir de uma string.
    /// </summary>
    public async Task<string> RenderFromStringAsync(string templateContent, object data)
    {
        _logger.LogInformation("Renderizando template a partir de string");

        try
        {
            var template = Template.Parse(templateContent);
            var result = await template.RenderAsync(data);

            _logger.LogInformation("Template renderizado com sucesso. Tamanho: {Size} bytes", 
                result.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renderizar template a partir de string");
            throw;
        }
    }

    /// <summary>
    /// Obtém a lista de templates disponíveis.
    /// </summary>
    public async Task<List<string>> GetAvailableTemplatesAsync()
    {
        _logger.LogInformation("Listando templates disponíveis em: {Path}", _templatesPath);

        try
        {
            if (!Directory.Exists(_templatesPath))
            {
                return new List<string>();
            }

            var files = Directory.GetFiles(_templatesPath, "*.scriban")
                .Select(f => Path.GetFileName(f))
                .ToList();

            _logger.LogInformation("Total de templates encontrados: {Count}", files.Count);
            return await Task.FromResult(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar templates");
            throw;
        }
    }

    /// <summary>
    /// Carrega um template do caminho especificado.
    /// </summary>
    public async Task<string> LoadTemplateAsync(string templatePath)
    {
        _logger.LogInformation("Carregando template de: {Path}", templatePath);

        try
        {
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template não encontrado: {templatePath}");
            }

            var content = await File.ReadAllTextAsync(templatePath);
            _logger.LogInformation("Template carregado com sucesso. Tamanho: {Size} bytes", 
                content.Length);

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar template de {Path}", templatePath);
            throw;
        }
    }

    /// <summary>
    /// Valida a sintaxe de um template.
    /// </summary>
    public async Task<TemplateValidationResult> ValidateTemplateAsync(string templateContent)
    {
        _logger.LogInformation("Validando template");

        var result = new TemplateValidationResult { IsValid = true };

        try
        {
            Template.Parse(templateContent);
            _logger.LogInformation("Template validado com sucesso");
        }
        catch (Exception ex) when (ex.GetType().Name == "ScriptParseException")
        {
            result.IsValid = false;
            result.Errors.Add($"Erro de parse: {ex.Message}");
            _logger.LogError(ex, "Erro ao validar template");
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Erro desconhecido: {ex.Message}");
            _logger.LogError(ex, "Erro desconhecido ao validar template");
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Cria um template padrão para Controller gerado.
    /// </summary>
    public async Task<string> CreateDefaultControllerTemplateAsync()
    {
        const string template = @"// Generated from config {{ config.config_hash }}
// Entity: {{ entity.entity_id }}
// Generated at: {{ now }}

namespace {{ namespace }}.Controllers;

using Microsoft.AspNetCore.Mvc;
using {{ namespace }}.Core.Models;
using {{ namespace }}.Services;

/// <summary>
/// Controller para {{ entity.entity_name }}.
/// Gerado automaticamente pelo Gerador Frontend Enterprise.
/// </summary>
[ApiController]
[Route(""api/[controller]"")]
public partial class {{ entity.entity_id }}Controller : ControllerBase
{
    private readonly ILogger<{{ entity.entity_id }}Controller> _logger;

    public {{ entity.entity_id }}Controller(ILogger<{{ entity.entity_id }}Controller> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as entidades.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        _logger.LogInformation(""Listando {{ entity.entity_name }}"");
        return Ok(new { message = ""Implementar listagem"" });
    }

    /// <summary>
    /// Obtém uma entidade por ID.
    /// </summary>
    [HttpGet(""{id}"")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation(""Obtendo {{ entity.entity_name }} com ID: {Id}"", id);
        return Ok(new { message = ""Implementar obtenção por ID"" });
    }

    /// <summary>
    /// Cria uma nova entidade.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object model)
    {
        _logger.LogInformation(""Criando nova {{ entity.entity_name }}"");
        return Ok(new { message = ""Implementar criação"" });
    }

    /// <summary>
    /// Atualiza uma entidade existente.
    /// </summary>
    [HttpPut(""{id}"")]
    public async Task<IActionResult> Update(int id, [FromBody] object model)
    {
        _logger.LogInformation(""Atualizando {{ entity.entity_name }} com ID: {Id}"", id);
        return Ok(new { message = ""Implementar atualização"" });
    }

    /// <summary>
    /// Deleta uma entidade.
    /// </summary>
    [HttpDelete(""{id}"")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation(""Deletando {{ entity.entity_name }} com ID: {Id}"", id);
        return Ok(new { message = ""Implementar deleção"" });
    }
}

// Partial class para customizações
public partial class {{ entity.entity_id }}Controller
{
    // Adicione seus métodos customizados aqui
}
";

        var templatePath = Path.Combine(_templatesPath, "Controller.cs.scriban");
        await File.WriteAllTextAsync(templatePath, template);
        _logger.LogInformation("Template padrão de Controller criado em: {Path}", templatePath);

        return await Task.FromResult(template);
    }

    /// <summary>
    /// Cria um template padrão para Razor View gerada.
    /// </summary>
    public async Task<string> CreateDefaultRazorTemplateAsync()
    {
        const string template = @"@{
    ViewData[""Title""] = ""{{ entity.entity_name }}"";
}

<!-- Generated from config {{ config.config_hash }} -->
<!-- Entity: {{ entity.entity_id }} -->
<!-- Generated at: {{ now }} -->

<div class=""container-fluid"">
    <div class=""row mb-3"">
        <div class=""col-md-6"">
            <h2>{{ entity.entity_name }}</h2>
        </div>
        <div class=""col-md-6 text-end"">
            <button class=""btn btn-primary"" id=""btnNew"">
                <i class=""fas fa-plus""></i> Novo
            </button>
        </div>
    </div>

    <div class=""row"">
        <div class=""col-md-12"">
            <div class=""card"">
                <div class=""card-body"">
                    <table id=""dataTable"" class=""table table-striped table-hover"">
                        <thead>
                            <tr>
                                {{ for column in grid.columns }}
                                <th>{{ column.label }}</th>
                                {{ end }}
                                <th>Ações</th>
                            </tr>
                        </thead>
                        <tbody>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Modal para criar/editar -->
<div class=""modal fade"" id=""formModal"" tabindex=""-1"">
    <div class=""modal-dialog modal-lg"">
        <div class=""modal-content"">
            <div class=""modal-header"">
                <h5 class=""modal-title"">{{ entity.entity_name }}</h5>
                <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button>
            </div>
            <div class=""modal-body"">
                <form id=""entityForm"">
                    {{ for field in form.fields }}
                    <div class=""mb-3"">
                        <label for=""{{ field.field }}"" class=""form-label"">{{ field.label }}</label>
                        <input type=""text"" class=""form-control"" id=""{{ field.field }}"" name=""{{ field.field }}"" />
                    </div>
                    {{ end }}
                </form>
            </div>
            <div class=""modal-footer"">
                <button type=""button"" class=""btn btn-secondary"" data-bs-dismiss=""modal"">Cancelar</button>
                <button type=""button"" class=""btn btn-primary"" id=""btnSave"">Salvar</button>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script src=""~/js/{{ entity.entity_id | downcase }}.generated.js""></script>
}
";

        var templatePath = Path.Combine(_templatesPath, "Index.cshtml.scriban");
        await File.WriteAllTextAsync(templatePath, template);
        _logger.LogInformation("Template padrão de Razor criado em: {Path}", templatePath);

        return await Task.FromResult(template);
    }
}

/// <summary>
/// Extensão para registrar o Template Engine na injeção de dependência.
/// </summary>
public static class TemplateEngineExtensions
{
    public static IServiceCollection AddScribanTemplateEngine(
        this IServiceCollection services,
        string templatesPath = "Templates")
    {
        services.AddSingleton<ITemplateEngine>(sp =>
            new ScribanTemplateEngine(
                sp.GetRequiredService<ILogger<ScribanTemplateEngine>>(),
                templatesPath));

        return services;
    }
}
