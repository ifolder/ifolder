#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	iFolderComponent.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# iFolderComponent variables
#=============================================================================
iFolderComponent_SOURCES = AssemblyInfo.cs iFolderAdvanced.cs Win32Window.cs iFolderComponent.cs
iFolderComponent_INCLUDES = 
iFolderComponent_FLAGS = /t:library
iFolderComponent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) iFolder.dll CollectionStore.dll System.Drawing.dll System.Windows.Forms.dll AddressBook.dll FormsBookLib.dll
iFolderComponent_LIBPATH = $(ADDRESSBOOK_ROOT)/src/stage $(SIMIAS_ROOT)/src/stage ../iFolder ../FormsBookLib
iFolderComponent_LDFLAGS = 

#=============================================================================
# iFolderComponentTests variables
#=============================================================================
iFolderComponentTests_SOURCES = iFolderComponentTests/AssemblyInfo.cs iFolderComponentTests/iFolderComponentTests.cs
iFolderComponentTests_INCLUDES = 
iFolderComponentTests_FLAGS = /t:library
iFolderComponentTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) iFolderComponent.dll nunit.framework.dll nunit.extensions.dll
iFolderComponentTests_LIBPATH = iFolderComponentTests iFolderComponentTests/.. iFolderComponentTests/../../../tools/NUnit/bin iFolderComponentTests/../../../tools/NUnit/bin
iFolderComponentTests_LDFLAGS = 

#=============================================================================
# iFolderShell variables
#=============================================================================
iFolderShell_SOURCES = iFolderShell/ContextMenu$(OBJ_EXT) iFolderShell/IconOverlay$(OBJ_EXT) iFolderShell/iFolderShell$(OBJ_EXT) iFolderShell/PropertySheet$(OBJ_EXT) iFolderShell/Registry$(OBJ_EXT) iFolderShell/ShareiFolder$(OBJ_EXT) iFolderShell/iFolderShell$(RES_EXT)
iFolderShell_INCLUDES = $(CINC)iFolderShell
iFolderShell_FLAGS = $(CDEF)_USRDLL $(CDEF)IFOLDERSHELL_EXPORTS
iFolderShell_LIBS = comctl32$(LIB_EXT)
iFolderShell_LIBPATH = iFolderShell
iFolderShell_LDFLAGS = $(SHARED_LIB_FLAG)
iFolderShell_DEF = $(DEF)iFolderShell/iFolderShell.Def

#=============================================================================
# globals
#=============================================================================

TARGETS = iFolderComponent.dll iFolderComponent$(TYPE_LIB_EXT) iFolderComponentTests.dll $(LIB_PRE)iFolderShell$(SHARED_LIB_EXT)
CLEAN_FILES = Check.xml iFolderComponentTests.pdb iFolderComponentTests.Doc.xml iFolderShell.exp iFolderShell.lib Test.xml Check.xml iFolderShell.pdb iFolderShell.Doc.xml iFolderShell/ContextMenu$(OBJ_EXT) iFolderShell/IconOverlay$(OBJ_EXT) iFolderShell/iFolderShell$(OBJ_EXT) iFolderShell/PropertySheet$(OBJ_EXT) iFolderShell/Registry$(OBJ_EXT) iFolderShell/ShareiFolder$(OBJ_EXT) iFolderShell/iFolderShell$(RES_EXT) Check.xml iFolderComponent.pdb iFolderComponent.Doc.xml  iFolderComponent.xml iFolderComponent.Doc.xml
CLEAN_DIRS = iFolderComponentTests/bin iFolderComponentTests/obj  bin obj 
NUNIT_TESTS = iFolderComponentTests.dll 
STAGE_FILES = $(TARGETS) iFolderComponentTests.Doc.xml iFolderComponent.Doc.xml iFolderComponentTests.pdb iFolderComponent.pdb 
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
	$(NUNIT) $(NUNIT_FLAGS) /xml:iFolderComponent.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# iFolderComponent targets
#=============================================================================

# fix path seperator
iFolderComponent_SOURCES := $(subst /,$(SEP), $(iFolderComponent_SOURCES))
iFolderComponent_INCLUDES := $(subst /,$(SEP), $(iFolderComponent_INCLUDES))
iFolderComponent_DEF := $(subst /,$(SEP), $(iFolderComponent_DEF))
iFolderComponent_LIBPATH := $(subst /,$(SEP), $(iFolderComponent_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(iFolderComponent_LIBPATH) $(iFolderComponent_INCLUDES)

iFolderComponent.dll: $(iFolderComponent_SOURCES) $(iFolderComponent_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:iFolderComponent.Doc.xml $(CSCFLAGS) $(iFolderComponent_FLAGS) $(iFolderComponent_LIBS:%=/r:%) $(iFolderComponent_LIBPATH:%=/lib:%) $(iFolderComponent_SOURCES)

#=============================================================================
# iFolderComponent type library targets
#=============================================================================

iFolderComponent$(TYPE_LIB_EXT): iFolderComponent.dll
	$(TLBX) $< $(LDOUT)$@
	
#=============================================================================
# iFolderComponentTests targets
#=============================================================================

# fix path seperator
iFolderComponentTests_SOURCES := $(subst /,$(SEP), $(iFolderComponentTests_SOURCES))
iFolderComponentTests_INCLUDES := $(subst /,$(SEP), $(iFolderComponentTests_INCLUDES))
iFolderComponentTests_DEF := $(subst /,$(SEP), $(iFolderComponentTests_DEF))
iFolderComponentTests_LIBPATH := $(subst /,$(SEP), $(iFolderComponentTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(iFolderComponentTests_LIBPATH) $(iFolderComponentTests_INCLUDES)

iFolderComponentTests.dll: $(iFolderComponentTests_SOURCES) $(iFolderComponentTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:iFolderComponentTests.Doc.xml $(CSCFLAGS) $(iFolderComponentTests_FLAGS) $(iFolderComponentTests_LIBS:%=/r:%) $(iFolderComponentTests_LIBPATH:%=/lib:%) $(iFolderComponentTests_SOURCES)

#=============================================================================
# iFolderShell targets
#=============================================================================

# fix path seperator
iFolderShell_SOURCES := $(subst /,$(SEP), $(iFolderShell_SOURCES))
iFolderShell_INCLUDES := $(subst /,$(SEP), $(iFolderShell_INCLUDES))
iFolderShell_DEF := $(subst /,$(SEP), $(iFolderShell_DEF))
iFolderShell_LIBPATH := $(subst /,$(SEP), $(iFolderShell_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(iFolderShell_LIBPATH) $(iFolderShell_INCLUDES)

%$(OBJ_EXT) : %.c ../../config.mk
	$(CC) $(COUT)$@ $(iFolderShell_FLAGS) $(iFolderShell_INCLUDES) $(CFLAGS) $<

%$(OBJ_EXT) : %.cpp ../../config.mk
	$(CXX) $(COUT)$@ $(iFolderShell_FLAGS) $(iFolderShell_INCLUDES) $(CXXFLAGS) $<

%$(RES_EXT) : %.rc ../../config.mk
	$(RC) $(RCFLAGS) $(CHARFLAGS) $(COUT)$@ $<

$(LIB_PRE)iFolderShell$(SHARED_LIB_EXT): $(iFolderShell_SOURCES) ../../config.mk
	$(LD) $(iFolderShell_LDFLAGS) $(LDFLAGS) $(LDOUT)$@ $(iFolderShell_LIBPATH:%=$(LDINC)%) $(iFolderShell_DEF) $(iFolderShell_SOURCES) $(iFolderShell_LIBS:%=$(LIBFLAG)%) $(LIBS)

