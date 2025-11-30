# syntax=docker/dockerfile:1
FROM python:3.14-alpine

WORKDIR /app
COPY dataGenerator/requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt
COPY dataGenerator/main.py ./

ENV HOST=mosquitto \
    PORT=1883 \
    DATA_MIN=0 \
    DATA_MAX=100 \
    INTERVAL=1.0 \
    SENSOR_ID=compose-generator

CMD ["python", "main.py"]
