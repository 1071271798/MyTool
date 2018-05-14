@echo off
setlocal enabledelayedexpansion
::for  %%i in (*.mp3) do (
::	for /f "delims=@ tokens=1*" %%j in ("%%i") do (
::		set newPath=%%j
::		set newPath=!newPath!.mp3
::		echo %%i
::		echo !newPath!
::		rename "%%i" !newPath!
::	)
::)
for  %%i in (*.mp3) do (
	for /f "delims=. tokens=1*" %%j in ("%%i") do (
		set newPath=%%j
		set newPath=!newPath!.png
		echo %%i
		echo !newPath!
		rename "%%i" !newPath!
	)
)
pause