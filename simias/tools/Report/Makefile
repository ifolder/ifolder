#######################################################################
#  $RCSfile$
#
#  Copyright (C) 2004 Novell, Inc.
#
#  This program is free software; you can redistribute it and/or
#  modify it under the terms of the GNU General Public
#  License as published by the Free Software Foundation; either
#  version 2 of the License, or (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
#  General Public License for more details.
#
#  You should have received a copy of the GNU General Public
#  License along with this program; if not, write to the Free
#  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#
#  Author: Rob
#
#######################################################################
CSCFLAGS = /optimize+ /d:MONO /warn:4 /d:TRACE /nologo
#CSCFLAGS = /debug+ /d:DEBUG /d:TRACE

Report_CSFILES = AssemblyInfo.cs Report.cs
Report_FLAGS = /t:exe
Report_LIBS = System.dll System.Data.dll System.Xml.dll
Report_LIBPATH = 

all: Report.exe

Report.exe: $(Report_CSFILES)
	csc /out:$@ /doc:Report.doc.xml $(CSCFLAGS) $(Report_FLAGS) $(Report_LIBS:%=/r:%) $(Report_LIBPATH:%=/lib:%) $(Report_CSFILES)

clean:
	rm -f Report.exe Report.pdb Report.suo Report.csproj.user *.xml
	rm -rf obj bin


