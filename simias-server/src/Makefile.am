# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SimiasServer_CSFILES = $(srcdir)/Service.cs $(srcdir)/Ajax.cs $(srcdir)/Domain.cs $(srcdir)/DomainInfo.cs $(srcdir)/ProvisionInfo.cs $(srcdir)/DomainService.asmx.cs $(srcdir)/Authentication.cs $(srcdir)/Register.aspx.cs
SimiasServer_CSFILES_CSC := $(subst /,$(SEP),$(SimiasServer_CSFILES))
SimiasServer_INCLUDES =
SimiasServer_RESOURCES =
SimiasServer_FLAGS = $(CSC_LIBFLAG)
SimiasServer_LIBPATH = $(DESTDIR)$(webbindir) ../../simias/src/SimiasLib.dll ../../simias/src/SimiasClient
SimiasServer_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll SimiasLib.dll SimiasClient.dll
#SimiasServer_LIBPATH = ../../src/SimiasLib.dll ../../src/SimiasClient $(DESTDIR)$(webbindir)

EXTRA_DIST = DomainService.asmx Register.aspx $(SimiasServer_CSFILES) Simias.config.in Simias.Server.conf simiasserver.in simiasserver.cmd.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

SERVER_ASSEMBLIES := Simias.Server.dll

simiasserver: $(srcdir)/simiasserver.in
if !WINDOWS
	sed -e 's,@_simiasdatadir_@,@simiasdatadir@,g' -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif	

simiasserver.cmd: $(srcdir)/simiasserver.cmd.in
if WINDOWS
	sed -e 's,@_simiasdatadir_@,$(shell cygpath --mixed $(simiasdatadir)),g' $< > $@
	chmod +x $@
endif
	
Simias.config: $(srcdir)/Simias.config.in
	sed -e 's,@_StoreProviderAssembly_@,@StoreProviderAssembly@,g' -e 's,@_StoreProviderType_@,@StoreProviderType@,g' $< > $@
	chmod +x $@

Simias.Server.dll: $(SimiasServer_CSFILES) $(SimiasServer_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasServer_FLAGS) $(SimiasServer_LIBPATH:%=/lib:%) $(SimiasServer_LIBS:%=/r:%) $(SimiasServer_RESOURCES:%=$(ICON_FLAG)%) $(SimiasServer_CSFILES_CSC) $(DOC_FILE_CMD)
	
all: $(SERVER_ASSEMBLIES) Simias.config simiasserver simiasserver.cmd

if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = Simias.Server.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(simiasdatadir) $(simiasdatadir)/modules
	$(INSTALL_PROGRAM) Simias.config $(simiasdatadir)
	$(INSTALL_PROGRAM) DomainService.asmx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) Register.aspx $(DEBUG_FILES) $(DESTDIR)$(webdir)
	$(INSTALL_PROGRAM) $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.Server.conf $(simiasdatadir)/modules
if WINDOWS	
	$(INSTALL_PROGRAM) simiasserver.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simiasserver $(DESTDIR)$(bindir)
endif	

uninstall-local:
	cd $(DESTDIR)$(webdir); rm -f DomainService.asmx
	cd $(DESTDIR)$(webdir); rm -f Register.aspx
	cd $(DESTDIR)$(webbindir); rm -f $(SERVER_ASSEMBLIES) $(DEBUG_CHECK_FILES)
	cd $(simiasdatadir); rm -f Simias.config
	cd $(simiasdatadir)/modules; rm -f Simias.Server.conf
if WINDOWS	
	cd $(DESTDIR)$(bindir); rm -f simiasserver.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simiasserver
endif

installcheck-local: install
		
.PHONY: test
test: installcheck

CLEAN_FILES = $(SERVER_ASSEMBLIES) $(DOC_FILE_CLEAN) Simias.config simiasserver simiasserver.cmd

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

#distclean-local:
#	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

