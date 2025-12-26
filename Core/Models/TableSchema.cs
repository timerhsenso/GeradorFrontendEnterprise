namespace GeradorFrontendEnterprise.Core.Models;

using GeradorFrontendEnterprise.Core.Enums;

/// <summary>
/// Representa o schema completo de uma tabela SQL Server.
/// Fonte da verdade para estrutura de dados.
/// </summary>
public class TableSchema
{
    /// <summary>
    /// Nome do schema SQL (ex: "dbo", "rhu", "tre").
    /// </summary>
    public string SchemaName { get; set; } = "dbo";

    /// <summary>
    /// Nome da tabela (ex: "TreTiposTreinamento").
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da tabela (do extended property).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Lista de colunas da tabela.
    /// </summary>
    public List<ColumnSchema> Columns { get; set; } = new();

    /// <summary>
    /// Definição da chave primária.
    /// </summary>
    public PrimaryKeySchema? PrimaryKey { get; set; }

    /// <summary>
    /// Lista de chaves estrangeiras.
    /// </summary>
    public List<ForeignKeySchema> ForeignKeys { get; set; } = new();

    /// <summary>
    /// Lista de índices.
    /// </summary>
    public List<IndexSchema> Indexes { get; set; } = new();

    /// <summary>
    /// Propriedades estendidas (metadados do SQL).
    /// </summary>
    public Dictionary<string, string> ExtendedProperties { get; set; } = new();

    /// <summary>
    /// Data/hora da leitura do schema.
    /// </summary>
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Hash SHA256 do schema (para idempotência).
    /// </summary>
    public string? SchemaHash { get; set; }

    /// <summary>
    /// Obtém o nome completo qualificado da tabela.
    /// </summary>
    public string GetFullyQualifiedName() => $"[{SchemaName}].[{TableName}]";

    /// <summary>
    /// Obtém a coluna de chave primária.
    /// </summary>
    public ColumnSchema? GetPrimaryKeyColumn()
    {
        if (PrimaryKey?.Columns.Count > 0)
        {
            return Columns.FirstOrDefault(c => c.ColumnName == PrimaryKey.Columns[0]);
        }
        return null;
    }

    /// <summary>
    /// Obtém todas as colunas que são chaves estrangeiras.
    /// </summary>
    public List<ColumnSchema> GetForeignKeyColumns()
    {
        var fkColumnNames = ForeignKeys
            .SelectMany(fk => fk.ColumnNames)
            .Distinct()
            .ToList();

        return Columns.Where(c => fkColumnNames.Contains(c.ColumnName)).ToList();
    }

    /// <summary>
    /// Obtém todas as colunas que são obrigatórias (NOT NULL).
    /// </summary>
    public List<ColumnSchema> GetRequiredColumns()
    {
        return Columns.Where(c => !c.IsNullable && !c.IsIdentity && !c.IsComputed).ToList();
    }

    /// <summary>
    /// Valida a integridade do schema.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SchemaName))
            errors.Add("SchemaName não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(TableName))
            errors.Add("TableName não pode estar vazio.");

        if (Columns.Count == 0)
            errors.Add("A tabela deve ter pelo menos uma coluna.");

        if (PrimaryKey == null || PrimaryKey.Columns.Count == 0)
            errors.Add("A tabela deve ter uma chave primária definida.");

        // Validar que colunas da PK existem
        if (PrimaryKey != null)
        {
            foreach (var pkCol in PrimaryKey.Columns)
            {
                if (!Columns.Any(c => c.ColumnName == pkCol))
                    errors.Add($"Coluna da PK '{pkCol}' não encontrada na tabela.");
            }
        }

        // Validar que colunas das FKs existem
        foreach (var fk in ForeignKeys)
        {
            foreach (var fkCol in fk.ColumnNames)
            {
                if (!Columns.Any(c => c.ColumnName == fkCol))
                    errors.Add($"Coluna da FK '{fkCol}' não encontrada na tabela.");
            }
        }

        return errors;
    }
}

/// <summary>
/// Representa uma coluna individual de uma tabela.
/// </summary>
public class ColumnSchema
{
    /// <summary>
    /// Nome da coluna.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de dado SQL (ex: "varchar", "int", "datetime2").
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de dado SQL mapeado para enum.
    /// </summary>
    public SqlDataType SqlDataType { get; set; } = SqlDataType.Unknown;

    /// <summary>
    /// Tipo CLR correspondente (ex: typeof(string), typeof(int)).
    /// </summary>
    public Type? ClrType { get; set; }

    /// <summary>
    /// Indica se a coluna aceita NULL.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Indica se a coluna é identity (auto-increment).
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Indica se a coluna é computada.
    /// </summary>
    public bool IsComputed { get; set; }

    /// <summary>
    /// Comprimento máximo (para varchar, nvarchar, etc).
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Precisão (para decimal, numeric).
    /// </summary>
    public int? Precision { get; set; }

    /// <summary>
    /// Escala (para decimal, numeric).
    /// </summary>
    public int? Scale { get; set; }

    /// <summary>
    /// Valor padrão da coluna.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Descrição da coluna (do extended property).
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Propriedades estendidas.
    /// </summary>
    public Dictionary<string, string> ExtendedProperties { get; set; } = new();

    /// <summary>
    /// Ordinal da coluna na tabela.
    /// </summary>
    public int OrdinalPosition { get; set; }

    /// <summary>
    /// Obtém a representação em string do tipo CLR.
    /// </summary>
    public string GetClrTypeName()
    {
        if (ClrType == null)
            return "object";

        if (IsNullable && ClrType.IsValueType)
            return $"{ClrType.Name}?";

        return ClrType.Name;
    }

    /// <summary>
    /// Valida a integridade da coluna.
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ColumnName))
            errors.Add("ColumnName não pode estar vazio.");

        if (string.IsNullOrWhiteSpace(DataType))
            errors.Add("DataType não pode estar vazio.");

        if (ClrType == null)
            errors.Add($"ClrType não foi mapeado para {DataType}.");

        return errors;
    }
}

/// <summary>
/// Representa a chave primária de uma tabela.
/// </summary>
public class PrimaryKeySchema
{
    /// <summary>
    /// Nome da constraint de PK.
    /// </summary>
    public string ConstraintName { get; set; } = string.Empty;

    /// <summary>
    /// Nomes das colunas que compõem a PK.
    /// </summary>
    public List<string> Columns { get; set; } = new();

    /// <summary>
    /// Indica se é clustered (padrão é true).
    /// </summary>
    public bool IsClustered { get; set; } = true;

    /// <summary>
    /// Obtém o nome da coluna única (para PKs simples).
    /// </summary>
    public string? GetSingleColumnName()
    {
        return Columns.Count == 1 ? Columns[0] : null;
    }
}

/// <summary>
/// Representa uma chave estrangeira.
/// </summary>
public class ForeignKeySchema
{
    /// <summary>
    /// Nome da constraint de FK.
    /// </summary>
    public string ConstraintName { get; set; } = string.Empty;

    /// <summary>
    /// Nomes das colunas locais que formam a FK.
    /// </summary>
    public List<string> ColumnNames { get; set; } = new();

    /// <summary>
    /// Schema da tabela referenciada.
    /// </summary>
    public string ReferencedSchema { get; set; } = "dbo";

    /// <summary>
    /// Nome da tabela referenciada.
    /// </summary>
    public string ReferencedTable { get; set; } = string.Empty;

    /// <summary>
    /// Nomes das colunas referenciadas.
    /// </summary>
    public List<string> ReferencedColumns { get; set; } = new();

    /// <summary>
    /// Ação ao deletar (CASCADE, SET NULL, NO ACTION, etc).
    /// </summary>
    public string? DeleteAction { get; set; }

    /// <summary>
    /// Ação ao atualizar.
    /// </summary>
    public string? UpdateAction { get; set; }

    /// <summary>
    /// Obtém o nome completo qualificado da tabela referenciada.
    /// </summary>
    public string GetReferencedTableFullyQualified()
    {
        return $"[{ReferencedSchema}].[{ReferencedTable}]";
    }
}

/// <summary>
/// Representa um índice de tabela.
/// </summary>
public class IndexSchema
{
    /// <summary>
    /// Nome do índice.
    /// </summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>
    /// Nomes das colunas do índice.
    /// </summary>
    public List<string> ColumnNames { get; set; } = new();

    /// <summary>
    /// Indica se é unique.
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Indica se é clustered.
    /// </summary>
    public bool IsClustered { get; set; }

    /// <summary>
    /// Indica se é primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }
}
