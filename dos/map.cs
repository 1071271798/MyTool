@echo off
setlocal enabledelayedexpansion
call :put ��� hello
call :put ���� haha
call :get ���
call :get ����
call :pop ���
call :pop ����
call :get ���
call :get ����
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