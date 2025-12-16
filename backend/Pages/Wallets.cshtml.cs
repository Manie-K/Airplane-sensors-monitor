using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AirplaneSensorsMonitor.Services;
using AirplaneSensorsMonitor.Model;

namespace AirplaneSensorsMonitor.Pages
{
    public class WalletsModel : PageModel
    {
        private readonly ISensorService _sensorService;
        private readonly ISensorTokenService _sensorTokenService;

        public IEnumerable<SensorData> SensorData { get; set; } = new List<SensorData>();

        public WalletsModel(ISensorTokenService tokenService, ISensorService sensorService)
        {
            _sensorTokenService = tokenService;
            _sensorService = sensorService;
        }

        public async Task OnGetAsync()
        {
            var sensors = _sensorService.GetAvailableSensors();

            var list = new List<SensorData>();

            foreach (var s in sensors)
            {
                s.Balance = await _sensorTokenService.GetBalanceAsync(s.SensorId);
                list.Add(s);
            }

            SensorData = list;
        }
    }
}