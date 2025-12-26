namespace GeradorFrontendEnterprise.Core.Models;

using GeradorFrontendEnterprise.Core.Enums;

/// <summary>
/// Representa um conflito detectado entre banco e manifesto.
/// </summary>
public class Conflict
{
    /// <summary>
    /// Tipo de conflito.
    /// </summary>
    public ConflictType Type { get; set; }

    /// <summary>
    /// Nome do campo envolvido no conflito.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Valor do banco de dados.
    /// </summary>
    public string? DatabaseValue { get; set; }

    /// <summary>
    /// Valor do manifesto.
    /// </summary>
    public string? ManifestValue { get; set; }

    /// <summary>
    /// Descrição do conflito.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Resolução sugerida.
    /// </summary>
    public Core.Enums.ConflictResolution SuggestedResolution { get; set; } = Core.Enums.ConflictResolution.RequiresManualReview;
}

/// <summary>
/// Resultado da resolução de conflitos.
/// </summary>
public class ConflictResolutionResult
{
    /// <summary>
    /// Indica se a resolução foi bem-sucedida.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Conflitos não resolvidos.
    /// </summary>
    public List<Conflict> UnresolvedConflicts { get; set; } = new();

    /// <summary>
    /// Avisos durante a resolução.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Erros durante a resolução.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Resultado da validação de código.
/// </summary>
public class CodeValidationResult
{
    /// <summary>
    /// Indica se o código é válido.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Erros de compilação.
    /// </summary>
    public List<CodeError> CompilationErrors { get; set; } = new();

    /// <summary>
    /// Problemas de estilo.
    /// </summary>
    public List<StyleIssue> StyleIssues { get; set; } = new();
}

/// <summary>
/// Erro de compilação.
/// </summary>
public class CodeError
{
    /// <summary>
    /// Arquivo com erro.
    /// </summary>
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem do erro.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Linha do erro.
    /// </summary>
    public int? Line { get; set; }

    /// <summary>
    /// Coluna do erro.
    /// </summary>
    public int? Column { get; set; }
}

/// <summary>
/// Problema de estilo.
/// </summary>
public class StyleIssue
{
    /// <summary>
    /// Arquivo com problema.
    /// </summary>
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do problema.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severidade (Info, Warning, Error).
    /// </summary>
    public string Severity { get; set; } = "Info";
}

/// <summary>
/// Estatísticas de geração.
/// </summary>
public class GenerationStatistics
{
    /// <summary>
    /// Total de arquivos gerados.
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Número de arquivos C#.
    /// </summary>
    public int CSharpFiles { get; set; }

    /// <summary>
    /// Número de arquivos Razor.
    /// </summary>
    public int RazorFiles { get; set; }

    /// <summary>
    /// Número de arquivos JavaScript.
    /// </summary>
    public int JavaScriptFiles { get; set; }

    /// <summary>
    /// Número de arquivos CSS.
    /// </summary>
    public int CssFiles { get; set; }

    /// <summary>
    /// Tamanho total em bytes.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Total de linhas de código.
    /// </summary>
    public int TotalLines { get; set; }
}

/// <summary>
/// Resultado da inicialização do wizard.
/// </summary>
public class WizardInitializationResult
{
    /// <summary>
    /// Indica se a inicialização foi bem-sucedida.
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// Manifesto da entidade.
    /// </summary>
    public EntityManifest? Manifest { get; set; }

    /// <summary>
    /// Schema da tabela.
    /// </summary>
    public TableSchema? Schema { get; set; }

    /// <summary>
    /// Configuração padrão sugerida.
    /// </summary>
    public WizardConfig? SuggestedConfig { get; set; }

    /// <summary>
    /// Erros durante a inicialização.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Resultado da validação.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indica se a validação foi bem-sucedida.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Erros de validação.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Conflitos detectados.
    /// </summary>
    public List<Conflict> Conflicts { get; set; } = new();

    /// <summary>
    /// Avisos de validação.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Adiciona um erro.
    /// </summary>
    public void AddError(string message)
    {
        Errors.Add(message);
        IsValid = false;
    }

    /// <summary>
    /// Adiciona um conflito.
    /// </summary>
    public void AddConflict(Conflict conflict)
    {
        Conflicts.Add(conflict);
        IsValid = false;
    }

    /// <summary>
    /// Adiciona um aviso.
    /// </summary>
    public void AddWarning(string message)
    {
        Warnings.Add(message);
    }
}
