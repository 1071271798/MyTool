@if "%1"=="123" echo "true"
@if not "%1"=="2" echo "%1!=2"
@echo "false"
@if exist test2.bat echo "test2.bat exist"
@if not exist act6 echo "act6 not exist"
@goto x2
@echo "sb"
:x2
@echo this is x2
@choice /c ync /T 10 /D Y /M "ȷ���밴Y�����밴N��ȡ���밴C."
@if errorlevel==3 goto cccc
@if errorlevel==2 goto nnnn
@if errorlevel==1 goto yyyy
:yyyy
@echo choice yes
@goto end
:nnnn
@echo choice no
@goto end
:cccc
@echo choice cannel
@goto end
:end
@echo good bye
@pause