InviteAgent_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/InviteAgent.cs
InviteAgent_CSFILES_CSC := $(subst /,$(SEP),$(InviteAgent_CSFILES))
InviteAgent_INCLUDES =
InviteAgent_RESOURCES =
InviteAgent_FLAGS = $(CSC_LIBFLAG)
InviteAgent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll Configuration.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll Agent.dll AddressBook.dll Common.dll SyncCommon.dll
InviteAgent_LIBPATH = Library ../CollectionStore ../StoreProvider ../Agent ../AddressBook ../Common ../SyncCommon ../Configuration ../CollectionEvents

InviteAgentConsole_CSFILES = $(srcdir)/ConsoleApp/AssemblyInfo.cs $(srcdir)/ConsoleApp/MainClass.cs
InviteAgentConsole_CSFILES_CSC := $(subst /,$(SEP),$(InviteAgentConsole_CSFILES))
InviteAgentConsole_INCLUDES =
InviteAgentConsole_RESOURCES = $(srcdir)/ConsoleApp/App$(ICON_EXT)
InviteAgentConsole_FLAGS = $(CSC_EXEFLAG)
InviteAgentConsole_LIBS = $(InviteAgent_LIBS) $(SYSTEM_XML) InviteAgent.dll
InviteAgentConsole_LIBPATH = $(InviteAgent_LIBPATH)

InviteAgentGui_CSFILES = $(srcdir)/FormApp/AssemblyInfo.cs $(srcdir)/FormApp/MainForm.cs
InviteAgentGui_CSFILES_CSC := $(subst /,$(SEP),$(InviteAgentGui_CSFILES))
InviteAgentGui_INCLUDES =
InviteAgentGui_RESOURCES = $(srcdir)/FormApp/App$(ICON_EXT)
InviteAgentGui_FLAGS = $(CSC_EXEFLAG)
InviteAgentGui_LIBS = $(InviteAgent_LIBS) System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) InviteAgent.dll
InviteAgentGui_LIBPATH = $(InviteAgent_LIBPATH)

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

EXTRA_DIST = $(InviteAgent_CSFILES) $(InviteAgentConsole_CSFILES) $(InviteAgentGui_CSFILES) $(EmailInviteAgent_CSFILES) $(InviteAgentTests_CSFILES) $(InviteAgentConsole_RESOURCES) $(InviteAgentGui_RESOURCES)

CUR_DIR := $(shell pwd)

all: InviteAgent.dll EmailInviteAgent.dll InviteAgentConsole.exe InviteAgentGui.exe

InviteAgent.dll: $(InviteAgent_CSFILES) $(InviteAgent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAgent_FLAGS) $(InviteAgent_LIBS:%=/r:%) $(InviteAgent_LIBPATH:%=/lib:%) $(InviteAgent_CSFILES_CSC) /doc:InviteAgent.xml

EmailInviteAgent.dll: $(EmailInviteAgent_CSFILES) $(EmailInviteAgent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(EmailInviteAgent_FLAGS) $(EmailInviteAgent_LIBS:%=/r:%) $(EmailInviteAgent_LIBPATH:%=/lib:%) $(EmailInviteAgent_CSFILES_CSC) /doc:EmailInviteAgent.xml

InviteAgentConsole.exe: $(InviteAgentConsole_CSFILES) $(InviteAgentConsole_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAgentConsole_FLAGS) $(InviteAgentConsole_LIBS:%=/r:%) $(InviteAgentConsole_LIBPATH:%=/lib:%) $(InviteAgentConsole_RESOURCES:%=$(ICON_FLAG)%) $(InviteAgentConsole_CSFILES_CSC) /doc:InviteAgentConsole.xml

InviteAgentGui.exe: $(InviteAgentGui_CSFILES) $(InviteAgentGui_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAgentGui_FLAGS) $(InviteAgentGui_LIBS:%=/r:%) $(InviteAgentGui_LIBPATH:%=/lib:%) $(InviteAgentGui_RESOURCES:%=$(ICON_FLAG)%) $(InviteAgentGui_CSFILES_CSC) /doc:InviteAgentGui.xml

InviteAgentTests.dll: $(InviteAgentTests_CSFILES) $(InviteAgentTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAgentTests_FLAGS) $(InviteAgentTests_LIBS:%=/r:%) $(InviteAgentTests_LIBPATH:%=/lib:%) $(InviteAgentTests_CSFILES_CSC)
	
install-exec-local: InviteAgent.dll EmailInviteAgent.dll InviteAgentConsole.exe InviteAgentGui.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) InviteAgent.dll EmailInviteAgent.dll InviteAgentConsole.exe InviteAgentGui.exe $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f InviteAgent.dll EmailInviteAgent.dll InviteAgentConsole.exe InviteAgentGui.exe InviteAgentTests.xml $(NUNIT_TESTS)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:InviteAgentTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = InviteAgent.dll EmailInviteAgent.dll InviteAgentConsole.exe InviteAgentGui.exe $(NUNIT_TESTS) InviteAgent.xml EmailInviteAgent.xml InviteAgentConsole.xml InviteAgentGui.xml InviteAgentTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

