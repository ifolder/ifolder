InvitationWizard_CSFILES = $(srcdir)/AcceptDeclinePage.cs $(srcdir)/AssemblyInfo.cs $(srcdir)/BaseWizardPage.cs $(srcdir)/CompletionPage.cs $(srcdir)/InteriorPageTemplate.cs $(srcdir)/InvitationWizard.cs $(srcdir)/SelectiFolderLocationPage.cs $(srcdir)/SelectInvitationPage.cs $(srcdir)/WelcomePage.cs $(srcdir)/WelcomePageTemplate.cs
InvitationWizard_CSFILES_CSC := $(subst /,$(SEP),$(InvitationWizard_CSFILES))
InvitationWizard_INCLUDES = 
InvitationWizard_RESOURCES = $(srcdir)/App$(ICON_EXT)
InvitationWizard_FLAGS = $(CSC_WINEXEFLAG)
InvitationWizard_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) iFolder.dll Agent.dll
InvitationWizard_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin ../iFolder

DATA_FILES = App.ico Invitation.ico invitewiz.png

EXTRA_DIST = $(InvitationWizard_CSFILES) $(DATA_FILES)

all: InvitationWizard.exe

InvitationWizard.exe: $(InvitationWizard_CSFILES) $(InvitationWizard_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InvitationWizard_FLAGS) $(InvitationWizard_LIBS:%=/r:%) $(InvitationWizard_LIBPATH:%=/lib:%) $(InvitationWizard_RESOURCES:%=$(ICON_FLAG)%) $(InvitationWizard_CSFILES_CSC) /doc:InvitationWizard.doc.xml

install-exec-local: InvitationWizard.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) InvitationWizard.exe $(DATA_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f InvitationWizard.exe $(DATA_FILES)
    
installcheck-local: install
	
CLEAN_FILES = InvitationWizard.exe InvitationWizard.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

