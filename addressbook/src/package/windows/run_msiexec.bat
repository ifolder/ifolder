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
REM msiexec {install|uninstall} path_to_msi
REM 
setlocal
set MSIEXEC=msiexec
set MSI_MODE=%1
set MSI_FILE=%2
set MSI_LOG=msiexec-%MSI_MODE%-%PROJECT%.log

if NOT EXIST %MSI_FILE% goto missing_file

if %MSI_MODE%==install goto install

REM uninstall
:uninstall
set MSI_OPT=/x
goto msiexec

REM install
:install
set MSI_OPT=/i

REM msiexec
:msiexec
echo %MSIEXEC% %MSI_OPT% %MSI_FILE% /q /L* %MSI_LOG%
%MSIEXEC% %MSI_OPT% %MSI_FILE% /q /L* %MSI_LOG%

if ERRORLEVEL 1 goto msiexec_error

REM success
exit /b 0

REM error - MSI file does not exist
:missing_file
@echo %MSI_FILE% does not exist
exit /b 1

REM error - msiexec failed
:msiexec_error
type %MSI_LOG%
exit /b 1

