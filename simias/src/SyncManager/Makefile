#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	SyncManager.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# SyncManager variables
#=============================================================================
SyncManager_SOURCES = Library/AssemblyInfo.cs Library/DebugServerChannelSink.cs Library/DebugServerChannelSinkProvider.cs Library/SyncChannel.cs Library/SyncChannelFactory.cs Library/SyncCollectionManager.cs Library/SyncCredentialCache.cs Library/SyncManager.cs Library/SyncPing.cs Library/SyncStoreManager.cs
SyncManager_INCLUDES = 
SyncManager_FLAGS = /t:library
SyncManager_LIBS = System.dll System.Data.dll $(SYSTEM_XML) CollectionStore.dll Identity.dll StoreProvider.dll Common.dll CollectionWatcher.dll System.Runtime.Remoting.dll StoreWatcher.dll SyncCommon.dll
SyncManager_LIBPATH = Library Library/../../CollectionStore Library/../../Identity Library/../../StoreProvider Library/../../Common Library/../../CollectionWatcher Library/../../StoreWatcher Library/../../stage
SyncManager_LDFLAGS = 

#=============================================================================
# SyncManagerTests variables
#=============================================================================
SyncManagerTests_SOURCES = Tests/AssemblyInfo.cs Tests/SyncManagerTests.cs
SyncManagerTests_INCLUDES = 
SyncManagerTests_FLAGS = /t:library
SyncManagerTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll SyncManager.dll Common.dll SyncCommon.dll
SyncManagerTests_LIBPATH = Tests Tests/../../../tools/NUnit/bin Tests/../../Common Tests/../../stage
SyncManagerTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = SyncManager.dll SyncManagerTests.dll 
CLEAN_FILES = Check.xml SyncManager.pdb SyncManager.Doc.xml Check.xml SyncManagerTests.pdb SyncManagerTests.Doc.xml  SyncManager.xml SyncManager.Doc.xml
CLEAN_DIRS = Library/bin Library/obj Tests/bin Tests/obj 
NUNIT_TESTS = SyncManagerTests.dll 
STAGE_FILES = $(TARGETS) SyncManager.Doc.xml SyncManagerTests.Doc.xml SyncManager.pdb SyncManagerTests.pdb 
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
	$(NUNIT) $(NUNIT_FLAGS) /xml:SyncManager.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# SyncManager targets
#=============================================================================

# fix path seperator
SyncManager_SOURCES := $(subst /,$(SEP), $(SyncManager_SOURCES))
SyncManager_INCLUDES := $(subst /,$(SEP), $(SyncManager_INCLUDES))
SyncManager_DEF := $(subst /,$(SEP), $(SyncManager_DEF))
SyncManager_LIBPATH := $(subst /,$(SEP), $(SyncManager_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SyncManager_LIBPATH) $(SyncManager_INCLUDES)

SyncManager.dll: $(SyncManager_SOURCES) $(SyncManager_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SyncManager.Doc.xml $(CSCFLAGS) $(SyncManager_FLAGS) $(SyncManager_LIBS:%=/r:%) $(SyncManager_LIBPATH:%=/lib:%) $(SyncManager_SOURCES)

#=============================================================================
# SyncManagerTests targets
#=============================================================================

# fix path seperator
SyncManagerTests_SOURCES := $(subst /,$(SEP), $(SyncManagerTests_SOURCES))
SyncManagerTests_INCLUDES := $(subst /,$(SEP), $(SyncManagerTests_INCLUDES))
SyncManagerTests_DEF := $(subst /,$(SEP), $(SyncManagerTests_DEF))
SyncManagerTests_LIBPATH := $(subst /,$(SEP), $(SyncManagerTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SyncManagerTests_LIBPATH) $(SyncManagerTests_INCLUDES)

SyncManagerTests.dll: $(SyncManagerTests_SOURCES) $(SyncManagerTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SyncManagerTests.Doc.xml $(CSCFLAGS) $(SyncManagerTests_FLAGS) $(SyncManagerTests_LIBS:%=/r:%) $(SyncManagerTests_LIBPATH:%=/lib:%) $(SyncManagerTests_SOURCES)

