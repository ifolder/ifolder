#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	CollectionWatcher.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# CollectionWatcher variables
#=============================================================================
CollectionWatcher_SOURCES = Library/AssemblyInfo.cs Library/CollectionWatcher.cs
CollectionWatcher_INCLUDES = 
CollectionWatcher_FLAGS = /t:library
CollectionWatcher_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll Identity.dll CollectionEvents.dll CollectionStore.dll Common.dll Agent.dll SyncCommon.dll
CollectionWatcher_LIBPATH = Library ../CollectionStore ../StoreProvider ../Identity ../Common ../SyncCommon ../Configuration ../CollectionEvents ../Agent
CollectionWatcher_LDFLAGS = 

#=============================================================================
# CollectionWatcherTests variables
#=============================================================================
CollectionWatcherTests_SOURCES = Tests/AssemblyInfo.cs Tests/CollectionWatcherTests.cs
CollectionWatcherTests_INCLUDES = 
CollectionWatcherTests_FLAGS = /t:library
CollectionWatcherTests_LIBS = $(CollectionWatcher_LIBS) $(SYSTEM_XML) nunit.framework.dll CollectionWatcher.dll
CollectionWatcherTests_LIBPATH = $(CollectionWatcher_LIBPATH) Tests ../../tools/NUnit/bin
CollectionWatcherTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES =
LIB_FILES = CollectionWatcher.dll
DATA_FILES = 
TEST_BIN_FILES =
TEST_LIB_FILES = CollectionWatcherTests.dll
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = CollectionWatcher.dll CollectionWatcherTests.dll 
CLEAN_FILES = Check.xml CollectionWatcher.pdb CollectionWatcher.Doc.xml Check.xml CollectionWatcherTests.pdb CollectionWatcherTests.Doc.xml  CollectionWatcher.xml CollectionWatcher.Doc.xml
CLEAN_DIRS = Library/bin Library/obj Tests/bin Tests/obj 
NUNIT_TESTS = CollectionWatcherTests.dll 
STAGE_FILES = $(TARGETS) CollectionWatcher.Doc.xml CollectionWatcherTests.Doc.xml CollectionWatcher.pdb CollectionWatcherTests.pdb 
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
#	$(NUNIT) $(NUNIT_FLAGS) /xml:CollectionWatcher.Test.xml $(NUNIT_TESTS)

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# CollectionWatcher targets
#=============================================================================

# fix path seperator
CollectionWatcher_SOURCES := $(subst /,$(SEP), $(CollectionWatcher_SOURCES))
CollectionWatcher_INCLUDES := $(subst /,$(SEP), $(CollectionWatcher_INCLUDES))
CollectionWatcher_DEF := $(subst /,$(SEP), $(CollectionWatcher_DEF))
CollectionWatcher_LIBPATH := $(subst /,$(SEP), $(CollectionWatcher_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CollectionWatcher_LIBPATH) $(CollectionWatcher_INCLUDES)

CollectionWatcher.dll: $(CollectionWatcher_SOURCES) $(CollectionWatcher_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:CollectionWatcher.Doc.xml $(CSCFLAGS) $(CollectionWatcher_FLAGS) $(CollectionWatcher_LIBS:%=/r:%) $(CollectionWatcher_LIBPATH:%=/lib:%) $(CollectionWatcher_SOURCES)

#=============================================================================
# CollectionWatcherTests targets
#=============================================================================

# fix path seperator
CollectionWatcherTests_SOURCES := $(subst /,$(SEP), $(CollectionWatcherTests_SOURCES))
CollectionWatcherTests_INCLUDES := $(subst /,$(SEP), $(CollectionWatcherTests_INCLUDES))
CollectionWatcherTests_DEF := $(subst /,$(SEP), $(CollectionWatcherTests_DEF))
CollectionWatcherTests_LIBPATH := $(subst /,$(SEP), $(CollectionWatcherTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CollectionWatcherTests_LIBPATH) $(CollectionWatcherTests_INCLUDES)

CollectionWatcherTests.dll: $(CollectionWatcherTests_SOURCES) $(CollectionWatcherTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:CollectionWatcherTests.Doc.xml $(CSCFLAGS) $(CollectionWatcherTests_FLAGS) $(CollectionWatcherTests_LIBS:%=/r:%) $(CollectionWatcherTests_LIBPATH:%=/lib:%) $(CollectionWatcherTests_SOURCES)

include ../../common.mk
