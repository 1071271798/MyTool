@echo off
set /a num1=10
set /a num2=12
call :add %num1% %num2%
call :sub %num1% %num2%
call :mul %num1% %num2%
call :exc %num1% %num2%
set /a num1=%num1%+10
set /a num2=%num2%+10
echo %num1%
echo %num2%
set str1=123456
set str1=%str1:~1%
echo %str1%
set str1=%str1%sb
echo %str1%
pause
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