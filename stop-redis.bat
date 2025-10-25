@echo off
echo ========================================
echo   Deteniendo Redis
echo ========================================
echo.

docker-compose down

if %errorlevel% neq 0 (
    echo [ERROR] No se pudo detener Redis
    pause
    exit /b 1
)

echo.
echo [OK] Redis detenido correctamente
echo.
pause

