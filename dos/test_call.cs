@echo off
echo ·��:%~dp0
echo ������ʱ�ļ�>tmp.txt
::���浱��Ŀ¼��������Ŀ¼
pushd C:\Windows
call :sub tmp.txt
::�ָ�Ŀ¼
popd
call :sub tmp.txt
pause
type tmp.txt|more
:sub
echo ������ֵ: %1
echo ɾ������: %~1
echo ���䵽·��: %~f1
echo ���䵽һ����������: %~d1
echo ���䵽һ��·��: %~p1
echo ���䵽һ���ļ���: %~n1
echo ���䵽һ���ļ���չ��: %~x1
echo �����·��ֻ���ж���: %~s1
echo ���䵽�ļ�����: %~a1
echo ���䵽�ļ�������/ʱ��: %~t1
echo ���䵽�ļ��Ĵ�С: %~z1
echo ��չ���������ź�·��: %~dp1
echo ��չ���ļ�������չ��: %~nx1
echo ��չ������DIR�������: %~ftza1
echo.
goto :eof