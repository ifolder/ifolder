# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasApp_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/server.cs $(srcdir)/ApplicationServer.cs $(srcdir)/IApplicationHost.cs $(srcdir)/IWebSource.cs $(srcdir)/Tracing.cs $(srcdir)/XSPApplicationHost.cs $(srcdir)/BaseApplicationHost.cs $(srcdir)/BaseRequestBroker.cs $(srcdir)/MonoWorkerRequest.cs $(srcdir)/XSPWorkerRequest.cs $(srcdir)/LingeringNetworkStream.cs $(srcdir)/InitialWorkerRequest.cs
SimiasApp_CSFILES_CSC := $(subst /,$(SEP),$(SimiasApp_CSFILES))
SimiasApp_INCLUDES =
SimiasApp_RESOURCES = $(srcdir)/App$(ICON_EXT)
SimiasApp_FLAGS = $(CSC_EXEFLAG)
SimiasApp_LIBS = System.dll System.Data.dll $(SYSTEM_XML) SimiasClient.dll SimiasLib.dll Simias.Web.dll System.Web.dll
SimiasApp_LIBPATH = $(NUNIT_LIBPATH) $(LOG4NET_DIR) ../SimiasClient ../SimiasLib.dll ../WebService

EXTRA_DIST = App.ico $(SimiasApp_CSFILES) AssemblyInfo.cs.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: SimiasApp.exe

SimiasApp.exe: AssemblyInfo.cs $(SimiasApp_CSFILES) $(SimiasApp_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasApp_FLAGS) $(SIMIAS_LIBS) $(SimiasApp_LIBS:%=/r:%) $(SimiasApp_LIBPATH:%=/lib:%) $(SimiasApp_RESOURCES:%=$(ICON_FLAG)%) $(SimiasApp_CSFILES_CSC) $(DOC_FILE_CMD)

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if DEBUG
if WINDOWS
DEBUG_FILES = SimiasApp.pdb
endif
endif

install-exec-local: SimiasApp.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SimiasApp.exe $(DEBUG_FILES) $(DESTDIR)$(webbindir)

	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f SimiasApp.exe $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = SimiasApp.exe $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

