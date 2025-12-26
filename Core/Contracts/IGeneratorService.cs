namespace GeradorFrontendEnterprise.Core.Contracts;

using GeradorFrontendEnterprise.Core.Models;

/// <summary>
/// Contrato para o serviço gerador de código.
/// Responsável pela orquestração final da geração.
/// </summary>
public interface IGeneratorService
{
    /// <summary>
    /// Gera todos os arquivos para uma entidade.
    /// </summary>
    /// <param name="config">Configuração do wizard.</param>
    /// <param name="schema">Schema da tabela.</param>
    /// <param name="manifest">Manifesto da entidade.</param>
    /// <returns>Resultado da geração.</returns>
    Task<GenerationResult> GenerateAsync(
        WizardConfig config,
        TableSchema schema,
        EntityManifest manifest);

    /// <summary>
    /// Gera o arquivo ZIP com todos os arquivos.
    /// </summary>
    /// <param name="result">Resultado da geração.</param>
    /// <param name="outputPath">Caminho de saída.</param>
    /// <returns>Caminho do arquivo ZIP.</returns>
    Task<string> CreateZipPackageAsync(GenerationResult result, string outputPath);

    /// <summary>
    /// Valida o código gerado.
    /// </summary>
    /// <param name="result">Resultado da geração.</param>
    /// <returns>Resultado da validação.</returns>
    Task<CodeValidationResult> ValidateGeneratedCodeAsync(GenerationResult result);

    /// <summary>
    /// Obtém estatísticas da geração.
    /// </summary>
    /// <param name="result">Resultado da geração.</param>
    /// <returns>Estatísticas.</returns>
    GenerationStatistics GetStatistics(GenerationResult result);
}
