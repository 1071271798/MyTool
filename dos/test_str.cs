@echo off
set aa=abcdefghijklmn
set cmdstr=%aa%
echo %cmdstr%
::字符串截取
echo 截取前4个字符
echo %aa:~0,4%
echo 截掉前4个字符
echo %aa:~4%
echo 截取后4个字符
echo %aa:~-4%
echo %aa:~10%
echo 截取第一个到倒数5个
echo %aa:~0,-5%
echo %aa:~0,9%
echo 3个开始，长度为5
echo %aa:~2,5%
echo 倒数10个开始，长度为5
echo %aa:~-10,5%
echo %aa:~4,5%
echo 倒数10个开始，长度为倒数5个
echo %aa:~-10,-4%
echo %aa:~4,-4%
echo %aa:~-10,6%
echo %aa:~4,6%
call test_return
echo %errorlevel%
::结论:
::%str:~n% 截掉前n位
::%str:~-n% 截取后n位，相当于%str:~len-n%,len为字符串长度
::%str:~a,b%  a为起始位的index，b截取长度
::%str:~a,-b% a为起始位的index，-b相当于len-b-a
::%str:~-a,-b% -a为起始位置,相当于len-a，-b相当于-b+a
pause