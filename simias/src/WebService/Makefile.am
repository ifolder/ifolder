# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

WebService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/ApplicationServer.cs $(srcdir)/IApplicationHost.cs $(srcdir)/IWebSource.cs $(srcdir)/Tracing.cs $(srcdir)/XSPApplicationHost.cs $(srcdir)/BaseApplicationHost.cs $(srcdir)/BaseRequestBroker.cs $(srcdir)/MonoWorkerRequest.cs $(srcdir)/XSPWorkerRequest.cs $(srcdir)/LingeringNetworkStream.cs $(srcdir)/InitialWorkerRequest.cs $(srcdir)/SimiasWebService.cs
WebService_CSFILES_CSC := $(subst /,$(SEP),$(WebService_CSFILES))
WebService_INCLUDES = 
WebService_RESOURCES =
WebService_FLAGS = $(CSC_LIBFLAG)
WebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Simias.dll System.Web.dll
WebService_LIBPATH = ../Simias.dll

EXTRA_DIST = $(WebService_CSFILES)

CUR_DIR := $(shell pwd)

all: SimiasWebService.dll

SimiasWebService.dll: $(WebService_CSFILES) $(WebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(WebService_FLAGS) $(WebService_LIBS:%=/r:%) $(WebService_LIBPATH:%=/lib:%) $(WebService_CSFILES_CSC) /doc:SimiasWebService.doc.xml
	
if DEBUG
if WINDOWS
DEBUG_FILES = SimiasWebService.pdb
endif
endif

install-exec-local: SimiasWebService.dll
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) SimiasWebService.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)
endif
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f SimiasWebService.dll $(DEBUG_FILES)
endif
	
CLEAN_FILES = SimiasWebService.dll $(DEBUG_FILES) 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

