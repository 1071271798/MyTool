@echo off
setlocal enabledelayedexpansion
set str=%~1
set /a count=0
::循环次数越多，耗时越长
for /l %%i in (0,1,1000) do (
if "!str!"=="" goto end
set str=!str:~1!
set /a count+=1
)
:end
exit /b %count%