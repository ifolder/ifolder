#***********************************************************************
#*  $RCSfile$
#* 
#*  Copyright (C) 2004 Novell, Inc.
#*
#*  This library is free software; you can redistribute it and/or
#*  modify it under the terms of the GNU General Public
#*  License as published by the Free Software Foundation; either
#*  version 2 of the License, or (at your option) any later version.
#*
#*  This library is distributed in the hope that it will be useful,
#*  but WITHOUT ANY WARRANTY; without even the implied warranty of
#*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
#*  Library General Public License for more details.
#*
#*  You should have received a copy of the GNU General Public
#*  License along with this library; if not, write to the Free
#*  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#*
#*  Author: Rob
#* 
#***********************************************************************/

# configure makefile
include ../config.mk

#=============================================================================
# os independent component list
#=============================================================================
SUBDIRS_OS_INDEPENDENT = \

#=============================================================================
# linux component list
#=============================================================================
SUBDIRS_OS_LINUX = \
	$(SUBDIRS_OS_INDEPENDENT) \
	Configuration \
	Common \
	CommonGtkWindows \
	CollectionEvents \
	UserService \
	StoreProvider \
	FsProvider \
	SqliteProvider \
	Identity \
	WorkGroupIdentity \
	CollectionStore \
	FsWatcher \
	Agent \
	SyncCommon \
	StoreWatcher \
	CollectionWatcher \
	SyncManager \
	Sync \
	SyncLogicLite \
	SyncTools \
	AddressBook \
	Smtp \
	InviteAgent \

#=============================================================================
# windows component list
#=============================================================================
SUBDIRS_OS_WINDOWS = \
	$(SUBDIRS_OS_INDEPENDENT) \
	Configuration \
	Common \
	CommonFormWindows \
	CollectionEvents \
	UserService \
	StoreProvider \
	FsProvider \
	SqliteProvider \
	Identity \
	WorkGroupIdentity \
	CollectionStore \
	FsWatcher \
    Agent \
	SyncCommon \
	StoreWatcher \
	CollectionWatcher \
	SyncManager \
	Sync \
	SyncLogicLite \
	SyncTools \
	AddressBook \
	Smtp \
	InviteAgent \
	    
#=============================================================================
# netware component list
#=============================================================================
SUBDIRS_OS_NETWARE = \
	$(SUBDIRS_OS_INDEPENDENT) \

#=============================================================================
# darwin component list
#=============================================================================
SUBDIRS_OS_DARWIN = \
	$(SUBDIRS_OS_INDEPENDENT) \

#=============================================================================
# stage files
#=============================================================================
STAGE_FILES = \
	$(TOOLDIR)/NUnit/bin/nunit-gui.exe \
	$(TOOLDIR)/NUnit/bin/nunit-console.exe \
	$(TOOLDIR)/NUnit/bin/nunit.framework.dll \
	$(TOOLDIR)/NUnit/bin/nunit.extensions.dll \
	$(TOOLDIR)/NUnit/bin/nunit.tests.dll \
	$(TOOLDIR)/NUnit/bin/nunit.uikit.dll \
	$(TOOLDIR)/NUnit/bin/nunit.util.dll \

STAGE_FILES := $(subst /,$(SEP),$(STAGE_FILES))

#=============================================================================
# target component list
#=============================================================================
SUBDIRS =

ifeq "linux" "$(TARGET_OS)"
	SUBDIRS := $(SUBDIRS_OS_LINUX)
endif

ifeq "windows" "$(TARGET_OS)"
	SUBDIRS := $(SUBDIRS_OS_WINDOWS)
endif

ifeq "netware" "$(TARGET_OS)"
	SUBDIRS := $(SUBDIRS_OS_NETWARE)
endif

ifeq "darwin" "$(TARGET_OS)"
	SUBDIRS := $(SUBDIRS_OS_DARWIN)
endif

#=============================================================================
# all
#=============================================================================
.PHONY: all $(SUBDIRS)

all: $(SUBDIRS)

$(SUBDIRS):
	$(call HEADER,$@)
	@$(MAKE) -C $@ all

#=============================================================================
# stage
#=============================================================================
#.PHONY: stage $(STAGE_DIR) $(STAGE_FILES)

#stage: stage-clean stage-prep $(STAGE_DIR) $(STAGE_FILES)
	
#stage-prep:
#	$(call HEADER,stage)

#$(STAGE_DIR):
#	$(MKDIR) $(STAGE_DIR)

#$(STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# stage-clean
#=============================================================================
#.PHONY: stage-clean

#stage-clean:
#	$(call HEADER,$@)
#	-$(RMDIR) "$(STAGE_DIR)"

#=============================================================================
# clean
#=============================================================================
CLEAN_SUBDIRS = $(addprefix clean., $(SUBDIRS))

.PHONY: clean $(CLEAN_SUBDIRS)

clean: $(CLEAN_SUBDIRS)

$(CLEAN_SUBDIRS):
	$(call HEADER,$@)
	@$(MAKE) -i -k -C $(subst clean.,,$@) clean

#=============================================================================
# distclean - removes all files built by 'configure' or 'make'
#
# NOTE: After 'make distclean' the 'configure' command must be run again
#       before using this (or any other) Makefile.
#=============================================================================
.PHONY: distclean

distclean: clean package-clean api-doc-clean doc-clean mkfiles-clean
	$(call RM_IF_EXISTS,$(PRODUCT_NAME)-$(PRODUCT_VERSION).$(ZIP_EXT))

#=============================================================================
# test
#=============================================================================
TEST_SUBDIRS = $(addprefix test., $(SUBDIRS))

.PHONY: test $(TEST_SUBDIRS)

test: $(TEST_SUBDIRS) report

$(TEST_SUBDIRS):
	$(call HEADER,$@)
	@$(MAKE) -C $(subst test.,,$@) test
#	@$(MAKE) -i -k -C $(DESTDIR)$(bindir) -f $(SRCDIR)$(SEP)$(subst test.,,$@)$(SEP)Makefile test
#	@$(MAKE) -i -k -C $(STAGE_DIR) -f $(SRCDIR)$(SEP)$(subst test.,,$@)$(SEP)Makefile test
	
#=============================================================================
# check
#=============================================================================
CHECK_SUBDIRS = $(addprefix check., $(SUBDIRS))

.PHONY: check $(CHECK_SUBDIRS)

check: all $(CHECK_SUBDIRS)

$(CHECK_SUBDIRS):
	$(call HEADER,$@)
	$(if $(wildcard $(subst check.,,$@)/*.csproj), \
		$(FXCOP) $(FXCOP_FLAGS) /file:$(subst check.,,$@) /out:$(subst check.,,$@)$(SEP)Check.xml, \
		@echo No .Net project found. \
		)

#=============================================================================
# dist
#=============================================================================
.PHONY: dist

dist: clean
	$(call HEADER,$@)
	-$(RMDIR) ..$(SEP)$(SOURCE_DIST)
	$(MKDIR) ..$(SEP)$(SOURCE_DIST)
	$(CP_R) * ..$(SEP)$(SOURCE_DIST)
	$(MV) ..$(SEP)$(SOURCE_DIST) $(SOURCE_DIST)
	-$(RM) $(SOURCE_DIST).$(ZIP_EXT)
	$(ZIP_CREATE) $(SOURCE_DIST).$(ZIP_EXT)  $(SOURCE_DIST)$(SEP)\*
	-$(RMDIR) $(SOURCE_DIST)

#=============================================================================
# package - build install package for current platform (RPM, MSI, etc.)
#=============================================================================
.PHONY: package package-clean package-test

package: all package-nodeps

package-nodeps:
	$(call HEADER,$@)
	@$(MAKE) -C package all

package-install: package-nodeps
	$(call HEADER,$@)
	@$(MAKE) -C package install

package-uninstall:  package-nodeps
	$(call HEADER,$@)
	@$(MAKE) -C package uninstall

package-clean:
	$(call HEADER,$@)
	@$(MAKE) -C package clean

package-test:
	$(call HEADER,$@)
	@$(MAKE) -C package test

#=============================================================================
# install/uninstall
#=============================================================================
INSTALL_SUBDIRS = $(addprefix install., $(SUBDIRS))
.PHONY: install $(INSTALL_SUBDIRS)

install: $(INSTALL_SUBDIRS)

$(INSTALL_SUBDIRS):
	$(call HEADER,$@)
	@$(MAKE) -i -k -C $(subst install.,,$@) install

UNINSTALL_SUBDIRS = $(addprefix uninstall., $(SUBDIRS))
.PHONY: uninstall $(UNINSTALL_SUBDIRS)

uninstall: $(UNINSTALL_SUBDIRS)

$(UNINSTALL_SUBDIRS):
	$(call HEADER,$@)
	@$(MAKE) -i -k -C $(subst uninstall.,,$@) uninstall


INSTALLTEST_SUBDIRS = $(addprefix installtest., $(SUBDIRS))
.PHONY: installtest $(INSTALLTEST_SUBDIRS)

installtest: $(INSTALLTEST_SUBDIRS)

$(INSTALLTEST_SUBDIRS):
	$(call HEADER,$@)
	@$(MAKE) -i -k -C $(subst installtest.,,$@) installtest

UNINSTALLTEST_SUBDIRS = $(addprefix uninstalltest., $(SUBDIRS))
.PHONY: uninstalltest $(UNINSTALLTEST_SUBDIRS)

uninstalltest: $(UNINSTALLTEST_SUBDIRS)

$(UNINSTALLTEST_SUBDIRS):
	$(call HEADER,$@)
	@$(MAKE) -i -k -C $(subst uninstalltest.,,$@) uninstalltest


#=============================================================================
# doc
#=============================================================================
.PHONY: doc doc-nodeps

doc: all doc-nodeps

doc-nodeps:
	$(call HEADER,doc)
	$(NDOC) -recurse=$(STAGE_DIR) -OutputDirectory=$(DOCDIR)

#=============================================================================
# doc-clean
#=============================================================================
.PHONY: doc-clean

doc-clean:
	$(call HEADER,$@)
	-$(RMDIR) $(DOCDIR)

#=============================================================================
# api-doc
#=============================================================================
.PHONY: api-doc api-doc-nodeps

api-doc: all api-doc-nodeps

api-doc-nodeps:
	$(call HEADER,api-doc)
	$(NDOC) -project=$(SRCDIR)$(SEP)api-doc.ndoc

#=============================================================================
# api-doc-clean
#=============================================================================
.PHONY: api-doc-clean

api-doc-clean:
	$(call HEADER,$@)
	-$(RMDIR) $(APIDOCDIR)

#=============================================================================
# report
#=============================================================================
.PHONY: report

report:
	$(call HEADER,$@)
	$(REPORT) $(STAGE_DIR)


#=============================================================================
# File CVS History:
#
# $Log$
# Revision 1.5.2.3  2004/02/25 01:11:41  cgaisford
# updated all of the makefiles to run test cases
#
# Revision 1.5.2.2  2004/02/24 23:00:23  cgaisford
# updated common.mk and the Makefile so they both handle installtest and uninstalltest
#
# Revision 1.5.2.1  2004/02/24 22:11:56  cgaisford
# updated the makefile to pass install on down to subprojects
#
# Revision 1.5  2004/02/23 23:12:49  pthomas707
# Added package-install and package-uninstall.
#
# Revision 1.4  2004/02/23 21:46:02  ryoung65
# Added FsWatcher to the build
#
# Revision 1.3  2004/02/23 18:55:47  pthomas707
# Renamed install directory to package
#
# Revision 1.2  2004/02/21 23:59:59  cgaisford
# Modified the build process and structure.  Now configure can be run at the root level and then make can be run.  Cleaned up configure stuff that used to be down in src
#
# Revision 1.1.1.1  2004/02/21 07:02:13  cgaisford
# This is the first checkin of Simias
#
#
#=============================================================================
