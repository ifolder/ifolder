InviteAgent_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/InviteAgent.cs
InviteAgent_CSFILES_CSC := $(subst /,$(SEP),$(InviteAgent_CSFILES))
InviteAgent_INCLUDES =
InviteAgent_RESOURCES =
InviteAgent_FLAGS = $(CSC_LIBFLAG)
InviteAgent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll Configuration.dll StoreProvider.dll Common.dll CollectionEvents.dll CollectionStore.dll Agent.dll AddressBook.dll Common.dll SyncCommon.dll
InviteAgent_LIBPATH = Library ../CollectionStore  ../../external/SecureChannelSinks ../StoreProvider ../Agent ../AddressBook ../Common ../SyncCommon ../Configuration ../Common ../CollectionEvents

EmailInviteAgent_CSFILES = $(srcdir)/LibraryEmail/AssemblyInfo.cs $(srcdir)/LibraryEmail/EmailInviteAgent.cs
EmailInviteAgent_CSFILES_CSC := $(subst /,$(SEP),$(EmailInviteAgent_CSFILES))
EmailInviteAgent_INCLUDES =
EmailInviteAgent_RESOURCES =
EmailInviteAgent_FLAGS = $(CSC_LIBFLAG)
EmailInviteAgent_LIBS = $(InviteAgent_LIBS) $(SYSTEM_XML) InviteAgent.dll Smtp.dll
EmailInviteAgent_LIBPATH = $(InviteAgent_LIBPATH) LibraryEmail ../AddressBook ../Smtp

NUNIT_TESTS := InviteAgentTests.dll

InviteAgentTests_CSFILES = $(srcdir)/Tests/AssemblyInfo.cs $(srcdir)/Tests/InviteAgentTests.cs
InviteAgentTests_CSFILES_CSC := $(subst /,$(SEP),$(InviteAgentTests_CSFILES))
InviteAgentTests_INCLUDES =
InviteAgentTests_RESOURCES =
InviteAgentTests_FLAGS = $(CSC_LIBFLAG)
InviteAgentTests_LIBS = $(InviteAgent_LIBS) $(SYSTEM_XML) nunit.framework.dll
InviteAgentTests_LIBPATH = $(InviteAgent_LIBPATH) $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(InviteAgent_CSFILES) $(EmailInviteAgent_CSFILES) $(InviteAgentTests_CSFILES)

CUR_DIR := $(shell pwd)

all: InviteAgent.dll EmailInviteAgent.dll

InviteAgent.dll: $(InviteAgent_CSFILES) $(InviteAgent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAgent_FLAGS) $(InviteAgent_LIBS:%=/r:%) $(InviteAgent_LIBPATH:%=/lib:%) $(InviteAgent_CSFILES_CSC) /doc:InviteAgent.doc.xml

EmailInviteAgent.dll: $(EmailInviteAgent_CSFILES) $(EmailInviteAgent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(EmailInviteAgent_FLAGS) $(EmailInviteAgent_LIBS:%=/r:%) $(EmailInviteAgent_LIBPATH:%=/lib:%) $(EmailInviteAgent_CSFILES_CSC) /doc:EmailInviteAgent.doc.xml

InviteAgentTests.dll: $(InviteAgentTests_CSFILES) $(InviteAgentTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAgentTests_FLAGS) $(InviteAgentTests_LIBS:%=/r:%) $(InviteAgentTests_LIBPATH:%=/lib:%) $(InviteAgentTests_CSFILES_CSC)
	
install-exec-local: InviteAgent.dll EmailInviteAgent.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) InviteAgent.dll EmailInviteAgent.dll $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f InviteAgent.dll EmailInviteAgent.dll InviteAgentTests.test.xml $(NUNIT_TESTS)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:InviteAgentTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = InviteAgent.dll EmailInviteAgent.dll $(NUNIT_TESTS) InviteAgent.doc.xml EmailInviteAgent.doc.xml InviteAgentTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

