@echo off
echo 请选择启动项(输入序号按Enter):
echo 11.Jimu_Unity			12.Jimu_Android			13.RobotUnity_Git
echo 21.官方模型			22.开发资源svn
echo 31.jave-ee-workspace		32.Jimu-JavaWeb-Controller	33.jimu-JavaWeb-Interface
echo 41.AppServ-www
echo 51.webTool			52.dos
:select
set /p selectNum=请输入序号：
echo %selectNum%|findstr "[^0-9]">nul && (
echo 请输入数字
goto select
)
if %selectNum%==11 ( call explorer E:\RobotUnity_Git\Jimu-Unity\RobotUnity3D
goto end)
if %selectNum%==12 ( call explorer E:\RobotUnity_Git\Jimu-Android
goto end)
if %selectNum%==13 ( call explorer E:\RobotUnity_Git
goto end)
if %selectNum%==21 ( call explorer E:\Jimu文档\07开发\官方模型\default
goto end)
if %selectNum%==22 ( call explorer E:\Jimu文档\07开发
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
echo 请输入正确的序号
:end
goto select
pause
::清除屏幕
cls
::重新启动
call 快捷启动.bat