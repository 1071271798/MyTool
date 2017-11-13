@echo off
setlocal enabledelayedexpansion
::注意文件编码，转换为ANSI,否则有中文无法读取
::以制表符分割，不能以\t分割，否则会当成两个字符
set /a count=0
set /a rowMin=100
set /a rowMax=0
set /a maxWidth=30
for /f "delims=	 tokens=1-4" %%i in (config.txt) do (
	if !count!==0 (
		set id=%%i
		set name=%%j
		set targetPath=%%k
		set row=%%l
	) else (
		if "%%i" neq "" (
			set /a index=!count!-1
			set config[!index!].!id!=%%i
			set config[!index!].!name!=%%j
			set config[!index!].!targetPath!=%%k
			set config[!index!].!row!=%%l
			if %%l lss !rowMin! set /a rowMin=%%l
			if %%l gtr !rowMax! set /a rowMax=%%l
			set tmp=%%i.%%j
			call :setWidth !tmp! %maxWidth%
			if "!menuList[%%l]!"=="" (
				set menuList[%%l]=!tmp!
			) else (
				set menuList[%%l]=!menuList[%%l]!!tmp!
			)
		)
	)
	set /a count+=1
)
for /l %%i in (%rowMin%,1,%rowMax%) do (
	echo !menuList[%%i]!
)
:select
set /p selectNum=请输入序号：
echo %selectNum%|findstr "[^0-9]">nul && (
echo 请输入数字
goto select
)
set /a outputIndex=0
set current.id=0
set current.name=""
set current.path=""
set current.row=""
set /a arrayLen=%count%-1
:loop1
	if %outputIndex% equ %arrayLen% goto errorCode
	for /f "usebackq delims==. tokens=1-3" %%i in (`set config[%outputIndex%]`) do (
		set current.%%j=%%k
	)
	if !current.id!==%selectNum% (
		echo !current.path!
		call explorer !current.path!
		goto end
	)
	set /a outputIndex+=1
	goto loop1
:setWidth
	call :getStrLen %1
	if %strLen% lss %2 (
		set /a addLen=%2-%strLen%
		for /l %%i in (1,1,!addLen!) do (
			set tmp=!tmp! 
		)
	)
	goto :eof
:getStrLen
	set str=%~1
	set /a strLen=0
	for /l %%i in (0,1,100) do (
		if "!str!"=="" goto :eof
		set subStr=!str:~0,1!
		set str=!str:~1!
		echo !subStr!|findstr "[a-zA-Z0-9\-(){}\[\];',.+`~!@#$^_=]" > nul && (
			set /a strLen+=1
		) || (
			set /a strLen+=2
		)
	)
	goto :eof
:errorCode
	echo 请输入正确的序号
:end
	goto select