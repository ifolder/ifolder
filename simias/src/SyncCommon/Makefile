#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	SyncCommon.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# SyncCommon variables
#=============================================================================
SyncCommon_SOURCES = Library/AssemblyInfo.cs Library/SyncChannelFormatters.cs Library/SyncCollection.cs Library/SyncCollectionInfo.cs Library/SyncCollectionRoles.cs Library/SyncCollectionService.cs Library/SyncCollectionWorker.cs Library/SyncLogicFactory.cs Library/SyncNode.cs Library/SyncNodeInfo.cs Library/SyncNodeStream.cs Library/SyncNodeStreamInfo.cs Library/SyncProperties.cs Library/SyncStore.cs Library/SyncStoreInfo.cs Library/SyncStoreService.cs
SyncCommon_INCLUDES = 
SyncCommon_FLAGS = /t:library
SyncCommon_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll Identity.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll Common.dll System.Runtime.Remoting.dll Agent.dll
SyncCommon_LIBPATH = Library ../Configuration ../Identity ../StoreProvider ../CollectionEvents ../CollectionStore ../Common ../Agent 
SyncCommon_LDFLAGS = 

#=============================================================================
# SyncCommonTests variables
#=============================================================================
SyncCommonTests_SOURCES = Tests/AssemblyInfo.cs Tests/SyncCommonTests.cs
SyncCommonTests_INCLUDES = 
SyncCommonTests_FLAGS = /t:library
SyncCommonTests_LIBS = $(SyncCommon_LIBS) $(SYSTEM_XML) SyncCommon.dll CollectionStore.dll Common.dll Identity.dll nunit.framework.dll StoreProvider.dll
SyncCommonTests_LIBPATH = $(SyncCommon_LIBPATH) Tests ../../tools/NUnit/bin
SyncCommonTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES =
LIB_FILES = SyncCommon.dll
DATA_FILES = 
TEST_BIN_FILES =
TEST_LIB_FILES = SyncCommonTests.dll
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = SyncCommon.dll SyncCommonTests.dll 
CLEAN_FILES = Check.xml SyncCommon.pdb SyncCommon.Doc.xml Check.xml SyncCommonTests.pdb SyncCommonTests.Doc.xml  SyncCommon.xml SyncCommon.Doc.xml
CLEAN_DIRS = Library/bin Library/obj Tests/bin Tests/obj 
NUNIT_TESTS = SyncCommonTests.dll 
STAGE_FILES = $(TARGETS) SyncCommon.Doc.xml SyncCommonTests.Doc.xml SyncCommon.pdb SyncCommonTests.pdb 
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
#	$(NUNIT) $(NUNIT_FLAGS) /xml:SyncCommon.Test.xml $(NUNIT_TESTS)

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# SyncCommon targets
#=============================================================================

# fix path seperator
SyncCommon_SOURCES := $(subst /,$(SEP), $(SyncCommon_SOURCES))
SyncCommon_INCLUDES := $(subst /,$(SEP), $(SyncCommon_INCLUDES))
SyncCommon_DEF := $(subst /,$(SEP), $(SyncCommon_DEF))
SyncCommon_LIBPATH := $(subst /,$(SEP), $(SyncCommon_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SyncCommon_LIBPATH) $(SyncCommon_INCLUDES)

SyncCommon.dll: $(SyncCommon_SOURCES) $(SyncCommon_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SyncCommon.Doc.xml $(CSCFLAGS) $(SyncCommon_FLAGS) $(SyncCommon_LIBS:%=/r:%) $(SyncCommon_LIBPATH:%=/lib:%) $(SyncCommon_SOURCES)

#=============================================================================
# SyncCommonTests targets
#=============================================================================

# fix path seperator
SyncCommonTests_SOURCES := $(subst /,$(SEP), $(SyncCommonTests_SOURCES))
SyncCommonTests_INCLUDES := $(subst /,$(SEP), $(SyncCommonTests_INCLUDES))
SyncCommonTests_DEF := $(subst /,$(SEP), $(SyncCommonTests_DEF))
SyncCommonTests_LIBPATH := $(subst /,$(SEP), $(SyncCommonTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SyncCommonTests_LIBPATH) $(SyncCommonTests_INCLUDES)

SyncCommonTests.dll: $(SyncCommonTests_SOURCES) $(SyncCommonTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SyncCommonTests.Doc.xml $(CSCFLAGS) $(SyncCommonTests_FLAGS) $(SyncCommonTests_LIBS:%=/r:%) $(SyncCommonTests_LIBPATH:%=/lib:%) $(SyncCommonTests_SOURCES)

include ../../common.mk
