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
EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/simias-msi.ism $(srcdir)/simias-msm.ism $(srcdir)/simias-sdk-msm.ism

CUR_DIR := $(shell pwd)
OUT_DIR := $(shell cygpath --windows $(CUR_DIR))

MERGE_MODULES_WIN  := $(shell cygpath --windows "$(USERPROFILE)\My Documents\My InstallShield DevStudio Projects\MergeModules")
MERGE_MODULES_UNIX := $(shell cygpath --unix "$(USERPROFILE)\My Documents\My InstallShield DevStudio Projects\MergeModules")

RELEASE = 1
MSI_FILE     = $(PACKAGE)-$(VERSION)-$(RELEASE)-setup.exe
MSM_FILE     = $(PACKAGE)-$(VERSION)-$(RELEASE).msm
SDK_MSM_FILE = $(PACKAGE)-sdk-$(VERSION)-$(RELEASE).msm

MSI = simias-msi
MSM = simias-msm
SDK_MSM = simias-sdk-msm

MSI_LOG_FILE     = issabld-$(MSI).log
MSM_LOG_FILE     = issabld-$(MSM).log
SDK_MSM_LOG_FILE = issabld-$(SDK_MSM)-msm.log

#ISSABLD = ../../tools/InstallShield/IsSABld.exe
ISSABLD_CONFIG  = web
ISSABLD_RELEASE = release

DISK1         = $(ISSABLD_CONFIG)/$(ISSABLD_RELEASE)/DiskImages/DISK1
MSI_DISK1     = $(MSI)/$(DISK1)/setup.exe
MSM_DISK1     = $(MSM)/$(DISK1)/simias.msm
SDK_MSM_DISK1 = $(SDK_MSM)/$(DISK1)/simias_sdk.msm

all: $(MSI_FILE)

$(MSI_FILE): issabld $(MSM_FILE) $(SDK_MSM_FILE) $(MSI).ism
	@rm -f $@ "$(MSI_DISK1)" "$(MSI_LOG_FILE)"
	@CMD='"$(ISSABLD)" -p "$(MSI).ism" -o "$(MERGE_MODULES_WIN)"'; \
		echo $$CMD; \
		eval $$CMD > "$(MSI_LOG_FILE)" 2>&1; \
		if test -f "$(MSI_DISK1)"; then \
			ln "$(MSI_DISK1)" $@; \
		else \
			cat "$(MSI_LOG_FILE);" \
			echo FAILED; exit 1; \
		fi
	@test "x" = "x$(INSTALL_PKGS)" || \
		CMD='cp -f $@ "$(INSTALL_PKGS)"'; \
		echo $$CMD; \
		eval $$CMD

$(MSM_FILE): issabld simias-install $(MSM).ism
	@rm -f $@ "$(MSM_DISK1)" "$(MSM_LOG_FILE)"
	@CMD='"$(ISSABLD)" -p "$(MSM).ism"'; \
		echo $$CMD; \
		eval $$CMD > "$(MSM_LOG_FILE)" 2>&1; \
		if test -f "$(MSM_DISK1)"; then \
			ln "$(MSM_DISK1)" $@; \
		else \
			cat "$(MSM_LOG_FILE)"; \
			echo FAILED; exit 1; \
		fi 
	@mkdir -p "$(MERGE_MODULES_UNIX)"; \
		CMD='cp -f "$(MSM_DISK1)" "$(MERGE_MODULES_UNIX)"'; \
		echo $$CMD; \
		eval $$CMD

$(SDK_MSM_FILE): issabld simias-doc $(SDK_MSM).ism
	@rm -f $@ "$(SDK_MSM_DISK1)" "$(SDK_MSM_LOG_FILE)"
	@CMD='"$(ISSABLD)" -p "$(SDK_MSM).ism"'; \
		echo $$CMD; \
		eval $$CMD > "$(SDK_MSM_LOG_FILE)" 2>&1; \
		if test -f "$(SDK_MSM_DISK1)"; then \
			ln "$(SDK_MSM_DISK1)" $@; \
		else \
			cat "$(MSM_LOG_FILE)"; \
			echo FAILED; \
		fi 
	@mkdir -p "$(MERGE_MODULES_UNIX)"; \
		CMD='cp -f "$(SDK_MSM_DISK1)" "$(MERGE_MODULES_UNIX)"'; \
		echo $$CMD; \
		eval $$CMD

.PHONY: simias-install simias simias-configure simias-dist simias-doc

issabld:
	@if test -z "$(ISSABLD)"; then \
		echo ""; \
		echo "ERROR: environment variable ISSABLD is not set"; \
		echo ""; \
		echo "NOTE:  InstallShield is used to build MSI/MSM packages."; \
		echo "       Set ISSABLD to location of IsSABld.exe."; \
		echo ""; \
		exit 1; \
	fi

simias-install: simias
	$(MAKE) -C MSI/BUILD/$(PACKAGE)-$(VERSION) install
	
simias: simias-configure
	$(MAKE) -C MSI/BUILD/$(PACKAGE)-$(VERSION) all
	
simias-configure: simias-dist
	rm -rf MSI
	mkdir MSI
	mkdir -p MSI/BUILD
	mkdir -p MSI/INSTALL
	cd MSI/BUILD; \
		tar -xzf $(CUR_DIR)/$(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz
	cd MSI/BUILD/$(PACKAGE)-$(VERSION); \
		./configure --prefix=$(CUR_DIR)/MSI/INSTALL
	
simias-dist:
	$(MAKE) -C $(top_srcdir) dist

simias-doc: simias
	$(MAKE) -C $(top_srcdir)/doc all
	
clean-local:
	rm -rf $(COMMON_CLEAN_FILES)
	rm -rf *-setup.exe *.msi *.msm *.log
	rm -rf MSI
	rm -rf $(MSI)/$(ISSABLD_CONFIG)
	rm -rf $(MSM)/$(ISSABLD_CONFIG)
	rm -rf $(SDK_MSM)/$(ISSABLD_CONFIG)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES) simias.spec simias-sdk.spec

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

# do nothing for the following standard targets
all clean:

