#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	SyncLogicLite.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# SyncLogicLite variables
#=============================================================================
SyncLogicLite_SOURCES = Library/AssemblyInfo.cs Library/SyncCollectionServiceLite.cs Library/SyncCollectionWorkerLite.cs Library/SyncLogicFactoryLite.cs Library/SyncPacket.cs Library/SyncPacketData.cs Library/SyncPacketHeader.cs
SyncLogicLite_INCLUDES = 
SyncLogicLite_FLAGS = /t:library
SyncLogicLite_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll SyncCommon.dll Identity.dll StoreProvider.dll CollectionStore.dll
SyncLogicLite_LIBPATH = Library Library/../../stage Library/../../stage Library/../../stage Library/../../stage Library/../../stage
SyncLogicLite_LDFLAGS = 

#=============================================================================
# SyncLogicLiteTests variables
#=============================================================================
SyncLogicLiteTests_SOURCES = Tests/AssemblyInfo.cs Tests/SyncLogicLiteTests.cs
SyncLogicLiteTests_INCLUDES = 
SyncLogicLiteTests_FLAGS = /t:library
SyncLogicLiteTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll Common.dll Identity.dll StoreProvider.dll SyncCommon.dll SyncLogicLite.dll SyncManager.dll CollectionStore.dll
SyncLogicLiteTests_LIBPATH = Tests Tests/../../stage Tests/../../stage Tests/../../stage Tests/../../stage Tests/../../stage Tests/../../stage Tests/../../stage Tests/../../stage
SyncLogicLiteTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = SyncLogicLite.dll SyncLogicLiteTests.dll 
CLEAN_FILES = Check.xml SyncLogicLite.pdb SyncLogicLite.Doc.xml Check.xml SyncLogicLiteTests.pdb SyncLogicLiteTests.Doc.xml  SyncLogicLite.xml SyncLogicLite.Doc.xml
CLEAN_DIRS = Library/bin Library/obj Tests/bin Tests/obj 
NUNIT_TESTS = SyncLogicLiteTests.dll 
STAGE_FILES = $(TARGETS) SyncLogicLite.Doc.xml SyncLogicLiteTests.Doc.xml SyncLogicLite.pdb SyncLogicLiteTests.pdb 
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
	$(NUNIT) $(NUNIT_FLAGS) /xml:SyncLogicLite.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# SyncLogicLite targets
#=============================================================================

# fix path seperator
SyncLogicLite_SOURCES := $(subst /,$(SEP), $(SyncLogicLite_SOURCES))
SyncLogicLite_INCLUDES := $(subst /,$(SEP), $(SyncLogicLite_INCLUDES))
SyncLogicLite_DEF := $(subst /,$(SEP), $(SyncLogicLite_DEF))
SyncLogicLite_LIBPATH := $(subst /,$(SEP), $(SyncLogicLite_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SyncLogicLite_LIBPATH) $(SyncLogicLite_INCLUDES)

SyncLogicLite.dll: $(SyncLogicLite_SOURCES) $(SyncLogicLite_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SyncLogicLite.Doc.xml $(CSCFLAGS) $(SyncLogicLite_FLAGS) $(SyncLogicLite_LIBS:%=/r:%) $(SyncLogicLite_LIBPATH:%=/lib:%) $(SyncLogicLite_SOURCES)

#=============================================================================
# SyncLogicLiteTests targets
#=============================================================================

# fix path seperator
SyncLogicLiteTests_SOURCES := $(subst /,$(SEP), $(SyncLogicLiteTests_SOURCES))
SyncLogicLiteTests_INCLUDES := $(subst /,$(SEP), $(SyncLogicLiteTests_INCLUDES))
SyncLogicLiteTests_DEF := $(subst /,$(SEP), $(SyncLogicLiteTests_DEF))
SyncLogicLiteTests_LIBPATH := $(subst /,$(SEP), $(SyncLogicLiteTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SyncLogicLiteTests_LIBPATH) $(SyncLogicLiteTests_INCLUDES)

SyncLogicLiteTests.dll: $(SyncLogicLiteTests_SOURCES) $(SyncLogicLiteTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SyncLogicLiteTests.Doc.xml $(CSCFLAGS) $(SyncLogicLiteTests_FLAGS) $(SyncLogicLiteTests_LIBS:%=/r:%) $(SyncLogicLiteTests_LIBPATH:%=/lib:%) $(SyncLogicLiteTests_SOURCES)

