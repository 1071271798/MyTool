@echo off
xcopy 1.txt act7 /-Y
if errorlevel 4 echo ����������д�̴���
if errorlevel 3 echo Ԥ�ô�����ֹ�ļ���������
if errorlevel 2 echo �û�ͨ��ctrl-c��ֹ��������
if errorlevel 1 echo δ�ҵ������ļ�
if errorlevel 0 echo �ɹ������ļ�