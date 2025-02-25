@echo off
bash "%~dp0version.sh"
dotnet build Source/StockpileSelector/StockpileSelector.csproj -c Release -v n