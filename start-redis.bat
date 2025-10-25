@echo off
echo ========================================
echo   Iniciando Redis con Docker
echo ========================================
echo.

REM Verificar si Docker está corriendo
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker no está corriendo.
    echo Por favor, inicia Docker Desktop primero.
    pause
    exit /b 1
)

echo [OK] Docker está corriendo
echo.

REM Iniciar Redis con docker-compose
echo Iniciando Redis...
docker-compose up -d

if %errorlevel% neq 0 (
    echo [ERROR] No se pudo iniciar Redis
    pause
    exit /b 1
)

echo.
echo ========================================
echo   Redis iniciado correctamente!
echo ========================================
echo.
echo Puerto: 6379
echo Contenedor: staygo-redis
echo.
echo Comandos utiles:
echo   - Ver logs:        docker logs -f staygo-redis
echo   - Conectar CLI:    docker exec -it staygo-redis redis-cli
echo   - Detener:         docker-compose down
echo.
echo Ahora puedes ejecutar: dotnet run
echo.
pause

