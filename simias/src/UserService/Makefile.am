UserService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/UserService.cs
UserService_CSFILES_CSC := $(subst /,$(SEP),$(UserService_CSFILES))
UserService_INCLUDES = 
UserService_RESOURCES =
UserService_FLAGS = $(CSC_LIBFLAG)
UserService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Configuration.dll Common.dll CollectionEvents.dll
UserService_LIBPATH = /opt/gnome2/lib /usr/lib ../Configuration ../Common ../CollectionEvents


EXTRA_DIST = $(UserService_CSFILES) 

CUR_DIR := $(shell pwd)

all: UserService.dll

UserService.dll: $(UserService_CSFILES) $(UserService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(UserService_FLAGS) $(UserService_LIBS:%=/r:%) $(UserService_LIBPATH:%=/lib:%) $(UserService_CSFILES_CSC) /doc:UserService.doc.xml

install-exec-local: UserService.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) UserService.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f UserService.dll $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	
CLEAN_FILES = UserService.dll $(NUNIT_TESTS) UserService.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

