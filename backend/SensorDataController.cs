using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AirplaneSensorsMonitor
{
    [ApiController]
    [Route("api/sensordata")]
    public class SensorDataController : ControllerBase
    {
        private readonly SensorDataService _sensorDataService;

        public SensorDataController(SensorDataService sensorDataService)
        {
            _sensorDataService = sensorDataService;
        }

        [HttpGet]
        public IActionResult GetSensorData(string? sensorType = null, string? sensorId = null, bool sortValueDescending = false, bool sortTimestampDescending = false)
        {
            var data = _sensorDataService.GetSensorData(sensorType, sensorId, sortValueDescending, sortTimestampDescending);
            return Ok(data);
        }

        [HttpGet("export")]
        public IActionResult ExportSensorData(string format, string? sensorType = null, string? sensorId = null, bool sortValueDescending = false, bool sortTimestampDescending = false)
        {
            var content = _sensorDataService.ExportSensorData(format, sensorType, sensorId, sortValueDescending, sortTimestampDescending);
            var contentType = format == "json" ? "application/json" : "text/csv";
            var fileName = $"messages_{DateTime.Now:ddMMyyyy_HHmmss}.{format}";
            return File(Encoding.UTF8.GetBytes(content), contentType, fileName);
        }

    }

}
