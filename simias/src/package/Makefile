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
include ../../config.mk

#=============================================================================
# Common targets
#=============================================================================
#TARGETS           = server client
TARGETS           = client-msm client
CLEAN_TARGETS     = $(addsuffix -clean,     $(TARGETS))
TEST_TARGETS      = $(addsuffix -test,      $(TARGETS))
INSTALL_TARGETS   = $(addsuffix -install,   $(TARGETS))
UNINSTALL_TARGETS = $(addsuffix -uninstall, $(TARGETS))

ALL_TARGETS = $(TARGETS) $(CLEAN_TARGETS) $(TEST_TARGETS) $(INSTALL_TARGETS) $(UNINSTALL_TARGETS)

.PHONY: all       $(TARGETS)
.PHONY: clean     $(CLEAN_TARGETS)
.PHONY: test      $(TEST_TARGETS)
.PHONY: install   $(INSTALL_TARGETS)
.PHONY: uninstall $(UNINSTALL_TARGETS)

all:       $(TARGETS)
clean:     $(CLEAN_TARGETS)
test:      $(TEST_TARGETS)
install:   $(INSTALL_TARGETS)
uninstall: $(UNINSTALL_TARGETS)

define header
@echo '' 
@echo --------------------------------------------------------------------------------
@echo  $@ for $(TARGET_OS)
@echo --------------------------------------------------------------------------------
endef

#=============================================================================
# $(BUILD_OS) must match $(TARGET_OS) for now
#=============================================================================
ifneq ($(BUILD_OS),$(TARGET_OS))

$(ALL_TARGETS):
	$(header)
	@echo  Cannot build $(TARGET_OS) from $(BUILD_OS)
	@exit 1

#=============================================================================
# Linux
#=============================================================================
else
ifeq ($(TARGET_OS),linux)

$(ALL_TARGETS):
	@$(MAKE) -C linux $@

#=============================================================================
# Windows
#=============================================================================
else	
ifeq ($(TARGET_OS),windows)

$(ALL_TARGETS):
	@$(MAKE) -C windows $@

#=============================================================================
# unsupported $(TARGET_OS)
#=============================================================================
else

$(ALL_TARGETS):
	$(header)
	@echo  $(TARGET_OS) not yet supported 
	@exit 1

endif # ifeq ($(TARGET_OS),windows)

endif # ifeq ($(TARGET_OS),linux)

endif # ifneq ($(BUILD_OS),$(TARGET_OS))

#=============================================================================
# File CVS History:
#
# $Log$
# Revision 1.2  2004/02/24 00:19:57  pthomas707
# Added client-msm targe.
#
# Revision 1.1  2004/02/23 20:07:55  pthomas707
# Package makefile
#
#
#=============================================================================
	

