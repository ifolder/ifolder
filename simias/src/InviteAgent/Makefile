#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	InviteAgent.sln
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# InviteAgent variables
#=============================================================================
InviteAgent_SOURCES = Library/AssemblyInfo.cs Library/InviteAgent.cs
InviteAgent_INCLUDES = 
InviteAgent_FLAGS = /t:library
InviteAgent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll CollectionStore.dll StoreProvider.dll Identity.dll Agent.dll AddressBook.dll Common.dll SyncCommon.dll
InviteAgent_LIBPATH = Library Library/../../CollectionStore Library/../../StoreProvider Library/../../Identity Library/../../Agent Library/../../AddressBook Library/../../Common Library/../../stage
InviteAgent_LDFLAGS = 

#=============================================================================
# InviteAgentConsole variables
#=============================================================================
InviteAgentConsole_SOURCES = ConsoleApp/AssemblyInfo.cs ConsoleApp/MainClass.cs
InviteAgentConsole_INCLUDES = 
InviteAgentConsole_FLAGS = /t:exe $(ICON_FLAG)ConsoleApp/App$(ICON_EXT)
InviteAgentConsole_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Agent.dll CollectionStore.dll StoreProvider.dll Identity.dll
InviteAgentConsole_LIBPATH = ConsoleApp ConsoleApp/../../Agent ConsoleApp/../../CollectionStore ConsoleApp/../../StoreProvider ConsoleApp/../../Identity
InviteAgentConsole_LDFLAGS = 

#=============================================================================
# InviteAgentGui variables
#=============================================================================
InviteAgentGui_SOURCES = FormApp/AssemblyInfo.cs FormApp/MainForm.cs
InviteAgentGui_INCLUDES = 
InviteAgentGui_FLAGS = /t:winexe $(ICON_FLAG)FormApp/App$(ICON_EXT)
InviteAgentGui_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) Agent.dll CollectionStore.dll StoreProvider.dll Identity.dll
InviteAgentGui_LIBPATH = FormApp FormApp/../../Agent FormApp/../../CollectionStore FormApp/../../StoreProvider FormApp/../../Identity
InviteAgentGui_LDFLAGS = 

#=============================================================================
# EmailInviteAgent variables
#=============================================================================
EmailInviteAgent_SOURCES = LibraryEmail/AssemblyInfo.cs LibraryEmail/EmailInviteAgent.cs
EmailInviteAgent_INCLUDES = 
EmailInviteAgent_FLAGS = /t:library
EmailInviteAgent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll Agent.dll StoreProvider.dll Identity.dll AddressBook.dll CollectionStore.dll InviteAgent.dll Common.dll Configuration.dll Smtp.dll
EmailInviteAgent_LIBPATH = LibraryEmail LibraryEmail/../../Agent LibraryEmail/../../StoreProvider LibraryEmail/../../Identity LibraryEmail/../../AddressBook LibraryEmail/../../CollectionStore LibraryEmail/../Library LibraryEmail/../../Common LibraryEmail/../../Configuration LibraryEmail/../../Smtp
EmailInviteAgent_LDFLAGS = 

#=============================================================================
# InviteAgentTests variables
#=============================================================================
InviteAgentTests_SOURCES = Tests/AssemblyInfo.cs Tests/InviteAgentTests.cs
InviteAgentTests_INCLUDES = 
InviteAgentTests_FLAGS = /t:library
InviteAgentTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) CollectionStore.dll AddressBook.dll nunit.framework.dll Agent.dll StoreProvider.dll Identity.dll InviteAgent.dll Common.dll SyncCommon.dll
InviteAgentTests_LIBPATH = Tests Tests/../../CollectionStore Tests/../../AddressBook Tests/../../../tools/NUnit/bin Tests/../../Agent Tests/../../StoreProvider Tests/../../Identity Tests/../InviteAgent Tests/../../Common Tests/../../stage
InviteAgentTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = InviteAgent.dll InviteAgentConsole.exe InviteAgentGui.exe EmailInviteAgent.dll InviteAgentTests.dll 
CLEAN_FILES = Check.xml InviteAgent.pdb InviteAgent.Doc.xml Check.xml InviteAgentConsole.pdb InviteAgentConsole.Doc.xml Check.xml InviteAgentGui.pdb InviteAgentGui.Doc.xml Check.xml EmailInviteAgent.pdb EmailInviteAgent.Doc.xml Check.xml InviteAgentTests.pdb InviteAgentTests.Doc.xml  InviteAgent.xml InviteAgent.Doc.xml
CLEAN_DIRS = Library/bin Library/obj ConsoleApp/bin ConsoleApp/obj FormApp/bin FormApp/obj LibraryEmail/bin LibraryEmail/obj Tests/bin Tests/obj 
NUNIT_TESTS = InviteAgentTests.dll 
STAGE_FILES = $(TARGETS) InviteAgent.Doc.xml InviteAgentConsole.Doc.xml InviteAgentGui.Doc.xml EmailInviteAgent.Doc.xml InviteAgentTests.Doc.xml InviteAgent.pdb InviteAgentConsole.pdb InviteAgentGui.pdb EmailInviteAgent.pdb InviteAgentTests.pdb 
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
	$(NUNIT) $(NUNIT_FLAGS) /xml:InviteAgent.Test.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(call CP_IF_EXISTS,$(subst $(STAGE_DIR)$(SEP),,$@),$(STAGE_DIR))

# extra stage files
$(EXTRA_STAGE_FILES):
	$(call CP_IF_EXISTS,$@,$(STAGE_DIR))

#=============================================================================
# InviteAgent targets
#=============================================================================

# fix path seperator
InviteAgent_SOURCES := $(subst /,$(SEP), $(InviteAgent_SOURCES))
InviteAgent_INCLUDES := $(subst /,$(SEP), $(InviteAgent_INCLUDES))
InviteAgent_DEF := $(subst /,$(SEP), $(InviteAgent_DEF))
InviteAgent_LIBPATH := $(subst /,$(SEP), $(InviteAgent_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(InviteAgent_LIBPATH) $(InviteAgent_INCLUDES)

InviteAgent.dll: $(InviteAgent_SOURCES) $(InviteAgent_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:InviteAgent.Doc.xml $(CSCFLAGS) $(InviteAgent_FLAGS) $(InviteAgent_LIBS:%=/r:%) $(InviteAgent_LIBPATH:%=/lib:%) $(InviteAgent_SOURCES)

#=============================================================================
# InviteAgentConsole targets
#=============================================================================

# fix path seperator
InviteAgentConsole_SOURCES := $(subst /,$(SEP), $(InviteAgentConsole_SOURCES))
InviteAgentConsole_INCLUDES := $(subst /,$(SEP), $(InviteAgentConsole_INCLUDES))
InviteAgentConsole_DEF := $(subst /,$(SEP), $(InviteAgentConsole_DEF))
InviteAgentConsole_LIBPATH := $(subst /,$(SEP), $(InviteAgentConsole_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(InviteAgentConsole_LIBPATH) $(InviteAgentConsole_INCLUDES)

InviteAgentConsole.exe: $(InviteAgentConsole_SOURCES) $(InviteAgentConsole_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:InviteAgentConsole.Doc.xml $(CSCFLAGS) $(InviteAgentConsole_FLAGS) $(InviteAgentConsole_LIBS:%=/r:%) $(InviteAgentConsole_LIBPATH:%=/lib:%) $(InviteAgentConsole_SOURCES)

#=============================================================================
# InviteAgentGui targets
#=============================================================================

# fix path seperator
InviteAgentGui_SOURCES := $(subst /,$(SEP), $(InviteAgentGui_SOURCES))
InviteAgentGui_INCLUDES := $(subst /,$(SEP), $(InviteAgentGui_INCLUDES))
InviteAgentGui_DEF := $(subst /,$(SEP), $(InviteAgentGui_DEF))
InviteAgentGui_LIBPATH := $(subst /,$(SEP), $(InviteAgentGui_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(InviteAgentGui_LIBPATH) $(InviteAgentGui_INCLUDES)

InviteAgentGui.exe: $(InviteAgentGui_SOURCES) $(InviteAgentGui_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:InviteAgentGui.Doc.xml $(CSCFLAGS) $(InviteAgentGui_FLAGS) $(InviteAgentGui_LIBS:%=/r:%) $(InviteAgentGui_LIBPATH:%=/lib:%) $(InviteAgentGui_SOURCES)

#=============================================================================
# EmailInviteAgent targets
#=============================================================================

# fix path seperator
EmailInviteAgent_SOURCES := $(subst /,$(SEP), $(EmailInviteAgent_SOURCES))
EmailInviteAgent_INCLUDES := $(subst /,$(SEP), $(EmailInviteAgent_INCLUDES))
EmailInviteAgent_DEF := $(subst /,$(SEP), $(EmailInviteAgent_DEF))
EmailInviteAgent_LIBPATH := $(subst /,$(SEP), $(EmailInviteAgent_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(EmailInviteAgent_LIBPATH) $(EmailInviteAgent_INCLUDES)

EmailInviteAgent.dll: $(EmailInviteAgent_SOURCES) $(EmailInviteAgent_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:EmailInviteAgent.Doc.xml $(CSCFLAGS) $(EmailInviteAgent_FLAGS) $(EmailInviteAgent_LIBS:%=/r:%) $(EmailInviteAgent_LIBPATH:%=/lib:%) $(EmailInviteAgent_SOURCES)

#=============================================================================
# InviteAgentTests targets
#=============================================================================

# fix path seperator
InviteAgentTests_SOURCES := $(subst /,$(SEP), $(InviteAgentTests_SOURCES))
InviteAgentTests_INCLUDES := $(subst /,$(SEP), $(InviteAgentTests_INCLUDES))
InviteAgentTests_DEF := $(subst /,$(SEP), $(InviteAgentTests_DEF))
InviteAgentTests_LIBPATH := $(subst /,$(SEP), $(InviteAgentTests_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(InviteAgentTests_LIBPATH) $(InviteAgentTests_INCLUDES)

InviteAgentTests.dll: $(InviteAgentTests_SOURCES) $(InviteAgentTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:InviteAgentTests.Doc.xml $(CSCFLAGS) $(InviteAgentTests_FLAGS) $(InviteAgentTests_LIBS:%=/r:%) $(InviteAgentTests_LIBPATH:%=/lib:%) $(InviteAgentTests_SOURCES)

