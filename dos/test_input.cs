@echo off
time /t>tmp.txt
set /p var<tmp.txt
echo %var%
del tmp.txt