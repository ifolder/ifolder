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

perl -w configure.pl %1 %2 %3 %4 %5 %6 %7 %8 %9

