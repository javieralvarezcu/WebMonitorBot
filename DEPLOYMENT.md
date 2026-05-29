Despliegue remoto con Docker

Requisitos en el servidor remoto:
- Docker instalado
- Docker Compose (v2) instalado
- Acceso para subir archivos o clonar el repo

Pasos recomendados:

1) Preparar el repositorio en el servidor

- Opción A: clonar el repo en el servidor
  git clone https://github.com/javieralvarezcu/WebMonitorBot.git

- Opción B: transferir archivos necesarios (.env, docker-compose.yml, WebMonitorBot/*, Dockerfile)

2) Configurar variables de entorno

- Copia .env.example a .env y edítalo con los valores reales:
  cp .env.example .env
  (editar .env con un editor y rellenar TELEGRAM_BOT_TOKEN, DEEPSEEK__APIKEY, DATABASE_CONN)

3) Construir la imagen

  docker compose build

4) Arrancar en background

  docker compose up -d

5) Logs y gestión

- Ver logs:
  docker compose logs -f webmonitor

- Parar:
  docker compose down