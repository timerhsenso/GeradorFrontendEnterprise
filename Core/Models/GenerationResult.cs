namespace GeradorFrontendEnterprise.Core.Models;

/// <summary>
/// Representa o resultado de uma geração de código.
/// </summary>
public class GenerationResult
{
    /// <summary>
    /// Identificador único da geração.
    /// </summary>
    public string GenerationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identificador da configuração utilizada.
    /// </summary>
    public string ConfigId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador da entidade.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Status da geração (Success, Warning, Error).
    /// </summary>
    public GenerationStatus Status { get; set; } = GenerationStatus.Success;

    /// <summary>
    /// Data/hora da geração.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tempo total de geração em milissegundos.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Arquivos gerados.
    /// </summary>
    public List<GeneratedFile> Files { get; set; } = new();

    /// <summary>
    /// Mensagens de aviso.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Mensagens de erro.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Hash SHA256 da geração (para rastreabilidade).
    /// </summary>
    public string? GenerationHash { get; set; }

    /// <summary>
    /// Caminho do arquivo ZIP gerado.
    /// </summary>
    public string? OutputZipPath { get; set; }

    /// <summary>
    /// Metadados da geração.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Indica se a geração foi bem-sucedida.
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Obtém o número total de arquivos gerados.
    /// </summary>
    public int GetTotalFileCount() => Files.Count;

    /// <summary>
    /// Obtém o tamanho total dos arquivos em bytes.
    /// </summary>
    public long GetTotalFileSize() => Files.Sum(f => f.ContentLength);

    /// <summary>
    /// Obtém arquivos por tipo.
    /// </summary>
    public List<GeneratedFile> GetFilesByType(string fileType)
    {
        return Files.Where(f => f.FileType == fileType).ToList();
    }

    /// <summary>
    /// Adiciona um aviso.
    /// </summary>
    public void AddWarning(string message)
    {
        Warnings.Add(message);
        if (Status == GenerationStatus.Success)
            Status = GenerationStatus.Warning;
    }

    /// <summary>
    /// Adiciona um erro.
    /// </summary>
    public void AddError(string message)
    {
        Errors.Add(message);
        Status = GenerationStatus.Error;
    }

    /// <summary>
    /// Valida o resultado.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ConfigId))
            errors.Add("ConfigId não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(EntityId))
            errors.Add("EntityId não pode estar vazio.");

        if (Files.Count == 0)
            errors.Add("Nenhum arquivo foi gerado.");

        foreach (var file in Files)
        {
            var fileErrors = file.Validate();
            errors.AddRange(fileErrors);
        }

        return errors;
    }
}

/// <summary>
/// Status de uma geração.
/// </summary>
public enum GenerationStatus
{
    /// <summary>
    /// Geração bem-sucedida.
    /// </summary>
    Success,

    /// <summary>
    /// Geração com avisos.
    /// </summary>
    Warning,

    /// <summary>
    /// Geração com erros.
    /// </summary>
    Error,

    /// <summary>
    /// Geração em progresso.
    /// </summary>
    InProgress,

    /// <summary>
    /// Geração cancelada.
    /// </summary>
    Cancelled
}

/// <summary>
/// Representa um arquivo gerado.
/// </summary>
public class GeneratedFile
{
    /// <summary>
    /// Caminho relativo do arquivo (ex: "Controllers/TreTiposTreinamentoController.generated.cs").
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Caminho completo do arquivo.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Nome do arquivo.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de arquivo (ex: "csharp", "razor", "javascript", "css").
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo do arquivo.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Tamanho do conteúdo em bytes.
    /// </summary>
    public long ContentLength => System.Text.Encoding.UTF8.GetByteCount(Content);

    /// <summary>
    /// Hash SHA256 do arquivo.
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// Indica se é um arquivo gerado (true) ou customizado (false).
    /// </summary>
    public bool IsGenerated { get; set; } = true;

    /// <summary>
    /// Descrição do arquivo.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Validação do arquivo.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(RelativePath))
            errors.Add("RelativePath não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(FileName))
            errors.Add("FileName não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(FileType))
            errors.Add("FileType não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(Content))
            errors.Add("Content não pode estar vazio.");

        return errors;
    }
}

/// <summary>
/// Representa um resumo de geração para logging.
/// </summary>
public class GenerationSummary
{
    /// <summary>
    /// Identificador da geração.
    /// </summary>
    public string GenerationId { get; set; } = string.Empty;

    /// <summary>
    /// Identificador da configuração.
    /// </summary>
    public string ConfigId { get; set; } = string.Empty;

    /// <summary>
    /// Hash da configuração.
    /// </summary>
    public string ConfigHash { get; set; } = string.Empty;

    /// <summary>
    /// Entidade gerada.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Status final.
    /// </summary>
    public GenerationStatus Status { get; set; }

    /// <summary>
    /// Número de arquivos gerados.
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Tamanho total em bytes.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Tempo de geração em milissegundos.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Número de avisos.
    /// </summary>
    public int WarningCount { get; set; }

    /// <summary>
    /// Número de erros.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Data/hora da geração.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Cria um resumo a partir de um resultado.
    /// </summary>
    public static GenerationSummary FromResult(GenerationResult result)
    {
        return new GenerationSummary
        {
            GenerationId = result.GenerationId,
            EntityId = result.EntityId,
            Status = result.Status,
            FileCount = result.Files.Count,
            TotalSize = result.GetTotalFileSize(),
            DurationMs = result.DurationMs,
            WarningCount = result.Warnings.Count,
            ErrorCount = result.Errors.Count,
            GeneratedAt = result.GeneratedAt
        };
    }
}
