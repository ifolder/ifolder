# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Simias_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/Simias.cs $(srcdir)/server.cs $(srcdir)/ApplicationServer.cs $(srcdir)/IApplicationHost.cs $(srcdir)/IWebSource.cs $(srcdir)/Tracing.cs $(srcdir)/XSPApplicationHost.cs $(srcdir)/BaseApplicationHost.cs $(srcdir)/BaseRequestBroker.cs $(srcdir)/MonoWorkerRequest.cs $(srcdir)/XSPWorkerRequest.cs $(srcdir)/LingeringNetworkStream.cs $(srcdir)/InitialWorkerRequest.cs
Simias_CSFILES_CSC := $(subst /,$(SEP),$(Simias_CSFILES))
Simias_INCLUDES =
Simias_RESOURCES = $(srcdir)/App$(ICON_EXT)
Simias_FLAGS = $(CSC_EXEFLAG)
Simias_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll SimiasClient.dll
Simias_LIBPATH = ../SimiasClient

EXTRA_DIST = App.ico $(Simias_CSFILES) AssemblyInfo.cs.in $(srcdir)/SimiasDirectoryMapping.in $(srcdir)/simias.cmd $(srcdir)/simias.in

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

if WINDOWS
SIMIAS_PATH = $(shell cygpath --mixed $(webbindir))/Simias.exe
else
SIMIAS_PATH = @webbindir@/Simias.exe
endif

all: Simias.exe

Simias.exe: AssemblyInfo.cs $(Simias_CSFILES) $(Simias_RESOURCES) SimiasDirectoryMapping simias
	$(CSC) /out:$@ $(CSCFLAGS) $(Simias_FLAGS) $(SIMIAS_LIBS) $(Simias_LIBS:%=/r:%) $(Simias_LIBPATH:%=/lib:%) $(Simias_RESOURCES:%=$(ICON_FLAG)%) $(Simias_CSFILES_CSC) $(DOC_FILE_CMD)

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

SimiasDirectoryMapping: $(srcdir)/SimiasDirectoryMapping.in
	sed -e 's,@_mappingpath_@,$(SIMIAS_PATH),g' $< > $@
	chmod +x $@

simias: $(srcdir)/simias.in
if !WINDOWS
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif	

if DEBUG
if WINDOWS
DEBUG_FILES = Simias.pdb
endif
endif

install-exec-local: Simias.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Simias.exe $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SimiasDirectoryMapping $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) simias.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simias $(DESTDIR)$(bindir)
endif

	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f SimiasDirectoryMapping
if WINDOWS
	cd $(DESTDIR)$(bindir); rm -f simias.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simias
endif

installcheck-local: install
	
CLEAN_FILES = Simias.exe $(DOC_FILE_CLEAN) AssemblyInfo.cs SimiasDirectoryMapping simias

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

