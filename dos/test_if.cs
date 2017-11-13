@echo off
cd C:\
if exist \CrackCaptcha.log type \CrackCaptcha.log
if not exist \CrackCaptcha.log echo \CrackCaptcha.log does not exist
set a=1
if %a%==3 (
echo 3
) else if %a%==2 (
echo 2 
) else if %a%==1 (
echo 1 
) else (
echo ÆäËû
)