#========================================================================
#  $RCSfile$
# 
#  Copyright (C) 2004 Novell, Inc.
#
#  This library is free software; you can redistribute it and/or
#  modify it under the terms of the GNU General Public
#  License as published by the Free Software Foundation; either
#  version 2 of the License, or (at your option) any later version.
#
#  This library is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
#  Library General Public License for more details.
#
#  You should have received a copy of the GNU General Public
#  License along with this library; if not, write to the Free
#  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#
#  Author: Paul Thomas <pthomas@novell.com>
# 
#========================================================================

# configure makefile
include ../../../config.mk

#=============================================================================
# The 'common' target is a dependency of all other targets.  It is used to
# verify that this makefile is being invoked in a valid environment.
#=============================================================================
ALL_TARGETS = all clean test install uninstall

.PHONY: common $(ALL_TARGETS)

$(ALL_TARGETS): common

# Verify that we are building for Windows
ifneq ($(TARGET_OS),windows)
  ERROR_MESSAGE += "ERROR: This Makefile is for Windows only (BUILD_OS=$(BUILD_OS), TARGET_OS=$(TARGET_OS))\n"
endif

# Check and set required variables
ifndef PROJECT
  ERROR_MESSAGE += "ERROR: PROJECT must be set (ex: client, server)\n"
endif
ifndef PRODUCT_NAME
  ERROR_MESSAGE += "ERROR: PRODUCT_NAME must be set (ex: denali, ifolder)\n"
endif
ifndef PRODUCT_VERSION
  ERROR_MESSAGE += "ERROR: PRODUCT_VERSION must be set\n"
endif
ifndef PACKAGE_RELEASE
  ERROR_MESSAGE += "ERROR: PACKAGE_RELEASE must be set\n"
endif

# If any errors, output message and exit
ifdef ERROR_MESSAGE
  common:
	@echo -e $(ERROR_MESSAGE)
	@exit 1
endif

#=============================================================================
# Set and export variables
#=============================================================================
RUN_DEVENV = call run_devenv.bat
SLN_FILE = installs.sln

SIMPLE_MSI_FILENAME = $(PROJECT).msi
FULL_MSI_FILENAME   = $(PRODUCT_NAME)-$(PROJECT)-$(PRODUCT_VERSION)-$(PACKAGE_RELEASE).msi

ifdef DEBUG
  CONFIGURATION = Debug
else
  CONFIGURATION = Release
endif

RUN_MSIEXEC   = call run_msiexec.bat
MSI_INSTALL   = $(RUN_MSIEXEC) install
MSI_UNINSTALL = $(RUN_MSIEXEC) uninstall

define header
@echo '' 
@echo --------------------------------------------------------------------------------
@echo  $(PROJECT)$(subst -all,,-$@) for $(TARGET_OS)
@echo --------------------------------------------------------------------------------
endef

#=============================================================================
# Targets
#=============================================================================
all:
	$(header)
	$(RUN_DEVENV) $(SLN_FILE) $(PROJECT) $(CONFIGURATION)
	copy /y $(PROJECT)\\$(CONFIGURATION)\\$(SIMPLE_MSI_FILENAME) $(FULL_MSI_FILENAME)

# devenv creates both Release and Debug directories when building either one so remove both
clean:
	$(header)
	-$(RMDIR) $(PROJECT)\Release
	-$(RMDIR) $(PROJECT)\Debug
	-$(RMDIR) server\Release
	-$(RMDIR) server\Debug
	-$(RM) *-$(PROJECT).log
	-$(RM) *-$(PROJECT)-*.msi
	-$(RM) /ah installs.suo

test:
	$(header)
	@echo No package tests defined

install:
	$(header)
	@$(MSI_INSTALL) $(FULL_MSI_FILENAME)

uninstall:
	$(header)
	@$(MSI_UNINSTALL) $(FULL_MSI_FILENAME)

#=============================================================================
# File CVS History:
#
# $Log$
# Revision 1.1  2004/02/23 20:28:48  pthomas707
# Morphed Denali MSI build into Simias MSI build.
#
#
#=============================================================================

