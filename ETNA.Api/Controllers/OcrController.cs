using ETNA.Api.Models.DTOs;
using ETNA.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ETNA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OcrController : ControllerBase
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<OcrController> _logger;

    public OcrController(IOcrService ocrService, ILogger<OcrController> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    /// <summary>
    /// Scan a vehicle number plate image
    /// </summary>
    [HttpPost("scan-plate")]
    public async Task<ActionResult<VehicleDataDto>> ScanPlate([FromBody] OcrScanRequest request)
    {
        if (string.IsNullOrEmpty(request.Base64Image))
        {
            return BadRequest(new VehicleDataDto(
                null, null, null, null, null, null, null, null, null, null,
                false, "Image data is required"
            ));
        }

        _logger.LogInformation("Processing plate scan request");
        var result = await _ocrService.ProcessImageAsync(request.Base64Image, "plate");
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Scan an RC (Registration Certificate) card image
    /// </summary>
    [HttpPost("scan-rc")]
    public async Task<ActionResult<VehicleDataDto>> ScanRcCard([FromBody] OcrScanRequest request)
    {
        if (string.IsNullOrEmpty(request.Base64Image))
        {
            return BadRequest(new VehicleDataDto(
                null, null, null, null, null, null, null, null, null, null,
                false, "Image data is required"
            ));
        }

        _logger.LogInformation("Processing RC card scan request");
        var result = await _ocrService.ProcessImageAsync(request.Base64Image, "rc");
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Generic scan endpoint that accepts mode as parameter
    /// </summary>
    [HttpPost("scan")]
    public async Task<ActionResult<VehicleDataDto>> Scan([FromBody] OcrScanRequest request)
    {
        if (string.IsNullOrEmpty(request.Base64Image))
        {
            return BadRequest(new VehicleDataDto(
                null, null, null, null, null, null, null, null, null, null,
                false, "Image data is required"
            ));
        }

        var mode = request.Mode?.ToLowerInvariant() ?? "plate";
        if (mode != "plate" && mode != "rc")
        {
            return BadRequest(new VehicleDataDto(
                null, null, null, null, null, null, null, null, null, null,
                false, "Invalid mode. Use 'plate' or 'rc'"
            ));
        }

        _logger.LogInformation("Processing {Mode} scan request", mode);
        var result = await _ocrService.ProcessImageAsync(request.Base64Image, mode);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}
