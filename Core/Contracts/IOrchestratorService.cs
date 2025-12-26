namespace GeradorFrontendEnterprise.Core.Contracts;

using GeradorFrontendEnterprise.Core.Enums;
using GeradorFrontendEnterprise.Core.Models;

/// <summary>
/// Contrato para o serviço orquestrador.
/// Responsável por coordenar todo o fluxo de geração.
/// </summary>
public interface IOrchestratorService
{
    /// <summary>
    /// Inicia o fluxo do wizard.
    /// </summary>
    /// <param name="entityId">Identificador da entidade.</param>
    /// <returns>Resultado da inicialização.</returns>
    Task<WizardInitializationResult> InitializeWizardAsync(string entityId);

    /// <summary>
    /// Carrega o schema da entidade.
    /// </summary>
    /// <param name="entityId">Identificador da entidade.</param>
    /// <returns>Schema da tabela.</returns>
    Task<TableSchema> LoadSchemaAsync(string entityId);

    /// <summary>
    /// Detecta conflitos entre banco e manifesto.
    /// </summary>
    /// <param name="entityId">Identificador da entidade.</param>
    /// <returns>Lista de conflitos.</returns>
    Task<List<Conflict>> DetectConflictsAsync(string entityId);

    /// <summary>
    /// Resolve conflitos de acordo com as decisões do usuário.
    /// </summary>
    /// <param name="entityId">Identificador da entidade.</param>
    /// <param name="resolutions">Resoluções aplicadas.</param>
    /// <returns>Resultado da resolução.</returns>
    Task<ConflictResolutionResult> ResolveConflictsAsync(
        string entityId,
        Dictionary<string, ConflictResolution> resolutions);

    /// <summary>
    /// Valida a configuração do wizard.
    /// </summary>
    /// <param name="config">Configuração do wizard.</param>
    /// <returns>Resultado da validação.</returns>
    Task<ValidationResult> ValidateConfigurationAsync(WizardConfig config);

    /// <summary>
    /// Gera o código a partir da configuração.
    /// </summary>
    /// <param name="config">Configuração do wizard.</param>
    /// <returns>Resultado da geração.</returns>
    Task<GenerationResult> GenerateCodeAsync(WizardConfig config);

    /// <summary>
    /// Salva a configuração para reutilização futura.
    /// </summary>
    /// <param name="config">Configuração do wizard.</param>
    /// <returns>ID da configuração salva.</returns>
    Task<string> SaveConfigurationAsync(WizardConfig config);

    /// <summary>
    /// Carrega uma configuração salva.
    /// </summary>
    /// <param name="configId">ID da configuração.</param>
    /// <returns>Configuração carregada.</returns>
    Task<WizardConfig> LoadConfigurationAsync(string configId);

    /// <summary>
    /// Obtém o histórico de gerações de uma entidade.
    /// </summary>
    /// <param name="entityId">Identificador da entidade.</param>
    /// <returns>Lista de gerações.</returns>
    Task<List<GenerationSummary>> GetGenerationHistoryAsync(string entityId);
}
