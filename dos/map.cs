@echo off
setlocal enabledelayedexpansion
call :put ÄãºÃ hello
call :put ¹þ¹þ haha
call :get ÄãºÃ
call :get ¹þ¹þ
call :pop ÄãºÃ
call :pop ¹þ¹þ
call :get ÄãºÃ
call :get ¹þ¹þ
pause
:put
	set myMap[%1]=%2
	goto :eof
:pop
	set myMap[%1]=
	goto :eof
:get
	set key=%1
	echo !myMap[%key%]!
	goto :eof