#=============================================================================
# Module:	sln2mk Generated Makefile
#
# Source:	FormsInvitationWizard.sln
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# InvitationWizard variables
#=============================================================================
InvitationWizard_SOURCES = AcceptDeclinePage.cs AssemblyInfo.cs BaseWizardPage.cs CompletionPage.cs InteriorPageTemplate.cs InvitationWizard.cs SelectiFolderLocationPage.cs SelectInvitationPage.cs WelcomePage.cs WelcomePageTemplate.cs
InvitationWizard_INCLUDES = 
InvitationWizard_FLAGS = /t:winexe $(ICON_FLAG)App$(ICON_EXT)
InvitationWizard_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) iFolder.dll Agent.dll
InvitationWizard_LIBPATH = $(SIMIAS_ROOT)/src/stage ../iFolder
InvitationWizard_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = InvitationWizard.exe 
CLEAN_FILES = Check.xml InvitationWizard.pdb InvitationWizard.Doc.xml  FormsInvitationWizard.xml FormsInvitationWizard.Doc.xml
CLEAN_DIRS = bin obj 
NUNIT_TESTS = 
STAGE_FILES = $(TARGETS) InvitationWizard.Doc.xml InvitationWizard.pdb 
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
# InvitationWizard targets
#=============================================================================

# fix path seperator
InvitationWizard_SOURCES := $(subst /,$(SEP), $(InvitationWizard_SOURCES))
InvitationWizard_INCLUDES := $(subst /,$(SEP), $(InvitationWizard_INCLUDES))
InvitationWizard_DEF := $(subst /,$(SEP), $(InvitationWizard_DEF))
InvitationWizard_LIBPATH := $(subst /,$(SEP), $(InvitationWizard_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(InvitationWizard_LIBPATH) $(InvitationWizard_INCLUDES)

InvitationWizard.exe: $(InvitationWizard_SOURCES) $(InvitationWizard_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:InvitationWizard.Doc.xml $(CSCFLAGS) $(InvitationWizard_FLAGS) $(InvitationWizard_LIBS:%=/r:%) $(InvitationWizard_LIBPATH:%=/lib:%) $(InvitationWizard_SOURCES)

