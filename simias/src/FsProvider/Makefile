#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	FsProvider.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# FsProvider variables
#=============================================================================
FsProvider_SOURCES = AssemblyInfo.cs FsProvider.cs FsResultSet.cs
FsProvider_INCLUDES = 
FsProvider_FLAGS = /t:library
FsProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Configuration.dll StoreProvider.dll
FsProvider_LIBPATH = ../Configuration ../StoreProvider
FsProvider_LDFLAGS = 

#=============================================================================
# FsProviderTests variables
#=============================================================================
FsProviderTests_SOURCES = FsProviderTests/AssemblyInfo.cs FsProviderTests/FsTests1.cs
FsProviderTests_INCLUDES = 
FsProviderTests_FLAGS = /t:library
FsProviderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll Configuration.dll StoreProvider.dll ProviderTest.dll
FsProviderTests_LIBPATH = FsProviderTests ../../tools/NUnit/bin ../Configuration ../StoreProvider
FsProviderTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES = 
LIB_FILES = FsProvider.dll
DATA_FILES = 
TEST_BIN_FILES = 
TEST_LIB_FILES = FsProviderTests.dll
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS =FsProvider.dll StoreProvider.dll ProviderTest.dll FsProviderTests.dll 
CLEAN_FILES = Check.xml FsProvider.pdb FsProvider.Doc.xml Check.xml FsProviderTests.pdb FsProviderTests.Doc.xml  FsProvider.xml FsProvider.Doc.xml
CLEAN_DIRS = bin obj FsProviderTests/bin FsProviderTests/obj 
NUNIT_TESTS = FsProviderTests.dll 
STAGE_FILES = $(TARGETS) FsProvider.Doc.xml StoreProvider.Doc.xml ProviderTest.Doc.xml FsProviderTests.Doc.xml FsProvider.pdb StoreProvider.pdb ProviderTest.pdb FsProviderTests.pdb 
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
#	$(NUNIT) $(NUNIT_FLAGS) /xml:FsProvider.Test.xml $(NUNIT_TESTS)

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# FsProvider targets
#=============================================================================

# fix path seperator
FsProvider_SOURCES := $(subst /,$(SEP), $(FsProvider_SOURCES))
FsProvider_INCLUDES := $(subst /,$(SEP), $(FsProvider_INCLUDES))
FsProvider_DEF := $(subst /,$(SEP), $(FsProvider_DEF))
FsProvider_LIBPATH := $(subst /,$(SEP), $(FsProvider_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(FsProvider_LIBPATH) $(FsProvider_INCLUDES)

FsProvider.dll: $(FsProvider_SOURCES) $(FsProvider_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:FsProvider.Doc.xml $(CSCFLAGS) $(FsProvider_FLAGS) $(FsProvider_LIBS:%=/r:%) $(FsProvider_LIBPATH:%=/lib:%) $(FsProvider_SOURCES)

#=============================================================================
# FsProviderTests targets
#=============================================================================

# fix path seperator
FsProviderTests_SOURCES := $(subst /,$(SEP), $(FsProviderTests_SOURCES))
FsProviderTests_INCLUDES := $(subst /,$(SEP), $(FsProviderTests_INCLUDES))
FsProviderTests_DEF := $(subst /,$(SEP), $(FsProviderTests_DEF))
FsProviderTests_LIBPATH := $(subst /,$(SEP), $(FsProviderTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(FsProviderTests_LIBPATH) $(FsProviderTests_INCLUDES)

FsProviderTests.dll: $(FsProviderTests_SOURCES) $(FsProviderTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:FsProviderTests.Doc.xml $(CSCFLAGS) $(FsProviderTests_FLAGS) $(FsProviderTests_LIBS:%=/r:%) $(FsProviderTests_LIBPATH:%=/lib:%) $(FsProviderTests_SOURCES)

include ../../common.mk
