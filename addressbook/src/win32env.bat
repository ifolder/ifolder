@ECHO OFF

REM ***********************************************************************
REM *  $RCSfile$
REM * 
REM *  Copyright (C) 2004 Novell, Inc.
REM *
REM *  This library is free software; you can redistribute it and/or
REM *  modify it under the terms of the GNU General Public
REM *  License as published by the Free Software Foundation; either
REM *  version 2 of the License, or (at your option) any later version.
REM *
REM *  This library is distributed in the hope that it will be useful,
REM *  but WITHOUT ANY WARRANTY; without even the implied warranty of
REM *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
REM *  Library General Public License for more details.
REM *
REM *  You should have received a copy of the GNU General Public
REM *  License along with this library; if not, write to the Free
REM *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
REM *
REM *  Author: Rob
REM * 
REM ***********************************************************************/

REM put make into the path
SET PATH=%CD%\..\tools\gnu\win32;%PATH%

REM put perl into the path
SET PATH=%CD%\..\tools\Perl\bin;%PATH%

REM enable the visual studio command line tools
CALL "%VS71COMNTOOLS%\vsvars32.bat" > NUL

REM register nunit.framework.dll
gacutil /silent /nologo /i ../tools/NUnit/bin/nunit.framework.dll /f
gacutil /silent /nologo /i ../tools/NUnit/bin/nunit.util.dll /f

ECHO.
ECHO Welcome to the Denali Win32 Build Environment!
ECHO.

REM===========================================================================
REM File CVS History:
REM
REM $Log$
REM Revision 1.1  2004/02/21 19:01:16  cgaisford
REM Initial revision
REM
REM Revision 1.1.1.1  2004/02/21 18:18:17  cgaisford
REM Inital checkin
REM
REM Revision 1.2  2004/02/20 18:11:52  rlyon
REM Added and replaced headers.
REM
REM Revision 1.1  2003/09/30 02:59:51  rlyon
REM ..in with the new.
REM
REM Revision 1.3  2003/09/16 19:55:45  rlyon
REM Added assemblies to the cache for NUnit
REM
REM Revision 1.2  2003/09/11 23:29:51  rlyon
REM Changed configure to a Perl script.
REM
REM
REM===========================================================================


