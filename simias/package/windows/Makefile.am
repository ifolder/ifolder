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
EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/simias-install.sln $(srcdir)/simias-msi/simias-msi.vdproj $(srcdir)/simias-msm/simias-msm.vdproj $(srcdir)/simias-sdk-msi/simias-sdk-msi.vdproj $(srcdir)/simias-sdk-msm/simias-sdk-msm.vdproj

RELEASE = 1
MSI_FILE     = $(PACKAGE)-$(VERSION)-$(RELEASE).msi
MSM_FILE     = $(PACKAGE)-$(VERSION)-$(RELEASE).msm
SDK_MSI_FILE = $(PACKAGE)-sdk-$(VERSION)-$(RELEASE).msi
SDK_MSM_FILE = $(PACKAGE)-sdk-$(VERSION)-$(RELEASE).msm
MSI_LOG_FILE     = devenv-simias-msi.log
MSM_LOG_FILE     = devenv-simias-msm.log
SDK_MSI_LOG_FILE = devenv-simias-sdk-msi.log
SDK_MSM_LOG_FILE = devenv-simias-sdk-msm.log

.PHONY: package package-sdk package-clean package-install package-uninstall simias simias-doc devenv

all clean:

package: $(MSI_FILE)

package-sdk: package $(SDK_MSI_FILE)

simias:
	make -C ../.. all
	
simias-doc:
	make -C ../../doc all
	
devenv:
	@if ! test -x "$(VSINSTALLDIR)/devenv.exe"; then echo "Error: Microsoft Visual Studio .NET is currently required to build MSI and MSM packages"; exit 1; fi

ndoc:
	@if ! test -x "$(NDOC_CMD)"; then echo "Error: You must configure using --with-ndoc-path to build SDK packages"; exit 1; fi

$(MSI_FILE): devenv $(MSM_FILE) simias-install.sln simias-msi/simias-msi.vdproj
	@rm -f $(MSI_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" simias-install.sln /build $(DEVENV_CONFIGURATION) /project simias-msi /out $(MSI_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv simias-msi/$(DEVENV_CONFIGURATION)/simias.msi .; \
		ln simias.msi $@; \
	else \
		grep -a "ERROR:" $(MSI_LOG_FILE); \
	fi

$(MSM_FILE): devenv simias simias-install.sln simias-msm/simias-msm.vdproj
	@rm -f $(MSM_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" simias-install.sln /build $(DEVENV_CONFIGURATION) /project simias-msm /out $(MSM_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv simias-msm/$(DEVENV_CONFIGURATION)/simias.msm .; \
		ln simias.msm $@; \
	else \
		grep -a "ERROR:" $(MSM_LOG_FILE); \
	fi

$(SDK_MSI_FILE): devenv $(SDK_MSM_FILE) simias-install.sln simias-sdk-msi/simias-sdk-msi.vdproj
	@rm -f $(SDK_MSI_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" simias-install.sln /build $(DEVENV_CONFIGURATION) /project simias-sdk-msi /out $(SDK_MSI_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv simias-sdk-msi/$(DEVENV_CONFIGURATION)/simias-sdk.msi .; \
		ln simias-sdk.msi $@; \
	else \
		grep -a "ERROR:" $(SDK_MSI_LOG_FILE); \
	fi

$(SDK_MSM_FILE): devenv ndoc $(MSM_FILE) simias-doc simias-install.sln simias-sdk-msm/simias-sdk-msm.vdproj
	@rm -f $(SDK_MSM_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" simias-install.sln /build $(DEVENV_CONFIGURATION) /project simias-sdk-msm /out $(SDK_MSM_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv simias-sdk-msm/$(DEVENV_CONFIGURATION)/simias-sdk.msm .; \
		ln simias-sdk.msm $@; \
	else \
		grep -a "ERROR:" $(SDK_MSM_LOG_FILE); \
	fi

package-clean clean-local:
	rm -rf  *.msi *.msm Release Debug */Release */Debug *.log *.suo

distclean-local: package-clean
	rm -f simias.spec simias-sdk.spec

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

