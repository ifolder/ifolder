# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SUBDIRS = 

GaimDomain_CSFILES = $(srcdir)/GaimService.cs $(srcdir)/GaimDomain.cs $(srcdir)/GaimDomainInfo.cs $(srcdir)/GaimDomainService.asmx.cs $(srcdir)/LocationProvider.cs $(srcdir)/GaimDomainProvider.cs $(srcdir)/GaimObjects.cs
GaimDomain_CSFILES_CSC := $(subst /,$(SEP),$(GaimDomain_CSFILES))
GaimDomain_INCLUDES =
GaimDomain_RESOURCES =
GaimDomain_FLAGS = $(CSC_LIBFLAG)
GaimDomain_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll Simias.dll SimiasClient.dll Novell.Security.ClientPasswordManager.NetCredential.dll
GaimDomain_LIBPATH = ../../src/Simias.dll ../../src/SimiasClient $(DESTDIR)$(webbindir) $(NOVELL_SECURITY_DIR)

EXTRA_DIST = $(GaimDomain_CSFILES) $(srcdir)/GaimDomainService.wsdl $(srcdir)/GaimDomainService.asmx

NUNIT_TESTS := Simias.Gaim.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

Simias.Gaim.dll: $(GaimDomain_CSFILES) $(GaimDomain_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(GaimDomain_FLAGS) $(GaimDomain_LIBS:%=/r:%) $(GaimDomain_LIBPATH:%=/lib:%) $(GaimDomain_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.Gaim.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) GaimDomainService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)
	

uninstall-local:
	cd $(DESTDIR)$(webbindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SimiasTests.test.xml $(NUNIT_TESTS)
	cd $(DESTDIR)$(webdir); rm -f  GaimDomainService.asmx

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) Simias.GaimDomain.doc.xml 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
#distclean-local:
#	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

