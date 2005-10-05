# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimpleServer_CSFILES = $(srcdir)/Service.cs $(srcdir)/SimpleDomain.cs $(srcdir)/DomainInfo.cs $(srcdir)/ProvisionInfo.cs $(srcdir)/DomainService.asmx.cs $(srcdir)/SimpleServerAuthentication.cs
SimpleServer_CSFILES_CSC := $(subst /,$(SEP),$(SimpleServer_CSFILES))
SimpleServer_INCLUDES =
SimpleServer_RESOURCES =
SimpleServer_FLAGS = $(CSC_LIBFLAG)
SimpleServer_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll
SimpleServer_LIBPATH = ../../src/SimiasLib.dll ../../src/SimiasClient $(DESTDIR)$(webbindir)

EXTRA_DIST = DomainService.asmx $(SimpleServer_CSFILES) SimpleServer.xml Simias.config.in Simias.SimpleServer.conf simpleserver.in simpleserver.cmd.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

NUNIT_TESTS := Simias.SimpleServer.dll

simpleserver: simpleserver.in
	sed -e 's,@_simiasdatadir_@,@simiasdatadir@,g' -e 's,@_webbindir_@,@webbindir@,g' $< > simpleserver
	chmod +x $@

simpleserver.cmd: simpleserver.cmd.in
if WINDOWS
	sed -e 's,@_simiasdatadir_@,$(shell cygpath --mixed $(simiasdatadir)),g' $< > simpleserver.cmd
	chmod +x $@
endif
	
Simias.config: Simias.config.in
	sed -e 's,@_StoreProviderAssembly_@,@StoreProviderAssembly@,g' -e 's,@_StoreProviderType_@,@StoreProviderType@,g' $< > Simias.config
	chmod +x $@

all: $(NUNIT_TESTS) Simias.config simpleserver simpleserver.cmd

Simias.SimpleServer.dll: $(SimpleServer_CSFILES) $(SimpleServer_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimpleServer_FLAGS) $(SimpleServer_LIBS:%=/r:%) $(SimpleServer_LIBPATH:%=/lib:%) $(SimpleServer_RESOURCES:%=$(ICON_FLAG)%) $(SimpleServer_CSFILES_CSC) $(DOC_FILE_CMD)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.SimpleServer.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(simiasdatadir) $(simiasdatadir)/modules
	$(INSTALL_PROGRAM) SimpleServer.xml $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) Simias.config $(simiasdatadir)
	$(INSTALL_PROGRAM) DomainService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.SimpleServer.conf $(simiasdatadir)/modules
if WINDOWS	
	$(INSTALL_PROGRAM) simpleserver.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simpleserver $(DESTDIR)$(bindir)
endif	

uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f DomainService.asmx
	cd $(DESTDIR)$(sysconfdir); rm -f SimpleServer.xml
	cd $(DESTDIR)$(webbindir); rm -f $(NUNIT_TESTS) $(DEBUG_CHECK_FILES)
	cd $(simiasdatadir); rm -f Simias.config
	cd $(simiasdatadir)/modules; rm -f Simias.SimpleServer.conf
if WINDOWS	
	cd $(DESTDIR)$(bindir); rm -f simpleserver.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simpleserver
endif

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

