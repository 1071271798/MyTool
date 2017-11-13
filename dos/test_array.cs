@echo off
::初始化数组
set /a obj_length=3
set obj[0].Name=Test1
set obj[0].Value=Hello World
set obj[1].Name=Test2
set obj[1].Value=blahblah
set obj[2].Name=Test3
set obj[2].Value=hehehe
::取值
set /a obj_index=0
:loopStart
	if %obj_index% equ %obj_length% goto :eof
	set obj_current.Name=0
	set obj_current.Value=0
	::可以用于分割字符串
	for /f "usebackq delims==. tokens=1-3" %%i in (`set obj[%obj_index%]`) do (
		set obj_current.%%j=%%k
	)
	echo Name = %obj_current.Name%
	echo Value = %obj_current.Value%
	echo.
	set /a obj_index+=1
goto loopStart