#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	MiniFolder.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# MiniFolder variables
#=============================================================================
MiniFolder_SOURCES = Application/AssemblyInfo.cs Application/MiniFolder.cs
MiniFolder_INCLUDES = 
MiniFolder_FLAGS = /t:exe $(ICON_FLAG)Application/App$(ICON_EXT)
MiniFolder_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll CollectionStore.dll StoreProvider.dll Identity.dll SyncManager.dll CollectionWatcher.dll SyncCommon.dll SyncLogicLite.dll Sync.dll
MiniFolder_LIBPATH = Application $(SIMIAS_ROOT)/src/stage ../stage
MiniFolder_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = MiniFolder.exe 
CLEAN_FILES = Check.xml MiniFolder.pdb MiniFolder.Doc.xml  MiniFolder.xml MiniFolder.Doc.xml
CLEAN_DIRS = Application/bin Application/obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) MiniFolder.Doc.xml MiniFolder.pdb 
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
all: $(TARGETS) $(STAGE_FILES) $(EXTRA_STAGE_FILES)

# clean
clean: $(CLEAN_TARGETS)
	-$(RMDIR) $(CLEAN_DIRS)

$(CLEAN_TARGETS): 
	$(call RM_IF_EXISTS,$(subst clean.,,$@))

# test
test:
	@echo No NUnit test assemblies found.

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# MiniFolder targets
#=============================================================================

# fix path seperator
MiniFolder_SOURCES := $(subst /,$(SEP), $(MiniFolder_SOURCES))
MiniFolder_INCLUDES := $(subst /,$(SEP), $(MiniFolder_INCLUDES))
MiniFolder_DEF := $(subst /,$(SEP), $(MiniFolder_DEF))
MiniFolder_LIBPATH := $(subst /,$(SEP), $(MiniFolder_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(MiniFolder_LIBPATH) $(MiniFolder_INCLUDES)

MiniFolder.exe: $(MiniFolder_SOURCES) $(MiniFolder_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:MiniFolder.Doc.xml $(CSCFLAGS) $(MiniFolder_FLAGS) $(MiniFolder_LIBS:%=/r:%) $(MiniFolder_LIBPATH:%=/lib:%) $(MiniFolder_SOURCES)

