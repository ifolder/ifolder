Common_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/MyDns.cs $(srcdir)/Library/MyEnvironment.cs $(srcdir)/Library/MyPath.cs $(srcdir)/Library/MyPlatformID.cs $(srcdir)/Library/MyRuntimeID.cs $(srcdir)/Library/MyTrace.cs
Common_CSFILES_CSC := $(subst /,$(SEP),$(Common_CSFILES))
Common_INCLUDES = 
Common_RESOURCES =
Common_FLAGS = $(CSC_LIBFLAG)
Common_LIBS = System.dll System.Data.dll $(SYSTEM_XML)
Common_LIBPATH = /opt/gnome2/lib /usr/lib

NUNIT_TESTS := CommonTests.dll

CommonTests_CSFILES = $(srcdir)/Tests/AssemblyInfo.cs $(srcdir)/Tests/CommonTests.cs
CommonTests_CSFILES_CSC := $(subst /,$(SEP),$(CommonTests_CSFILES))
CommonTests_INCLUDES = 
CommonTests_RESOURCES =
CommonTests_FLAGS = $(CSC_LIBFLAG)
CommonTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Common.dll
CommonTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(Common_CSFILES) $(CommonTests_CSFILES)

CUR_DIR := $(shell pwd)

all: Common.dll

Common.dll: $(Common_CSFILES) $(Common_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Common_FLAGS) $(Common_LIBS:%=/r:%) $(Common_LIBPATH:%=/lib:%) $(Common_CSFILES_CSC) /doc:Common.xml

CommonTests.dll: $(CommonTests_CSFILES) $(CommonTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CommonTests_FLAGS) $(CommonTests_LIBS:%=/r:%) $(CommonTests_LIBPATH:%=/lib:%) $(CommonTests_CSFILES_CSC)
	
install-exec-local: Common.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Common.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Common.dll CommonTests.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:CommonTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Common.dll $(NUNIT_TESTS) Common.xml CommonTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

