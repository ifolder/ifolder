# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

mDnsService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/mDnsService.cs 
Service_CSFILES_CSC := $(subst /,$(SEP),$(mDnsService_CSFILES))
Service_INCLUDES = 
Service_RESOURCES =
Service_FLAGS = $(CSC_LIBFLAG)
Service_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll Simias.Service.dll mDnsResponder.dll

if WINDOWS
Service_LIBPATH = /opt/gnome2/lib /usr/lib ../Common ../CollectionEvents ../Service ../../external/mDnsResponder/net/bin
endif

if MONO
Service_LIBPATH = /opt/gnome2/lib /usr/lib ../Common ../CollectionEvents ../Service ../../external/mDnsResponder/mono/bin
endif

CUR_DIR := $(shell pwd)

all: Simias.Service.mDnsService.dll

Simias.Service.mDnsService.dll: $(Service_CSFILES) $(Service_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Service_FLAGS) $(Service_LIBS:%=/r:%) $(Service_LIBPATH:%=/lib:%) $(Service_CSFILES_CSC) /doc:Simias.Service.doc.xml
	
if DEBUG
if WINDOWS
DEBUG_FILES = Simias.Service.mDnsService.pdb
endif
endif

install-exec-local: Simias.Service.mDnsService.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Simias.Service.mDnsService.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Simias.Service.mDnsService.dll $(DEBUG_FILES)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:ServiceTest.test.xml $(NUNIT_TESTS)
	
	
CLEAN_FILES = Simias.Service.mDnsService.dll 

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

