@echo off
REM========================================================================
REM  $RCSfile$
REM 
REM  Copyright (C) 2004 Novell, Inc.
REM
REM  This library is free software; you can redistribute it and/or
REM  modify it under the terms of the GNU General Public
REM  License as published by the Free Software Foundation; either
REM  version 2 of the License, or (at your option) any later version.
REM
REM  This library is distributed in the hope that it will be useful,
REM  but WITHOUT ANY WARRANTY; without even the implied warranty of
REM  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
REM  Library General Public License for more details.
REM
REM  You should have received a copy of the GNU General Public
REM  License along with this library; if not, write to the Free
REM  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
REM
REM  Author: Paul Thomas <pthomas@novell.com>
REM 
REM========================================================================
REM 
REM devenv {install|uninstall} path_to_msi
REM 
setlocal
set DEVENV=%VSINSTALLDIR%/devenv.exe
set SLN_FILE=%1
set PROJECT=%2
set CONFIGURATION=%3
set DEVENV_LOG=devenv-%PROJECT%.log

if NOT EXIST %SLN_FILE% goto missing_file

REM clear log file
del /f /q %DEVENV_LOG% 2> NUL

REM devenv
echo "%DEVENV%" %SLN_FILE% /build %CONFIGURATION% /project %PROJECT% /out %DEVENV_LOG%
"%DEVENV%" %SLN_FILE% /build %CONFIGURATION% /project %PROJECT% /out %DEVENV_LOG%

if ERRORLEVEL 1 goto devenv_error

REM success
exit /b 0

REM error - SLN file does not exist
:missing_file
@echo %SLN_FILE% does not exist
exit /b 1

REM error - devenv failed
:devenv_error
type %DEVENV_LOG%
exit /b 1

