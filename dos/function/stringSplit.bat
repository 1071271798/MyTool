@echo off
set text=%1
set split=%2
:hehe
set /p str=please input string:
call getStrLen.bat %str%
echo %errorlevel%
goto hehe
pause