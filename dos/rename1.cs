@echo off
setlocal enabledelayedexpansion
for  %%i in (*.mp3) do (
	for /f "tokens=1*" %%j in ("%%i") do (
		if "%%j"=="����ϱ��������" (
			for /f "delims==_ tokens=1*" %%a in ("%%k") do (
				set newPath=%%a
				set newPath=!newPath!.mp3
				echo %%i
				echo !newPath!
				rename "%%i" !newPath!
			)
		)
	)
)
pause