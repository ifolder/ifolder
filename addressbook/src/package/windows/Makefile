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

TARGETS        := client-msm client 
#TARGETS        := client client-msm server
TARGET_TYPES   := clean test install uninstall

CLIENT_TARGETS     := $(foreach type,$(TARGET_TYPES),$(addsuffix -$(type), client))
CLIENT_MSM_TARGETS := $(foreach type,$(TARGET_TYPES),$(addsuffix -$(type), client-msm))
#SERVER_TARGETS    := $(foreach type,$(TARGET_TYPES),$(addsuffix -$(type), server))

ALL_TARGETS    := all $(TARGETS) $(TARGET_TYPES) $(CLIENT_TARGETS) $(CLIENT_MSM_TARGETS)

CLEAN_TARGETS     := $(filter %-clean,    $(ALL_TARGETS))
TEST_TARGETS      := $(filter %-test,     $(ALL_TARGETS))
INSTALL_TARGETS   := $(filter %-install,  $(ALL_TARGETS))
UNINSTALL_TARGETS := $(filter %-uninstall,$(ALL_TARGETS))

.PHONY: $(ALL_TARGETS)

all:       $(TARGETS)
clean:     $(CLEAN_TARGETS)
test:      $(TEST_TARGETS)
install:   $(INSTALL_TARGETS)
uninstall: $(UNINSTALL_TARGETS)

# Variables used by windows.mk
export PROJECT
client     $(CLIENT_TARGETS):     PROJECT=client
server     $(SERVER_TARGETS):     PROJECT=server
client-msm $(CLIENT_MSM_TARGETS): PROJECT=client-msm

export SLN_FILE=installs.sln

client: client-msm
	$(MAKE) -f msi.mk

client-msm:
	$(MAKE) -f msm.mk

server:
	$(MAKE) -f msi.mk

$(CLIENT_TARGETS):
	$(MAKE) -f msi.mk $(subst client-,,$@)

$(SERVER_TARGETS):
	$(MAKE) -f msi.mk $(subst server-,,$@)

$(CLIENT_MSM_TARGETS):
	$(MAKE) -f msm.mk $(subst client-msm-,,$@)

dump:
	@echo TARGETS            = $(TARGETS)
	@echo TARGET_TYPES       = $(TARGET_TYPES)
	@echo CLIENT_TARGETS     = $(CLIENT_TARGETS)
	@echo CLIENT_MSM_TARGETS = $(CLIENT_MSM_TARGETS)
	@echo SERVER_TARGETS     = $(SERVER_TARGETS)
	@echo ALL_TARGETS        = $(ALL_TARGETS)
	@echo CLEAN_TARGETS      = $(CLEAN_TARGETS)
	@echo TEST_TARGETS       = $(TEST_TARGETS)
	@echo INSTALL_TARGETS    = $(INSTALL_TARGETS)
	@echo UNINSTALL_TARGETS  = $(UNINSTALL_TARGETS)

#=============================================================================
# File CVS History:
#
# $Log$
# Revision 1.1  2004/02/24 00:17:12  pthomas707
# Morphed Simias MSI/MSM build to Address Book MSI/MSM build.
#
#
#=============================================================================

