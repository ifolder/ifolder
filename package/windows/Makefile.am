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
#  Author: Paul Thomas <pthomas@novell.com>
#
#######################################################################
ShutdowniFolder_CSFILES = $(srcdir)/ShutdowniFolder/ShutdowniFolder.cs $(srcdir)/ShutdowniFolder/AssemblyInfo.cs

EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/ifolder-install.sln $(srcdir)/ifolder-msi/ifolder-msi.vdproj $(srcdir)/ShutdowniFolder/ShutdowniFolder.csproj $(ShutdowniFolder_CSFILES) $(srcdir)/ShutdowniFolder/App.ico

RELEASE = 1
MSI_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).msi
MSI_LOG_FILE = devenv-ifolder-msi.log

.PHONY: package package-clean package-install package-uninstall ifolder devenv

all clean:

package: $(MSI_FILE)

devenv:
	@if ! test -x "$(VSINSTALLDIR)/devenv.exe"; then echo "Error: Microsoft Visual Studio .NET is currently required to build MSI packages"; exit 1; fi

$(MSI_FILE): devenv ifolder ifolder-install.sln ifolder-msi/ifolder-msi.vdproj ShutdowniFolder/ShutdowniFolder.exe
	@rm -f $(MSI_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ifolder-msi /out $(MSI_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv ifolder-msi/$(DEVENV_CONFIGURATION)/ifolder.msi .; \
		ln ifolder.msi $@; \
	else \
		grep -a "ERROR:" $(MSI_LOG_FILE); \
	fi

ifolder:
	make -C ../.. all
	
ShutdowniFolder/ShutdowniFolder.exe: ShutdowniFolder/ShutdowniFolder.csproj $(ShutdowniFolder_CSFILES)
	"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ShutdowniFolder /out devenv-shutdown-msi.log

package-install: $(MSI_FILE)
	@CMD='msiexec /i $(MSI_FILE) /qb+ /L* install-msi.log'; \
	echo $$CMD; \
	if ! eval $$CMD; then \
		grep -a "ERROR:" install-msi.log; \
	fi
	
package-uninstall:
	@CMD='msiexec /x $(MSI_FILE) /qb+ /L* uninstall-msi.log'; \
	echo $$CMD; \
	if ! eval $$CMD; then \
		grep -a "ERROR:" uninstall-msi.log; \
	fi

package-clean clean-local:
	rm -rf *.msi Release Debug */Release */Debug *.log *.suo
	cd ShutdowniFolder; rm -rf ShutdowniFolder.exe bin obj *.log *.suo *.csproj.user

distclean-local: package-clean

maintainer-clean-local:
	rm -f Makefile.in

