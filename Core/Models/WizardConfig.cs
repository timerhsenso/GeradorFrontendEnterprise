namespace GeradorFrontendEnterprise.Core.Models;

using GeradorFrontendEnterprise.Core.Enums;

/// <summary>
/// Representa a configuração completa do wizard.
/// Fonte da verdade para layout visual e apresentação.
/// </summary>
public class WizardConfig
{
    /// <summary>
    /// Identificador único da configuração.
    /// </summary>
    public string ConfigId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identificador da entidade.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Nome da entidade.
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Módulo da entidade.
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Versão da configuração (para rastreabilidade).
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Data/hora da criação.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data/hora da última modificação.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Configuração do layout da grid.
    /// </summary>
    public GridLayoutConfig GridLayout { get; set; } = new();

    /// <summary>
    /// Configuração do layout do formulário.
    /// </summary>
    public FormLayoutConfig FormLayout { get; set; } = new();

    /// <summary>
    /// Campos do formulário.
    /// </summary>
    public List<FormField> FormFields { get; set; } = new();

    /// <summary>
    /// Resoluções de conflitos aplicadas.
    /// </summary>
    public Dictionary<string, Core.Enums.ConflictResolution> ConflictResolutions { get; set; } = new();

    /// <summary>
    /// Hash SHA256 da configuração (para idempotência).
    /// </summary>
    public string? ConfigHash { get; set; }

    /// <summary>
    /// Obtém um campo pelo nome.
    /// </summary>
    public FormField? GetField(string fieldName)
    {
        return FormFields.FirstOrDefault(f => f.Field.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Valida a integridade da configuração.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(EntityId))
            errors.Add("EntityId não pode estar vazio.");

        if (GridLayout.Fields.Count == 0)
            errors.Add("GridLayout deve ter pelo menos um campo.");

        if (FormLayout.Fields.Count == 0)
            errors.Add("FormLayout deve ter pelo menos um campo.");

        if (FormFields.Count == 0)
            errors.Add("Deve haver pelo menos um campo no formulário.");

        // Validar que todos os campos da grid existem em FormFields
        foreach (var field in GridLayout.Fields)
        {
            if (!FormFields.Any(f => f.Field == field.FieldName))
                errors.Add($"Campo '{field.FieldName}' da grid não encontrado em FormFields.");
        }

        // Validar que todos os campos do form existem
        foreach (var field in FormFields)
        {
            var fieldErrors = field.Validate();
            errors.AddRange(fieldErrors);
        }

        return errors;
    }
}

/// <summary>
/// Configuração do layout da grid (DataTables).
/// </summary>
public class GridLayout
{
    /// <summary>
    /// Colunas a exibir na grid.
    /// </summary>
    public List<GridColumn> Columns { get; set; } = new();

    /// <summary>
    /// Opções da grid.
    /// </summary>
    public GridOptions Options { get; set; } = new();
}

/// <summary>
/// Representação de uma coluna na grid.
/// </summary>
public class GridColumn
{
    /// <summary>
    /// Nome do campo.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Rótulo da coluna.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Ordem de exibição.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Largura da coluna (em pixels ou %).
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// Indica se é visível.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Indica se é ordenável.
    /// </summary>
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Indica se é filtrável.
    /// </summary>
    public bool Filterable { get; set; } = true;

    /// <summary>
    /// Tipo de formatação (ex: "date", "currency", "percentage").
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Alinhamento (left, center, right).
    /// </summary>
    public string Alignment { get; set; } = "left";

    /// <summary>
    /// Validação da coluna.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Field))
            errors.Add("Field não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(Label))
            errors.Add("Label não pode estar vazio.");

        return errors;
    }
}

/// <summary>
/// Opções da grid.
/// </summary>
public class GridOptions
{
    /// <summary>
    /// Processamento no servidor (true) ou cliente (false).
    /// </summary>
    public bool ServerSide { get; set; } = true;

    /// <summary>
    /// Tamanho padrão da página.
    /// </summary>
    public int PageSize { get; set; } = 10;

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

    /// <summary>
    /// Formatos de exportação suportados (csv, excel, pdf).
    /// </summary>
    public List<string> ExportFormats { get; set; } = new() { "csv", "excel", "pdf" };
}

/// <summary>
/// Configuração do layout do formulário.
/// </summary>
public class FormLayout
{
    /// <summary>
    /// Abas do formulário.
    /// </summary>
    public List<FormTab> Tabs { get; set; } = new();

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
/// Aba do formulário.
/// </summary>
public class FormTab
{
    /// <summary>
    /// Identificador da aba.
    /// </summary>
    public string TabId { get; set; } = string.Empty;

    /// <summary>
    /// Rótulo da aba.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Ordem de exibição.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Campos da aba.
    /// </summary>
    public List<string> Fields { get; set; } = new();
}

/// <summary>
/// Campo do formulário.
/// </summary>
public class FormField
{
    /// <summary>
    /// Nome do campo.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Rótulo do campo.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de input.
    /// </summary>
    public FormInputType InputType { get; set; } = FormInputType.Text;

    /// <summary>
    /// Indica se é obrigatório.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Indica se é somente leitura.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Placeholder.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Valor padrão.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Validações.
    /// </summary>
    public List<string> Validations { get; set; } = new();

    /// <summary>
    /// Validação do campo.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Field))
            errors.Add("Field não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(Label))
            errors.Add("Label não pode estar vazio.");

        return errors;
    }
}
