#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	StoreProvider.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# StoreProvider variables
#=============================================================================
StoreProvider_SOURCES = AssemblyInfo.cs CSPException.cs IProvider.cs IResultSet.cs Provider.cs Query.cs Record.cs
StoreProvider_INCLUDES = 
StoreProvider_FLAGS = /t:library
StoreProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll
StoreProvider_LIBPATH = ../Configuration
StoreProvider_LDFLAGS = 

#=============================================================================
# ProviderTest variables
#=============================================================================
ProviderTest_SOURCES = Test/AssemblyInfo.cs Test/ProviderTests1.cs
ProviderTest_INCLUDES = 
ProviderTest_FLAGS = /t:library
ProviderTest_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll nunit.framework.dll
ProviderTest_LIBPATH = ../Configuration Test ../../tools/NUnit/bin
ProviderTest_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES = 
LIB_FILES = StoreProvider.dll
DATA_FILES = 
TEST_BIN_FILES = 
TEST_LIB_FILES = ProviderTest.dll
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = StoreProvider.dll ProviderTest.dll 
CLEAN_FILES = Check.xml StoreProvider.pdb StoreProvider.Doc.xml Check.xml ProviderTest.pdb ProviderTest.Doc.xml  StoreProvider.xml StoreProvider.Doc.xml
CLEAN_DIRS = bin obj Test/bin Test/obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) StoreProvider.Doc.xml ProviderTest.Doc.xml StoreProvider.pdb ProviderTest.pdb 
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
# StoreProvider targets
#=============================================================================

# fix path seperator
StoreProvider_SOURCES := $(subst /,$(SEP), $(StoreProvider_SOURCES))
StoreProvider_INCLUDES := $(subst /,$(SEP), $(StoreProvider_INCLUDES))
StoreProvider_DEF := $(subst /,$(SEP), $(StoreProvider_DEF))
StoreProvider_LIBPATH := $(subst /,$(SEP), $(StoreProvider_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(StoreProvider_LIBPATH) $(StoreProvider_INCLUDES)

StoreProvider.dll: $(StoreProvider_SOURCES) $(StoreProvider_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:StoreProvider.Doc.xml $(CSCFLAGS) $(StoreProvider_FLAGS) $(StoreProvider_LIBS:%=/r:%) $(StoreProvider_LIBPATH:%=/lib:%) $(StoreProvider_SOURCES)

#=============================================================================
# ProviderTest targets
#=============================================================================

# fix path seperator
ProviderTest_SOURCES := $(subst /,$(SEP), $(ProviderTest_SOURCES))
ProviderTest_INCLUDES := $(subst /,$(SEP), $(ProviderTest_INCLUDES))
ProviderTest_DEF := $(subst /,$(SEP), $(ProviderTest_DEF))
ProviderTest_LIBPATH := $(subst /,$(SEP), $(ProviderTest_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(ProviderTest_LIBPATH) $(ProviderTest_INCLUDES)

ProviderTest.dll: $(ProviderTest_SOURCES) $(ProviderTest_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:ProviderTest.Doc.xml $(CSCFLAGS) $(ProviderTest_FLAGS) $(ProviderTest_LIBS:%=/r:%) $(ProviderTest_LIBPATH:%=/lib:%) $(ProviderTest_SOURCES)

include ../../common.mk
