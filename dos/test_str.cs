@echo off
set aa=abcdefghijklmn
set cmdstr=%aa%
echo %cmdstr%
::�ַ�����ȡ
echo ��ȡǰ4���ַ�
echo %aa:~0,4%
echo �ص�ǰ4���ַ�
echo %aa:~4%
echo ��ȡ��4���ַ�
echo %aa:~-4%
echo %aa:~10%
echo ��ȡ��һ��������5��
echo %aa:~0,-5%
echo %aa:~0,9%
echo 3����ʼ������Ϊ5
echo %aa:~2,5%
echo ����10����ʼ������Ϊ5
echo %aa:~-10,5%
echo %aa:~4,5%
echo ����10����ʼ������Ϊ����5��
echo %aa:~-10,-4%
echo %aa:~4,-4%
echo %aa:~-10,6%
echo %aa:~4,6%
call test_return
echo %errorlevel%
::����:
::%str:~n% �ص�ǰnλ
::%str:~-n% ��ȡ��nλ���൱��%str:~len-n%,lenΪ�ַ�������
::%str:~a,b%  aΪ��ʼλ��index��b��ȡ����
::%str:~a,-b% aΪ��ʼλ��index��-b�൱��len-b-a
::%str:~-a,-b% -aΪ��ʼλ��,�൱��len-a��-b�൱��-b+a
pause