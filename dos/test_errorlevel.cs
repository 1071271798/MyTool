@echo off
xcopy 1.txt act7 /-Y
if errorlevel 4 echo 拷贝过程中写盘错误
if errorlevel 3 echo 预置错误阻止文件拷贝操作
if errorlevel 2 echo 用户通过ctrl-c终止拷贝操作
if errorlevel 1 echo 未找到拷贝文件
if errorlevel 0 echo 成功拷贝文件