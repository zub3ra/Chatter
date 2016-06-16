@echo off

REM Echo Getting Source from GIT
rem call git --git-dir="%~dp0.git" pull || (GOTO ERROR)

REM call git --git-dir="%~dp0.git" --work-tree="%~dp0" pull && (echo success) || (GOTO ERROR)
SET FOUND_SOURCE=0
REM Echo Building Source
FOR %%i IN (%~dp0*.csproj) DO (
SET FOUND_SOURCE=1

REM Use Microsoft Build Tools 2015 that includes C# 6 (https://www.microsoft.com/en-us/download/details.aspx?id=48159)
"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild" %%i /t:Clean;Build || (GOTO ERROR)
)

IF "%FOUND_SOURCE%"=="0" ( echo No source found, check batch argument
GOTO ERROR  )

GOTO END

:ERROR
echo Error building!

:END
