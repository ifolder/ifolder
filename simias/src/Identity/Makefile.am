Identity_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/IIdentity.cs
Identity_CSFILES_CSC := $(subst /,$(SEP),$(Identity_CSFILES))
Identity_INCLUDES = 
Identity_RESOURCES =
Identity_FLAGS = $(CSC_LIBFLAG)
Identity_LIBS = System.dll System.Data.dll $(SYSTEM_XML)
Identity_LIBPATH =

NUNIT_TESTS := 

IdentityTests_CSFILES = 
IdentityTests_CSFILES_CSC := $(subst /,$(SEP),$(IdentityTests_CSFILES))
IdentityTests_INCLUDES = 
IdentityTests_RESOURCES =
IdentityTests_FLAGS =
IdentityTests_LIBS = 
IdentityTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(Identity_CSFILES) $(IdentityTests_CSFILES)

CUR_DIR := $(shell pwd)

all: Identity.dll

Identity.dll: $(Identity_CSFILES) $(Identity_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Identity_FLAGS) $(Identity_LIBS:%=/r:%) $(Identity_LIBPATH:%=/lib:%) $(Identity_CSFILES_CSC) /doc:Identity.xml

#IdentityTests.exe: $(IdentityTests_CSFILES) $(IdentityTests_RESOURCES)
#	$(CSC) /out:$@ $(CSCFLAGS) $(IdentityTests_FLAGS) $(IdentityTests_LIBS:%=/r:%) $(IdentityTests_LIBPATH:%=/lib:%) $(IdentityTests_CSFILES_CSC)
	
install-exec-local: Identity.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Identity.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Identity.dll IdentityTests.xml $(NUNIT_TESTS)
    
#installcheck-local: install $(NUNIT_TESTS)
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:IdentityTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Identity.dll $(NUNIT_TESTS) Identity.xml IdentityTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

