namespace GeradorFrontendEnterprise.Core.Models;

using GeradorFrontendEnterprise.Core.Enums;

/// <summary>
/// Representa os metadados de uma entidade obtidos do manifesto da API.
/// Fonte da verdade para regras de negócio e permissões.
/// </summary>
public class EntityManifest
{
    /// <summary>
    /// Identificador único da entidade (ex: "TreTiposTreinamento").
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Nome amigável da entidade (ex: "Tipos de Treinamento").
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da entidade.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Módulo ao qual a entidade pertence (ex: "Treinamento", "RH").
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Código do sistema (para controle de acesso).
    /// </summary>
    public int CdSistema { get; set; }

    /// <summary>
    /// Código da função (para controle de acesso).
    /// </summary>
    public int CdFuncao { get; set; }

    /// <summary>
    /// Schema do banco de dados (ex: "dbo", "rhu").
    /// </summary>
    public string DatabaseSchema { get; set; } = "dbo";

    /// <summary>
    /// Nome da tabela no banco de dados.
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// String de conexão do SQL Server.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Rotas da API para CRUD.
    /// </summary>
    public ManifestRoutes Routes { get; set; } = new();

    /// <summary>
    /// Permissões de acesso.
    /// </summary>
    public List<PermissionManifest> Permissions { get; set; } = new();

    /// <summary>
    /// Campos da entidade conforme manifesto.
    /// </summary>
    public List<FieldManifest> Fields { get; set; } = new();

    /// <summary>
    /// Configurações adicionais.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Data/hora da leitura do manifesto.
    /// </summary>
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Obtém um campo pelo nome.
    /// </summary>
    public FieldManifest? GetField(string fieldName)
    {
        return Fields.FirstOrDefault(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Obtém todos os campos que são chaves estrangeiras.
    /// </summary>
    public List<FieldManifest> GetForeignKeyFields()
    {
        return Fields.Where(f => f.IsForeignKey).ToList();
    }

    /// <summary>
    /// Obtém todos os campos obrigatórios.
    /// </summary>
    public List<FieldManifest> GetRequiredFields()
    {
        return Fields.Where(f => f.IsRequired && !f.IsIdentity).ToList();
    }

    /// <summary>
    /// Valida a integridade do manifesto.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(EntityId))
            errors.Add("EntityId não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(EntityName))
            errors.Add("EntityName não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(TableName))
            errors.Add("TableName não pode estar vazio.");

        if (CdSistema <= 0)
            errors.Add("CdSistema deve ser maior que zero.");

        if (CdFuncao <= 0)
            errors.Add("CdFuncao deve ser maior que zero.");

        if (Fields.Count == 0)
            errors.Add("A entidade deve ter pelo menos um campo.");

        return errors;
    }
}

/// <summary>
/// Representa as rotas da API para uma entidade.
/// </summary>
public class ManifestRoutes
{
    /// <summary>
    /// Rota para listar entidades (GET).
    /// </summary>
    public string? List { get; set; }

    /// <summary>
    /// Rota para obter uma entidade por ID (GET).
    /// </summary>
    public string? GetById { get; set; }

    /// <summary>
    /// Rota para criar uma entidade (POST).
    /// </summary>
    public string? Create { get; set; }

    /// <summary>
    /// Rota para atualizar uma entidade (PUT).
    /// </summary>
    public string? Update { get; set; }

    /// <summary>
    /// Rota para deletar uma entidade (DELETE).
    /// </summary>
    public string? Delete { get; set; }

    /// <summary>
    /// Rota para deletar múltiplas entidades (DELETE).
    /// </summary>
    public string? DeleteBatch { get; set; }

    /// <summary>
    /// Rota para exportar dados (GET).
    /// </summary>
    public string? Export { get; set; }

    /// <summary>
    /// Rota para importar dados (POST).
    /// </summary>
    public string? Import { get; set; }

    /// <summary>
    /// Obtém a rota base (extrai do primeiro endpoint disponível).
    /// </summary>
    public string? GetBaseRoute()
    {
        var route = List ?? GetById ?? Create ?? Update ?? Delete;
        if (string.IsNullOrEmpty(route))
            return null;

        // Extrai a parte base (ex: "/api/tipos-treinamento" de "/api/tipos-treinamento/list")
        var parts = route.Split('/');
        return string.Join('/', parts.Take(parts.Length - 1));
    }
}

/// <summary>
/// Representa uma permissão de acesso.
/// </summary>
public class PermissionManifest
{
    /// <summary>
    /// Tipo de permissão (ex: "Create", "Read", "Update", "Delete").
    /// </summary>
    public string PermissionType { get; set; } = string.Empty;

    /// <summary>
    /// Indica se está habilitada por padrão.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Descrição da permissão.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Roles que têm essa permissão.
    /// </summary>
    public List<string> AllowedRoles { get; set; } = new();
}

/// <summary>
/// Representa um campo de uma entidade no manifesto.
/// </summary>
public class FieldManifest
{
    /// <summary>
    /// Nome do campo (ex: "IdTipoTreinamento").
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Rótulo amigável (ex: "ID Tipo Treinamento").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Descrição do campo.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de dado CLR (ex: "int", "string", "datetime").
    /// </summary>
    public string ClrType { get; set; } = string.Empty;

    /// <summary>
    /// Indica se é chave primária.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Indica se é chave estrangeira.
    /// </summary>
    public bool IsForeignKey { get; set; }

    /// <summary>
    /// Indica se é obrigatório.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Indica se é identity (auto-increment).
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Indica se é computado.
    /// </summary>
    public bool IsComputed { get; set; }

    /// <summary>
    /// Comprimento máximo (para strings).
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Precisão (para decimais).
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Escala (para decimais).
    /// </summary>
    public int? Scale { get; set; }

    /// <summary>
    /// Valor padrão.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Tipo de input sugerido para formulário.
    /// </summary>
    public FormInputType SuggestedInputType { get; set; } = FormInputType.Text;

    /// <summary>
    /// Se é FK, informações sobre a tabela referenciada.
    /// </summary>
    public ForeignKeyInfo? ForeignKeyInfo { get; set; }

    /// <summary>
    /// Propriedades adicionais.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Valida a integridade do campo.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(FieldName))
            errors.Add("FieldName não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(ClrType))
            errors.Add("ClrType não pode estar vazio.");

        if (IsForeignKey && ForeignKeyInfo == null)
            errors.Add("ForeignKeyInfo deve estar preenchido para campos FK.");

        return errors;
    }
}

/// <summary>
/// Informações sobre uma chave estrangeira.
/// </summary>
public class ForeignKeyInfo
{
    /// <summary>
    /// Nome da tabela referenciada.
    /// </summary>
    public string ReferencedTable { get; set; } = string.Empty;

    /// <summary>
    /// Schema da tabela referenciada.
    /// </summary>
    public string ReferencedSchema { get; set; } = "dbo";

    /// <summary>
    /// Nome da coluna referenciada.
    /// </summary>
    public string ReferencedColumn { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint da API para carregar os dados da FK.
    /// </summary>
    public string? LookupEndpoint { get; set; }

    /// <summary>
    /// Campo a usar como valor no lookup.
    /// </summary>
    public string LookupValueField { get; set; } = "id";

    /// <summary>
    /// Campo a usar como texto no lookup.
    /// </summary>
    public string LookupTextField { get; set; } = "nome";

    /// <summary>
    /// Dependências de cascata (campos que disparam reload).
    /// </summary>
    public List<string> CascadeDependencies { get; set; } = new();

    /// <summary>
    /// Obtém o nome completo qualificado da tabela referenciada.
    /// </summary>
    public string GetReferencedTableFullyQualified()
    {
        return $"[{ReferencedSchema}].[{ReferencedTable}]";
    }
}
