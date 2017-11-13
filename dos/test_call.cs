@echo off
echo 路径:%~dp0
echo 生成临时文件>tmp.txt
::保存当期目录，设置新目录
pushd C:\Windows
call :sub tmp.txt
::恢复目录
popd
call :sub tmp.txt
pause
type tmp.txt|more
:sub
echo 正常的值: %1
echo 删除引号: %~1
echo 扩充到路径: %~f1
echo 扩充到一个驱动器号: %~d1
echo 扩充到一个路径: %~p1
echo 扩充到一个文件名: %~n1
echo 扩充到一个文件扩展名: %~x1
echo 扩充的路径只含有短名: %~s1
echo 扩充到文件属性: %~a1
echo 扩充到文件的日期/时间: %~t1
echo 扩充到文件的大小: %~z1
echo 扩展到驱动器号和路径: %~dp1
echo 扩展到文件名和扩展名: %~nx1
echo 扩展到类似DIR的输出行: %~ftza1
echo.
goto :eof