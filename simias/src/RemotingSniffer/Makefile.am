# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

RemotingSnifferLibrary_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/SnifferClientChannelSink.cs $(srcdir)/Library/SnifferClientChannelSinkProvider.cs $(srcdir)/Library/SnifferServerChannelSink.cs $(srcdir)/Library/SnifferServerChannelSinkProvider.cs $(srcdir)/Library/SnifferMessage.cs
RemotingSnifferLibrary_CSFILES_CSC := $(subst /,$(SEP),$(RemotingSnifferLibrary_CSFILES))
RemotingSnifferLibrary_INCLUDES =
RemotingSnifferLibrary_RESOURCES =
RemotingSnifferLibrary_FLAGS = $(CSC_LIBFLAG)
RemotingSnifferLibrary_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Runtime.Remoting.dll Common.dll
RemotingSnifferLibrary_LIBPATH = Library ../Common

NUNIT_TESTS := 

EXTRA_DIST = $(RemotingSnifferLibrary_CSFILES)

CUR_DIR := $(shell pwd)

all: RemotingSnifferLibrary.dll

RemotingSnifferLibrary.dll: $(RemotingSnifferLibrary_CSFILES) $(RemotingSnifferLibrary_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(RemotingSnifferLibrary_FLAGS) $(RemotingSnifferLibrary_LIBS:%=/r:%) $(RemotingSnifferLibrary_LIBPATH:%=/lib:%) $(RemotingSnifferLibrary_CSFILES_CSC) /doc:RemotingSnifferLibrary.doc.xml

install-exec-local: RemotingSnifferLibrary.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) RemotingSnifferLibrary.dll $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f RemotingSnifferLibrary.dll RemotingSnifferLibraryTests.test.xml $(NUNIT_TESTS)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:RemotingSnifferLibraryTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = RemotingSnifferLibrary.dll $(NUNIT_TESTS) RemotingSnifferLibrary.doc.xml RemotingSnifferLibraryTests.test.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

