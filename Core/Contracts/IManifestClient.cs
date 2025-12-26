namespace GeradorFrontendEnterprise.Core.Contracts;

using GeradorFrontendEnterprise.Core.Models;

/// <summary>
/// Contrato para comunicação com a API de manifesto.
/// Responsável por obter metadados de entidades.
/// </summary>
public interface IManifestClient
{
    /// <summary>
    /// Obtém o manifesto de uma entidade.
    /// </summary>
    /// <param name="entityId">Identificador da entidade.</param>
    /// <returns>Manifesto da entidade.</returns>
    Task<EntityManifest> GetEntityManifestAsync(string entityId);

    /// <summary>
    /// Obtém todos os manifestos disponíveis.
    /// </summary>
    /// <returns>Lista de manifestos.</returns>
    Task<List<EntityManifest>> GetAllManifestsAsync();

    /// <summary>
    /// Obtém manifestos de um módulo específico.
    /// </summary>
    /// <param name="moduleName">Nome do módulo.</param>
    /// <returns>Lista de manifestos do módulo.</returns>
    Task<List<EntityManifest>> GetManifestsByModuleAsync(string moduleName);

    /// <summary>
    /// Valida se o usuário tem permissão para uma entidade.
    /// </summary>
    /// <param name="entityId">Identificador da entidade.</param>
    /// <param name="permissionType">Tipo de permissão (Create, Read, Update, Delete).</param>
    /// <returns>True se tem permissão.</returns>
    Task<bool> HasPermissionAsync(string entityId, string permissionType);

    /// <summary>
    /// Testa a conexão com a API de manifesto.
    /// </summary>
    /// <returns>True se a conexão é bem-sucedida.</returns>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// Obtém a URL base da API.
    /// </summary>
    string GetBaseUrl();
}
