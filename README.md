# Airplane-sensors-monitor

Demonstration project showcasing an end-to-end IoT data flow: generator → MQTT broker → ASP.NET Core backend → MongoDB → Razor Pages UI with tables, filters, and SignalR updates.

## Development Workflow

This project utilizes the **Docker Compose Override** pattern to provide a seamless local development experience with hot-reloading, eliminating the need to install the .NET SDK on your host machine.

When you run `docker compose up`, Docker automatically merges `docker-compose.yaml` (Production configuration) with `docker-compose.override.yaml` (Development configuration).

### Port Configuration
To distinguish between environments and avoid caching issues, the application runs on different ports depending on the mode:

### Running the Production Build Locally

If you want to test the actual production build (e.g., to verify the final Docker image size or startup behavior) without the development tools, you must explicitly ignore the override file:

```
docker compose -f docker-compose.yaml up --build
```

or you can delete the `docker-compose.override.yaml`.

## Running with Docker

Requiremes Docker Desktop or any compatible engine and `docker compose`.

1. Build and launch all services:

	```bash
	docker compose up --build
	```

2. The web app becomes available at http://localhost:8080. The MQTT broker listens on `localhost:1883`, MongoDB on `localhost:27017`.

3. The `data-generator` service periodically publishes random JSON payloads to the configured host/port (default `mosquitto:1883`). Value range, interval, and sensor identifier are configurable via environment variables (`dataGenerator/main.py`).

To stop the stack, use `Ctrl+C`, then `docker compose down` to free resources (MongoDB data persists in the `mongo-data` volume).

## Configurable Environment Variables

| Name | Description | Default value |
| --- | --- | --- |
| `ConnectionStrings__MongoDb` | MongoDB connection string used by the backend | `mongodb://mongo:27017/sensors` |
| `Mqtt__Host` / `Mqtt__Port` | MQTT broker host and port | `mosquitto` / `1883` |
| `Mqtt__TopicFilter` | Topic filter the backend subscribes to | `sensors/#` |
| `HOST` / `PORT` | Target host/port for the generator | `mosquitto` / `1883` |
| `DATA_MIN` / `DATA_MAX` | Range of generated values | `0` / `100` |
| `INTERVAL` | Publish interval in seconds | `1.0` |
| `SENSOR_ID` | Sensor identifier included in payloads | `test-generator` |

## Container Topology

| Service | Description |
| --- | --- |
| `backend` | ASP.NET Core 8.0 (Razor Pages + API + SignalR); consumes MQTT, stores in MongoDB, serves the UI. |
| `mongo` | MongoDB 7.0 storing raw sensor data. |
| `mosquitto` | Eclipse Mosquitto MQTT broker handling IoT traffic. |
| `data-generator` | Python script (`main.py`) emitting random values with a configurable sensor ID. |

## Local Run (without Docker)

1. Start MongoDB and Mosquitto manually using the tools available on your OS:
	- **macOS**: `brew services start mongodb-community@7.0` and `brew services start mosquitto`
	- **Linux (systemd)**: `sudo systemctl start mongod` and `sudo systemctl start mosquitto`
	- **Windows**: start the installed services from *Services.msc* or run `net start MongoDB` / `net start mosquitto` (if registered as services); alternatively run the binaries (`mongod.exe`, `mosquitto.exe`) in separate terminals.
2. Provide environment variables or edit `appsettings.json` (`ConnectionStrings.MongoDb`, `Mqtt` section).
3. Inside `backend/`, run `dotnet run` (or press ▶️ in Rider / Visual Studio) to start the web app.

## License

Released under the MIT License (see files in `docs/`).
