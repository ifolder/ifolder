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
	GtkAddrBook \

#=============================================================================
# windows component list
#=============================================================================
SUBDIRS_OS_WINDOWS = \
	$(SUBDIRS_OS_INDEPENDENT) \
	ABGui \
	FormsBookLib \
	FormsAddrBook \
	    
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
# mkfiles
#=============================================================================
MKFILES = $(patsubst %,%/Makefile, $(SLNDIRS))

.PHONY: mkfiles -mkfiles

mkfiles: -mkfiles $(MKFILES)

-mkfiles:
	$(call HEADER,mkfiles)

$(MKFILES): FORCE
	-$(SLN2MK) $(wildcard $(dir $@)*.sln) $(SLN_CONFIG)

FORCE:

#=============================================================================
# mkfiles-clean
#=============================================================================
MKFILES_CLEAN = $(addprefix clean., $(MKFILES))

.PHONY: mkfiles-clean -mkfiles-clean $(MKFILES_CLEAN)

mkfiles-clean: -mkfiles-clean $(MKFILES_CLEAN)

-mkfiles-clean:
	$(call HEADER,mkfiles-clean)

$(MKFILES_CLEAN):
		$(call RM_IF_EXISTS,$(subst /,$(SEP),$(subst clean.,,$@)))

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
# Revision 1.1  2004/02/21 19:01:16  cgaisford
# Initial revision
#
# Revision 1.1.1.1  2004/02/21 18:18:17  cgaisford
# Inital checkin
#
# Revision 1.91  2004/02/20 18:11:52  rlyon
# Added and replaced headers.
#
# Revision 1.90  2004/02/20 17:28:15  pthomas
# fixed up dist target
#
# Revision 1.89  2004/02/19 19:45:35  ryoung
# added UserService and FsWatcher to the linux build.
#
# Revision 1.88  2004/02/19 19:15:12  ryoung
# Added UserService to the Build
#
# Revision 1.87  2004/02/19 17:59:45  pthomas
# added dist target to build source tarball
#
# Revision 1.86  2004/02/18 21:25:58  rlyon
# Minor bug fixes to the make system.
#
# Revision 1.85  2004/02/13 19:34:06  cgaisford
# updated
#
# Revision 1.84  2004/02/13 17:59:40  pthomas
# Added some -nodeps targets
#
# Revision 1.83  2004/02/13 17:47:52  ryoung
# Added FsWatcher to the windows build
#
# Revision 1.82  2004/02/12 23:51:25  pthomas
# Added distclean target
#
# Revision 1.81  2004/02/12 21:58:54  pthomas
# added stage-clean to clean target
#
# Revision 1.80  2004/02/12 20:39:42  ryoung
# Added a line to copy the StoreBrowser to the Stage directory.
#
# Revision 1.79  2004/02/10 18:26:38  rlyon
# 1. Added an acceptance tests project with a sync test.
#
# Revision 1.78  2004/02/10 18:06:35  rlyon
# Removed SyncLogicOpt
#
# Revision 1.77  2004/02/08 01:09:50  rlyon
# Moved the makefile generation to the configuration step.
#
# Revision 1.76  2004/02/07 00:13:51  rlyon
# Skipping the error checking on sln2mk per Dale's request.
#
# Revision 1.75  2004/02/06 16:12:49  cgaisford
# updated the Makefile to include Smtp
#
# Revision 1.74  2004/02/03 23:49:13  pthomas
# added api-doc targets
#
# Revision 1.73  2004/02/03 20:38:11  ryoung
# Added CollectionEvents to Linux build.
#
# Revision 1.72  2004/02/03 20:36:00  rlyon
# Fixed some handling of the default path of null.
#
# Revision 1.71  2004/02/03 18:12:21  rlyon
# New model for Sync, Invitation, and iFolder.
#
# Revision 1.70  2004/02/02 23:07:51  cgaisford
# updated the main makefile to include the Configuration dude
#
# Revision 1.69  2004/02/02 15:27:26  rlyon
# A ton of fun with sync.
#
# Revision 1.68  2004/01/30 21:07:03  ryoung
# Added the CollectionEvents system to the windows build.
#
# Revision 1.67  2004/01/29 18:52:29  bgetter
# Added FormsAddrBook to project lists.
#
# Revision 1.66  2004/01/29 04:11:42  rlyon
# Added Common, CommonFormWindows, and CommonGtkWindows.
#
# Revision 1.65  2004/01/29 00:02:42  cgaisford
# added the configuration component and tests to build on Linux only for now
#
# Revision 1.64  2004/01/23 10:14:04  bgetter
# Added FormsBookLib.
#
# Revision 1.63  2004/01/22 16:04:07  bgetter
# Added FormsInvitationWizard to solution and build lists.
#
# Revision 1.62  2004/01/22 03:47:50  cgaisford
# oops. this make file needs work, why is stage getting cleaned up with a normal make?
#
# Revision 1.61  2004/01/22 03:46:43  cgaisford
# fixed a null value in the details display
#
# Revision 1.60  2004/01/20 21:31:54  ryoung
# Added the SqliteProvider to the build.
#
# Revision 1.59  2004/01/17 01:32:37  ryoung
# Added the SqliteProvider to the windows build.
#
# Revision 1.58  2004/01/16 23:34:32  rlyon
# A few changes. :-)
# Added CommonSchemes.
#
# Revision 1.57  2004/01/16 23:27:25  dolds
# change sync directory to Sync, update sync code
#
# Revision 1.56  2004/01/09 17:45:11  bgetter
# Renamed TrayAppForms project to FormsTrayApp.
#
# Revision 1.55  2004/01/08 18:14:15  cgaisford
# updated Makefile to build GtkAddBook and not Book
#
# Revision 1.54  2004/01/08 18:11:35  cgaisford
# moved TrayAppGtk to GtkTrayApp
#
# Revision 1.53  2003/12/19 03:56:00  cgaisford
# re-added stuff
#
# Revision 1.52  2003/12/19 00:25:06  rlyon
# Moved ABGui for Linux.
#
# Revision 1.51  2003/12/18 22:15:26  bgetter
# Added TrayAppForms project to the makefile.
#
# Revision 1.50  2003/12/18 20:02:40  dolds
# hook up to use iFolder.updateiFolder and GetSingleNodeByLeafName
# change sync status update delegate to take a boolean
#
# Revision 1.49  2003/12/18 19:09:52  rlyon
# Working on the Invite tests.
#
# Revision 1.48  2003/12/18 17:44:21  rlyon
# Split up the Agent because of dependencies.
#
# Revision 1.47  2003/12/18 08:11:53  cgaisford
# updated Book's makefile and the main makefile to make book
#
# Revision 1.46  2003/12/18 00:29:16  rlyon
# Re-work agent.
#
# Revision 1.45  2003/12/17 00:16:25  rlyon
# Removed Suse fix, Makefile bug, added mkfile-clean to dist-clean.
#
# Revision 1.44  2003/12/17 00:01:55  rlyon
# A test for Suse
#
# Revision 1.43  2003/12/16 23:38:16  rlyon
# More Agent Work and removed ABCmd from the build.
#
# Revision 1.42  2003/12/16 20:42:20  rlyon
# Added Agent to the Linux build.
#
# Revision 1.41  2003/12/16 00:55:33  rlyon
# Agent Work.
#
# Revision 1.40  2003/12/16 00:02:53  cgaisford
# Added the TrayAppGtk project and updated the Makefile to build it on Linux.  Also changed the Book makefile to only copy and delete the Book glade files
#
# Revision 1.39  2003/12/15 23:05:15  cgaisford
# I removed the nautilus-plugin project for this iteration
#
# Revision 1.38  2003/12/15 21:12:34  rlyon
# Rework of stage for easier compiling on Linux.
#
# Revision 1.37  2003/12/15 16:20:12  dolds
# First checkin of sync assembly
#
# Revision 1.36  2003/12/05 00:06:28  mlasky
# Added assemblies Identity and WorkGroupIdentity
#
# Revision 1.35  2003/11/25 18:31:11  ryoung
# Added StoreProvider and FsProvider to the build.
#
# Revision 1.34  2003/11/21 16:43:28  cgaisford
# updated the makefile and configure.pl to handle the darwin (OSX) platform
# I also had to move the os independent projects to os dependent because they
# do not build on OS X.
#
# Revision 1.33  2003/11/20 18:08:37  rlyon
# A few minor modifications to sln2mk.exe.
#
# Revision 1.32  2003/11/18 21:17:31  bgetter
# Added iFolder to the list for a solution-generated makefile.
#
# Revision 1.31  2003/11/13 00:01:37  bgetter
# Added iFolderShell and added extra parameter passed into sln2mk.exe (debug flag).
#
# Revision 1.30  2003/11/06 07:36:39  rlyon
# Created the Denali admin project.
#
# Revision 1.29  2003/11/05 18:46:20  cgaisford
# moved AddressBookUI to Book
#
# Revision 1.28  2003/11/04 17:03:51  rlyon
# Moved iFolderAdmin.
#
# Revision 1.27  2003/11/04 00:22:25  bgetter
# Removed ShellExtension project ... the new shell extension will be added soon.
#
# Revision 1.26  2003/11/03 21:26:52  rlyon
# Remove iFolderAdmin from Linux build until the process command has a work-around.
#
# Revision 1.25  2003/10/31 04:22:28  rlyon
# Some remoting bug fixes for Mono.
# Fixed the Makefile stage for Windows.
# Minor other bug changes.
#
# Revision 1.24  2003/10/30 22:16:06  rlyon
# Added the NUnit assemblies to stage.
# Removed the ChangeLog component.
#
# Revision 1.23  2003/10/30 07:17:20  rlyon
# Created iFolderAdmin.
#
# Revision 1.22  2003/10/28 00:16:01  rlyon
# Fixed a bug to copy documentation files to the stage area.
#
# Revision 1.21  2003/10/24 18:46:25  rlyon
# Bug fix in echo.
#
# Revision 1.20  2003/10/24 18:25:47  rlyon
# Cosmetic change to support blank lines on Windows and Linux.
#
# Revision 1.19  2003/10/19 03:19:31  rlyon
# More cosmetic changes.
# Added some robustness to Report.
#
# Revision 1.18  2003/10/19 02:18:28  rlyon
# More cosmetic changes to the top Makefile.
#
# Revision 1.17  2003/10/18 02:18:27  rlyon
# 1. Updated ChangeLog to create a collection in a sub-directory.
# 2. Fixed the Report application to handle failed cases with no time.
# 3. Added makefiles for Report and sln2mk.
# 4. Cleaned-up the output of the main makefile.
#
# Revision 1.16  2003/10/15 21:12:22  bgetter
# Moved iFolder to SUBDIRS_OS_INDEPENDENT since the ShellExtension project is now dependent upon it.
#
# Revision 1.15  2003/10/15 04:58:53  rlyon
# 1.  Added product information per Paul's proposal.
# 2.  Cleaned up the target headers and consolidated.
# 3.  Removed the compiler checking and the --skip-checks options.
# 4.  Cleaned up the Makefile targets.
#
# Revision 1.14  2003/10/10 00:10:01  pthomas
# added dist as install dependency
#
# Revision 1.13  2003/10/09 23:12:57  rlyon
# Added the report tool.
#
# Revision 1.12  2003/10/09 22:26:28  pthomas
# Added install and uninstall targets. Also removed ExampleApp package.
#
# Revision 1.11  2003/10/09 21:35:22  bgetter
# Added ShellExtension to Makefile.
#
# Revision 1.10  2003/10/09 20:37:14  rlyon
# Removed TrayApp and added iFolder
#
# Revision 1.9  2003/10/09 20:06:05  rlyon
# Removed Example Applications
#
# Revision 1.8  2003/10/09 19:52:24  rlyon
# Fixed a path correction problem in sln2mk for AddressBook.
#
# Revision 1.7  2003/10/09 15:15:54  cgaisford
# added TrayApp and Nautilus-Plugin to build
#
# Revision 1.6  2003/10/06 22:17:03  rlyon
# Added a work-around for a mcs compiling issue when resolving /r: assemblies
#
# Revision 1.5  2003/10/06 21:02:05  rlyon
# Created a unit test case stagging area.
#
# Revision 1.4  2003/10/03 22:01:04  rlyon
# Finished .vcproj conversion project.
# Added new mailing lists to website.
#
# Revision 1.3  2003/10/02 19:46:48  ryoung
# Changed project settings to include FlaimProvider FlaimProviderTests and FlaimWrapper.
#
# Revision 1.2  2003/10/02 17:50:03  rlyon
# Added component lists for target os specific components.
#
# Revision 1.1  2003/09/30 02:59:51  rlyon
# ..in with the new.
#
# Revision 1.22  2003/09/26 18:14:38  banderso
# Added the address book to the makefile
#
# Revision 1.21  2003/09/24 20:35:50  rlyon
# Updated the Example Project to use the Novell.iFolder namespace.
# Added NDoc to the make system.
#
# Revision 1.20  2003/09/24 17:06:33  rlyon
# Fixed some dependencies.
#
# Revision 1.19  2003/09/18 18:00:40  rlyon
# Renamed test and check xml files to Test.xml and Check.xml respectively.
#
# Revision 1.18  2003/09/18 17:53:17  rlyon
# Upgrade to NUnit v2.1
#
# Revision 1.17  2003/09/17 23:01:20  rlyon
# Modifications for FxCop
#
# Revision 1.16  2003/09/17 17:57:28  mlasky
# Added CollectionStore
#
# Revision 1.15  2003/09/16 23:08:56  rlyon
# Bug fixes for Mike
#
# Revision 1.14  2003/09/16 22:40:49  pthomas
# Added install-related targets and build rules.
#
# Revision 1.13  2003/09/16 21:02:41  rlyon
# ExampleApp Win32 bug fixes
#
# Revision 1.12  2003/09/16 19:16:52  rlyon
# Fixed the Makefile generation dependencies.
#
# Revision 1.11  2003/09/15 20:43:28  rlyon
# Updates to the build and configure steps for C#
#
# Revision 1.10  2003/09/12 17:14:25  rlyon
# *** empty log message ***
#
#
#=============================================================================
