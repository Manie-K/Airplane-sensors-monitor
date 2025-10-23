import os
import time
import json
import socket
import random
import datetime


class EnvLoader:
    def __init__(self):
        self.host = os.environ.get("HOST", "127.0.0.1")
        self.port = int(os.environ.get("PORT", "5000"))
        self.data_min = float(os.environ.get("DATA_MIN", "0"))
        self.data_max = float(os.environ.get("DATA_MAX", "100"))
        self.interval = float(os.environ.get("INTERVAL", "1.0"))
        self.sensor_id = os.environ.get("SENSOR_ID", f"{socket.gethostname()}")


class Logger:

    @staticmethod
    def info(message: str):
        print(f"[{datetime.datetime.now().strftime('%H:%M:%S.%f')[:-3]}] [INFO] {message}")

    @staticmethod
    def warning(message: str):
        print(f"[{datetime.datetime.now().strftime('%H:%M:%S.%f')[:-3]}] [WARN]  {message}")

    @staticmethod
    def error(message: str):
        print(f"[{datetime.datetime.now().strftime('%H:%M:%S.%f')[:-3]}] [ERROR] {message}")


class DataGenerator:

    def __init__(self, config: EnvLoader, logger: Logger):
        self.config = config
        self.log = logger

    def generate_value(self) -> float:
        return random.uniform(self.config.data_min, self.config.data_max)

    def run(self):
        self.log.info(f"Generator started. Target: {self.config.host}:{self.config.port}")

        while True:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            try:
                sock.settimeout(3)
                sock.connect((self.config.host, self.config.port))
                self.log.info(f"Connected to {self.config.host}:{self.config.port}")

                while True:
                    value = self.generate_value()
                    payload = json.dumps({
                        "source": self.config.sensor_id,
                        "value": round(value, 2)
                    }).encode()
                    sock.sendall(payload)
                    self.log.info(f"SENT: {payload.decode().strip()}")
                    time.sleep(self.config.interval)

            except (ConnectionRefusedError, TimeoutError):
                self.log.error(f"No connection to {self.config.host}:{self.config.port}, retrying in 1s...")
                time.sleep(1)

            except (BrokenPipeError, ConnectionResetError):
                self.log.warning("Connection lost, retrying in 1s...")
                time.sleep(1)

            except KeyboardInterrupt:
                self.log.info("Generator stopped by user.")
                break

            finally:
                sock.close()


if __name__ == "__main__":
    config = EnvLoader()
    logger = Logger()
    generator = DataGenerator(config, logger)
    generator.run()
