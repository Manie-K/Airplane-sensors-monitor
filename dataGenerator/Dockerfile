# syntax=docker/dockerfile:1
FROM python:3.14-alpine

WORKDIR /app
COPY dataGenerator/requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt
COPY dataGenerator/main.py ./

CMD ["python", "main.py"]
