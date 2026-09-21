@echo off
REM Pokrece backend i frontend, svaki u svom prozoru.
REM Zatvaranje prozora zaustavlja odgovarajuci posluzitelj.

echo Pokrecem backend i frontend...

start "ResourceBooking - backend" cmd /k "cd /d %~dp0backend\ResourceBooking.Api && dotnet run"
start "ResourceBooking - frontend" cmd /k "cd /d %~dp0frontend && npm run dev"

echo.
echo Backend:    http://localhost:5001
echo Swagger:    https://localhost:7001/swagger
echo Aplikacija: http://localhost:5173
echo.
echo Pricekaj nekoliko sekundi pa otvori http://localhost:5173
timeout /t 8 >nul
