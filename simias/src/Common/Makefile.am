# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Common_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/MyDns.cs $(srcdir)/Library/MyEnvironment.cs $(srcdir)/Library/MyPath.cs $(srcdir)/Library/MyPlatformID.cs $(srcdir)/Library/MyRuntimeID.cs $(srcdir)/Library/MyTrace.cs $(srcdir)/Library/Configuration.cs $(srcdir)/Library/ISimiasLog.cs $(srcdir)/Library/SimiasLog.cs $(srcdir)/Library/SimiasLogManager.cs
Common_CSFILES_CSC := $(subst /,$(SEP),$(Common_CSFILES))
Common_INCLUDES = 
Common_RESOURCES =
Common_FLAGS = $(CSC_LIBFLAG)
Common_LIBS = System.dll System.Data.dll $(SYSTEM_XML) log4net.dll
Common_LIBPATH = /opt/gnome2/lib /usr/lib $(LOG4NET_DIR)

NUNIT_TESTS := CommonTests.dll

CommonTests_CSFILES = $(srcdir)/Tests/AssemblyInfo.cs $(srcdir)/Tests/CommonTests.cs $(srcdir)/Tests/ConfigurationTests.cs
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
	$(CSC) /out:$@ $(CSCFLAGS) $(Common_FLAGS) $(Common_LIBS:%=/r:%) $(Common_LIBPATH:%=/lib:%) $(Common_CSFILES_CSC) /doc:Common.doc.xml

CommonTests.dll: $(CommonTests_CSFILES) $(CommonTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CommonTests_FLAGS) $(CommonTests_LIBS:%=/r:%) $(CommonTests_LIBPATH:%=/lib:%) $(CommonTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = Common.pdb
DEBUG_CHECK_FILES = CommonTests.pdb
endif
endif

install-exec-local: Common.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Common.dll $(DEBUG_FILES) $(LOG4NET_DIR)/log4net.dll $(DESTDIR)$(bindir)
	cp $(LOG4NET_DIR)/../../../../LICENSE.txt $(DESTDIR)$(prefix)/log4net.LICENSE.txt
        
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Common.dll CommonTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) log4net.dll $(DEBUG_CHECK_FILES)
	cd $(DESTDIR)$(prefix); rm -f log4net.LICENSE.txt
	
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:CommonTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Common.dll $(NUNIT_TESTS) Common.doc.xml CommonTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

