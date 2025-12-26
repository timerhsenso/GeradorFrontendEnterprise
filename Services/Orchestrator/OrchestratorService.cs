namespace GeradorFrontendEnterprise.Services.Orchestrator;

using GeradorFrontendEnterprise.Core.Contracts;
using GeradorFrontendEnterprise.Core.Enums;
using ConflictResolution = GeradorFrontendEnterprise.Core.Enums.ConflictResolution;
using GeradorFrontendEnterprise.Core.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// Serviço orquestrador que coordena todo o fluxo de geração.
/// Responsável por integrar SchemaReader, ManifestClient e TemplateEngine.
/// </summary>
public class OrchestratorService : IOrchestratorService
{
    private readonly ISchemaReader _schemaReader;
    private readonly IManifestClient _manifestClient;
    private readonly ITemplateEngine _templateEngine;
    private readonly IGeneratorService _generatorService;
    private readonly ILogger<OrchestratorService> _logger;
    private readonly string _configStoragePath;

    public OrchestratorService(
        ISchemaReader schemaReader,
        IManifestClient manifestClient,
        ITemplateEngine templateEngine,
        IGeneratorService generatorService,
        ILogger<OrchestratorService> logger,
        string? configStoragePath = null)
    {
        _schemaReader = schemaReader;
        _manifestClient = manifestClient;
        _templateEngine = templateEngine;
        _generatorService = generatorService;
        _logger = logger;
        _configStoragePath = configStoragePath ?? "GeneratedConfigs";

        // Criar diretório de armazenamento se não existir
        if (!Directory.Exists(_configStoragePath))
        {
            Directory.CreateDirectory(_configStoragePath);
            _logger.LogInformation("Diretório de configurações criado: {Path}", _configStoragePath);
        }
    }

    /// <summary>
    /// Inicia o fluxo do wizard.
    /// </summary>
    public async Task<WizardInitializationResult> InitializeWizardAsync(string entityId)
    {
        _logger.LogInformation("Inicializando wizard para entidade: {EntityId}", entityId);

        var result = new WizardInitializationResult { IsSuccessful = false };

        try
        {
            // 1. Obter manifesto da entidade
            _logger.LogInformation("Obtendo manifesto para {EntityId}", entityId);
            var manifest = await _manifestClient.GetEntityManifestAsync(entityId);
            result.Manifest = manifest;

            // 2. Carregar schema do banco
            _logger.LogInformation("Carregando schema para {EntityId}", entityId);
            var schema = await LoadSchemaAsync(entityId);
            result.Schema = schema;

            // 3. Criar configuração padrão sugerida
            result.SuggestedConfig = CreateDefaultConfig(schema, manifest);

            result.IsSuccessful = true;
            _logger.LogInformation("Wizard inicializado com sucesso para {EntityId}", entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar wizard para {EntityId}", entityId);
            result.Errors.Add($"Erro ao inicializar wizard: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Carrega o schema da entidade.
    /// </summary>
    public async Task<TableSchema> LoadSchemaAsync(string entityId)
    {
        _logger.LogInformation("Carregando schema para entidade: {EntityId}", entityId);

        try
        {
            // Obter manifesto para determinar tabela
            var manifest = await _manifestClient.GetEntityManifestAsync(entityId);

            // Extrair schema e tabela do manifesto (assumindo formato "schema.table")
            var parts = manifest.TableName.Split('.');
            var schemaName = parts.Length > 1 ? parts[0] : "dbo";
            var tableName = parts.Length > 1 ? parts[1] : manifest.TableName;

            // Ler schema do banco
            var schema = await _schemaReader.ReadTableSchemaAsync(
                manifest.ConnectionString ?? "Server=localhost;Database=master;Trusted_Connection=true;",
                schemaName,
                tableName);

            _logger.LogInformation("Schema carregado com sucesso para {EntityId}", entityId);
            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar schema para {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Detecta conflitos entre banco e manifesto.
    /// </summary>
    public async Task<List<Conflict>> DetectConflictsAsync(string entityId)
    {
        _logger.LogInformation("Detectando conflitos para entidade: {EntityId}", entityId);

        try
        {
            var schema = await LoadSchemaAsync(entityId);
            var manifest = await _manifestClient.GetEntityManifestAsync(entityId);

            var conflicts = new List<Conflict>();

            // 1. Validar campos
            foreach (var manifestField in manifest.Fields)
            {
                var dbColumn = schema.Columns.FirstOrDefault(c =>
                    c.ColumnName.Equals(manifestField.FieldName, StringComparison.OrdinalIgnoreCase));

                if (dbColumn == null)
                {
                    conflicts.Add(new Conflict
                    {
                        Type = ConflictType.FieldNotInDatabase,
                        FieldName = manifestField.FieldName,
                        ManifestValue = manifestField.ClrType,
                        Description = $"Campo '{manifestField.FieldName}' existe no manifesto mas não no banco."
                    });
                }
                else
                {
                    // Validar tipo
                    if (dbColumn.ClrType?.Name != manifestField.ClrType)
                    {
                        conflicts.Add(new Conflict
                        {
                            Type = ConflictType.TypeMismatch,
                            FieldName = manifestField.FieldName,
                            DatabaseValue = dbColumn.ClrType?.Name,
                            ManifestValue = manifestField.ClrType,
                            Description = $"Tipo diferente: banco={dbColumn.ClrType?.Name}, manifesto={manifestField.ClrType}"
                        });
                    }

                    // Validar nullability
                    if (dbColumn.IsNullable != !manifestField.IsRequired)
                    {
                        conflicts.Add(new Conflict
                        {
                            Type = ConflictType.NullabilityMismatch,
                            FieldName = manifestField.FieldName,
                            DatabaseValue = dbColumn.IsNullable ? "nullable" : "not null",
                            ManifestValue = manifestField.IsRequired ? "required" : "optional",
                            Description = $"Nullability diferente para '{manifestField.FieldName}'"
                        });
                    }
                }
            }

            // 2. Validar campos que existem no banco mas não no manifesto
            foreach (var dbColumn in schema.Columns)
            {
                if (!manifest.Fields.Any(f =>
                    f.FieldName.Equals(dbColumn.ColumnName, StringComparison.OrdinalIgnoreCase)))
                {
                    conflicts.Add(new Conflict
                    {
                        Type = ConflictType.FieldNotInManifest,
                        FieldName = dbColumn.ColumnName,
                        DatabaseValue = dbColumn.ClrType?.Name,
                        Description = $"Campo '{dbColumn.ColumnName}' existe no banco mas não no manifesto."
                    });
                }
            }

            _logger.LogInformation("Detectados {Count} conflitos para {EntityId}", conflicts.Count, entityId);
            return conflicts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao detectar conflitos para {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Resolve conflitos de acordo com as decisões do usuário.
    /// </summary>
    public async Task<ConflictResolutionResult> ResolveConflictsAsync(
        string entityId,
        Dictionary<string, ConflictResolution> resolutions)
    {
        _logger.LogInformation("Resolvendo {Count} conflitos para {EntityId}", 
            resolutions.Count, entityId);

        var result = new ConflictResolutionResult { IsSuccessful = true };

        try
        {
            var conflicts = await DetectConflictsAsync(entityId);

            foreach (var conflict in conflicts)
            {
                var key = $"{conflict.FieldName}_{conflict.Type}";

                if (resolutions.TryGetValue(key, out var resolution))
                {
                    _logger.LogInformation("Resolvendo conflito {Key} com estratégia {Strategy}",
                        key, resolution.ToString());

                    // Aplicar resolução (será implementado em camadas superiores)
                }
                else
                {
                    result.UnresolvedConflicts.Add(conflict);
                    result.Warnings.Add($"Conflito não resolvido: {conflict.Description}");
                }
            }

            if (result.UnresolvedConflicts.Any())
            {
                result.IsSuccessful = false;
            }

            _logger.LogInformation("Resolução de conflitos concluída. Não resolvidos: {Count}",
                result.UnresolvedConflicts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resolver conflitos para {EntityId}", entityId);
            result.IsSuccessful = false;
            result.Errors.Add($"Erro ao resolver conflitos: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Valida a configuração do wizard.
    /// </summary>
    public async Task<ValidationResult> ValidateConfigurationAsync(WizardConfig config)
    {
        _logger.LogInformation("Validando configuração para entidade: {EntityId}", config.EntityId);

        var result = new ValidationResult { IsValid = true };

        try
        {
            // 1. Validar modelo
            var errors = config.Validate();
            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    result.AddError(error);
                }
                return result;
            }

            // 2. Validar que a entidade existe
            var manifest = await _manifestClient.GetEntityManifestAsync(config.EntityId);
            if (manifest == null)
            {
                result.AddError($"Entidade '{config.EntityId}' não encontrada no manifesto.");
                return result;
            }

            // 3. Validar que o schema existe
            var schema = await LoadSchemaAsync(config.EntityId);
            if (schema.Columns.Count == 0)
            {
                result.AddError($"Schema para '{config.EntityId}' não contém colunas.");
                return result;
            }

            // 4. Validar campos da grid
            foreach (var gridField in config.GridLayout.Fields)
            {
                var column = schema.Columns.FirstOrDefault(c =>
                    c.ColumnName.Equals(gridField.FieldName, StringComparison.OrdinalIgnoreCase));

                if (column == null)
                {
                    result.AddWarning($"Campo '{gridField.FieldName}' da grid não encontrado no schema.");
                }
            }

            // 5. Validar campos do form
            foreach (var formField in config.FormLayout.Fields)
            {
                var column = schema.Columns.FirstOrDefault(c =>
                    c.ColumnName.Equals(formField.FieldName, StringComparison.OrdinalIgnoreCase));

                if (column == null)
                {
                    result.AddWarning($"Campo '{formField.FieldName}' do form não encontrado no schema.");
                }
            }

            _logger.LogInformation("Configuração validada com sucesso para {EntityId}", config.EntityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar configuração para {EntityId}", config.EntityId);
            result.AddError($"Erro ao validar configuração: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Gera o código a partir da configuração.
    /// </summary>
    public async Task<GenerationResult> GenerateCodeAsync(WizardConfig config)
    {
        _logger.LogInformation("Iniciando geração de código para {EntityId}", config.EntityId);

        var startTime = DateTime.UtcNow;

        try
        {
            // 1. Validar configuração
            var validationResult = await ValidateConfigurationAsync(config);
            if (!validationResult.IsValid)
            {
                return new GenerationResult
                {
                    IsSuccessful = false,
                    Errors = validationResult.Errors,
                    EntityId = config.EntityId
                };
            }

            // 2. Carregar schema e manifesto
            var schema = await LoadSchemaAsync(config.EntityId);
            var manifest = await _manifestClient.GetEntityManifestAsync(config.EntityId);

            // 3. Calcular hash da configuração
            config.ConfigHash = CalculateConfigHash(config);

            // 4. Gerar código
            var result = await _generatorService.GenerateAsync(config, schema, manifest);

            result.GeneratedAt = DateTime.UtcNow;
            result.DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation("Geração concluída em {Duration}ms para {EntityId}",
                result.DurationMs, config.EntityId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar código para {EntityId}", config.EntityId);
            return new GenerationResult
            {
                IsSuccessful = false,
                Errors = new List<string> { $"Erro ao gerar código: {ex.Message}" },
                EntityId = config.EntityId
            };
        }
    }

    /// <summary>
    /// Salva a configuração para reutilização futura.
    /// </summary>
    public async Task<string> SaveConfigurationAsync(WizardConfig config)
    {
        _logger.LogInformation("Salvando configuração para {EntityId}", config.EntityId);

        try
        {
            var configId = Guid.NewGuid().ToString();
            var fileName = Path.Combine(_configStoragePath, $"{configId}.json");

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(fileName, json);

            _logger.LogInformation("Configuração salva com ID: {ConfigId}", configId);
            return configId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar configuração para {EntityId}", config.EntityId);
            throw;
        }
    }

    /// <summary>
    /// Carrega uma configuração salva.
    /// </summary>
    public async Task<WizardConfig> LoadConfigurationAsync(string configId)
    {
        _logger.LogInformation("Carregando configuração: {ConfigId}", configId);

        try
        {
            var fileName = Path.Combine(_configStoragePath, $"{configId}.json");

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"Configuração não encontrada: {configId}");
            }

            var json = await File.ReadAllTextAsync(fileName);
            var config = JsonSerializer.Deserialize<WizardConfig>(json);

            if (config == null)
            {
                throw new InvalidOperationException("Configuração desserializada é nula.");
            }

            _logger.LogInformation("Configuração carregada com sucesso: {ConfigId}", configId);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar configuração: {ConfigId}", configId);
            throw;
        }
    }

    /// <summary>
    /// Obtém o histórico de gerações de uma entidade.
    /// </summary>
    public async Task<List<GenerationSummary>> GetGenerationHistoryAsync(string entityId)
    {
        _logger.LogInformation("Obtendo histórico de gerações para {EntityId}", entityId);

        try
        {
            var summaries = new List<GenerationSummary>();

            if (!Directory.Exists(_configStoragePath))
            {
                return summaries;
            }

            var files = Directory.GetFiles(_configStoragePath, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var config = JsonSerializer.Deserialize<WizardConfig>(json);

                    if (config?.EntityId == entityId)
                    {
                        summaries.Add(new GenerationSummary
                        {
                            ConfigId = Path.GetFileNameWithoutExtension(file),
                            EntityId = config.EntityId,
                            GeneratedAt = File.GetLastWriteTimeUtc(file),
                            ConfigHash = config.ConfigHash
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao ler arquivo de configuração: {File}", file);
                }
            }

            _logger.LogInformation("Histórico obtido: {Count} gerações para {EntityId}",
                summaries.Count, entityId);

            return summaries.OrderByDescending(s => s.GeneratedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter histórico de gerações para {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Cria uma configuração padrão sugerida.
    /// </summary>
    private WizardConfig CreateDefaultConfig(TableSchema schema, EntityManifest manifest)
    {
        _logger.LogInformation("Criando configuração padrão para {EntityId}", manifest.EntityId);

        var config = new WizardConfig
        {
            EntityId = manifest.EntityId,
            EntityName = manifest.EntityName,
            Module = manifest.Module,
            GridLayout = new GridLayoutConfig
            {
                Fields = schema.Columns
                    .Where(c => !c.IsComputed)
                    .Take(5)
                    .Select((c, i) => new GridFieldConfig
                    {
                        FieldName = c.ColumnName,
                        Label = FormatLabel(c.ColumnName),
                        Width = "auto",
                        Order = i,
                        IsVisible = true,
                        IsSearchable = true,
                        IsSortable = true
                    })
                    .ToList()
            },
            FormLayout = new FormLayoutConfig
            {
                Fields = schema.Columns
                    .Where(c => !c.IsComputed && !c.IsIdentity)
                    .Select((c, i) => new FormFieldConfig
                    {
                        FieldName = c.ColumnName,
                        Label = FormatLabel(c.ColumnName),
                        Order = i,
                        IsRequired = !c.IsNullable,
                        IsReadOnly = c.IsIdentity,
                        InputType = MapInputType(c.ClrType)
                    })
                    .ToList()
            }
        };

        return config;
    }

    /// <summary>
    /// Formata um nome de coluna para label legível.
    /// </summary>
    private string FormatLabel(string columnName)
    {
        // Converter camelCase ou snake_case para Title Case
        var result = System.Text.RegularExpressions.Regex.Replace(columnName, "([a-z])([A-Z])", "$1 $2");
        result = System.Text.RegularExpressions.Regex.Replace(result, "_", " ");
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result.ToLower());
    }

    /// <summary>
    /// Mapeia tipo CLR para tipo de input do form.
    /// </summary>
    private FormInputType MapInputType(Type? clrType)
    {
        if (clrType == null)
            return FormInputType.Text;

        var typeName = clrType.Name.Replace("Nullable`1", "").Replace("[", "").Replace("]", "");

        return typeName switch
        {
            "Int32" or "Int64" or "Int16" or "Byte" => FormInputType.Number,
            "Decimal" or "Double" or "Single" => FormInputType.Number,
            "Boolean" => FormInputType.Checkbox,
            "DateTime" => FormInputType.DateTime,
            "DateTimeOffset" => FormInputType.DateTime,
            "TimeSpan" => FormInputType.Time,
            "Guid" => FormInputType.Text,
            "Byte[]" => FormInputType.File,
            _ => FormInputType.Text
        };
    }

    /// <summary>
    /// Calcula hash SHA256 da configuração.
    /// </summary>
    private string CalculateConfigHash(WizardConfig config)
    {
        var json = JsonSerializer.Serialize(config);
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash);
    }
}

/// <summary>
/// Extensão para registrar o Orchestrator Service na injeção de dependência.
/// </summary>
public static class OrchestratorServiceExtensions
{
    public static IServiceCollection AddOrchestratorService(
        this IServiceCollection services,
        string configStoragePath = "GeneratedConfigs")
    {
        services.AddScoped<IOrchestratorService>(sp =>
            new OrchestratorService(
                sp.GetRequiredService<ISchemaReader>(),
                sp.GetRequiredService<IManifestClient>(),
                sp.GetRequiredService<ITemplateEngine>(),
                sp.GetRequiredService<IGeneratorService>(),
                sp.GetRequiredService<ILogger<OrchestratorService>>(),
                configStoragePath));

        return services;
    }
}
