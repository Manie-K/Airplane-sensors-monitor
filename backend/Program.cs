using AirplaneSensorsMonitor;
using AirplaneSensorsMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddSingleton<IMqttService, MqttService>();
builder.Services.AddSingleton<ISensorService, SensorService>();
builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddHostedService(provider => (IHostedService)provider.GetRequiredService<IMqttService>());
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<SensorDataHub>("/sensorDataHub"); // SingalR endpoint

app.Run();
