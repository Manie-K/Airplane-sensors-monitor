using AirplaneSensorsMonitor.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AirplaneSensorsMonitor.Controllers
{
    [ApiController]
    [Route("api/sensordata")]
    public class SensorDataController : ControllerBase
    {
        private readonly ISensorDataService _sensorDataService;

        public SensorDataController(ISensorDataService sensorDataService)
        {
            _sensorDataService = sensorDataService;
        }

        [HttpGet]
        public IActionResult GetSensorData(int? sensorId = null, string ? sensorType = null, bool sortValueDescending = false, bool sortTimestampDescending = false)
        {
            var data = _sensorDataService.GetSensorData(sensorId, sensorType, sortValueDescending, sortTimestampDescending);
            return Ok(data);
        }

        [HttpGet("export")]
        public IActionResult ExportSensorData(string format, int? sensorId = null, string? sensorType = null, bool sortValueDescending = false, bool sortTimestampDescending = false)
        {
            var content = _sensorDataService.ExportSensorData(format, sensorId, sensorType, sortValueDescending, sortTimestampDescending);
            var contentType = format == "json" ? "application/json" : "text/csv";
            var fileName = $"messages_{DateTime.Now:ddMMyyyy_HHmmss}.{format}";
            return File(Encoding.UTF8.GetBytes(content), contentType, fileName);
        }

    }

}
