#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	SyncTools.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# SyncPing variables
#=============================================================================
SyncPing_SOURCES = Ping/AssemblyInfo.cs Ping/SyncPinger.cs
SyncPing_INCLUDES = 
SyncPing_FLAGS = /t:exe $(ICON_FLAG)Ping/App$(ICON_EXT)
SyncPing_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll Configuration.dll StoreProvider.dll CollectionEvents.dll Identity.dll CollectionStore.dll Agent.dll SyncCommon.dll CollectionWatcher.dll StoreWatcher.dll SyncManager.dll
SyncPing_LIBPATH = Ping ../SyncCommon ../SyncManager ../Common ../CollectionEvents ../CollectionStore ../Configuration ../StoreProvider ../Identity ../Agent ../CollectionWatcher ../StoreWatcher
SyncPing_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES = SyncPing.exe
LIB_FILES =
DATA_FILES = 
TEST_BIN_FILES =
TEST_LIB_FILES =
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = SyncPing.exe 
CLEAN_FILES = Check.xml SyncPing.pdb SyncPing.Doc.xml  SyncTools.xml SyncTools.Doc.xml
CLEAN_DIRS = Ping/bin Ping/obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) SyncPing.Doc.xml SyncPing.pdb 
STAGE_FILES := $(addprefix $(STAGE_DIR)$(SEP), $(STAGE_FILES))

#=============================================================================
# custom makefile
#=============================================================================
ifneq "$(wildcard custom.mk)" ""
	include custom.mk
endif

#=============================================================================
# global targets
#=============================================================================

# fix path seperator
STAGE_FILES := $(subst /,$(SEP),$(STAGE_FILES))
EXTRA_STAGE_FILES := $(subst /,$(SEP),$(EXTRA_STAGE_FILES))
CLEAN_FILES := $(subst /,$(SEP),$(CLEAN_FILES))
CLEAN_DIRS := $(subst /,$(SEP),$(CLEAN_DIRS))

# build clean file list up
CLEAN_FILES := $(sort $(CLEAN_FILES) $(TARGETS))
CLEAN_TARGETS := $(addprefix clean., $(CLEAN_FILES))

.PHONY: $(CLEAN_TARGETS) $(EXTRA_STAGE_FILES)

# all
all: $(TARGETS)
#all: $(TARGETS) $(STAGE_FILES) $(EXTRA_STAGE_FILES)

# clean
clean: $(CLEAN_TARGETS)
	-$(RMDIR) $(CLEAN_DIRS)

$(CLEAN_TARGETS): 
	$(call RM_IF_EXISTS,$(subst clean.,,$@))

# test
#test:
#	@echo No NUnit test assemblies found.

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# SyncPing targets
#=============================================================================

# fix path seperator
SyncPing_SOURCES := $(subst /,$(SEP), $(SyncPing_SOURCES))
SyncPing_INCLUDES := $(subst /,$(SEP), $(SyncPing_INCLUDES))
SyncPing_DEF := $(subst /,$(SEP), $(SyncPing_DEF))
SyncPing_LIBPATH := $(subst /,$(SEP), $(SyncPing_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SyncPing_LIBPATH) $(SyncPing_INCLUDES)

SyncPing.exe: $(SyncPing_SOURCES) $(SyncPing_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SyncPing.Doc.xml $(CSCFLAGS) $(SyncPing_FLAGS) $(SyncPing_LIBS:%=/r:%) $(SyncPing_LIBPATH:%=/lib:%) $(SyncPing_SOURCES)

include ../../common.mk
