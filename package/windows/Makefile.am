ShutdowniFolder_CSFILES = $(srcdir)/ShutdowniFolder/ShutdowniFolder.cs $(srcdir)/ShutdowniFolder/AssemblyInfo.cs

EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/ifolder-install.sln $(srcdir)/ifolder-msi/ifolder-msi.vdproj $(srcdir)/ShutdowniFolder/ShutdowniFolder.csproj $(ShutdowniFolder_CSFILES) $(srcdir)/ShutdowniFolder/App.ico

.PHONY: package package-clean package-install package-uninstall ifolder

all clean:

package: ifolder.msi

ifolder.msi: ifolder ifolder-install.sln ifolder-msi/ifolder-msi.vdproj ShutdowniFolder/ShutdowniFolder.exe
	"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ifolder-msi /out devenv-ifolder-msi.log
	mv ifolder-msi/$(DEVENV_CONFIGURATION)/ifolder.msi $@

ifolder:
	make -C ../.. all
	
ShutdowniFolder/ShutdowniFolder.exe: ShutdowniFolder/ShutdowniFolder.csproj $(ShutdowniFolder_CSFILES)
	"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ShutdowniFolder /out devenv-shutdown-msi.log

package-install: ifolder.msi
	msiexec /i ifolder.msi /q /L* install-msi.log
	
package-uninstall:
	msiexec /x ifolder.msi /q /L* uninstall-msi.log

package-clean clean-local:
	rm -rf *.msi Release Debug */Release */Debug *.log *.suo
	cd ShutdowniFolder; rm -rf ShutdowniFolder.exe bin obj *.log *.suo *.csproj.user

distclean-local: package-clean
	rm -f ../*/Makefile ../linux/ifolder.spec

maintainer-clean-local:
	rm -f ../*/Makefile.in

