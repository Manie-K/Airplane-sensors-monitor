using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AirplaneSensorsMonitor.Services;
using AirplaneSensorsMonitor.Model;

namespace AirplaneSensorsMonitor.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ISensorService _sensorService;

        public IEnumerable<SensorData> SensorData { get; set; } = new List<SensorData>();
        public IEnumerable<SensorSummary> SensorSummaries { get; set; } = new List<SensorSummary>();

        [BindProperty(SupportsGet = true)]
        public string? SensorType { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SensorId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "TimeDesc";

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Paused { get; set; } = false;

        public DashboardModel(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

        public void OnGet()
        {
            bool? sortValueDesc = SortBy == "ValueDesc" ? true : SortBy == "ValueAsc" ? false : null;
            bool? sortTimeDesc = SortBy == "TimeDesc" ? true : SortBy == "TimeAsc" ? false : null;

            if (Paused)
            {
                SensorData = _sensorService.GetSensorData(SensorId, SensorType, sortValueDesc, sortTimeDesc, StartDate, EndDate).Take(200).ToList();
            }
            else
            {
                SensorData = new List<SensorData>();
            }

            SensorSummaries = _sensorService.GetSensorSummaries(SensorId, SensorType, sortValueDesc, sortTimeDesc, StartDate, EndDate);
        }
    }
}
