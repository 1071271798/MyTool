echo off
set input="%1 - ¸±±¾.jpg"
echo %input%
set /a output=%1+35
rename %input% %output%.jpg
