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
include config.mk

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

all: stage $(SUBDIRS)

$(SUBDIRS):
	$(call HEADER,$@)
	@$(MAKE) -C $@ all

#=============================================================================
# stage
#=============================================================================
.PHONY: stage $(STAGE_DIR) $(STAGE_FILES)

stage: stage-clean stage-prep $(STAGE_DIR) $(STAGE_FILES)

stage-prep:
	$(call HEADER,stage)

$(STAGE_DIR):
	$(MKDIR) $(STAGE_DIR)

$(STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# stage-clean
#=============================================================================
.PHONY: stage-clean

stage-clean:
	$(call HEADER,$@)
	-$(RMDIR) "$(STAGE_DIR)"

#=============================================================================
# clean
#=============================================================================
CLEAN_SUBDIRS = $(addprefix clean., $(SUBDIRS))

.PHONY: clean $(CLEAN_SUBDIRS)

clean: $(CLEAN_SUBDIRS) stage-clean
	$(call HEADER,$@)
	-$(RM) $(ALL_SOURCE_DISTS)

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
	$(call RM_IF_EXISTS,config.mk)

#=============================================================================
# test
#=============================================================================
TEST_SUBDIRS = $(addprefix test., $(SUBDIRS))

.PHONY: test $(TEST_SUBDIRS)

test: all $(TEST_SUBDIRS) report

$(TEST_SUBDIRS):
	$(call HEADER,$@)
	@$(MAKE) -i -k -C $(STAGE_DIR) -f $(SRCDIR)$(SEP)$(subst test.,,$@)$(SEP)Makefile test
	
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
	-$(RM) $(SOURCE_DIST)$(SEP)config.mk
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
	@$(MAKE) -C install all

package-clean:
	$(call HEADER,$@)
	@$(MAKE) -C install clean

package-test:
	$(call HEADER,$@)
	@$(MAKE) -C install test

#=============================================================================
# install/uninstall
#=============================================================================
.PHONY: install uninstall

install: dist
	$(call HEADER,$@)
	@$(MAKE) -C install install

uninstall:
	$(call HEADER,$@)
	@$(MAKE) -C install uninstall

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
# Revision 1.1  2004/02/21 07:02:13  cgaisford
# Initial revision
#
#
#=============================================================================
