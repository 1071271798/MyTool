@echo on
@set /p var="������ļ�����"
@if exist %var% del %var%
@for /l %%i in (0,1,5000) do (
@set /p tmpText=""
@echo %tmpText%>>%var%)