# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

InvitationWizard_CSFILES = $(srcdir)/AcceptDeclinePage.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/BaseWizardPage.cs $(srcdir)/CompletionPage.cs $(srcdir)/InteriorPageTemplate.cs $(srcdir)/InvitationWizard.cs $(srcdir)/SelectiFolderLocationPage.cs $(srcdir)/SelectInvitationPage.cs $(srcdir)/WelcomePage.cs $(srcdir)/WelcomePageTemplate.cs
InvitationWizard_CSFILES_CSC := $(subst /,$(SEP),$(InvitationWizard_CSFILES))
InvitationWizard_INCLUDES =
InvitationWizard_RESOURCES = $(srcdir)/App$(ICON_EXT)
InvitationWizard_FLAGS = $(CSC_WINEXEFLAG)
InvitationWizard_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) Novell.iFolder.dll
InvitationWizard_LIBPATH = ../iFolder

DATA_FILES = $(srcdir)/App.ico $(srcdir)/Invitation.ico $(srcdir)/InvitationWizard.exe.manifest
RESX_FILES = $(srcdir)/AcceptDeclinePage.resx $(srcdir)/BaseWizardPage.resx $(srcdir)/CompletionPage.resx $(srcdir)/InteriorPageTemplate.resx $(srcdir)/InvitationWizard.resx $(srcdir)/SelectInvitationPage.resx $(srcdir)/SelectiFolderLocationPage.resx $(srcdir)/WelcomePage.resx $(srcdir)/WelcomePageTemplate.resx

PROJECT_FILES = $(srcdir)/InvitationWizard.csproj $(srcdir)/FormsInvitationWizard.sln

EXTRA_DIST = $(InvitationWizard_CSFILES) $(DATA_FILES) $(RESX_FILES) $(PROJECT_FILES) AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: InvitationWizard.exe

InvitationWizard.exe: $(InvitationWizard_CSFILES) $(InvitationWizard_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InvitationWizard_FLAGS) $(SIMIAS_LIBS) $(InvitationWizard_LIBS:%=/r:%) $(InvitationWizard_LIBPATH:%=/lib:%) $(InvitationWizard_RESOURCES:%=$(ICON_FLAG)%) $(InvitationWizard_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = InvitationWizard.pdb
endif
endif

install-exec-local: InvitationWizard.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) InvitationWizard.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(DATA_FILES) $(DESTDIR)$(bindir)
if WINDOWS
if DEBUG	
	$(INSTALL_PROGRAM) $(PROJECT_FILES) $(DESTDIR)$(bindir)
endif
endif

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f InvitationWizard.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f $(DATA_FILES:$(srcdir)/%=%)
if WINDOWS
if DEBUG	
	cd $(DESTDIR)$(bindir); rm -f $(PROJECT_FILES:$(srcdir)/%=%)
endif
endif

CLEAN_FILES = InvitationWizard.exe $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

