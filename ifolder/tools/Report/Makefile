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


