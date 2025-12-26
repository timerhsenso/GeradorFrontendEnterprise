namespace GeradorFrontendEnterprise.Infrastructure.SchemaReader;

using System.Data;
using System.Data.SqlClient;
using GeradorFrontendEnterprise.Core.Contracts;
using GeradorFrontendEnterprise.Core.Enums;
using GeradorFrontendEnterprise.Core.Models;

/// <summary>
/// Implementação do leitor de schema para SQL Server.
/// Extrai estrutura completa de tabelas usando Information_Schema e Extended Properties.
/// </summary>
public class SqlServerSchemaReader : ISchemaReader
{
    private readonly ILogger<SqlServerSchemaReader> _logger;

    public SqlServerSchemaReader(ILogger<SqlServerSchemaReader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lê o schema completo de uma tabela.
    /// </summary>
    public async Task<TableSchema> ReadTableSchemaAsync(
        string connectionString,
        string schemaName,
        string tableName)
    {
        _logger.LogInformation("Lendo schema da tabela [{Schema}].[{Table}]", schemaName, tableName);

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var tableSchema = new TableSchema
        {
            SchemaName = schemaName,
            TableName = tableName,
            ReadAt = DateTime.UtcNow
        };

        // 1. Ler colunas
        tableSchema.Columns = await ReadColumnsAsync(connection, schemaName, tableName);
        _logger.LogInformation("Lidas {Count} colunas", tableSchema.Columns.Count);

        // 2. Ler chave primária
        tableSchema.PrimaryKey = await ReadPrimaryKeyAsync(connection, schemaName, tableName);
        if (tableSchema.PrimaryKey != null)
            _logger.LogInformation("PK encontrada: {Columns}", string.Join(", ", tableSchema.PrimaryKey.Columns));

        // 3. Ler chaves estrangeiras
        tableSchema.ForeignKeys = await ReadForeignKeysAsync(connection, schemaName, tableName);
        _logger.LogInformation("Lidas {Count} chaves estrangeiras", tableSchema.ForeignKeys.Count);

        // 4. Ler índices
        tableSchema.Indexes = await ReadIndexesAsync(connection, schemaName, tableName);
        _logger.LogInformation("Lidos {Count} índices", tableSchema.Indexes.Count);

        // 5. Ler propriedades estendidas
        tableSchema.ExtendedProperties = await ReadExtendedPropertiesAsync(connection, schemaName, tableName);
        _logger.LogInformation("Lidas {Count} propriedades estendidas", tableSchema.ExtendedProperties.Count);

        // 6. Validar schema
        var validationErrors = tableSchema.Validate();
        if (validationErrors.Any())
        {
            _logger.LogWarning("Erros na validação do schema: {Errors}", string.Join("; ", validationErrors));
        }

        return tableSchema;
    }

    /// <summary>
    /// Lê todas as tabelas de um schema.
    /// </summary>
    public async Task<List<TableSchema>> ReadAllTablesAsync(
        string connectionString,
        string schemaName)
    {
        _logger.LogInformation("Lendo todas as tabelas do schema [{Schema}]", schemaName);

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var tables = new List<TableSchema>();

        var sql = @"
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = @SchemaName 
            AND TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_NAME";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var tableName = reader.GetString(0);
            var tableSchema = await ReadTableSchemaAsync(connectionString, schemaName, tableName);
            tables.Add(tableSchema);
        }

        _logger.LogInformation("Lidas {Count} tabelas", tables.Count);
        return tables;
    }

    /// <summary>
    /// Valida a consistência entre schema do banco e manifesto.
    /// </summary>
    public async Task<ValidationResult> ValidateConsistencyAsync(
        TableSchema dbSchema,
        EntityManifest manifest)
    {
        _logger.LogInformation("Validando consistência entre banco e manifesto para {Entity}", manifest.EntityId);

        var result = new ValidationResult { IsValid = true };

        // 1. Validar que a tabela existe no banco
        if (string.IsNullOrEmpty(dbSchema.TableName))
        {
            result.AddError("Tabela não encontrada no banco de dados.");
            return result;
        }

        // 2. Validar campos
        foreach (var manifestField in manifest.Fields)
        {
            var dbColumn = dbSchema.Columns.FirstOrDefault(c => 
                c.ColumnName.Equals(manifestField.FieldName, StringComparison.OrdinalIgnoreCase));

            if (dbColumn == null)
            {
                var conflict = new Conflict
                {
                    Type = ConflictType.FieldNotInDatabase,
                    FieldName = manifestField.FieldName,
                    ManifestValue = manifestField.ClrType,
                    Description = $"Campo '{manifestField.FieldName}' existe no manifesto mas não no banco."
                };
                result.AddConflict(conflict);
            }
            else
            {
                // Validar tipo
                if (dbColumn.ClrType?.Name != manifestField.ClrType)
                {
                    var conflict = new Conflict
                    {
                        Type = ConflictType.TypeMismatch,
                        FieldName = manifestField.FieldName,
                        DatabaseValue = dbColumn.ClrType?.Name,
                        ManifestValue = manifestField.ClrType,
                        Description = $"Tipo diferente para '{manifestField.FieldName}': banco={dbColumn.ClrType?.Name}, manifesto={manifestField.ClrType}"
                    };
                    result.AddConflict(conflict);
                }

                // Validar nullability
                if (dbColumn.IsNullable != !manifestField.IsRequired)
                {
                    var conflict = new Conflict
                    {
                        Type = ConflictType.NullabilityMismatch,
                        FieldName = manifestField.FieldName,
                        DatabaseValue = dbColumn.IsNullable ? "nullable" : "not null",
                        ManifestValue = manifestField.IsRequired ? "required" : "optional",
                        Description = $"Nullability diferente para '{manifestField.FieldName}'"
                    };
                    result.AddConflict(conflict);
                }
            }
        }

        // 3. Validar campos que existem no banco mas não no manifesto
        foreach (var dbColumn in dbSchema.Columns)
        {
            if (!manifest.Fields.Any(f => 
                f.FieldName.Equals(dbColumn.ColumnName, StringComparison.OrdinalIgnoreCase)))
            {
                result.AddWarning($"Campo '{dbColumn.ColumnName}' existe no banco mas não no manifesto.");
            }
        }

        return result;
    }

    /// <summary>
    /// Testa a conexão com o SQL Server.
    /// </summary>
    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            _logger.LogInformation("Conexão com SQL Server bem-sucedida");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar com SQL Server");
            return false;
        }
    }

    /// <summary>
    /// Lê as colunas de uma tabela.
    /// </summary>
    private async Task<List<ColumnSchema>> ReadColumnsAsync(
        SqlConnection connection,
        string schemaName,
        string tableName)
    {
        var columns = new List<ColumnSchema>();

        var sql = @"
            SELECT 
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.NUMERIC_PRECISION,
                c.NUMERIC_SCALE,
                c.COLUMN_DEFAULT,
                COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY,
                COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsComputed') AS IS_COMPUTED,
                ORDINAL_POSITION
            FROM INFORMATION_SCHEMA.COLUMNS c
            WHERE c.TABLE_SCHEMA = @SchemaName 
            AND c.TABLE_NAME = @TableName
            ORDER BY ORDINAL_POSITION";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);
        command.Parameters.AddWithValue("@TableName", tableName);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(0);
            var dataType = reader.GetString(1);
            var isNullable = reader.GetString(2) == "YES";
            var maxLength = reader.IsDBNull(3) ? null : (int?)reader.GetInt32(3);
            var precision = reader.IsDBNull(4) ? null : (int?)reader.GetByte(4);
            var scale = reader.IsDBNull(5) ? null : (int?)reader.GetByte(5);
            var defaultValue = reader.IsDBNull(6) ? null : reader.GetString(6);
            var isIdentity = reader.IsDBNull(7) ? false : reader.GetInt32(7) == 1;
            var isComputed = reader.IsDBNull(8) ? false : reader.GetInt32(8) == 1;
            var ordinalPosition = reader.GetInt32(9);

            var column = new ColumnSchema
            {
                ColumnName = columnName,
                DataType = dataType,
                SqlDataType = MapSqlDataType(dataType),
                ClrType = MapClrType(dataType, isNullable),
                IsNullable = isNullable,
                IsIdentity = isIdentity,
                IsComputed = isComputed,
                MaxLength = maxLength,
                Precision = precision,
                Scale = scale,
                DefaultValue = defaultValue,
                OrdinalPosition = ordinalPosition
            };

            columns.Add(column);
        }

        return columns;
    }

    /// <summary>
    /// Lê a chave primária de uma tabela.
    /// </summary>
    private async Task<PrimaryKeySchema?> ReadPrimaryKeyAsync(
        SqlConnection connection,
        string schemaName,
        string tableName)
    {
        var sql = @"
            SELECT 
                tc.CONSTRAINT_NAME,
                kcu.COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
                ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
            WHERE tc.TABLE_SCHEMA = @SchemaName 
            AND tc.TABLE_NAME = @TableName
            AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ORDER BY kcu.ORDINAL_POSITION";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);
        command.Parameters.AddWithValue("@TableName", tableName);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        var pk = new PrimaryKeySchema
        {
            ConstraintName = reader.GetString(0),
            Columns = new List<string> { reader.GetString(1) }
        };

        while (await reader.ReadAsync())
        {
            pk.Columns.Add(reader.GetString(1));
        }

        return pk;
    }

    /// <summary>
    /// Lê as chaves estrangeiras de uma tabela.
    /// </summary>
    private async Task<List<ForeignKeySchema>> ReadForeignKeysAsync(
        SqlConnection connection,
        string schemaName,
        string tableName)
    {
        var fks = new Dictionary<string, ForeignKeySchema>();

        var sql = @"
            SELECT 
                rc.CONSTRAINT_NAME,
                kcu.COLUMN_NAME,
                ccu.TABLE_SCHEMA,
                ccu.TABLE_NAME,
                ccu.COLUMN_NAME,
                rc.DELETE_RULE,
                rc.UPDATE_RULE
            FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
                ON rc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
            JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu 
                ON rc.UNIQUE_CONSTRAINT_NAME = ccu.CONSTRAINT_NAME
            WHERE kcu.TABLE_SCHEMA = @SchemaName 
            AND kcu.TABLE_NAME = @TableName
            ORDER BY rc.CONSTRAINT_NAME, kcu.ORDINAL_POSITION";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);
        command.Parameters.AddWithValue("@TableName", tableName);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var constraintName = reader.GetString(0);
            var columnName = reader.GetString(1);
            var refSchema = reader.GetString(2);
            var refTable = reader.GetString(3);
            var refColumn = reader.GetString(4);
            var deleteRule = reader.GetString(5);
            var updateRule = reader.GetString(6);

            if (!fks.ContainsKey(constraintName))
            {
                fks[constraintName] = new ForeignKeySchema
                {
                    ConstraintName = constraintName,
                    ReferencedSchema = refSchema,
                    ReferencedTable = refTable,
                    DeleteAction = deleteRule,
                    UpdateAction = updateRule
                };
            }

            fks[constraintName].ColumnNames.Add(columnName);
            fks[constraintName].ReferencedColumns.Add(refColumn);
        }

        return fks.Values.ToList();
    }

    /// <summary>
    /// Lê os índices de uma tabela.
    /// </summary>
    private async Task<List<IndexSchema>> ReadIndexesAsync(
        SqlConnection connection,
        string schemaName,
        string tableName)
    {
        var indexes = new Dictionary<string, IndexSchema>();

        var sql = @"
            SELECT 
                i.name,
                c.name,
                i.is_unique,
                i.type,
                i.is_primary_key
            FROM sys.indexes i
            INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            INNER JOIN sys.tables t ON i.object_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = @SchemaName 
            AND t.name = @TableName
            AND i.index_id > 0
            ORDER BY i.name, ic.key_ordinal";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);
        command.Parameters.AddWithValue("@TableName", tableName);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var indexName = reader.GetString(0);
            var columnName = reader.GetString(1);
            var isUnique = reader.GetBoolean(2);
            var indexType = reader.GetByte(3);
            var isPrimaryKey = reader.GetBoolean(4);

            if (!indexes.ContainsKey(indexName))
            {
                indexes[indexName] = new IndexSchema
                {
                    IndexName = indexName,
                    IsUnique = isUnique,
                    IsClustered = indexType == 1,
                    IsPrimaryKey = isPrimaryKey
                };
            }

            indexes[indexName].ColumnNames.Add(columnName);
        }

        return indexes.Values.ToList();
    }

    /// <summary>
    /// Lê as propriedades estendidas de uma tabela.
    /// </summary>
    private async Task<Dictionary<string, string>> ReadExtendedPropertiesAsync(
        SqlConnection connection,
        string schemaName,
        string tableName)
    {
        var properties = new Dictionary<string, string>();

        var sql = @"
            SELECT 
                ep.name,
                ep.value
            FROM sys.extended_properties ep
            INNER JOIN sys.tables t ON ep.major_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = @SchemaName 
            AND t.name = @TableName
            AND ep.minor_id = 0";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SchemaName", schemaName);
        command.Parameters.AddWithValue("@TableName", tableName);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var name = reader.GetString(0);
            var value = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            properties[name] = value;
        }

        return properties;
    }

    /// <summary>
    /// Mapeia tipo SQL para SqlDataType enum.
    /// </summary>
    private SqlDataType MapSqlDataType(string sqlType)
    {
        return sqlType.ToLower() switch
        {
            "bigint" => SqlDataType.BigInt,
            "int" => SqlDataType.Int,
            "smallint" => SqlDataType.SmallInt,
            "tinyint" => SqlDataType.TinyInt,
            "decimal" => SqlDataType.Decimal,
            "numeric" => SqlDataType.Numeric,
            "float" => SqlDataType.Float,
            "real" => SqlDataType.Real,
            "char" => SqlDataType.Char,
            "varchar" => SqlDataType.VarChar,
            "nchar" => SqlDataType.NChar,
            "nvarchar" => SqlDataType.NVarChar,
            "text" => SqlDataType.Text,
            "ntext" => SqlDataType.NText,
            "datetime" => SqlDataType.DateTime,
            "datetime2" => SqlDataType.DateTime2,
            "smalldatetime" => SqlDataType.SmallDateTime,
            "date" => SqlDataType.Date,
            "time" => SqlDataType.Time,
            "datetimeoffset" => SqlDataType.DateTimeOffset,
            "binary" => SqlDataType.Binary,
            "varbinary" => SqlDataType.VarBinary,
            "image" => SqlDataType.Image,
            "bit" => SqlDataType.Bit,
            "uniqueidentifier" => SqlDataType.Uniqueidentifier,
            "xml" => SqlDataType.Xml,
            "json" => SqlDataType.Json,
            _ => SqlDataType.Unknown
        };
    }

    /// <summary>
    /// Mapeia tipo SQL para tipo CLR.
    /// </summary>
    private Type? MapClrType(string sqlType, bool isNullable)
    {
        var baseType = sqlType.ToLower() switch
        {
            "bigint" => typeof(long),
            "int" => typeof(int),
            "smallint" => typeof(short),
            "tinyint" => typeof(byte),
            "decimal" => typeof(decimal),
            "numeric" => typeof(decimal),
            "float" => typeof(double),
            "real" => typeof(float),
            "char" => typeof(string),
            "varchar" => typeof(string),
            "nchar" => typeof(string),
            "nvarchar" => typeof(string),
            "text" => typeof(string),
            "ntext" => typeof(string),
            "datetime" => typeof(DateTime),
            "datetime2" => typeof(DateTime),
            "smalldatetime" => typeof(DateTime),
            "date" => typeof(DateTime),
            "time" => typeof(TimeSpan),
            "datetimeoffset" => typeof(DateTimeOffset),
            "binary" => typeof(byte[]),
            "varbinary" => typeof(byte[]),
            "image" => typeof(byte[]),
            "bit" => typeof(bool),
            "uniqueidentifier" => typeof(Guid),
            "xml" => typeof(string),
            "json" => typeof(string),
            _ => typeof(object)
        };

        if (isNullable && baseType.IsValueType)
        {
            return typeof(Nullable<>).MakeGenericType(baseType);
        }

        return baseType;
    }
}
