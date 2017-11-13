@echo off
setlocal enabledelayedexpansion
for  %%i in (*.bat) do (
	for /f "delims==. tokens=1*" %%j in ("%%i") do (
		set newPath=%%j
		set newPath=!newPath!.cs
		echo %%i
		echo !newPath!
		rename "%%i" !newPath!
	)
)
pause