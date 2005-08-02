# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasServer_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/SimiasServer.cs
SimiasServer_CSFILES_CSC := $(subst /,$(SEP),$(SimiasServer_CSFILES))
SimiasServer_INCLUDES =
SimiasServer_RESOURCES = $(srcdir)/App$(ICON_EXT)
SimiasServer_FLAGS = $(CSC_EXEFLAG)
SimiasServer_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.Services.dll SimiasClient.dll
SimiasServer_LIBPATH = ../SimiasClient

PROJECT_FILES = $(srcdir)/SimiasServer.csproj

EXTRA_DIST = $(SimiasServer_CSFILES) $(PROJECT_FILES) App.ico simias.in simias.cmd SimiasServer.exe.config AssemblyInfo.cs.in

simias: $(srcdir)/simias.in
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@

CUR_DIR := $(shell pwd)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: SimiasServer.exe simias

SimiasServer.exe: AssemblyInfo.cs $(SimiasServer_CSFILES) $(SimiasServer_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasServer_FLAGS) $(SimiasServer_LIBS:%=/r:%) $(SimiasServer_LIBPATH:%=/lib:%) $(SimiasServer_CSFILES_CSC) $(DOC_FILE_CMD)

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if DEBUG
if WINDOWS
DEBUG_FILES = SimiasServer.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) SimiasServer.exe SimiasServer.exe.config $(DEBUG_FILES) $(DESTDIR)$(bindir)
if WINDOWS
	$(INSTALL_PROGRAM) simias.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simias $(DESTDIR)$(bindir)
endif
if WINDOWS
if DEBUG	
	$(INSTALL_PROGRAM) $(PROJECT_FILES) $(DESTDIR)$(bindir)
endif
endif

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SimiasServer.exe $(DEBUG_FILES) SimiasServer.exe.config
if WINDOWS
	cd $(DESTDIR)$(bindir); rm -f simias.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simias
endif
if WINDOWS
if DEBUG	
	cd $(DESTDIR)$(bindir); rm -f $(PROJECT_FILES:$(srcdir)/%=%)
endif
endif
	
CLEAN_FILES = SimiasServer.exe $(DEBUG_FILES) $(DOC_FILE_CLEAN) simias AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

