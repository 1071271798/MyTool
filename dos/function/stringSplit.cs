@echo off
setlocal enabledelayedexpansion
set text=%~1
set splitStr=%~2
set /a count=0
:split
::set /p str=please input string:
::call getStrLen.bat %str%
::echo %errorlevel%
::可以用于分割字符串
	if "%splitStr%"==" " (
		for /f "tokens=1*" %%i in ("%text%") do (
			set array[%count%]=%%i
			set text=%%j
			if "!text!"=="" goto end
			set /a count+=1
		)
	) else (
		for /f "delims==%splitStr% tokens=1*" %%i in ("%text%") do (
			set array[%count%]=%%i
			set text=%%j
			if "!text!"=="" goto end
			set /a count+=1
		)
	)
goto split
:end
for /l %%i in (0,1,!count!) do (
	echo !array[%%i]!
)
pause