namespace GeradorFrontendEnterprise.Services.Generator;

using GeradorFrontendEnterprise.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using GeradorFrontendEnterprise.Core.Models;
using System.IO.Compression;
using System.Text;

/// <summary>
/// Serviço gerador de código.
/// Responsável pela orquestração final da geração de arquivos.
/// </summary>
public class GeneratorService : IGeneratorService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly ILogger<GeneratorService> _logger;
    private readonly string _outputPath;

    public GeneratorService(
        ITemplateEngine templateEngine,
        ILogger<GeneratorService> logger,
        string outputPath = "GeneratedCode")
    {
        _templateEngine = templateEngine;
        _logger = logger;
        _outputPath = outputPath;

        if (!Directory.Exists(_outputPath))
        {
            Directory.CreateDirectory(_outputPath);
            _logger.LogInformation("Diretório de saída criado: {Path}", _outputPath);
        }
    }

    /// <summary>
    /// Gera todos os arquivos para uma entidade.
    /// </summary>
    public async Task<GenerationResult> GenerateAsync(
        WizardConfig config,
        TableSchema schema,
        EntityManifest manifest)
    {
        _logger.LogInformation("Iniciando geração para {EntityId}", config.EntityId);

        var result = new GenerationResult
        {
            EntityId = config.EntityId,
            IsSuccessful = true,
            GeneratedAt = DateTime.UtcNow
        };

        try
        {
            var entityPath = Path.Combine(_outputPath, config.EntityId);
            if (Directory.Exists(entityPath))
            {
                Directory.Delete(entityPath, true);
            }
            Directory.CreateDirectory(entityPath);

            // Preparar dados para templates
            var templateData = new
            {
                config = config,
                entity = manifest,
                schema = schema,
                now = DateTime.UtcNow,
                ns = "GeneratedCode"
            };

            // 1. Gerar Controller
            _logger.LogInformation("Gerando Controller para {EntityId}", config.EntityId);
            var controllerContent = await GenerateControllerAsync(config, manifest, schema, templateData);
            var controllerPath = Path.Combine(entityPath, $"{config.EntityId}Controller.generated.cs");
            await File.WriteAllTextAsync(controllerPath, controllerContent);
            result.Files.Add(new GeneratedFile
            {
                FileName = $"{config.EntityId}Controller.generated.cs",
                FilePath = controllerPath,
                FileType = "Controller",
                Content = controllerContent
            });

            // 2. Gerar ViewModel
            _logger.LogInformation("Gerando ViewModel para {EntityId}", config.EntityId);
            var viewModelContent = await GenerateViewModelAsync(config, schema, templateData);
            var viewModelPath = Path.Combine(entityPath, $"{config.EntityId}ViewModel.generated.cs");
            await File.WriteAllTextAsync(viewModelPath, viewModelContent);
            result.Files.Add(new GeneratedFile
            {
                FileName = $"{config.EntityId}ViewModel.generated.cs",
                FilePath = viewModelPath,
                FileType = "ViewModel",
                Content = viewModelContent
            });

            // 3. Gerar Razor View
            _logger.LogInformation("Gerando Razor View para {EntityId}", config.EntityId);
            var viewContent = await GenerateRazorViewAsync(config, schema, templateData);
            var viewPath = Path.Combine(entityPath, $"Index.generated.cshtml");
            await File.WriteAllTextAsync(viewPath, viewContent);
            result.Files.Add(new GeneratedFile
            {
                FileName = $"Index.generated.cshtml",
                FilePath = viewPath,
                FileType = "View",
                Content = viewContent
            });

            // 4. Gerar JavaScript
            _logger.LogInformation("Gerando JavaScript para {EntityId}", config.EntityId);
            var jsContent = await GenerateJavaScriptAsync(config, schema, templateData);
            var jsPath = Path.Combine(entityPath, $"{config.EntityId.ToLower()}.generated.js");
            await File.WriteAllTextAsync(jsPath, jsContent);
            result.Files.Add(new GeneratedFile
            {
                FileName = $"{config.EntityId.ToLower()}.generated.js",
                FilePath = jsPath,
                FileType = "JavaScript",
                Content = jsContent
            });

            // 5. Gerar CSS
            _logger.LogInformation("Gerando CSS para {EntityId}", config.EntityId);
            var cssContent = await GenerateCssAsync(config, schema, templateData);
            var cssPath = Path.Combine(entityPath, $"{config.EntityId.ToLower()}.generated.css");
            await File.WriteAllTextAsync(cssPath, cssContent);
            result.Files.Add(new GeneratedFile
            {
                FileName = $"{config.EntityId.ToLower()}.generated.css",
                FilePath = cssPath,
                FileType = "CSS",
                Content = cssContent
            });

            // 6. Gerar arquivos customizáveis
            _logger.LogInformation("Gerando arquivos customizáveis para {EntityId}", config.EntityId);
            await GenerateCustomizableFilesAsync(config, entityPath, result);

            _logger.LogInformation("Geração concluída com sucesso para {EntityId}. {Count} arquivos gerados.",
                config.EntityId, result.Files.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar código para {EntityId}", config.EntityId);
            result.IsSuccessful = false;
            result.Errors.Add($"Erro ao gerar código: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Gera o arquivo ZIP com todos os arquivos.
    /// </summary>
    public async Task<string> CreateZipPackageAsync(GenerationResult result, string outputPath)
    {
        _logger.LogInformation("Criando pacote ZIP para {EntityId}", result.EntityId);

        try
        {
            var zipFileName = $"{result.EntityId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            var zipPath = Path.Combine(outputPath, zipFileName);

            using (var zipFile = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in result.Files)
                {
                    zipFile.CreateEntryFromFile(file.FilePath, file.FileName);
                }
            }

            _logger.LogInformation("Pacote ZIP criado: {ZipPath}", zipPath);
            return zipPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pacote ZIP para {EntityId}", result.EntityId);
            throw;
        }
    }

    /// <summary>
    /// Valida o código gerado.
    /// </summary>
    public async Task<CodeValidationResult> ValidateGeneratedCodeAsync(GenerationResult result)
    {
        _logger.LogInformation("Validando código gerado para {EntityId}", result.EntityId);

        var validationResult = new CodeValidationResult { IsValid = true };

        try
        {
            foreach (var file in result.Files)
            {
                // Validações básicas
                if (string.IsNullOrWhiteSpace(file.Content))
                {
                    validationResult.IsValid = false;
                    validationResult.CompilationErrors.Add(new CodeError
                    {
                        File = file.FileName,
                        Message = "Arquivo vazio"
                    });
                }

                // Validar sintaxe C#
                if (file.FileType == "Controller" || file.FileType == "ViewModel")
                {
                    ValidateCSharpSyntax(file, validationResult);
                }

                // Validar sintaxe Razor
                if (file.FileType == "View")
                {
                    ValidateRazorSyntax(file, validationResult);
                }

                // Validar sintaxe JavaScript
                if (file.FileType == "JavaScript")
                {
                    ValidateJavaScriptSyntax(file, validationResult);
                }
            }

            _logger.LogInformation("Validação concluída. IsValid: {IsValid}", validationResult.IsValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar código gerado para {EntityId}", result.EntityId);
            validationResult.IsValid = false;
        }

        return await Task.FromResult(validationResult);
    }

    /// <summary>
    /// Obtém estatísticas da geração.
    /// </summary>
    public GenerationStatistics GetStatistics(GenerationResult result)
    {
        _logger.LogInformation("Calculando estatísticas para {EntityId}", result.EntityId);

        var stats = new GenerationStatistics
        {
            TotalFiles = result.Files.Count,
            CSharpFiles = result.Files.Count(f => f.FileType == "Controller" || f.FileType == "ViewModel"),
            RazorFiles = result.Files.Count(f => f.FileType == "View"),
            JavaScriptFiles = result.Files.Count(f => f.FileType == "JavaScript"),
            CssFiles = result.Files.Count(f => f.FileType == "CSS"),
            TotalSize = result.Files.Sum(f => Encoding.UTF8.GetByteCount(f.Content)),
            TotalLines = result.Files.Sum(f => f.Content.Split('\n').Length)
        };

        return stats;
    }

    /// <summary>
    /// Gera o Controller.
    /// </summary>
    private async Task<string> GenerateControllerAsync(
        WizardConfig config,
        EntityManifest manifest,
        TableSchema schema,
        object templateData)
    {
        const string template = @"// Generated from config {{ config.config_hash }}
// Entity: {{ entity.entity_id }}
// Generated at: {{ now }}

namespace GeneratedCode.Controllers;

using Microsoft.AspNetCore.Mvc;
using GeneratedCode.Models;

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

    [HttpGet]
    public IActionResult List()
    {
        _logger.LogInformation(""Listando {{ entity.entity_name }}"");
        return Ok(new { message = ""Implementar listagem"" });
    }

    [HttpGet(""{id}"")]
    public IActionResult GetById(int id)
    {
        _logger.LogInformation(""Obtendo {{ entity.entity_name }} com ID: {Id}"", id);
        return Ok(new { message = ""Implementar obtenção por ID"" });
    }

    [HttpPost]
    public IActionResult Create([FromBody] object model)
    {
        _logger.LogInformation(""Criando nova {{ entity.entity_name }}"");
        return Ok(new { message = ""Implementar criação"" });
    }

    [HttpPut(""{id}"")]
    public IActionResult Update(int id, [FromBody] object model)
    {
        _logger.LogInformation(""Atualizando {{ entity.entity_name }} com ID: {Id}"", id);
        return Ok(new { message = ""Implementar atualização"" });
    }

    [HttpDelete(""{id}"")]
    public IActionResult Delete(int id)
    {
        _logger.LogInformation(""Deletando {{ entity.entity_name }} com ID: {Id}"", id);
        return Ok(new { message = ""Implementar deleção"" });
    }
}

public partial class {{ entity.entity_id }}Controller
{
    // Adicione seus métodos customizados aqui
}
";

        return await _templateEngine.RenderFromStringAsync(template, templateData);
    }

    /// <summary>
    /// Gera o ViewModel.
    /// </summary>
    private async Task<string> GenerateViewModelAsync(
        WizardConfig config,
        TableSchema schema,
        object templateData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Generated from config {{ config.config_hash }}");
        sb.AppendLine("// Generated at: {{ now }}");
        sb.AppendLine();
        sb.AppendLine("namespace GeneratedCode.Models;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// ViewModel para {{ entity.entity_name }}");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public class {{ entity.entity_id }}ViewModel");
        sb.AppendLine("{");

        foreach (var column in schema.Columns.Where(c => !c.IsComputed))
        {
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {column.ColumnName}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public {column.GetClrTypeName()} {column.ColumnName} {{ get; set; }}");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Gera a Razor View.
    /// </summary>
    private async Task<string> GenerateRazorViewAsync(
        WizardConfig config,
        TableSchema schema,
        object templateData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("@{");
        sb.AppendLine("    ViewData[\"Title\"] = \"{{ entity.entity_name }}\";");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("<div class=\"container-fluid\">");
        sb.AppendLine("    <div class=\"row mb-3\">");
        sb.AppendLine("        <div class=\"col-md-6\">");
        sb.AppendLine("            <h2>{{ entity.entity_name }}</h2>");
        sb.AppendLine("        </div>");
        sb.AppendLine("        <div class=\"col-md-6 text-end\">");
        sb.AppendLine("            <button class=\"btn btn-primary\" id=\"btnNew\">");
        sb.AppendLine("                <i class=\"fas fa-plus\"></i> Novo");
        sb.AppendLine("            </button>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine();
        sb.AppendLine("    <div class=\"row\">");
        sb.AppendLine("        <div class=\"col-md-12\">");
        sb.AppendLine("            <div class=\"card\">");
        sb.AppendLine("                <div class=\"card-body\">");
        sb.AppendLine("                    <table id=\"dataTable\" class=\"table table-striped\">");
        sb.AppendLine("                        <thead>");
        sb.AppendLine("                            <tr>");

        foreach (var field in config.GridLayout.Fields.Take(5))
        {
            sb.AppendLine($"                                <th>{field.Label}</th>");
        }

        sb.AppendLine("                                <th>Ações</th>");
        sb.AppendLine("                            </tr>");
        sb.AppendLine("                        </thead>");
        sb.AppendLine("                        <tbody></tbody>");
        sb.AppendLine("                    </table>");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</div>");

        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Gera o JavaScript.
    /// </summary>
    private async Task<string> GenerateJavaScriptAsync(
        WizardConfig config,
        TableSchema schema,
        object templateData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Generated from config {{ config.config_hash }}");
        sb.AppendLine("// Generated at: {{ now }}");
        sb.AppendLine();
        sb.AppendLine("$(document).ready(function() {");
        sb.AppendLine("    console.log('{{ entity.entity_name }} module loaded');");
        sb.AppendLine();
        sb.AppendLine("    // Inicializar DataTable");
        sb.AppendLine("    $('#dataTable').DataTable({");
        sb.AppendLine("        ajax: {");
        sb.AppendLine("            url: '/api/{{ entity.entity_id | downcase }}',");
        sb.AppendLine("            type: 'GET'");
        sb.AppendLine("        },");
        sb.AppendLine("        columns: [");

        foreach (var field in config.GridLayout.Fields.Take(5))
        {
            sb.AppendLine($"            {{ data: '{field.FieldName}' }},");
        }

        sb.AppendLine("        ],");
        sb.AppendLine("        language: { url: '/js/datatables.pt-BR.json' }");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine("    // Event handlers");
        sb.AppendLine("    $('#btnNew').click(function() {");
        sb.AppendLine("        console.log('Novo registro');");
        sb.AppendLine("    });");
        sb.AppendLine("});");

        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Gera o CSS.
    /// </summary>
    private async Task<string> GenerateCssAsync(
        WizardConfig config,
        TableSchema schema,
        object templateData)
    {
        var sb = new StringBuilder();
        sb.AppendLine("/* Generated from config {{ config.config_hash }} */");
        sb.AppendLine("/* Generated at: {{ now }} */");
        sb.AppendLine();
        sb.AppendLine(".{{ entity.entity_id | downcase }}-container {");
        sb.AppendLine("    padding: 20px;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".{{ entity.entity_id | downcase }}-grid {");
        sb.AppendLine("    margin-top: 20px;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine(".{{ entity.entity_id | downcase }}-form {");
        sb.AppendLine("    max-width: 600px;");
        sb.AppendLine("}");

        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Gera arquivos customizáveis (templates vazios para customização).
    /// </summary>
    private async Task GenerateCustomizableFilesAsync(
        WizardConfig config,
        string entityPath,
        GenerationResult result)
    {
        // Arquivo Controller customizável
        var controllerCustomPath = Path.Combine(entityPath, $"{config.EntityId}Controller.custom.cs");
        var controllerCustomContent = @$"namespace GeneratedCode.Controllers;

/// <summary>
/// Extensões customizadas para {{ config.EntityId }}Controller.
/// Este arquivo não será sobrescrito na regeneração.
/// </summary>
public partial class {config.EntityId}Controller
{{
    // Adicione seus métodos customizados aqui
}}
";
        await File.WriteAllTextAsync(controllerCustomPath, controllerCustomContent);
        result.Files.Add(new GeneratedFile
        {
            FileName = $"{config.EntityId}Controller.custom.cs",
            FilePath = controllerCustomPath,
            FileType = "Controller",
            Content = controllerCustomContent
        });

        // Arquivo ViewModel customizável
        var viewModelCustomPath = Path.Combine(entityPath, $"{config.EntityId}ViewModel.custom.cs");
        var viewModelCustomContent = @$"namespace GeneratedCode.Models;

/// <summary>
/// Extensões customizadas para {{ config.EntityId }}ViewModel.
/// Este arquivo não será sobrescrito na regeneração.
/// </summary>
public partial class {config.EntityId}ViewModel
{{
    // Adicione suas propriedades customizadas aqui
}}
";
        await File.WriteAllTextAsync(viewModelCustomPath, viewModelCustomContent);
        result.Files.Add(new GeneratedFile
        {
            FileName = $"{config.EntityId}ViewModel.custom.cs",
            FilePath = viewModelCustomPath,
            FileType = "ViewModel",
            Content = viewModelCustomContent
        });

        // Arquivo JavaScript customizável
        var jsCustomPath = Path.Combine(entityPath, $"{config.EntityId.ToLower()}.custom.js");
        var jsCustomContent = @$"// Extensões customizadas para {config.EntityId}
// Este arquivo não será sobrescrito na regeneração.

$(document).ready(function() {{
    // Adicione suas customizações JavaScript aqui
}});
";
        await File.WriteAllTextAsync(jsCustomPath, jsCustomContent);
        result.Files.Add(new GeneratedFile
        {
            FileName = $"{config.EntityId.ToLower()}.custom.js",
            FilePath = jsCustomPath,
            FileType = "JavaScript",
            Content = jsCustomContent
        });
    }

    /// <summary>
    /// Valida sintaxe C#.
    /// </summary>
    private void ValidateCSharpSyntax(GeneratedFile file, CodeValidationResult result)
    {
        try
        {
            // Validações básicas
            if (!file.Content.Contains("namespace"))
            {
                result.CompilationErrors.Add(new CodeError
                {
                    File = file.FileName,
                    Message = "Namespace não encontrado"
                });
                result.IsValid = false;
            }

            if (!file.Content.Contains("public class") && !file.Content.Contains("public partial class"))
            {
                result.CompilationErrors.Add(new CodeError
                {
                    File = file.FileName,
                    Message = "Classe pública não encontrada"
                });
                result.IsValid = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao validar sintaxe C# do arquivo {File}", file.FileName);
        }
    }

    /// <summary>
    /// Valida sintaxe Razor.
    /// </summary>
    private void ValidateRazorSyntax(GeneratedFile file, CodeValidationResult result)
    {
        try
        {
            // Validações básicas
            if (!file.Content.Contains("<") || !file.Content.Contains(">"))
            {
                result.StyleIssues.Add(new StyleIssue
                {
                    File = file.FileName,
                    Description = "Arquivo não contém tags HTML"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao validar sintaxe Razor do arquivo {File}", file.FileName);
        }
    }

    /// <summary>
    /// Valida sintaxe JavaScript.
    /// </summary>
    private void ValidateJavaScriptSyntax(GeneratedFile file, CodeValidationResult result)
    {
        try
        {
            // Validações básicas
            var openBraces = file.Content.Count(c => c == '{');
            var closeBraces = file.Content.Count(c => c == '}');

            if (openBraces != closeBraces)
            {
                result.StyleIssues.Add(new StyleIssue
                {
                    File = file.FileName,
                    Description = $"Chaves desbalanceadas: {{ = {openBraces}, }} = {closeBraces}",
                    Severity = "Warning"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao validar sintaxe JavaScript do arquivo {File}", file.FileName);
        }
    }
}

/// <summary>
/// Extensão para registrar o Generator Service na injeção de dependência.
/// </summary>
public static class GeneratorServiceExtensions
{
    public static IServiceCollection AddGeneratorService(
        this IServiceCollection services,
        string outputPath = "GeneratedCode")
    {
        services.AddScoped<IGeneratorService>(sp =>
            new GeneratorService(
                sp.GetRequiredService<ITemplateEngine>(),
                sp.GetRequiredService<ILogger<GeneratorService>>(),
                outputPath));

        return services;
    }
}
