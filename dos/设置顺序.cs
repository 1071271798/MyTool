
@echo off
setlocal enabledelayedexpansion
::新文件输出目录
if not exist newFiles md newFiles
::记录文件个数，用于设置时间
set /a count=0
::把当前时间记录进文件，用于时间恢复
time /t>tmp.txt
for %%i in (*.mp3) do (
::修改系统时间，使新生成的文件拥有不同的修改时间
if !count! LSS 10 (
time 15:00:0!count!
) else (
time 15:00:!count!)
set newPath=newFiles\%%i
echo !newPath!
::把文件写入新路径
type %%i>!newPath!
set /a count+=1
)
time<tmp.txt
del tmp.txt
pause