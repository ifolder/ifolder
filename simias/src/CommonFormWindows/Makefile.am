# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

CommonFormWindows_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/MyTraceForm.cs $(srcdir)/Library/MyTraceFormListener.cs
CommonFormWindows_CSFILES_CSC := $(subst /,$(SEP),$(CommonFormWindows_CSFILES))
CommonFormWindows_INCLUDES = 
CommonFormWindows_RESOURCES =
CommonFormWindows_FLAGS = $(CSC_LIBFLAG)
CommonFormWindows_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll System.Windows.Forms.dll Common.dll
CommonFormWindows_LIBPATH = Library ../Common

NUNIT_TESTS := CommonFormWindowsTests.dll

CommonFormWindowsTests_CSFILES = $(srcdir)/Tests/AssemblyInfo.cs $(srcdir)/Tests/CommonFormWindowsTests.cs
CommonFormWindowsTests_CSFILES_CSC := $(subst /,$(SEP),$(CommonFormWindowsTests_CSFILES))
CommonFormWindowsTests_INCLUDES = 
CommonFormWindowsTests_RESOURCES =
CommonFormWindowsTests_FLAGS = $(CSC_LIBFLAG)
CommonFormWindowsTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll Common.dll CommonFormWindows.dll System.Windows.Forms.dll
CommonFormWindowsTests_LIBPATH = Tests $(srcdir)/../../tools/NUnit/bin ../Common Library

EXTRA_DIST = $(CommonFormWindows_CSFILES) $(CommonFormWindowsTests_CSFILES)

CUR_DIR := $(shell pwd)

all: CommonFormWindows.dll

CommonFormWindows.dll: $(CommonFormWindows_CSFILES) $(CommonFormWindows_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CommonFormWindows_FLAGS) $(CommonFormWindows_LIBS:%=/r:%) $(CommonFormWindows_LIBPATH:%=/lib:%) $(CommonFormWindows_CSFILES_CSC) /doc:CommonFormWindows.doc.xml

CommonFormWindowsTests.dll: $(CommonFormWindowsTests_CSFILES) $(CommonFormWindowsTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CommonFormWindowsTests_FLAGS) $(CommonFormWindowsTests_LIBS:%=/r:%) $(CommonFormWindowsTests_LIBPATH:%=/lib:%) $(CommonFormWindowsTests_CSFILES_CSC)
	
install-exec-local: CommonFormWindows.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) CommonFormWindows.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f CommonFormWindows.dll CommonFormWindowsTests.test.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:CommonFormWindowsTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = CommonFormWindows.dll $(NUNIT_TESTS) CommonFormWindows.doc.xml CommonFormWindowsTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

