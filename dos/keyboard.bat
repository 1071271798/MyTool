@echo off
:input
set /p char=ÇëÊäÈë×Ö·û£º
echo %char%|findstr "[a-zA-Z0-9\-\\(){}\[\];':,./?*+`~!@#$^_=]" > nul && (
	echo 1
) || (
	echo 2
)
goto input