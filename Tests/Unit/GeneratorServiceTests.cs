using Xunit;
using Moq;
using GeradorFrontendEnterprise.Core.Contracts;
using GeradorFrontendEnterprise.Core.Models;
using GeradorFrontendEnterprise.Services.Generator;
using Microsoft.Extensions.Logging;

namespace GeradorFrontendEnterprise.Tests.Unit;

public class GeneratorServiceTests
{
    private readonly Mock<ITemplateEngine> _mockTemplateEngine;
    private readonly Mock<ILogger<GeneratorService>> _mockLogger;
    private readonly GeneratorService _generator;

    public GeneratorServiceTests()
    {
        _mockTemplateEngine = new Mock<ITemplateEngine>();
        _mockLogger = new Mock<ILogger<GeneratorService>>();

        _generator = new GeneratorService(
            _mockTemplateEngine.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GenerateAsync_WithValidConfig_ReturnsSuccessfulResult()
    {
        // Arrange
        var config = new WizardConfig { EntityId = "TestEntity" };
        var schema = new TableSchema { TableName = "test_entity" };
        var manifest = new EntityManifest { EntityId = "TestEntity" };

        _mockTemplateEngine.Setup(t => t.RenderAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync("generated code");

        // Act
        var result = await _generator.GenerateAsync(config, schema, manifest);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccessful);
        Assert.NotEmpty(result.Files);
    }

    [Fact]
    public async Task CreateZipPackageAsync_WithValidResult_ReturnsZipPath()
    {
        // Arrange
        var result = new GenerationResult
        {
            GenerationId = Guid.NewGuid().ToString(),
            EntityId = "TestEntity",
            IsSuccessful = true,
            Files = new List<GeneratedFile>
            {
                new GeneratedFile { FileName = "test.cs", Content = "code", FileType = "Controller" }
            }
        };
        var outputPath = Path.Combine(Path.GetTempPath(), "test_output");
        Directory.CreateDirectory(outputPath);

        // Act
        var zipPath = await _generator.CreateZipPackageAsync(result, outputPath);

        // Assert
        Assert.NotNull(zipPath);
        Assert.True(File.Exists(zipPath));

        // Cleanup
        File.Delete(zipPath);
        Directory.Delete(outputPath);
    }

    [Fact]
    public async Task ValidateGeneratedCodeAsync_WithValidCode_ReturnsValidResult()
    {
        // Arrange
        var result = new GenerationResult
        {
            GenerationId = Guid.NewGuid().ToString(),
            EntityId = "TestEntity",
            IsSuccessful = true,
            Files = new List<GeneratedFile>
            {
                new GeneratedFile { FileName = "test.cs", Content = "using System; public class Test {}", FileType = "Controller" }
            }
        };

        // Act
        var validationResult = await _generator.ValidateGeneratedCodeAsync(result);

        // Assert
        Assert.NotNull(validationResult);
        Assert.IsType<CodeValidationResult>(validationResult);
    }

    [Fact]
    public void GetStatistics_WithValidResult_ReturnsStatistics()
    {
        // Arrange
        var result = new GenerationResult
        {
            GenerationId = Guid.NewGuid().ToString(),
            EntityId = "TestEntity",
            IsSuccessful = true,
            Files = new List<GeneratedFile>
            {
                new GeneratedFile { FileName = "test.cs", Content = "code", FileType = "Controller" },
                new GeneratedFile { FileName = "test.js", Content = "js code", FileType = "JavaScript" }
            }
        };

        // Act
        var stats = _generator.GetStatistics(result);

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(2, stats.TotalFiles);
        Assert.Equal(1, stats.CSharpFiles);
        Assert.Equal(1, stats.JavaScriptFiles);
    }
}
