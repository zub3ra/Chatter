@ECHO OFF

IF "%CONFIGURATION%"=="" SET CONFIGURATION=Debug

star --resourcedir="%~dp0src\Chatter\wwwroot" "%~dp0src/Chatter/bin/%CONFIGURATION%/Chatter.exe"