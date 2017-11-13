@echo off
setlocal enabledelayedexpansion
echo hello
:input
set /p var=请输入算术：
echo %var%|findstr "^[0-9]*[\+\-\*/][0-9]*$" > nul && (
call :calculate %var%) || (
echo 请输入正确的算术表达式)
goto input
:calculate
set num1=""
set num2=""
set str=%1
set count=1
set operator=""
set index=""
for /l %%i in (0,1,1000) do (
if "!str!"=="" ( call :analysisOver !num1! !num2! !operator!
goto input
)
set index=!str:~0,1!
set str=!str:~1!
if !count!==1 ( 
if !index!==+ (
set count=2
set operator=!index!
) else if !index!==- (
set count=2
set operator=!index!
) else if !index!==* (
set count=2
set operator=!index!
) else if !index!==/ (
set count=2
set operator=!index!
) else (
if !num1!=="" (
set num1=!index!
) else (
set num1=!num1!!index!)
)
) else (
if !num2!=="" (
set num2=!index!
) else (
set num2=!num2!!index!)
)
)
:analysisOver
if %3==+ call :add %1 %2
if %3==- call :sub %1 %2
if %3==* call :mul %1 %2
if %3==/ call :exc %1 %2
goto :eof
:add
set /a tmp=%1+%2
echo %tmp%
goto :eof
:sub
set /a tmp=%1-%2
echo %tmp%
goto :eof
:mul
set /a tmp=%1*%2
echo %tmp%
goto :eof
:exc
set /a tmp=%1/%2
echo %tmp%
goto :eof