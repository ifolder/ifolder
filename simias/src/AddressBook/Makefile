#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	AddressBook.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# AddressBook variables
#=============================================================================
AddressBook_SOURCES = Address.cs AddressBook.cs AssemblyInfo.cs Common.cs Contact.cs ContactList.cs Email.cs Manager.cs Name.cs Phone.cs
AddressBook_INCLUDES = 
AddressBook_FLAGS = /t:library
AddressBook_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll Configuration.dll StoreProvider.dll CollectionEvents.dll Identity.dll CollectionStore.dll Identity.dll WorkGroupIdentity.dll StoreProvider.dll
AddressBook_LIBPATH = ../Common ../CollectionEvents ../CollectionStore ../Configuration ../StoreProvider ../Identity ../CollectionWatcher ../StoreWatcher ../WorkGroupIdentity
AddressBook_LDFLAGS = 

#=============================================================================
# AddressBookTests variables
#=============================================================================
AddressBookTests_SOURCES = AddressBookTests/AddressBookTests.cs AddressBookTests/AssemblyInfo.cs
AddressBookTests_INCLUDES = 
AddressBookTests_FLAGS = /t:exe
AddressBookTests_LIBS = $(AddressBook_LIBS) $(SYSTEM_XML) nunit.util.dll nunit.framework.dll AddressBook.dll
AddressBookTests_LIBPATH = $(AddressBook_LIBPATH) AddressBookTests ../../tools/NUnit/bin
AddressBookTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES =
LIB_FILES = AddressBook.dll
DATA_FILES = 
TEST_BIN_FILES =
TEST_LIB_FILES = AddressBookTests.exe
TEST_DATA_FILES = 

TARGETS = $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES)
#TARGETS = AddressBook.dll AddressBookTests.exe 
CLEAN_FILES = Check.xml AddressBook.pdb AddressBook.Doc.xml Check.xml AddressBookTests.pdb AddressBookTests.Doc.xml  AddressBook.xml AddressBook.Doc.xml
CLEAN_DIRS = bin obj AddressBookTests/bin AddressBookTests/obj 
NUNIT_TESTS = AddressBookTests.exe 
STAGE_FILES = $(TARGETS) AddressBook.Doc.xml AddressBookTests.Doc.xml AddressBook.pdb AddressBookTests.pdb 
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
#	$(NUNIT) $(NUNIT_FLAGS) /xml:AddressBook.Test.xml $(NUNIT_TESTS)

# stage files
#$(STAGE_FILES): $(TARGETS)
#	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
#$(EXTRA_STAGE_FILES):
#	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# AddressBook targets
#=============================================================================

# fix path seperator
AddressBook_SOURCES := $(subst /,$(SEP), $(AddressBook_SOURCES))
AddressBook_INCLUDES := $(subst /,$(SEP), $(AddressBook_INCLUDES))
AddressBook_DEF := $(subst /,$(SEP), $(AddressBook_DEF))
AddressBook_LIBPATH := $(subst /,$(SEP), $(AddressBook_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(AddressBook_LIBPATH) $(AddressBook_INCLUDES)

AddressBook.dll: $(AddressBook_SOURCES) $(AddressBook_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:AddressBook.Doc.xml $(CSCFLAGS) $(AddressBook_FLAGS) $(AddressBook_LIBS:%=/r:%) $(AddressBook_LIBPATH:%=/lib:%) $(AddressBook_SOURCES)

#=============================================================================
# AddressBookTests targets
#=============================================================================

# fix path seperator
AddressBookTests_SOURCES := $(subst /,$(SEP), $(AddressBookTests_SOURCES))
AddressBookTests_INCLUDES := $(subst /,$(SEP), $(AddressBookTests_INCLUDES))
AddressBookTests_DEF := $(subst /,$(SEP), $(AddressBookTests_DEF))
AddressBookTests_LIBPATH := $(subst /,$(SEP), $(AddressBookTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(AddressBookTests_LIBPATH) $(AddressBookTests_INCLUDES)

AddressBookTests.exe: $(AddressBookTests_SOURCES) $(AddressBookTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:AddressBookTests.Doc.xml $(CSCFLAGS) $(AddressBookTests_FLAGS) $(AddressBookTests_LIBS:%=/r:%) $(AddressBookTests_LIBPATH:%=/lib:%) $(AddressBookTests_SOURCES)

include ../../common.mk
