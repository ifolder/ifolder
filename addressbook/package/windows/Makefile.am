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
EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/addressbook-install.sln $(srcdir)/addressbook-msi/addressbook-msi.vdproj $(srcdir)/addressbook-msm/addressbook-msm.vdproj $(srcdir)/addressbook-sdk-msi/addressbook-sdk-msi.vdproj $(srcdir)/addressbook-sdk-msm/addressbook-sdk-msm.vdproj

RELEASE = 1
MSI_FILE     = $(PACKAGE)-$(VERSION)-$(RELEASE).msi
MSM_FILE     = $(PACKAGE)-$(VERSION)-$(RELEASE).msm
SDK_MSI_FILE = $(PACKAGE)-sdk-$(VERSION)-$(RELEASE).msi
SDK_MSM_FILE = $(PACKAGE)-sdk-$(VERSION)-$(RELEASE).msm
MSI_LOG_FILE     = devenv-addressbook-msi.log
MSM_LOG_FILE     = devenv-addressbook-msm.log
SDK_MSI_LOG_FILE = devenv-addressbook-sdk-msi.log
SDK_MSM_LOG_FILE = devenv-addressbook-sdk-msm.log

.PHONY: package package-sdk package-clean package-install package-uninstall addressbook addressbook-doc devenv

all clean:

package: $(MSI_FILE)

package-sdk: package $(SDK_MSI_FILE)

addressbook:
	make -C ../.. all
	
addressbook-doc:
	make -C ../../doc all
	
devenv:
	@if ! test -x "$(VSINSTALLDIR)/devenv.exe"; then echo "Error: Microsoft Visual Studio .NET is currently required to build MSI and MSM packages"; exit 1; fi

ndoc:
	@if ! test -x "$(NDOC_CMD)"; then echo "Error: You must configure using --with-ndoc-path to build SDK packages"; exit 1; fi

$(MSI_FILE): devenv $(MSM_FILE) addressbook-install.sln addressbook-msi/addressbook-msi.vdproj
	@rm -f $(MSI_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" addressbook-install.sln /build $(DEVENV_CONFIGURATION) /project addressbook-msi /out $(MSI_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv addressbook-msi/$(DEVENV_CONFIGURATION)/addressbook.msi .; \
		ln addressbook.msi $@; \
	else \
		grep -a "ERROR:" $(MSI_LOG_FILE); \
	fi

$(MSM_FILE): devenv addressbook addressbook-install.sln addressbook-msm/addressbook-msm.vdproj
	@rm -f $(MSM_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" addressbook-install.sln /build $(DEVENV_CONFIGURATION) /project addressbook-msm /out $(MSM_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv addressbook-msm/$(DEVENV_CONFIGURATION)/addressbook.msm .; \
		ln addressbook.msm $@; \
	else \
		grep -a "ERROR:" $(MSM_LOG_FILE); \
	fi

$(SDK_MSI_FILE): devenv $(SDK_MSM_FILE) addressbook-install.sln addressbook-sdk-msi/addressbook-sdk-msi.vdproj
	@rm -f $(SDK_MSI_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" addressbook-install.sln /build $(DEVENV_CONFIGURATION) /project addressbook-sdk-msi /out $(SDK_MSI_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv addressbook-sdk-msi/$(DEVENV_CONFIGURATION)/addressbook-sdk.msi .; \
		ln addressbook-sdk.msi $@; \
	else \
		grep -a "ERROR:" $(SDK_MSI_LOG_FILE); \
	fi

$(SDK_MSM_FILE): devenv ndoc $(MSM_FILE) addressbook-doc addressbook-install.sln addressbook-sdk-msm/addressbook-sdk-msm.vdproj
	@rm -f $(SDK_MSM_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" addressbook-install.sln /build $(DEVENV_CONFIGURATION) /project addressbook-sdk-msm /out $(SDK_MSM_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv addressbook-sdk-msm/$(DEVENV_CONFIGURATION)/addressbook-sdk.msm .; \
		ln addressbook-sdk.msm $@; \
	else \
		grep -a "ERROR:" $(SDK_MSM_LOG_FILE); \
	fi

package-clean clean-local:
	rm -rf  *.msi *.msm Release Debug */Release */Debug *.log *.suo

distclean-local: package-clean
	rm -f addressbook.spec

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

