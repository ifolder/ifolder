SUBDIRS = libsimias-event

# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasClient_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/CertPolicy.cs $(srcdir)/CollectionSyncEventArgs.cs $(srcdir)/FileSyncEventArgs.cs $(srcdir)/Configuration.cs $(srcdir)/DomainService.cs $(srcdir)/IProcEventClient.cs $(srcdir)/IProcEventData.cs $(srcdir)/IProcEventListener.cs $(srcdir)/IProcEventRegistration.cs $(srcdir)/IProcEventTypes.cs $(srcdir)/LocalService.cs $(srcdir)/Manager.cs $(srcdir)/MyEnvironment.cs $(srcdir)/MyPath.cs $(srcdir)/MyPlatformID.cs $(srcdir)/MyRuntimeID.cs $(srcdir)/NeedCredentialsEventArgs.cs $(srcdir)/NodeEventArgs.cs $(srcdir)/NodeTypes.cs $(srcdir)/NotifyEventArgs.cs $(srcdir)/SimiasEventArgs.cs $(srcdir)/WSInspection.cs $(srcdir)/Authentication.cs $(srcdir)/SimiasService.cs
SimiasClient_CSFILES := $(subst /,$(SEP),$(SimiasClient_CSFILES1))
SimiasClient_INCLUDES =
SimiasClient_RESOURCES =
SimiasClient_FLAGS = $(CSC_LIBFLAG)
SimiasClient_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Security.dll System.Web.dll System.Web.Services.dll Novell.Security.ClientPasswordManager.NetCredential.dll
SimiasClient_LIBPATH = $(NOVELL_SECURITY_DIR)

EXTRA_DIST = $(SimiasClient_CSFILES1) $(srcdir)/SimiasSetup.cs.in

SimiasSetup.cs: $(srcdir)/SimiasSetup.cs.in
	sed -e 's,@_prefix_@,@prefix@,g' -e 's,@_bindir_@,@bindir@,g' -e 's,@_datadir_@,@datadir@,g' -e 's,@_includedir_@,@includedir@,g' -e 's,@_infodir_@,@infodir@,g' -e 's,@_libexecdir_@,@libexecdir@,g' -e 's,@_libdir_@,@libdir@,g' -e 's,@_localstatedir_@,@localstatedir@,g' -e 's,@_mandir_@,@mandir@,g' -e 's,@_sbindir_@,@sbindir@,g' -e 's,@_sharedstatedir_@,@sharedstatedir@,g' -e 's,@_sysconfdir_@,@sysconfdir@,g' -e 's,@_webdir_@,@webdir@,g' -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_target_@,@target@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: SimiasClient.dll

$(srcdir)/SimiasService.cs: $(srcdir)/../WebService/Simias.wsdl
if WINDOWS
	wsdl /out:SimiasService.cs $(srcdir)/../WebService/Simias.wsdl
endif
if LINUX
	-wsdl -nologo -out:SimiasService.cs $(srcdir)/../WebService/Simias.wsdl
endif
if DARWIN
	wsdl -nologo -out:SimiasService.cs $(srcdir)/../WebService/Simias.wsdl
endif

SimiasClient.dll: $(srcdir)/SimiasSetup.cs $(SimiasClient_CSFILES1) $(SimiasClient_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasClient_FLAGS) $(SIMIAS_LIBS) $(SimiasClient_LIBS:%=/r:%) $(SimiasClient_LIBPATH:%=/lib:%) $(SimiasClient_RESOURCES:%=$(ICON_FLAG)%) $(SimiasClient_CSFILES) $(subst /,$(SEP),$(srcdir)/SimiasSetup.cs) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = SimiasClient.pdb
endif
endif

install-exec-local: SimiasClient.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SimiasClient.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(srcdir)/$(NOVELL_SECURITY_DIR)/Novell.Security.ClientPasswordManager.NetCredential.dll $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f SimiasClient.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(webbindir); rm -f Novell.Security.ClientPasswordManager.NetCredential.dll
	
installcheck-local: install
	
CLEAN_FILES = SimiasClient.dll $(DOC_FILE_CLEAN) $(srcdir)/SimiasService.cs SimiasSetup.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

