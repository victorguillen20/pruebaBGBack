@echo off
setlocal
cd /d "%~dp0"
dotnet run --project src\BG.Invoice.Api
