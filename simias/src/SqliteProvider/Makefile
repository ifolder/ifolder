#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	SqliteProvider.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# SqliteProviderTests variables
#=============================================================================
SqliteProviderTests_SOURCES = SqliteProviderTests/AssemblyInfo.cs SqliteProviderTests/SqliteTests1.cs
SqliteProviderTests_INCLUDES = 
SqliteProviderTests_FLAGS = /t:library
SqliteProviderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll ProviderTest.dll SqliteProvider.dll
SqliteProviderTests_LIBPATH = SqliteProviderTests SqliteProviderTests/../../../tools/NUnit/bin SqliteProviderTests/../../stage SqliteProviderTests/../../stage
SqliteProviderTests_LDFLAGS = 

#=============================================================================
# StoreProvider variables
#=============================================================================
StoreProvider_SOURCES = ../StoreProvider/AssemblyInfo.cs ../StoreProvider/CSPException.cs ../StoreProvider/IProvider.cs ../StoreProvider/IResultSet.cs ../StoreProvider/Provider.cs ../StoreProvider/Query.cs ../StoreProvider/Record.cs
StoreProvider_INCLUDES = 
StoreProvider_FLAGS = /t:library
StoreProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll
StoreProvider_LIBPATH = ../StoreProvider ../StoreProvider/../stage
StoreProvider_LDFLAGS = 

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
# ProviderTest variables
#=============================================================================
ProviderTest_SOURCES = ../StoreProvider/Test/AssemblyInfo.cs ../StoreProvider/Test/ProviderTests1.cs
ProviderTest_INCLUDES = 
ProviderTest_FLAGS = /t:library
ProviderTest_LIBS = System.dll System.Data.dll $(SYSTEM_XML) StoreProvider.dll nunit.framework.dll
ProviderTest_LIBPATH = ../StoreProvider/Test ../StoreProvider/Test/../../stage ../StoreProvider/Test/../../../tools/NUnit/bin
ProviderTest_LDFLAGS = 

#=============================================================================
# SqliteProvider variables
#=============================================================================
SqliteProvider_SOURCES = AssemblyInfo.cs SqliteProvider.cs SqliteResultSet.cs
SqliteProvider_INCLUDES = 
SqliteProvider_FLAGS = /t:library
SqliteProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) StoreProvider.dll SqliteWrapper.dll
SqliteProvider_LIBPATH = ../stage ../stage
SqliteProvider_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = SqliteProviderTests.dll StoreProvider.dll SqliteWrapper.dll ProviderTest.dll SqliteProvider.dll 
CLEAN_FILES = Check.xml SqliteProviderTests.pdb SqliteProviderTests.Doc.xml Check.xml StoreProvider.pdb StoreProvider.Doc.xml Check.xml SqliteWrapper.pdb SqliteWrapper.Doc.xml Check.xml ProviderTest.pdb ProviderTest.Doc.xml Check.xml SqliteProvider.pdb SqliteProvider.Doc.xml  SqliteProvider.xml SqliteProvider.Doc.xml
CLEAN_DIRS = SqliteProviderTests/bin SqliteProviderTests/obj ../StoreProvider/bin ../StoreProvider/obj SqliteWrapper/bin SqliteWrapper/obj ../StoreProvider/Test/bin ../StoreProvider/Test/obj bin obj 
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
all: $(TARGETS) $(STAGE_FILES) $(EXTRA_STAGE_FILES)

# clean
clean: $(CLEAN_TARGETS)
	-$(RMDIR) $(CLEAN_DIRS)

$(CLEAN_TARGETS): 
	$(call RM_IF_EXISTS,$(subst clean.,,$@))

# test
test:
	$(NUNIT) $(NUNIT_FLAGS) /xml:SqliteProvider.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

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

SqliteProviderTests.dll: $(SqliteProviderTests_SOURCES) $(SqliteProviderTests_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:SqliteProviderTests.Doc.xml $(CSCFLAGS) $(SqliteProviderTests_FLAGS) $(SqliteProviderTests_LIBS:%=/r:%) $(SqliteProviderTests_LIBPATH:%=/lib:%) $(SqliteProviderTests_SOURCES)

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

StoreProvider.dll: $(StoreProvider_SOURCES) $(StoreProvider_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:StoreProvider.Doc.xml $(CSCFLAGS) $(StoreProvider_FLAGS) $(StoreProvider_LIBS:%=/r:%) $(StoreProvider_LIBPATH:%=/lib:%) $(StoreProvider_SOURCES)

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

SqliteWrapper.dll: $(SqliteWrapper_SOURCES) $(SqliteWrapper_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:SqliteWrapper.Doc.xml $(CSCFLAGS) $(SqliteWrapper_FLAGS) $(SqliteWrapper_LIBS:%=/r:%) $(SqliteWrapper_LIBPATH:%=/lib:%) $(SqliteWrapper_SOURCES)

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

ProviderTest.dll: $(ProviderTest_SOURCES) $(ProviderTest_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:ProviderTest.Doc.xml $(CSCFLAGS) $(ProviderTest_FLAGS) $(ProviderTest_LIBS:%=/r:%) $(ProviderTest_LIBPATH:%=/lib:%) $(ProviderTest_SOURCES)

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

SqliteProvider.dll: $(SqliteProvider_SOURCES) $(SqliteProvider_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:SqliteProvider.Doc.xml $(CSCFLAGS) $(SqliteProvider_FLAGS) $(SqliteProvider_LIBS:%=/r:%) $(SqliteProvider_LIBPATH:%=/lib:%) $(SqliteProvider_SOURCES)

