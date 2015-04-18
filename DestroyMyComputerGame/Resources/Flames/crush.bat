@echo off
for /f "tokens=*" %%x in ('dir /b *.png') do (
echo "crushing %%x"
pngcrush -brute "%%x" temp.png
move /Y temp.png "%%x"
)