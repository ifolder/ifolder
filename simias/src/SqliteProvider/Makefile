#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	SqliteProvider.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# SqliteProviderTests variables
#=============================================================================
SqliteProviderTests_SOURCES = SqliteProviderTests/AssemblyInfo.cs SqliteProviderTests/SqliteTests1.cs
SqliteProviderTests_INCLUDES = 
SqliteProviderTests_FLAGS = /t:library
SqliteProviderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll Configuration.dll ProviderTest.dll SqliteProvider.dll
SqliteProviderTests_LIBPATH = SqliteProviderTests ../../tools/NUnit/bin ../Configuration ../StoreProvider
SqliteProviderTests_LDFLAGS = 

#=============================================================================
# SqliteWrapper variables
#=============================================================================
SqliteWrapper_SOURCES = SqliteWrapper/AssemblyInfo.cs SqliteWrapper/SqliteCommand.cs SqliteWrapper/SqliteConnection.cs SqliteWrapper/SqliteDataReader.cs SqliteWrapper/SqliteParameter.cs SqliteWrapper/SqliteParameterCollection.cs SqliteWrapper/SqliteTransaction.cs
SqliteWrapper_INCLUDES = 
SqliteWrapper_FLAGS = /t:library /unsafe
SqliteWrapper_LIBS = System.dll System.Data.dll $(SYSTEM_XML)
SqliteWrapper_LIBPATH = SqliteWrapper
SqliteWrapper_LDFLAGS = 

#=============================================================================
# SqliteProvider variables
#=============================================================================
SqliteProvider_SOURCES = AssemblyInfo.cs SqliteProvider.cs SqliteResultSet.cs
SqliteProvider_INCLUDES = 
SqliteProvider_FLAGS = /t:library
SqliteProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll SqliteWrapper.dll
SqliteProvider_LIBPATH = ../Configuration ../StoreProvider
SqliteProvider_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BUILD_LIB_FILES = SqliteWrapper.dll SqliteProvider.dll
BIN_FILES =
ifeq ($(TARGET_OS), linux)
	LIB_FILES = $(BUILD_LIB_FILES) ../../external/sqlite/linux/libsqlite.so
endif
ifeq ($(TARGET_OS), windows)
	LIB_FILES = $(BUILD_LIB_FILES) ../../external/sqlite/w32/sqlite.dll
endif
DATA_FILES = 
TEST_BIN_FILES =
TEST_LIB_FILES = SqliteProviderTests.dll 
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(BUILD_LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
CLEAN_FILES = Check.xml SqliteProviderTests.pdb SqliteProviderTests.Doc.xml Check.xml SqliteWrapper.pdb SqliteWrapper.Doc.xml SqliteProvider.pdb SqliteProvider.Doc.xml  SqliteProvider.xml SqliteProvider.Doc.xml
CLEAN_DIRS = SqliteProviderTests/bin SqliteProviderTests/obj SqliteWrapper/bin SqliteWrapper/obj bin obj 
NUNIT_TESTS = SqliteProviderTests.dll 
STAGE_FILES = $(TARGETS) SqliteProviderTests.Doc.xml StoreProvider.Doc.xml SqliteWrapper.Doc.xml ProviderTest.Doc.xml SqliteProvider.Doc.xml SqliteProviderTests.pdb StoreProvider.pdb SqliteWrapper.pdb ProviderTest.pdb SqliteProvider.pdb 
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
#	$(NUNIT) $(NUNIT_FLAGS) /xml:SqliteProvider.Test.xml $(NUNIT_TESTS)

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# SqliteProviderTests targets
#=============================================================================

# fix path seperator
SqliteProviderTests_SOURCES := $(subst /,$(SEP), $(SqliteProviderTests_SOURCES))
SqliteProviderTests_INCLUDES := $(subst /,$(SEP), $(SqliteProviderTests_INCLUDES))
SqliteProviderTests_DEF := $(subst /,$(SEP), $(SqliteProviderTests_DEF))
SqliteProviderTests_LIBPATH := $(subst /,$(SEP), $(SqliteProviderTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SqliteProviderTests_LIBPATH) $(SqliteProviderTests_INCLUDES)

SqliteProviderTests.dll: $(SqliteProviderTests_SOURCES) $(SqliteProviderTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SqliteProviderTests.Doc.xml $(CSCFLAGS) $(SqliteProviderTests_FLAGS) $(SqliteProviderTests_LIBS:%=/r:%) $(SqliteProviderTests_LIBPATH:%=/lib:%) $(SqliteProviderTests_SOURCES)

#=============================================================================
# SqliteWrapper targets
#=============================================================================

# fix path seperator
SqliteWrapper_SOURCES := $(subst /,$(SEP), $(SqliteWrapper_SOURCES))
SqliteWrapper_INCLUDES := $(subst /,$(SEP), $(SqliteWrapper_INCLUDES))
SqliteWrapper_DEF := $(subst /,$(SEP), $(SqliteWrapper_DEF))
SqliteWrapper_LIBPATH := $(subst /,$(SEP), $(SqliteWrapper_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SqliteWrapper_LIBPATH) $(SqliteWrapper_INCLUDES)

SqliteWrapper.dll: $(SqliteWrapper_SOURCES) $(SqliteWrapper_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SqliteWrapper.Doc.xml $(CSCFLAGS) $(SqliteWrapper_FLAGS) $(SqliteWrapper_LIBS:%=/r:%) $(SqliteWrapper_LIBPATH:%=/lib:%) $(SqliteWrapper_SOURCES)

#=============================================================================
# SqliteProvider targets
#=============================================================================

# fix path seperator
SqliteProvider_SOURCES := $(subst /,$(SEP), $(SqliteProvider_SOURCES))
SqliteProvider_INCLUDES := $(subst /,$(SEP), $(SqliteProvider_INCLUDES))
SqliteProvider_DEF := $(subst /,$(SEP), $(SqliteProvider_DEF))
SqliteProvider_LIBPATH := $(subst /,$(SEP), $(SqliteProvider_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(SqliteProvider_LIBPATH) $(SqliteProvider_INCLUDES)

SqliteProvider.dll: $(SqliteProvider_SOURCES) $(SqliteProvider_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SqliteProvider.Doc.xml $(CSCFLAGS) $(SqliteProvider_FLAGS) $(SqliteProvider_LIBS:%=/r:%) $(SqliteProvider_LIBPATH:%=/lib:%) $(SqliteProvider_SOURCES)

include ../../common.mk
