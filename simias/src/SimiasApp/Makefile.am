# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasApp_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/SimiasApp.cs $(srcdir)/ApplicationServer.cs $(srcdir)/IApplicationHost.cs $(srcdir)/IWebSource.cs $(srcdir)/Tracing.cs $(srcdir)/XSPApplicationHost.cs $(srcdir)/BaseApplicationHost.cs $(srcdir)/BaseRequestBroker.cs $(srcdir)/MonoWorkerRequest.cs $(srcdir)/XSPWorkerRequest.cs $(srcdir)/LingeringNetworkStream.cs $(srcdir)/InitialWorkerRequest.cs 
SimiasApp_CSFILES := $(subst /,$(SEP),$(SimiasApp_CSFILES1))
SimiasApp_INCLUDES = 
SimiasApp_RESOURCES = $(srcdir)/App$(ICON_EXT)
SimiasApp_FLAGS = $(CSC_EXEFLAG)
SimiasApp_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Simias.dll System.Web.dll
SimiasApp_LIBPATH = $(NUNIT_LIBPATH) $(LOG4NET_DIR) ../Simias.dll

EXTRA_DIST = App.ico $(SimiasApp_CSFILES) simias.in

all: SimiasApp.exe

SimiasApp.exe: $(SimiasApp_CSFILES) $(SimiasApp_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasApp_FLAGS) $(SIMIAS_LIBS) $(SimiasApp_LIBS:%=/r:%) $(SimiasApp_LIBPATH:%=/lib:%) $(SimiasApp_RESOURCES:%=$(ICON_FLAG)%) $(SimiasApp_CSFILES) /doc:SimiasApp.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = SimiasApp.pdb
endif
endif

install-exec-local: SimiasApp.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SimiasApp.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
if LINUX
	$(INSTALL_PROGRAM) simias $(DESTDIR)$(bindir)
endif
if DARWIN
	$(INSTALL_PROGRAM) simias $(DESTDIR)$(bindir)
endif

	
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SimiasApp.exe $(DEBUG_FILES)
if LINUX
	cd $(DESTDIR)$(bindir); rm -f simias
endif
if DARWIN 
	cd $(DESTDIR)$(bindir); rm -f simias
endif
    
installcheck-local: install
	
CLEAN_FILES = SimiasApp.exe SimiasApp.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

