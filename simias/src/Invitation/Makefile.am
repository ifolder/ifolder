# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Invitation_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/InvitationService.cs $(srcdir)/Library/IInvitationProvider.cs
Invitation_CSFILES_CSC := $(subst /,$(SEP),$(Invitation_CSFILES))
Invitation_INCLUDES =
Invitation_RESOURCES =
Invitation_FLAGS = $(CSC_LIBFLAG)
Invitation_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll StoreProvider.dll Common.dll CollectionStore.dll SyncCommon.dll
Invitation_LIBPATH = Library ../CollectionStore $(srcdir)/../../external/SecureChannelSinks ../StoreProvider ../AddressBook ../Common ../SyncCommon

SmtpInvitationProvider_CSFILES = $(srcdir)/SmtpProvider/AssemblyInfo.cs $(srcdir)/SmtpProvider/SmtpInvitationProvider.cs
SmtpInvitationProvider_CSFILES_CSC := $(subst /,$(SEP),$(SmtpInvitationProvider_CSFILES))
SmtpInvitationProvider_INCLUDES =
SmtpInvitationProvider_RESOURCES =
SmtpInvitationProvider_FLAGS = $(CSC_LIBFLAG)
SmtpInvitationProvider_LIBS = $(Invitation_LIBS) $(SYSTEM_XML) Invitation.dll Smtp.dll
SmtpInvitationProvider_LIBPATH = $(Invitation_LIBPATH) SmtpProvider ../AddressBook ../Smtp

NUNIT_TESTS := InvitationTests.dll

InvitationTests_CSFILES = $(srcdir)/Tests/AssemblyInfo.cs $(srcdir)/Tests/InvitationTests.cs
InvitationTests_CSFILES_CSC := $(subst /,$(SEP),$(InvitationTests_CSFILES))
InvitationTests_INCLUDES =
InvitationTests_RESOURCES =
InvitationTests_FLAGS = $(CSC_LIBFLAG)
InvitationTests_LIBS = $(Invitation_LIBS) $(SYSTEM_XML) nunit.framework.dll Invitation.dll
InvitationTests_LIBPATH = $(Invitation_LIBPATH) $(NUNIT_LIBPATH)

EXTRA_DIST = $(Invitation_CSFILES) $(SmtpInvitationProvider_CSFILES) $(InvitationTests_CSFILES)

CUR_DIR := $(shell pwd)

all: Invitation.dll SmtpInvitationProvider.dll

Invitation.dll: $(Invitation_CSFILES) $(Invitation_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Invitation_FLAGS) $(Invitation_LIBS:%=/r:%) $(Invitation_LIBPATH:%=/lib:%) $(Invitation_CSFILES_CSC) /doc:Invitation.doc.xml

SmtpInvitationProvider.dll: $(SmtpInvitationProvider_CSFILES) $(SmtpInvitationProvider_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SmtpInvitationProvider_FLAGS) $(SmtpInvitationProvider_LIBS:%=/r:%) $(SmtpInvitationProvider_LIBPATH:%=/lib:%) $(SmtpInvitationProvider_CSFILES_CSC) /doc:SmtpInvitationProvider.doc.xml

InvitationTests.dll: $(InvitationTests_CSFILES) $(InvitationTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InvitationTests_FLAGS) $(InvitationTests_LIBS:%=/r:%) $(InvitationTests_LIBPATH:%=/lib:%) $(InvitationTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = Invitation.pdb SmtpInvitationProvider.pdb
DEBUG_CHECK_FILES = InvitationTests.pdb
endif
endif

install-exec-local: Invitation.dll SmtpInvitationProvider.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Invitation.dll SmtpInvitationProvider.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Invitation.dll SmtpInvitationProvider.dll InvitationTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:InvitationTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Invitation.dll SmtpInvitationProvider.dll $(NUNIT_TESTS) Invitation.doc.xml SmtpInvitationProvider.doc.xml InvitationTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

