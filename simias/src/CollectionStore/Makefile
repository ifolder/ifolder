#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	CollectionStore.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# CollectionStore variables
#=============================================================================
CollectionStore_SOURCES = AccessControl.cs AssemblyInfo.cs CacheNode.cs Collection.cs DirectoryEntry.cs FileEntry.cs FileSystemEntry.cs ICSEnumerator.cs MultiValuedList.cs Node.cs NodeStream.cs Property.cs PropertyList.cs Store.cs StoreIdentity.cs
CollectionStore_INCLUDES = 
CollectionStore_FLAGS = /t:library
CollectionStore_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll System.Security.dll Identity.dll CollectionEvents.dll
CollectionStore_LIBPATH = ../Configuration ../StoreProvider ../Identity ../CollectionEvents
CollectionStore_LDFLAGS = 

#=============================================================================
# CollectionStoreTests variables
#=============================================================================
CollectionStoreTests_SOURCES = CollectionStoreTests/AssemblyInfo.cs CollectionStoreTests/CollectionStoreTests.cs
CollectionStoreTests_INCLUDES = 
CollectionStoreTests_FLAGS = /t:library
CollectionStoreTests_LIBS = $(CollectionStore_LIBS) CollectionStore.dll $(SYSTEM_XML) nunit.extensions.dll nunit.framework.dll
CollectionStoreTests_LIBPATH = $(CollectionStore_LIBPATH) CollectionStoreTests ../../tools/NUnit/bin
CollectionStoreTests_LDFLAGS = 


#=============================================================================
# globals
#=============================================================================
BIN_FILES =
LIB_FILES = CollectionStore.dll
DATA_FILES = 
TEST_BIN_FILES =
TEST_LIB_FILES = CollectionStoreTests.dll
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = CollectionStoreTests.dll CollectionStore.dll 
CLEAN_FILES = Check.xml CollectionStoreTests.pdb CollectionStoreTests.Doc.xml Check.xml CollectionStore.pdb CollectionStore.Doc.xml  CollectionStore.xml CollectionStore.Doc.xml
CLEAN_DIRS = CollectionStoreTests/bin CollectionStoreTests/obj bin obj 
#NUNIT_TESTS = CollectionStoreTests.dll 
STAGE_FILES = $(TARGETS) CollectionStoreTests.Doc.xml CollectionStore.Doc.xml CollectionStoreTests.pdb CollectionStore.pdb 
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
#	$(NUNIT) $(NUNIT_FLAGS) /xml:CollectionStore.Test.xml $(NUNIT_TESTS)

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# CollectionStoreTests targets
#=============================================================================

# fix path seperator
CollectionStoreTests_SOURCES := $(subst /,$(SEP), $(CollectionStoreTests_SOURCES))
CollectionStoreTests_INCLUDES := $(subst /,$(SEP), $(CollectionStoreTests_INCLUDES))
CollectionStoreTests_DEF := $(subst /,$(SEP), $(CollectionStoreTests_DEF))
CollectionStoreTests_LIBPATH := $(subst /,$(SEP), $(CollectionStoreTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CollectionStoreTests_LIBPATH) $(CollectionStoreTests_INCLUDES)

CollectionStoreTests.dll: $(CollectionStoreTests_SOURCES) $(CollectionStoreTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:CollectionStoreTests.Doc.xml $(CSCFLAGS) $(CollectionStoreTests_FLAGS) $(CollectionStoreTests_LIBS:%=/r:%) $(CollectionStoreTests_LIBPATH:%=/lib:%) $(CollectionStoreTests_SOURCES)

#=============================================================================
# CollectionStore targets
#=============================================================================

# fix path seperator
CollectionStore_SOURCES := $(subst /,$(SEP), $(CollectionStore_SOURCES))
CollectionStore_INCLUDES := $(subst /,$(SEP), $(CollectionStore_INCLUDES))
CollectionStore_DEF := $(subst /,$(SEP), $(CollectionStore_DEF))
CollectionStore_LIBPATH := $(subst /,$(SEP), $(CollectionStore_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(CollectionStore_LIBPATH) $(CollectionStore_INCLUDES)

CollectionStore.dll: $(CollectionStore_SOURCES) $(CollectionStore_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:CollectionStore.Doc.xml $(CSCFLAGS) $(CollectionStore_FLAGS) $(CollectionStore_LIBS:%=/r:%) $(CollectionStore_LIBPATH:%=/lib:%) $(CollectionStore_SOURCES)

include ../../common.mk
