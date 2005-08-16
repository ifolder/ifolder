# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimpleServer_CSFILES = $(srcdir)/Service.cs $(srcdir)/Domain.cs $(srcdir)/DomainInfo.cs $(srcdir)/ProvisionInfo.cs $(srcdir)/DomainService.asmx.cs $(srcdir)/Authentication.cs $(srcdir)/Ajax.cs $(srcdir)/Register.aspx.cs
SimpleServer_CSFILES_CSC := $(subst /,$(SEP),$(SimpleServer_CSFILES))
SimpleServer_INCLUDES =
SimpleServer_RESOURCES =
SimpleServer_FLAGS = $(CSC_LIBFLAG)
SimpleServer_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll Simias.dll SimiasClient.dll
SimpleServer_LIBPATH = ../../src/Simias.dll ../../src/SimiasClient $(DESTDIR)$(webbindir)

EXTRA_DIST = win.web.config mono.web.config DomainService.asmx Register.css Register.aspx $(SimpleServer_CSFILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

NUNIT_TESTS := Simias.Server.dll

all: $(NUNIT_TESTS)

Simias.Server.dll: $(SimpleServer_CSFILES) $(SimpleServer_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimpleServer_FLAGS) $(SimpleServer_LIBS:%=/r:%) $(SimpleServer_LIBPATH:%=/lib:%) $(SimpleServer_RESOURCES:%=$(ICON_FLAG)%) $(SimpleServer_CSFILES_CSC) $(DOC_FILE_CMD)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.Server.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SimiasServer.xml $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) DomainService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Register.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Register.css $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)
if WINDOWS
	$(INSTALL_PROGRAM) win.web.config $(DESTDIR)$(webdir)/web.config
else
	$(INSTALL_PROGRAM) mono.web.config $(DESTDIR)$(webdir)/web.config
endif	
	

uninstall-local:
	cd $(DESTDIR)$(webbindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SimiasTests.test.xml $(NUNIT_TESTS)
	cd $(DESTDIR)$(webdir); rm -f  web.config DomainService.asmx Register.asmx Register.css

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

#distclean-local:
#	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

