@echo off
netstat -a -n > a.txt
type a.txt | find "2764" && echo "Congratulations! You have infected GLACIER!"
