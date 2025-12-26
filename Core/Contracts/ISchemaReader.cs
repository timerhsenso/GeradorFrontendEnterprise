namespace GeradorFrontendEnterprise.Core.Contracts;

using GeradorFrontendEnterprise.Core.Models;

/// <summary>
/// Contrato para leitura de schema do SQL Server.
/// Responsável por extrair a estrutura real das tabelas.
/// </summary>
public interface ISchemaReader
{
    /// <summary>
    /// Lê o schema completo de uma tabela.
    /// </summary>
    /// <param name="connectionString">String de conexão do SQL Server.</param>
    /// <param name="schemaName">Nome do schema (ex: "dbo", "rhu").</param>
    /// <param name="tableName">Nome da tabela.</param>
    /// <returns>Schema da tabela.</returns>
    Task<TableSchema> ReadTableSchemaAsync(
        string connectionString,
        string schemaName,
        string tableName);

    /// <summary>
    /// Lê todas as tabelas de um schema.
    /// </summary>
    /// <param name="connectionString">String de conexão do SQL Server.</param>
    /// <param name="schemaName">Nome do schema.</param>
    /// <returns>Lista de schemas de tabelas.</returns>
    Task<List<TableSchema>> ReadAllTablesAsync(
        string connectionString,
        string schemaName);

    /// <summary>
    /// Valida a consistência entre schema do banco e manifesto.
    /// </summary>
    /// <param name="dbSchema">Schema do banco de dados.</param>
    /// <param name="manifest">Manifesto da API.</param>
    /// <returns>Resultado da validação.</returns>
    Task<ValidationResult> ValidateConsistencyAsync(
        TableSchema dbSchema,
        EntityManifest manifest);

    /// <summary>
    /// Testa a conexão com o SQL Server.
    /// </summary>
    /// <param name="connectionString">String de conexão.</param>
    /// <returns>True se a conexão é bem-sucedida.</returns>
    Task<bool> TestConnectionAsync(string connectionString);
}
