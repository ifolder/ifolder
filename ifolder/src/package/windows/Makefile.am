ShutdowniFolder_CSFILES = $(srcdir)/ShutdowniFolder/ShutdowniFolder.cs $(srcdir)/ShutdowniFolder/AssemblyInfo.cs

EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/ifolder-install.sln $(srcdir)/ifolder-msi/ifolder-msi.vdproj $(srcdir)/ShutdowniFolder/ShutdowniFolder.csproj $(ShutdowniFolder_CSFILES) $(srcdir)/ShutdowniFolder/App.ico

all: ifolder.msi

ifolder.msi: ifolder-install.sln ifolder-msi/ifolder-msi.vdproj ShutdowniFolder/ShutdowniFolder.exe
	make -C ../../.. all
	"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ifolder-msi /out devenv-ifolder-msi.log
	mv ifolder-msi/$(DEVENV_CONFIGURATION)/ifolder.msi $@

ShutdowniFolder/ShutdowniFolder.exe: ShutdowniFolder/ShutdowniFolder.csproj $(ShutdowniFolder_CSFILES)
	"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ShutdowniFolder /out devenv-shutdown-msi.log

install: ifolder.msi
	msiexec /i ifolder.msi /q /L* install-msi.log
	
uninstall:
	msiexec /x ifolder.msi /q /L* uninstall-msi.log

clean-local:
	rm -rf *.msi Release Debug */Release */Debug *.log *.suo
	cd ShutdowniFolder; rm -rf ShutdowniFolder.exe bin obj *.log *.suo

maintainer-clean-local:
	rm -f Makefile.in

