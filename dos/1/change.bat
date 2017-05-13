@echo off
for /r /d %%i in (*.*) do set path1=%%i set tmpq=%path1:act=xj% echo tmpq
pause