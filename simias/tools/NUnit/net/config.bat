@ECHO OFF

REM enable the VisualStudio command line tools
CALL "%VS71COMNTOOLS%\vsvars32.bat" > NUL

REM config NUnit
gacutil /i ./bin/nunit.core.dll /f
gacutil /i ./bin/nunit.framework.dll /f
