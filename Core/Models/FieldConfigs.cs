namespace GeradorFrontendEnterprise.Core.Models;

using GeradorFrontendEnterprise.Core.Enums;

/// <summary>
/// Configuração de um campo na grid.
/// </summary>
public class GridFieldConfig
{
    /// <summary>
    /// Nome do campo.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Rótulo do campo.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Largura da coluna.
    /// </summary>
    public string Width { get; set; } = "auto";

    /// <summary>
    /// Ordem de exibição.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indica se é visível.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Indica se é pesquisável.
    /// </summary>
    public bool IsSearchable { get; set; } = true;

    /// <summary>
    /// Indica se é ordenável.
    /// </summary>
    public bool IsSortable { get; set; } = true;
}

/// <summary>
/// Configuração de um campo no formulário.
/// </summary>
public class FormFieldConfig
{
    /// <summary>
    /// Nome do campo.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Rótulo do campo.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Ordem de exibição.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indica se é obrigatório.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indica se é somente leitura.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Tipo de input do formulário.
    /// </summary>
    public FormInputType InputType { get; set; } = FormInputType.Text;

    /// <summary>
    /// Placeholder do campo.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Valor padrão.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Validações customizadas.
    /// </summary>
    public List<string> Validations { get; set; } = new();
}

/// <summary>
/// Configuração do layout do formulário.
/// </summary>
public class FormLayoutConfig
{
    /// <summary>
    /// Campos do formulário.
    /// </summary>
    public List<FormFieldConfig> Fields { get; set; } = new();

    /// <summary>
    /// Número de colunas (1, 2, 3, 4).
    /// </summary>
    public int Columns { get; set; } = 2;

    /// <summary>
    /// Espaçamento entre campos.
    /// </summary>
    public string Spacing { get; set; } = "normal";
}

/// <summary>
/// Configuração do layout da grid.
/// </summary>
public class GridLayoutConfig
{
    /// <summary>
    /// Campos da grid.
    /// </summary>
    public List<GridFieldConfig> Fields { get; set; } = new();

    /// <summary>
    /// Número de registros por página.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Indica se é processada no servidor.
    /// </summary>
    public bool ServerSide { get; set; } = true;

    /// <summary>
    /// Indica se tem busca.
    /// </summary>
    public bool HasSearch { get; set; } = true;

    /// <summary>
    /// Indica se tem filtros.
    /// </summary>
    public bool HasFilters { get; set; } = true;

    /// <summary>
    /// Indica se tem exportação.
    /// </summary>
    public bool HasExport { get; set; } = true;
}
