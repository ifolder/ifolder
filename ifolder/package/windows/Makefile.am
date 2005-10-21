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

EXTRA_DIST = $(srcdir)/ifolder-msi.ism $(srcdir)/ifolder-msm.ism $(srcdir)/ifolder-sdk-msm.ism

export PKG_CONFIG_PATH := $(PKG_CONFIG_PATH)

CUR_DIR := $(shell pwd)

if WINDOWS
OUT_DIR := $(shell cygpath --windows $(CUR_DIR))
MERGE_MODULES_WIN  := $(shell cygpath --windows "$(USERPROFILE)\My Documents\My InstallShield DevStudio Projects\MergeModules")
MERGE_MODULES_UNIX := $(shell cygpath --unix "$(USERPROFILE)\My Documents\My InstallShield DevStudio Projects\MergeModules")
PREREQUISITES_WIN  := $(shell cygpath --windows "$(PROGRAMFILES)\InstallShield 11\SetupPrerequisites")
PREREQUISITES_UNIX := $(shell cygpath --unix "$(PROGRAMFILES)\InstallShield 11\SetupPrerequisites")
ISSABLD_DIR := $(shell cygpath --windows `dirname $(ISSABLD)`)
REGASM_DIR1 := $(shell cygpath --unix $$FRAMEWORKDIR/v1.1*)
REGASM_DIR2 := $(shell ls -d $(REGASM_DIR1))
REGASM_DIR := $(shell cygpath --windows $(REGASM_DIR2))
endif

RELEASE = 1

if DEBUG
DEBUG_OPTION = --enable-debug
MSI_FILE     = $(PACKAGE)_debug-$(VERSION)-$(RELEASE)-setup.exe
MSM_FILE     = $(PACKAGE)_debug-$(VERSION)-$(RELEASE).msm
SDK_MSM_FILE = $(PACKAGE)_debug-sdk-$(VERSION)-$(RELEASE).msm
else
MSI_FILE     = $(PACKAGE)-$(VERSION)-$(RELEASE)-setup.exe
MSM_FILE     = $(PACKAGE)-$(VERSION)-$(RELEASE).msm
SDK_MSM_FILE = $(PACKAGE)-sdk-$(VERSION)-$(RELEASE).msm
endif

MSI = ifolder-msi
MSM = ifolder-msm
SDK_MSM = ifolder-sdk-msm

MSI_LOG_FILE     = issabld-$(MSI).log
MSM_LOG_FILE     = issabld-$(MSM).log
SDK_MSM_LOG_FILE = issabld-$(SDK_MSM)-msm.log
BLD_VER_LOG_FILE = bldver.log

ISSABLD_CONFIG  = web
ISSABLD_RELEASE = release

DISK1         = $(ISSABLD_CONFIG)/$(ISSABLD_RELEASE)/DiskImages/DISK1
MSI_DISK1     = $(MSI)/$(DISK1)/setup.exe
MSM_DISK1     = $(MSM)/$(DISK1)/$(subst -,_,ifolder).msm
SDK_MSM_DISK1 = $(SDK_MSM)/$(DISK1)/$(subst -,_,ifolder)_sdk.msm

all: $(MSI_FILE)

$(MSI_FILE): issabld $(PACKAGE)-install $(MSI).ism
	@rm -f $@ "$(MSI_DISK1)" "$(MSI_LOG_FILE)"
	mkdir -p "$(PREREQUISITES_UNIX)";
#	mkdir -p "$(PROGRAMFILES)\InstallShield 11\SetupPrerequisites";
	cp -f SimiasSetupPrerequisite.prq "$(PREREQUISITES_UNIX)";
#	cp -f SimiasSetupPrerequisite.prq "$(PROGRAMFILES)\InstallShield 11\SetupPrerequisites";
#		echo $$CMD; \
#		eval $$CMD

	@CMD='"$(ISSABLD)" -t "$(REGASM_DIR)" -p "$(MSI).ism" -o "$(ISSABLD_DIR),$(MERGE_MODULES_WIN)"'; \
		echo $$CMD; \
		eval $$CMD > "$(MSI_LOG_FILE)" 2>&1; \
		if test -f "$(MSI_DISK1)"; then \
			ln "$(MSI_DISK1)" $@; \
		else \
			cat "$(MSI_LOG_FILE)"; \
			echo FAILED; exit 1; \
		fi
	@test "x" = "x$(INSTALL_PKGS)" || \
		CMD='cp -f $@ "$(INSTALL_PKGS)"'; \
		echo $$CMD; \
		eval $$CMD
        


$(MSM_FILE): issabld $(PACKAGE)-install $(MSM).ism
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

$(SDK_MSM_FILE): issabld $(PACKAGE)-doc $(SDK_MSM).ism
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

.PHONY: issabld $(PACKAGE)-install $(PACKAGE) $(PACKAGE)-configure $(PACKAGE)-dist $(PACKAGE)-doc

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
	
$(PACKAGE)-install: $(PACKAGE)
	$(MAKE) -C MSI/BUILD/$(PACKAGE)-$(VERSION) install
	
$(PACKAGE): $(PACKAGE)-configure
	$(MAKE) -C MSI/BUILD/$(PACKAGE)-$(VERSION) all
	
$(PACKAGE)-configure: $(PACKAGE)-dist
	rm -rf MSI
	mkdir MSI
	mkdir -p MSI/BUILD
	mkdir -p MSI/INSTALL
	cd MSI/BUILD; \
		tar -xzf $(CUR_DIR)/$(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz
	cd MSI/BUILD/$(PACKAGE)-$(VERSION); \
		./configure --prefix=$(CUR_DIR)/MSI/INSTALL $(DEBUG_OPTION)
	
$(PACKAGE)-dist:
	$(MAKE) -C $(top_srcdir) dist
	@test "x" = "x$(SOURCE_TARBALLS)" || \
		CMD='cp -f $(top_srcdir)/$(PACKAGE)-$(VERSION).tar.gz "$(SOURCE_TARBALLS)"'; \
		echo $$CMD; \
		eval $$CMD

$(PACKAGE)-doc: $(PACKAGE)
	$(MAKE) -C $(top_srcdir)/doc all
	
clean-local:
	rm -rf $(COMMON_CLEAN_FILES)
	rm -rf *-setup.exe *.msi *.msm *.log
	rm -rf MSI $(MSI) $(MSM) $(SDK_MSM)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES) ifolder.spec

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
