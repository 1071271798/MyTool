echo off
setlocal enabledelayedexpansion
for /l %%i in (1,1,35) do (
call rename.bat %%i
)