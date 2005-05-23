# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

WebService_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/ApplicationServer.cs $(srcdir)/IApplicationHost.cs $(srcdir)/IWebSource.cs $(srcdir)/Tracing.cs $(srcdir)/XSPApplicationHost.cs $(srcdir)/BaseApplicationHost.cs $(srcdir)/BaseRequestBroker.cs $(srcdir)/MonoWorkerRequest.cs $(srcdir)/XSPWorkerRequest.cs $(srcdir)/LingeringNetworkStream.cs $(srcdir)/InitialWorkerRequest.cs $(srcdir)/SimiasAspService.cs
WebService_CSFILES_CSC := $(subst /,$(SEP),$(WebService_CSFILES))
WebService_INCLUDES =
WebService_RESOURCES =
WebService_FLAGS = $(CSC_LIBFLAG)
WebService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Simias.dll System.Web.dll
WebService_LIBPATH = ../Simias.dll

EXTRA_DIST = $(WebService_CSFILES) AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

CUR_DIR := $(shell pwd)

all: SimiasAspService.dll

SimiasAspService.dll: $(WebService_CSFILES) $(WebService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(WebService_FLAGS) $(WebService_LIBS:%=/r:%) $(WebService_LIBPATH:%=/lib:%) $(WebService_CSFILES_CSC) $(DOC_FILE_CMD)
	
if DEBUG
if WINDOWS
DEBUG_FILES = SimiasAspService.pdb
endif
endif

install-exec-local: SimiasAspService.dll
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) SimiasAspService.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f SimiasAspService.dll $(DEBUG_FILES)
	
CLEAN_FILES = SimiasAspService.dll $(DEBUG_FILES) $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

