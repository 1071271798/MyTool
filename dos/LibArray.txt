set max=65535
set debug=
set debugerror=1
set deglev_1=error
set debugwarn=3
set deglev_3=warn
set debuginfo=7
set deglev_7=info
set debug_log=
set debug_log_dir=C:\
set debug_log_file=debug.log
set ver=1

:PrintError
	if "%~1" == "" ( set out_text=Unkown Error ... ) else ( set out_text=%~1 )    
	echo %out_text%
goto :eof

:Debug
	if defined debug ( 
		if %~1 LEQ debug ( 
			echo [ debug_deglev_%~1 ]  %~2 %~3
			if defined debug_log ( echo   [ debug_deglev_%~1 ]  %~2 %~3 >> %debug_log_dir%\%debug_log_file% )
		)
	)
goto :eof

:Array
rem Array <string array_name>
rem return -1 if not successful. return 0 if successful
	set return=
	if  "%~1" == "" ( 
		set return=-1
		call :Debug "%debugerror%" "STD_Array" "array_name="%~1" "
		goto :eof
	) 
	if defined %~1_c ( 
		call :FreeArray "%~1" 
		if  not return == 0 ( 
			call :Debug "%debugerror%" "STD_Array" "Fun_FreeArray_Return: %return%"
			set return= -1
			goto :eof
		) 
	)
	set %~1_c=0
	set return=0
	call :Debug "%debuginfo%" "STD_Array" "reutrn=%return%"
goto :eof

:FreeArray
rem FreeArray <string array_name>
rem return 0 if successful. return -1 array_name not vaild, return -2 array not vaild
	set return=
	if "%~1" == "" (
		set return=-1
		call :Debug "%debugerror%" "STD_FreeArray" "array_name="%~1" "
		goto :eof
	)
	if  not defined %~1_c (
		set return=-2
		call :Debug "%debugerror%" "STD_FreeArray" "not vaild array_count"
		goto :eof
	)
	for /l %%a in ( 1, 1, %count% ) do (
		set %~1_%%a=
	)
	set %~1_c=
	set return=0
	call :Debug "%debuginfo%" "STD_FreeArray" "reutrn=%return%"
goto :eof	

:CopyArray
rem CopyArray <string from_array_name> <string to_array_name> [int count]
rem return 0 if successful, return -1 array_name not vaild, return -2 array not vaild
	set return=
	if "%~1" == "" ( 
		set return=-1
		call :Debug "%debugerror%" "STD_CopyArray" "from_array_name="%~1"
		goto :eof
	)
	if not defined %~1_c (		
		set return=-2
		call :Debug "%debugerror%" "STD_CopyArray" "not vaild array_count"
		goto :eof
	)
	if "%~2" == "" ( 		
		set return=-1
		call :Debug "%debugerror%" "STD_CopyArray" "to_array_name="%~2"
		goto :eof
	)
	if "%~3" == "" ( set count=!%~1_c! ) else ( set count=%~3 )
	if %count% GTR !%~1_c! ( 
		set count=!%~1_c!
		call :Debug "%debugwarn%" "STD_CopyArray" "count[ %~3 ] > from_array_count[ !%~1_c! ] set count = from_array_count"
	)
	for /l %%a in ( 1, 1, %count% ) do (
		set %~2_%%a=!%~1_%%a!
	)
	set %~2_c=%count%
	set return=0
	call :Debug "%debuginfo%" "STD_CopyArray" "reutrn=%return%"
goto :eof

:InArray
rem InArray <string array_name> <string value>
rem return 0 if successful, return -1 array_name not vaild, return -2 value not vaild
	set return=
	if  "%~1" == "" (
		set return=-1
		call :Debug "%debugerror%" "STD_InArray" "array_name="%~1""
		goto :eof
	)
	if "%~2" == "" (
		set return=-1
		call :Debug "%debugerror%" "STD_InArray" "value="%~2""
		goto :eof
	)
	if not defined %~1_c (
		set return=-2
		call :Debug "%debugerror%" "STD_InArray" "not vaild array_count"
		goto :eof
	)
	for /L %%a in ( 1, 1, !%~1_c! ) do (
		if "%~1_%%a" == "%~2" (
			set return=%%a
			goto :eof
		)
	)
	set return=0
	call :Debug "%debuginfo%" "STD_InArray" "reutrn=%return%"
goto :eof