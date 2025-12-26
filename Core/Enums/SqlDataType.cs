namespace GeradorFrontendEnterprise.Core.Enums;

/// <summary>
/// Enumeração dos tipos de dados SQL Server suportados.
/// Mapeamento entre tipos SQL e tipos CLR.
/// </summary>
public enum SqlDataType
{
    // Numeric types
    BigInt,
    Int,
    SmallInt,
    TinyInt,
    Decimal,
    Numeric,
    Float,
    Real,
    
    // String types
    Char,
    VarChar,
    NChar,
    NVarChar,
    Text,
    NText,
    
    // Date/Time types
    DateTime,
    DateTime2,
    SmallDateTime,
    Date,
    Time,
    DateTimeOffset,
    
    // Binary types
    Binary,
    VarBinary,
    Image,
    
    // Other types
    Bit,
    Uniqueidentifier,
    Xml,
    Json,
    
    // Unknown
    Unknown
}

/// <summary>
/// Enumeração para tipos de conflito detectados.
/// </summary>
public enum ConflictType
{
    /// <summary>
    /// Campo existe no manifesto mas não no banco.
    /// </summary>
    FieldNotInDatabase,
    
    /// <summary>
    /// Campo existe no banco mas não no manifesto.
    /// </summary>
    FieldNotInManifest,
    
    /// <summary>
    /// Tipo CLR diferente entre manifesto e banco.
    /// </summary>
    TypeMismatch,
    
    /// <summary>
    /// Nullability diferente entre manifesto e banco.
    /// </summary>
    NullabilityMismatch,
    
    /// <summary>
    /// Primary Key diferente.
    /// </summary>
    PrimaryKeyMismatch,
    
    /// <summary>
    /// Foreign Key divergência.
    /// </summary>
    ForeignKeyMismatch
}

/// <summary>
/// Enumeração para resolução de conflitos.
/// </summary>
public enum ConflictResolution
{
    /// <summary>
    /// Usar valor do banco de dados.
    /// </summary>
    UseDatabase,
    
    /// <summary>
    /// Usar valor do manifesto.
    /// </summary>
    UseManifest,
    
    /// <summary>
    /// Ignorar o campo.
    /// </summary>
    Ignore,
    
    /// <summary>
    /// Requer ação manual do usuário.
    /// </summary>
    RequiresManualReview
}

/// <summary>
/// Enumeração para tipos de input em formulários.
/// </summary>
public enum FormInputType
{
    Text,
    TextArea,
    Number,
    Email,
    Password,
    Date,
    DateTime,
    Time,
    Select,
    MultiSelect,
    Checkbox,
    Radio,
    File,
    Hidden,
    Color,
    Tel,
    Url,
    Search,
    Range
}

/// <summary>
/// Enumeração para tipos de validação.
/// </summary>
public enum ValidationType
{
    Required,
    Email,
    Cpf,
    Cnpj,
    Regex,
    MinLength,
    MaxLength,
    Min,
    Max,
    Pattern,
    Custom
}

/// <summary>
/// Enumeração para contextos de visibilidade de campo.
/// </summary>
public enum FormContext
{
    Create,
    Edit,
    Details,
    List
}

/// <summary>
/// Enumeração para operadores de condição.
/// </summary>
public enum ConditionOperator
{
    Equals,
    NotEquals,
    Contains,
    NotContains,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    In,
    NotIn,
    StartsWith,
    EndsWith
}
