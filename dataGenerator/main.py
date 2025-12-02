import os
import time
import random
import datetime
import socket
import paho.mqtt.client as mqtt


class EnvLoader:
    def __init__(self):
        self.host = os.environ.get("HOST", "127.0.0.1")      # MQTT broker hostname
        self.port = int(os.environ.get("PORT", "1883"))      # MQTT port
        self.data_min = float(os.environ.get("DATA_MIN", "0"))
        self.data_max = float(os.environ.get("DATA_MAX", "100"))
        self.interval = float(os.environ.get("INTERVAL", "1.0"))
        self.sensor_id = os.environ.get("SENSOR_ID", "0")
        self.sensor_type = os.environ.get("SENSOR_TYPE", "test-sensor")


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
        self.client = mqtt.Client()
        self.topic = f"sensors/{self.config.sensor_type}/{self.config.sensor_id}"

    def generate_value(self) -> float:
        return random.uniform(self.config.data_min, self.config.data_max)

    def run(self):
        self.log.info(f"MQTT Generator starting → {self.config.host}:{self.config.port}")
        self.log.info(f"Publishing to topic: {self.topic}")

        while True:
            try:
                self.client.connect(self.config.host, self.config.port, 60)
                self.log.info("Connected to MQTT broker")

                while True:
                    value = round(self.generate_value(), 2)
                    self.client.publish(self.topic, str(value))

                    self.log.info(f"PUBLISHED {self.topic}: {value}")

                    time.sleep(self.config.interval)

            except Exception as e:
                self.log.error(f"MQTT connection error: {e}, retry in 1s...")
                time.sleep(1)


if __name__ == "__main__":
    config = EnvLoader()
    logger = Logger()
    generator = DataGenerator(config, logger)
    generator.run()
