Configuration_CSFILES = $(srcdir)/Configuration.cs
Configuration_CSFILES_CSC := $(subst /,$(SEP),$(Configuration_CSFILES))
Configuration_INCLUDES = 
Configuration_RESOURCES =
Configuration_FLAGS = $(CSC_LIBFLAG)
Configuration_LIBS = System.dll $(SYSTEM_XML)
Configuration_LIBPATH = /opt/gnome2/lib /usr/lib

NUNIT_TESTS := ConfTests.exe

ConfTests_CSFILES = $(srcdir)/ConfTests/ConfTests.cs
ConfTests_CSFILES_CSC := $(subst /,$(SEP),$(ConfTests_CSFILES))
ConfTests_INCLUDES = 
ConfTests_RESOURCES =
ConfTests_FLAGS = $(CSC_EXEFLAG)
ConfTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Configuration.dll
ConfTests_LIBPATH = ConfTests $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(Configuration_CSFILES) $(ConfTests_CSFILES)

CUR_DIR := $(shell pwd)

all: Configuration.dll

Configuration.dll: $(Configuration_CSFILES) $(Configuration_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Configuration_FLAGS) $(Configuration_LIBS:%=/r:%) $(Configuration_LIBPATH:%=/lib:%) $(Configuration_CSFILES_CSC) /doc:Configuration.doc.xml

ConfTests.exe: $(ConfTests_CSFILES) $(ConfTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ConfTests_FLAGS) $(ConfTests_LIBS:%=/r:%) $(ConfTests_LIBPATH:%=/lib:%) $(ConfTests_CSFILES_CSC)
	
install-exec-local: Configuration.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Configuration.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Configuration.dll ConfTests.test.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:ConfTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Configuration.dll $(NUNIT_TESTS) Configuration.doc.xml ConfTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

