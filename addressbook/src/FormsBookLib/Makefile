#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	FormsBookLib.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# FormsBookLib variables
#=============================================================================
FormsBookLib_SOURCES = AddBook.cs AssemblyInfo.cs BooksContacts.cs ContactEditor.cs ContactPicker.cs
FormsBookLib_INCLUDES = 
FormsBookLib_FLAGS = /t:library
FormsBookLib_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll System.Windows.Forms.dll AddressBook.dll CollectionStore.dll
FormsBookLib_LIBPATH = $(SIMIAS_ROOT)/src/stage
FormsBookLib_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = FormsBookLib.dll 
CLEAN_FILES = Check.xml FormsBookLib.pdb FormsBookLib.Doc.xml  FormsBookLib.xml FormsBookLib.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) FormsBookLib.Doc.xml FormsBookLib.pdb 
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
	@echo No NUnit test assemblies found.

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# FormsBookLib targets
#=============================================================================

# fix path seperator
FormsBookLib_SOURCES := $(subst /,$(SEP), $(FormsBookLib_SOURCES))
FormsBookLib_INCLUDES := $(subst /,$(SEP), $(FormsBookLib_INCLUDES))
FormsBookLib_DEF := $(subst /,$(SEP), $(FormsBookLib_DEF))
FormsBookLib_LIBPATH := $(subst /,$(SEP), $(FormsBookLib_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(FormsBookLib_LIBPATH) $(FormsBookLib_INCLUDES)

FormsBookLib.dll: $(FormsBookLib_SOURCES) $(FormsBookLib_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:FormsBookLib.Doc.xml $(CSCFLAGS) $(FormsBookLib_FLAGS) $(FormsBookLib_LIBS:%=/r:%) $(FormsBookLib_LIBPATH:%=/lib:%) $(FormsBookLib_SOURCES)

