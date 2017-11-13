@echo on
@set /p var="请出入文件名："
@if exist %var% del %var%
@for /l %%i in (0,1,5000) do (
@set /p tmpText=""
@echo %tmpText%>>%var%)