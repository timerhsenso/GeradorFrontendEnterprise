using Xunit;
using Moq;
using GeradorFrontendEnterprise.Core.Contracts;
using GeradorFrontendEnterprise.Core.Models;
using GeradorFrontendEnterprise.Core.Enums;
using GeradorFrontendEnterprise.Services.Orchestrator;
using Microsoft.Extensions.Logging;

namespace GeradorFrontendEnterprise.Tests.Unit;

public class OrchestratorServiceTests
{
    private readonly Mock<ISchemaReader> _mockSchemaReader;
    private readonly Mock<IManifestClient> _mockManifestClient;
    private readonly Mock<IGeneratorService> _mockGeneratorService;
    private readonly Mock<ILogger<OrchestratorService>> _mockLogger;
    private readonly OrchestratorService _orchestrator;

    public OrchestratorServiceTests()
    {
        _mockSchemaReader = new Mock<ISchemaReader>();
        _mockManifestClient = new Mock<IManifestClient>();
        _mockGeneratorService = new Mock<IGeneratorService>();
        _mockLogger = new Mock<ILogger<OrchestratorService>>();

        _orchestrator = new OrchestratorService(
            _mockSchemaReader.Object,
            _mockManifestClient.Object,
            null,
            _mockGeneratorService.Object,
            _mockLogger.Object,
            Path.Combine(Path.GetTempPath(), "configs")
        );
    }

    [Fact]
    public async Task InitializeWizardAsync_WithValidEntityId_ReturnsSuccessfulResult()
    {
        // Arrange
        var entityId = "TestEntity";
        var manifest = new EntityManifest { EntityId = entityId, EntityName = "Test Entity" };
        var schema = new TableSchema { TableName = "test_entity" };

        _mockManifestClient.Setup(m => m.GetEntityManifestAsync(entityId))
            .ReturnsAsync(manifest);
        _mockSchemaReader.Setup(s => s.ReadTableSchemaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(schema);

        // Act
        var result = await _orchestrator.InitializeWizardAsync(entityId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.Manifest);
        Assert.NotNull(result.Schema);
    }

    [Fact]
    public async Task DetectConflictsAsync_WithConflictingData_ReturnsConflicts()
    {
        // Arrange
        var entityId = "TestEntity";

        // Act
        var conflicts = await _orchestrator.DetectConflictsAsync(entityId);

        // Assert
        Assert.NotNull(conflicts);
        Assert.IsType<List<Conflict>>(conflicts);
    }

    [Fact]
    public async Task ValidateConfigurationAsync_WithValidConfig_ReturnsValidResult()
    {
        // Arrange
        var config = new WizardConfig
        {
            EntityId = "TestEntity",
            EntityName = "Test Entity",
            GridLayout = new GridLayoutConfig { Fields = new List<GridFieldConfig> { new GridFieldConfig { FieldName = "Id" } } },
            FormLayout = new FormLayoutConfig { Fields = new List<FormFieldConfig> { new FormFieldConfig { FieldName = "Id" } } },
            FormFields = new List<FormField> { new FormField { Field = "Id", Label = "ID" } }
        };

        // Act
        var result = await _orchestrator.ValidateConfigurationAsync(config);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task SaveConfigurationAsync_WithValidConfig_ReturnConfigId()
    {
        // Arrange
        var config = new WizardConfig { EntityId = "TestEntity" };

        // Act
        var configId = await _orchestrator.SaveConfigurationAsync(config);

        // Assert
        Assert.NotNull(configId);
        Assert.NotEmpty(configId);
    }

    [Fact]
    public async Task LoadConfigurationAsync_WithValidConfigId_ReturnsConfiguration()
    {
        // Arrange
        var config = new WizardConfig { EntityId = "TestEntity" };
        var configId = await _orchestrator.SaveConfigurationAsync(config);

        // Act
        var loadedConfig = await _orchestrator.LoadConfigurationAsync(configId);

        // Assert
        Assert.NotNull(loadedConfig);
        Assert.Equal(config.EntityId, loadedConfig.EntityId);
    }

    [Fact]
    public async Task GetGenerationHistoryAsync_WithValidEntityId_ReturnsHistory()
    {
        // Arrange
        var entityId = "TestEntity";

        // Act
        var history = await _orchestrator.GetGenerationHistoryAsync(entityId);

        // Assert
        Assert.NotNull(history);
        Assert.IsType<List<GenerationSummary>>(history);
    }
}
