#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	StoreWatcher.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# StoreWatcher variables
#=============================================================================
StoreWatcher_SOURCES = Library/AssemblyInfo.cs Library/StoreWatcher.cs
StoreWatcher_INCLUDES = 
StoreWatcher_FLAGS = /t:library
StoreWatcher_LIBS = System.dll System.Data.dll $(SYSTEM_XML) CollectionStore.dll StoreProvider.dll Identity.dll WorkGroupIdentity.dll SqliteProvider.dll SqliteWrapper.dll Common.dll
StoreWatcher_LIBPATH = Library Library/../../CollectionStore Library/../../StoreProvider Library/../../Identity Library/../../WorkGroupIdentity Library/../../SqliteProvider Library/../../SqliteProvider Library/../../Common
StoreWatcher_LDFLAGS = 

#=============================================================================
# StoreWatcherTests variables
#=============================================================================
StoreWatcherTests_SOURCES = Tests/AssemblyInfo.cs Tests/StoreWatcherTests.cs
StoreWatcherTests_INCLUDES = 
StoreWatcherTests_FLAGS = /t:library
StoreWatcherTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll CollectionStore.dll Identity.dll StoreProvider.dll Common.dll StoreWatcher.dll
StoreWatcherTests_LIBPATH = Tests Tests/../../../tools/NUnit/bin Tests/../../CollectionStore Tests/../../Identity Tests/../../StoreProvider Tests/../../Common Tests/../Library
StoreWatcherTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = StoreWatcher.dll StoreWatcherTests.dll 
CLEAN_FILES = Check.xml StoreWatcher.pdb StoreWatcher.Doc.xml Check.xml StoreWatcherTests.pdb StoreWatcherTests.Doc.xml  StoreWatcher.xml StoreWatcher.Doc.xml
CLEAN_DIRS = Library/bin Library/obj Tests/bin Tests/obj 
NUNIT_TESTS = StoreWatcherTests.dll 
STAGE_FILES = $(TARGETS) StoreWatcher.Doc.xml StoreWatcherTests.Doc.xml StoreWatcher.pdb StoreWatcherTests.pdb 
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
	$(NUNIT) $(NUNIT_FLAGS) /xml:StoreWatcher.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# StoreWatcher targets
#=============================================================================

# fix path seperator
StoreWatcher_SOURCES := $(subst /,$(SEP), $(StoreWatcher_SOURCES))
StoreWatcher_INCLUDES := $(subst /,$(SEP), $(StoreWatcher_INCLUDES))
StoreWatcher_DEF := $(subst /,$(SEP), $(StoreWatcher_DEF))
StoreWatcher_LIBPATH := $(subst /,$(SEP), $(StoreWatcher_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(StoreWatcher_LIBPATH) $(StoreWatcher_INCLUDES)

StoreWatcher.dll: $(StoreWatcher_SOURCES) $(StoreWatcher_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:StoreWatcher.Doc.xml $(CSCFLAGS) $(StoreWatcher_FLAGS) $(StoreWatcher_LIBS:%=/r:%) $(StoreWatcher_LIBPATH:%=/lib:%) $(StoreWatcher_SOURCES)

#=============================================================================
# StoreWatcherTests targets
#=============================================================================

# fix path seperator
StoreWatcherTests_SOURCES := $(subst /,$(SEP), $(StoreWatcherTests_SOURCES))
StoreWatcherTests_INCLUDES := $(subst /,$(SEP), $(StoreWatcherTests_INCLUDES))
StoreWatcherTests_DEF := $(subst /,$(SEP), $(StoreWatcherTests_DEF))
StoreWatcherTests_LIBPATH := $(subst /,$(SEP), $(StoreWatcherTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(StoreWatcherTests_LIBPATH) $(StoreWatcherTests_INCLUDES)

StoreWatcherTests.dll: $(StoreWatcherTests_SOURCES) $(StoreWatcherTests_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:StoreWatcherTests.Doc.xml $(CSCFLAGS) $(StoreWatcherTests_FLAGS) $(StoreWatcherTests_LIBS:%=/r:%) $(StoreWatcherTests_LIBPATH:%=/lib:%) $(StoreWatcherTests_SOURCES)

