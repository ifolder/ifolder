# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimpleServer_CSFILES = $(srcdir)/Service.cs $(srcdir)/SimpleDomain.cs $(srcdir)/DomainInfo.cs $(srcdir)/ProvisionInfo.cs $(srcdir)/DomainService.asmx.cs $(srcdir)/SimpleServerAuthentication.cs
SimpleServer_CSFILES_CSC := $(subst /,$(SEP),$(SimpleServer_CSFILES))
SimpleServer_INCLUDES =
SimpleServer_RESOURCES =
SimpleServer_FLAGS = $(CSC_LIBFLAG)
SimpleServer_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll
SimpleServer_LIBPATH = ../../src/SimiasLib.dll ../../src/SimiasClient $(DESTDIR)$(webbindir)

EXTRA_DIST = win.web.config mono.web.config DomainService.asmx $(SimpleServer_CSFILES) SimpleServer.xml simias-server-bootstrap.config.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

NUNIT_TESTS := Simias.SimpleServer.dll

if WINDOWS
	FIXED_WEBDIR := $(shell cygpath --mixed $(webdir))
else
	FIXED_WEBDIR := $(webdir)
endif

simias-server-bootstrap.config: simias-server-bootstrap.config.in
	sed -e 's,@_webdir_@,$(FIXED_WEBDIR),g' -e 's,@_StoreProviderAssembly_@,@StoreProviderAssembly@,g' -e 's,@_StoreProviderType_@,@StoreProviderType@,g' $< > simias-server-bootstrap.config
	chmod +x $@

all: $(NUNIT_TESTS) simias-server-bootstrap.config

Simias.SimpleServer.dll: $(SimpleServer_CSFILES) $(SimpleServer_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimpleServer_FLAGS) $(SimpleServer_LIBS:%=/r:%) $(SimpleServer_LIBPATH:%=/lib:%) $(SimpleServer_RESOURCES:%=$(ICON_FLAG)%) $(SimpleServer_CSFILES_CSC) $(DOC_FILE_CMD)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.SimpleServer.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SimpleServer.xml $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) simias-server-bootstrap.config $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) DomainService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)
if WINDOWS
	$(INSTALL_PROGRAM) win.web.config $(DESTDIR)$(webdir)/web.config
else
	$(INSTALL_PROGRAM) mono.web.config $(DESTDIR)$(webdir)/server.web.config
endif	
	

uninstall-local:
	cd $(DESTDIR)$(webbindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SimiasTests.test.xml $(NUNIT_TESTS)
	cd $(DESTDIR)$(webdir); rm -f  web.config DomainService.asmx

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

