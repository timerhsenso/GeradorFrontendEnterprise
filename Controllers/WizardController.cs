using Microsoft.AspNetCore.Mvc;
using GeradorFrontendEnterprise.Core.Contracts;
using GeradorFrontendEnterprise.Core.Models;
using GeradorFrontendEnterprise.Core.Enums;

namespace GeradorFrontendEnterprise.Controllers;

[Controller]
[Route("[controller]")]
public class WizardController : Controller
{
    private readonly IOrchestratorService _orchestrator;
    private readonly ILogger<WizardController> _logger;

    public WizardController(IOrchestratorService orchestrator, ILogger<WizardController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpGet("step1")]
    public IActionResult Step1()
    {
        _logger.LogInformation("Acessando Step1 do Wizard");
        return View();
    }

    [HttpPost("step1")]
    public async Task<IActionResult> Step1Submit(string entityId)
    {
        try
        {
            _logger.LogInformation("Iniciando wizard para entidade: {EntityId}", entityId);
            
            if (string.IsNullOrEmpty(entityId))
            {
                ModelState.AddModelError("entityId", "Selecione uma entidade");
                return View("Step1");
            }

            var result = await _orchestrator.InitializeWizardAsync(entityId);
            if (!result.IsSuccessful)
            {
                ModelState.AddModelError("", string.Join(", ", result.Errors));
                return View("Step1");
            }
            
            HttpContext.Session.SetString("EntityId", entityId);
            return RedirectToAction("Step2");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar wizard");
            ModelState.AddModelError("", "Erro ao iniciar wizard: " + ex.Message);
            return View("Step1");
        }
    }

    [HttpGet("step2")]
    public async Task<IActionResult> Step2()
    {
        try
        {
            var entityId = HttpContext.Session.GetString("EntityId");
            if (string.IsNullOrEmpty(entityId))
                return RedirectToAction("Step1");

            _logger.LogInformation("Detectando conflitos para entidade: {EntityId}", entityId);
            var conflicts = await _orchestrator.DetectConflictsAsync(entityId);
            
            return View(conflicts ?? new List<Conflict>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao detectar conflitos");
            ModelState.AddModelError("", "Erro: " + ex.Message);
            return View(new List<Conflict>());
        }
    }

    [HttpPost("step2")]
    public async Task<IActionResult> Step2Submit(Dictionary<string, int> resolutions)
    {
        try
        {
            var entityId = HttpContext.Session.GetString("EntityId");
            if (string.IsNullOrEmpty(entityId))
                return RedirectToAction("Step1");

            var conflictResolutions = resolutions?.ToDictionary(
                k => k.Key,
                v => (ConflictResolution)v.Value
            ) ?? new Dictionary<string, ConflictResolution>();
            
            _logger.LogInformation("Resolvendo conflitos para entidade: {EntityId}", entityId);
            var result = await _orchestrator.ResolveConflictsAsync(entityId, conflictResolutions);
            
            if (!result.IsSuccessful)
            {
                ModelState.AddModelError("", string.Join(", ", result.Errors));
                return await Step2();
            }

            return RedirectToAction("Step3");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resolver conflitos");
            ModelState.AddModelError("", "Erro: " + ex.Message);
            return await Step2();
        }
    }

    [HttpGet("step3")]
    public async Task<IActionResult> Step3()
    {
        try
        {
            var entityId = HttpContext.Session.GetString("EntityId");
            if (string.IsNullOrEmpty(entityId))
                return RedirectToAction("Step1");

            _logger.LogInformation("Carregando schema para entidade: {EntityId}", entityId);
            var schema = await _orchestrator.LoadSchemaAsync(entityId);
            
            var config = new WizardConfig 
            { 
                EntityId = entityId,
                EntityName = schema?.TableName ?? entityId,
                GridLayout = new GridLayoutConfig { Fields = new List<GridFieldConfig>() },
                FormLayout = new FormLayoutConfig { Fields = new List<FormFieldConfig>() },
                FormFields = new List<FormField>()
            };
            
            return View(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar schema");
            ModelState.AddModelError("", "Erro: " + ex.Message);
            return RedirectToAction("Step1");
        }
    }

    [HttpPost("step3")]
    public async Task<IActionResult> Step3Submit(WizardConfig config)
    {
        try
        {
            var entityId = HttpContext.Session.GetString("EntityId");
            if (string.IsNullOrEmpty(entityId))
                return RedirectToAction("Step1");

            config.EntityId = entityId;

            _logger.LogInformation("Validando configuração para entidade: {EntityId}", entityId);
            var validation = await _orchestrator.ValidateConfigurationAsync(config);
            
            if (!validation.IsValid)
            {
                ModelState.AddModelError("", string.Join(", ", validation.Errors));
                return View("Step3", config);
            }

            var configId = await _orchestrator.SaveConfigurationAsync(config);
            HttpContext.Session.SetString("ConfigId", configId);
            
            _logger.LogInformation("Configuração salva com ID: {ConfigId}", configId);
            return RedirectToAction("Step4");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar configuração");
            ModelState.AddModelError("", "Erro: " + ex.Message);
            return View("Step3", config);
        }
    }

    [HttpGet("step4")]
    public IActionResult Step4()
    {
        try
        {
            var entityId = HttpContext.Session.GetString("EntityId");
            if (string.IsNullOrEmpty(entityId))
                return RedirectToAction("Step1");

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao acessar step4");
            return RedirectToAction("Step1");
        }
    }

    [HttpPost("step4")]
    public async Task<IActionResult> Step4Submit()
    {
        try
        {
            var entityId = HttpContext.Session.GetString("EntityId");
            var configId = HttpContext.Session.GetString("ConfigId");
            
            if (string.IsNullOrEmpty(entityId) || string.IsNullOrEmpty(configId))
                return RedirectToAction("Step1");

            _logger.LogInformation("Gerando código para entidade: {EntityId}", entityId);
            var config = await _orchestrator.LoadConfigurationAsync(configId);
            
            var result = await _orchestrator.GenerateCodeAsync(config);
            
            if (!result.IsSuccessful)
            {
                ModelState.AddModelError("", string.Join(", ", result.Errors));
                return View("Step4");
            }

            HttpContext.Session.SetString("GenerationId", result.GenerationId);
            _logger.LogInformation("Código gerado com ID: {GenerationId}", result.GenerationId);
            
            return RedirectToAction("Step5");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar código");
            ModelState.AddModelError("", "Erro: " + ex.Message);
            return View("Step4");
        }
    }

    [HttpGet("step5")]
    public async Task<IActionResult> Step5()
    {
        try
        {
            var generationId = HttpContext.Session.GetString("GenerationId");
            if (string.IsNullOrEmpty(generationId))
                return RedirectToAction("Step1");

            _logger.LogInformation("Exibindo resultado da geração: {GenerationId}", generationId);
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao acessar step5");
            return RedirectToAction("Step1");
        }
    }

    [HttpGet("download/{generationId}")]
    public async Task<IActionResult> Download(string generationId)
    {
        try
        {
            var zipPath = Path.Combine("GeneratedCode", $"{generationId}.zip");
            
            if (!System.IO.File.Exists(zipPath))
            {
                _logger.LogWarning("Arquivo não encontrado: {ZipPath}", zipPath);
                return NotFound("Arquivo de código gerado não encontrado");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(zipPath);
            _logger.LogInformation("Baixando arquivo: {ZipPath}", zipPath);
            
            return File(fileBytes, "application/zip", $"GeneratedCode-{generationId}.zip");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar arquivo");
            return StatusCode(500, "Erro ao baixar arquivo: " + ex.Message);
        }
    }
}
