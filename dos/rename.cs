@echo off
setlocal enabledelayedexpansion
dir /b test-*.*>tmp.txt
::findstr "^test[^-]" tmp.txt > tmp1.txt
::for %%i in (*.*) do (
::echo %%i>>tmp.txt
::set a=%%i
::echo !a!|findstr test
::echo %errorlevel%
::if %errorlevel%==0 echo %%i
::)
for /f %%i in (tmp.txt) do (
set a=%%i
rename %%i !a:-=_!
)
pause