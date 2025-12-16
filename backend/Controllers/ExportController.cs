using Microsoft.AspNetCore.Mvc;
using AirplaneSensorsMonitor.Services;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly ISensorService _sensorService;

    public ExportController(ISensorService sensorService)
    {
        _sensorService = sensorService;
    }

    [HttpGet("{format}")]
    public IActionResult Export(string format, [FromQuery] int? sensorId, [FromQuery] string? sensorType,
                                [FromQuery] string? sortBy, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        bool? sortValueDesc = sortBy == "ValueDesc" ? true : sortBy == "ValueAsc" ? false : null;
        bool? sortTimeDesc = sortBy == "TimeDesc" ? true : sortBy == "TimeAsc" ? false : null;

        var data = _sensorService.ExportSensorData(format, sensorId, sensorType, sortValueDesc, sortTimeDesc, startDate, endDate);

        var contentType = format.ToLower() == "csv" ? "text/csv" : "application/json";
        var fileName = $"sensor_data.{format.ToLower()}";

        return File(System.Text.Encoding.UTF8.GetBytes(data), contentType, fileName);
    }
}
