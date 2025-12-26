namespace GeradorFrontendEnterprise.Core.Contracts;

using GeradorFrontendEnterprise.Core.Models;

/// <summary>
/// Contrato para o motor de templates.
/// Responsável por renderizar templates Scriban.
/// </summary>
public interface ITemplateEngine
{
    /// <summary>
    /// Renderiza um template com os dados fornecidos.
    /// </summary>
    /// <param name="templateName">Nome do template (ex: "Controller.cs.scriban").</param>
    /// <param name="data">Dados para renderização.</param>
    /// <returns>Conteúdo renderizado.</returns>
    Task<string> RenderAsync(string templateName, object data);

    /// <summary>
    /// Renderiza um template a partir de uma string.
    /// </summary>
    /// <param name="templateContent">Conteúdo do template.</param>
    /// <param name="data">Dados para renderização.</param>
    /// <returns>Conteúdo renderizado.</returns>
    Task<string> RenderFromStringAsync(string templateContent, object data);

    /// <summary>
    /// Obtém a lista de templates disponíveis.
    /// </summary>
    /// <returns>Lista de nomes de templates.</returns>
    Task<List<string>> GetAvailableTemplatesAsync();

    /// <summary>
    /// Carrega um template do caminho especificado.
    /// </summary>
    /// <param name="templatePath">Caminho do arquivo template.</param>
    /// <returns>Conteúdo do template.</returns>
    Task<string> LoadTemplateAsync(string templatePath);

    /// <summary>
    /// Valida a sintaxe de um template.
    /// </summary>
    /// <param name="templateContent">Conteúdo do template.</param>
    /// <returns>Resultado da validação.</returns>
    Task<TemplateValidationResult> ValidateTemplateAsync(string templateContent);
}

/// <summary>
/// Resultado da validação de um template.
/// </summary>
public class TemplateValidationResult
{
    /// <summary>
    /// Indica se o template é válido.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Mensagens de erro.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Mensagens de aviso.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
