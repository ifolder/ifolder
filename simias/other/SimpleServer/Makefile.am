# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimpleServer_CSFILES = $(srcdir)/Service.cs $(srcdir)/SimpleDomain.cs $(srcdir)/Authentication.cs
SimpleServer_CSFILES_CSC := $(subst /,$(SEP),$(SimpleServer_CSFILES))
SimpleServer_INCLUDES =
SimpleServer_RESOURCES =
SimpleServer_FLAGS = $(CSC_LIBFLAG)
SimpleServer_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll Simias.dll SimiasClient.dll Novell.Security.Web.IAuthenticationService.dll Novell.AddressBook.dll
SimpleServer_LIBPATH = ./ ../../src/Simias.dll ../../src/SimiasClient $(DESTDIR)$(webbindir)

EXTRA_DIST = $(SimpleServer_CSFILES)

NUNIT_TESTS := Simias.SimpleServer.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

Simias.SimpleServer.dll: $(SimpleServer_CSFILES) $(SimpleServer_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimpleServer_FLAGS) $(SimpleServer_LIBS:%=/r:%) $(SimpleServer_LIBPATH:%=/lib:%) $(SimpleServer_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.SimpleServer.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(webbindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SimiasTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) Simias.SimpleServer.doc.xml 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

