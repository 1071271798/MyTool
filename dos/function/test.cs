@echo off
set str="111"
echo %str%
echo.
echo %str%|findstr ""111"" > nul && (
echo %str% 
) || (
echo error
)
echo %str%|findstr "^".*"$" > nul && (
echo %str% 
) || (
echo error
)
pause