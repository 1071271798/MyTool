@echo off
echo ��ѡ��������(������Ű�Enter):
echo 11.Jimu_Unity			12.Jimu_Android			13.RobotUnity_Git
echo 21.�ٷ�ģ��			22.������Դsvn
echo 31.jave-ee-workspace		32.Jimu-JavaWeb-Controller	33.jimu-JavaWeb-Interface
echo 41.AppServ-www
echo 51.webTool			52.dos
:select
set /p selectNum=��������ţ�
echo %selectNum%|findstr "[^0-9]">nul && (
echo ����������
goto select
)
if %selectNum%==11 ( call explorer E:\RobotUnity_Git\Jimu-Unity\RobotUnity3D
goto end)
if %selectNum%==12 ( call explorer E:\RobotUnity_Git\Jimu-Android
goto end)
if %selectNum%==13 ( call explorer E:\RobotUnity_Git
goto end)
if %selectNum%==21 ( call explorer E:\Jimu�ĵ�\07����\�ٷ�ģ��\default
goto end)
if %selectNum%==22 ( call explorer E:\Jimu�ĵ�\07����
goto end)
if %selectNum%==31 ( call explorer E:\jave-ee-workspace
goto end)
if %selectNum%==32 ( call explorer E:\Jimu-JavaWeb-Controller
goto end)
if %selectNum%==33 ( call explorer E:\jimu-JavaWeb-Interface
goto end)
if %selectNum%==41 ( call explorer C:\AppServ\www
goto end)
if %selectNum%==51 ( call explorer E:\MyTool\webTool
goto end)
if %selectNum%==52 ( call explorer E:\MyTool\dos
goto end)
echo ��������ȷ�����
:end
goto select
pause
::�����Ļ
cls
::��������
call �������.bat