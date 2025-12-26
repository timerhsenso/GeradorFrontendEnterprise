using GeradorFrontendEnterprise.Core.Contracts;
using GeradorFrontendEnterprise.Core.Models;
using Newtonsoft.Json;

namespace GeradorFrontendEnterprise.Infrastructure.ManifestClient;

/// <summary>
/// Implementação do cliente de manifesto via HTTP.
/// Comunica com a API de manifesto para obter metadados de entidades.
/// </summary>
public class HttpManifestClient : IManifestClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpManifestClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;

    public HttpManifestClient(
        HttpClient httpClient,
        ILogger<HttpManifestClient> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        
        // Obter baseUrl da configuração
        _baseUrl = _configuration["ManifestApi:BaseUrl"] ?? "http://localhost:5000";
        _baseUrl = _baseUrl.TrimEnd('/');
        
        _logger.LogInformation("HttpManifestClient inicializado com BaseUrl: {BaseUrl}", _baseUrl);
    }

    /// <summary>
    /// Obtém o manifesto de uma entidade.
    /// </summary>
    public async Task<EntityManifest> GetEntityManifestAsync(string entityId)
    {
        _logger.LogInformation("Obtendo manifesto para entidade: {EntityId}", entityId);

        try
        {
            // Se não houver API configurada, retornar manifesto mock
            if (_baseUrl.Contains("localhost") || string.IsNullOrEmpty(_baseUrl))
            {
                _logger.LogWarning("API de manifesto não configurada, retornando manifesto mock");
                return GetMockManifest(entityId);
            }

            var url = $"{_baseUrl}/api/manifesto/entidades/{entityId}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao obter manifesto: {StatusCode} - {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                return GetMockManifest(entityId);
            }

            var content = await response.Content.ReadAsStringAsync();
            var manifest = JsonConvert.DeserializeObject<EntityManifest>(content);

            if (manifest == null)
            {
                _logger.LogWarning("Manifesto nulo para entidade: {EntityId}", entityId);
                return GetMockManifest(entityId);
            }

            _logger.LogInformation("Manifesto obtido com sucesso para entidade: {EntityId}", entityId);
            return manifest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter manifesto para entidade: {EntityId}", entityId);
            return GetMockManifest(entityId);
        }
    }

    /// <summary>
    /// Obtém todos os manifestos.
    /// </summary>
    public async Task<List<EntityManifest>> GetAllManifestsAsync()
    {
        _logger.LogInformation("Obtendo todos os manifestos");

        try
        {
            if (_baseUrl.Contains("localhost") || string.IsNullOrEmpty(_baseUrl))
            {
                _logger.LogWarning("API de manifesto não configurada, retornando manifestos mock");
                return GetMockManifests();
            }

            var url = $"{_baseUrl}/api/manifesto/entidades";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao obter manifestos: {StatusCode}", response.StatusCode);
                return GetMockManifests();
            }

            var content = await response.Content.ReadAsStringAsync();
            var manifests = JsonConvert.DeserializeObject<List<EntityManifest>>(content) ?? new List<EntityManifest>();

            _logger.LogInformation("Manifestos obtidos com sucesso: {Count}", manifests.Count);
            return manifests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter manifestos");
            return GetMockManifests();
        }
    }

    /// <summary>
    /// Obtém manifestos por módulo.
    /// </summary>
    public async Task<List<EntityManifest>> GetManifestsByModuleAsync(string module)
    {
        _logger.LogInformation("Obtendo manifestos do módulo: {Module}", module);

        try
        {
            if (_baseUrl.Contains("localhost") || string.IsNullOrEmpty(_baseUrl))
            {
                _logger.LogWarning("API de manifesto não configurada, retornando manifestos mock");
                return GetMockManifests().Where(m => m.Module == module).ToList();
            }

            var url = $"{_baseUrl}/api/manifesto/modulos/{module}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao obter manifestos do módulo: {Module}", module);
                return GetMockManifests().Where(m => m.Module == module).ToList();
            }

            var content = await response.Content.ReadAsStringAsync();
            var manifests = JsonConvert.DeserializeObject<List<EntityManifest>>(content) ?? new List<EntityManifest>();

            _logger.LogInformation("Manifestos do módulo obtidos com sucesso: {Module}, Count: {Count}", module, manifests.Count);
            return manifests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter manifestos do módulo: {Module}", module);
            return GetMockManifests().Where(m => m.Module == module).ToList();
        }
    }

    /// <summary>
    /// Valida se o usuário tem permissão para uma entidade.
    /// </summary>
    public async Task<bool> HasPermissionAsync(string entityId, string permissionType)
    {
        _logger.LogInformation("Verificando permissão {PermissionType} para entidade: {EntityId}", permissionType, entityId);

        try
        {
            var manifest = await GetEntityManifestAsync(entityId);
            if (manifest?.Permissions == null)
                return true;

            var hasPermission = manifest.Permissions.Any(p => 
                p.PermissionType.Equals(permissionType, StringComparison.OrdinalIgnoreCase));

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar permissão");
            return false;
        }
    }

    /// <summary>
    /// Obtém a URL base da API.
    /// </summary>
    public string GetBaseUrl()
    {
        return _baseUrl;
    }

    /// <summary>
    /// Testa a conexão com a API.
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        _logger.LogInformation("Testando conexão com API de manifesto");

        try
        {
            if (_baseUrl.Contains("localhost") || string.IsNullOrEmpty(_baseUrl))
            {
                _logger.LogWarning("API de manifesto não configurada, retornando true");
                return true;
            }

            var url = $"{_baseUrl}/health";
            var response = await _httpClient.GetAsync(url);
            var success = response.IsSuccessStatusCode;

            _logger.LogInformation("Teste de conexão: {Success}", success ? "OK" : "FALHA");
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao testar conexão");
            return false;
        }
    }

    /// <summary>
    /// Retorna um manifesto mock para desenvolvimento/testes.
    /// </summary>
    private EntityManifest GetMockManifest(string entityId)
    {
        return new EntityManifest
        {
            EntityId = entityId,
            EntityName = entityId,
            Module = "Default",
            TableName = entityId,
            DatabaseSchema = "dbo",
            CdSistema = 1,
            CdFuncao = 1,
            Routes = new ManifestRoutes 
            { 
                List = $"/api/{entityId.ToLower()}/list",
                GetById = $"/api/{entityId.ToLower()}/{{id}}",
                Create = $"/api/{entityId.ToLower()}",
                Update = $"/api/{entityId.ToLower()}/{{id}}",
                Delete = $"/api/{entityId.ToLower()}/{{id}}"
            },
            Permissions = new List<PermissionManifest>
            {
                new PermissionManifest { PermissionType = "Read", IsEnabled = true, AllowedRoles = new List<string> { "User" } },
                new PermissionManifest { PermissionType = "Create", IsEnabled = true, AllowedRoles = new List<string> { "Admin" } },
                new PermissionManifest { PermissionType = "Update", IsEnabled = true, AllowedRoles = new List<string> { "Admin" } },
                new PermissionManifest { PermissionType = "Delete", IsEnabled = true, AllowedRoles = new List<string> { "Admin" } }
            },
            Fields = new List<FieldManifest>(),
            ConnectionString = "Server=localhost;Database=YourDB;User Id=sa;Password=YourPassword;"
        };
    }

    /// <summary>
    /// Retorna manifestos mock para desenvolvimento/testes.
    /// </summary>
    private List<EntityManifest> GetMockManifests()
    {
        return new List<EntityManifest>
        {
            GetMockManifest("Customers"),
            GetMockManifest("Products"),
            GetMockManifest("Orders"),
            GetMockManifest("Invoices")
        };
    }
}
