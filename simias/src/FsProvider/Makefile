#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	FsProvider.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# FsProvider variables
#=============================================================================
FsProvider_SOURCES = AssemblyInfo.cs FsProvider.cs FsResultSet.cs
FsProvider_INCLUDES = 
FsProvider_FLAGS = /t:library
FsProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll StoreProvider.dll
FsProvider_LIBPATH = ../stage
FsProvider_LDFLAGS = 

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
# ProviderTest variables
#=============================================================================
ProviderTest_SOURCES = ../StoreProvider/Test/AssemblyInfo.cs ../StoreProvider/Test/ProviderTests1.cs
ProviderTest_INCLUDES = 
ProviderTest_FLAGS = /t:library
ProviderTest_LIBS = System.dll System.Data.dll $(SYSTEM_XML) StoreProvider.dll nunit.framework.dll
ProviderTest_LIBPATH = ../StoreProvider/Test ../StoreProvider/Test/../../stage ../StoreProvider/Test/../../../tools/NUnit/bin
ProviderTest_LDFLAGS = 

#=============================================================================
# FsProviderTests variables
#=============================================================================
FsProviderTests_SOURCES = FsProviderTests/AssemblyInfo.cs FsProviderTests/FsTests1.cs
FsProviderTests_INCLUDES = 
FsProviderTests_FLAGS = /t:library
FsProviderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll StoreProvider.dll ProviderTest.dll
FsProviderTests_LIBPATH = FsProviderTests FsProviderTests/../../../tools/NUnit/bin FsProviderTests/../../stage FsProviderTests/../../stage
FsProviderTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = FsProvider.dll StoreProvider.dll ProviderTest.dll FsProviderTests.dll 
CLEAN_FILES = Check.xml FsProvider.pdb FsProvider.Doc.xml Check.xml StoreProvider.pdb StoreProvider.Doc.xml Check.xml ProviderTest.pdb ProviderTest.Doc.xml Check.xml FsProviderTests.pdb FsProviderTests.Doc.xml  FsProvider.xml FsProvider.Doc.xml
CLEAN_DIRS = bin obj ../StoreProvider/bin ../StoreProvider/obj ../StoreProvider/Test/bin ../StoreProvider/Test/obj FsProviderTests/bin FsProviderTests/obj 
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
all: $(TARGETS) $(STAGE_FILES) $(EXTRA_STAGE_FILES)

# clean
clean: $(CLEAN_TARGETS)
	-$(RMDIR) $(CLEAN_DIRS)

$(CLEAN_TARGETS): 
	$(call RM_IF_EXISTS,$(subst clean.,,$@))

# test
test:
	$(NUNIT) $(NUNIT_FLAGS) /xml:FsProvider.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

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

FsProvider.dll: $(FsProvider_SOURCES) $(FsProvider_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:FsProvider.Doc.xml $(CSCFLAGS) $(FsProvider_FLAGS) $(FsProvider_LIBS:%=/r:%) $(FsProvider_LIBPATH:%=/lib:%) $(FsProvider_SOURCES)

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
# FsProviderTests targets
#=============================================================================

# fix path seperator
FsProviderTests_SOURCES := $(subst /,$(SEP), $(FsProviderTests_SOURCES))
FsProviderTests_INCLUDES := $(subst /,$(SEP), $(FsProviderTests_INCLUDES))
FsProviderTests_DEF := $(subst /,$(SEP), $(FsProviderTests_DEF))
FsProviderTests_LIBPATH := $(subst /,$(SEP), $(FsProviderTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(FsProviderTests_LIBPATH) $(FsProviderTests_INCLUDES)

FsProviderTests.dll: $(FsProviderTests_SOURCES) $(FsProviderTests_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:FsProviderTests.Doc.xml $(CSCFLAGS) $(FsProviderTests_FLAGS) $(FsProviderTests_LIBS:%=/r:%) $(FsProviderTests_LIBPATH:%=/lib:%) $(FsProviderTests_SOURCES)

