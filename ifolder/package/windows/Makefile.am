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

EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/ifolder-install.sln $(srcdir)/ifolder-msi/ifolder-msi.vdproj $(srcdir)/ifolder-msm/ifolder-msm.vdproj $(srcdir)/ifolder-sdk-msi/ifolder-sdk-msi.vdproj $(srcdir)/ifolder-sdk-msm/ifolder-sdk-msm.vdproj $(srcdir)/ShutdowniFolder/ShutdowniFolder.csproj $(ShutdowniFolder_CSFILES) $(srcdir)/ShutdowniFolder/App.ico

RELEASE = 1
MSI_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).msi
MSM_FILE     = $(PACKAGE)-$(VERSION)-$(RELEASE).msm
SDK_MSI_FILE = $(PACKAGE)-sdk-$(VERSION)-$(RELEASE).msi
SDK_MSM_FILE = $(PACKAGE)-sdk-$(VERSION)-$(RELEASE).msm
MSI_LOG_FILE = devenv-ifolder-msi.log
MSM_LOG_FILE     = devenv-ifolder-msm.log
SDK_MSI_LOG_FILE = devenv-ifolder-sdk-msi.log
SDK_MSM_LOG_FILE = devenv-ifolder-sdk-msm.log

.PHONY: package package-clean package-install package-uninstall ifolder devenv

all clean:

package: $(MSI_FILE)

package-sdk: package $(SDK_MSI_FILE)

ifolder:
	make -C ../.. all
	
ifolder-doc:
	make -C ../../doc all
	
devenv:
	@if ! test -x "$(VSINSTALLDIR)/devenv.exe"; then echo "Error: Microsoft Visual Studio .NET is currently required to build MSI and MSM packages"; exit 1; fi

ndoc:
	@if ! test -x "$(NDOC_CMD)"; then echo "Error: You must configure using --with-ndoc-path to build SDK packages"; exit 1; fi

$(MSI_FILE): devenv $(MSM_FILE) ifolder-install.sln ifolder-msi/ifolder-msi.vdproj ShutdowniFolder/ShutdowniFolder.exe
	@rm -f $(MSI_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ifolder-msi /out $(MSI_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv ifolder-msi/$(DEVENV_CONFIGURATION)/ifolder.msi .; \
		ln ifolder.msi $@; \
	else \
		grep -a "ERROR:" $(MSI_LOG_FILE); \
	fi

$(MSM_FILE): devenv ifolder ifolder-install.sln ifolder-msm/ifolder-msm.vdproj
	@rm -f $(MSM_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ifolder-msm /out $(MSM_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv ifolder-msm/$(DEVENV_CONFIGURATION)/ifolder.msm .; \
		ln ifolder.msm $@; \
	else \
		grep -a "ERROR:" $(MSM_LOG_FILE); \
	fi

$(SDK_MSI_FILE): devenv $(SDK_MSM_FILE) ifolder-install.sln ifolder-sdk-msi/ifolder-sdk-msi.vdproj
	@rm -f $(SDK_MSI_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ifolder-sdk-msi /out $(SDK_MSI_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv ifolder-sdk-msi/$(DEVENV_CONFIGURATION)/ifolder-sdk.msi .; \
		ln ifolder-sdk.msi $@; \
	else \
		grep -a "ERROR:" $(SDK_MSI_LOG_FILE); \
	fi

$(SDK_MSM_FILE): devenv ndoc $(MSM_FILE) ifolder-doc ifolder-install.sln ifolder-sdk-msm/ifolder-sdk-msm.vdproj
	@rm -f $(SDK_MSM_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ifolder-sdk-msm /out $(SDK_MSM_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv ifolder-sdk-msm/$(DEVENV_CONFIGURATION)/ifolder-sdk.msm .; \
		ln ifolder-sdk.msm $@; \
	else \
		grep -a "ERROR:" $(SDK_MSM_LOG_FILE); \
	fi

ShutdowniFolder/ShutdowniFolder.exe: ShutdowniFolder/ShutdowniFolder.csproj $(ShutdowniFolder_CSFILES)
	"$(VSINSTALLDIR)/devenv.exe" ifolder-install.sln /build $(DEVENV_CONFIGURATION) /project ShutdowniFolder /out devenv-shutdown-msi.log


package-clean clean-local:
	rm -rf  *.msi *.msm Release Debug */Release */Debug *.log *.suo
	cd ShutdowniFolder; rm -rf ShutdowniFolder.exe bin obj *.log *.suo *.csproj.user

distclean-local: package-clean
	rm -f ifolder.spec

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

