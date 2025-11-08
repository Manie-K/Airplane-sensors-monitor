using Microsoft.AspNetCore.Mvc.RazorPages;
using AirplaneSensorsMonitor.Services;

namespace AirplaneSensorsMonitor.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly MqttService _mqttService;

        public DashboardModel(MqttService mqttService)
        {
            _mqttService = mqttService;        
        }

        public List<SensorMessage> SensorMessages { get; set; }

        public void OnGet(string? sensorType, string? sensorId, bool sortDescending = false)
        {
            SensorMessages = _mqttService.GetMessages(sensorType, sensorId, sortDescending);
        }
    }
}
