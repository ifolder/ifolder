# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SimiasServer_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/SimiasServer.cs 
SimiasServer_CSFILES_CSC := $(subst /,$(SEP),$(SimiasServer_CSFILES))
SimiasServer_INCLUDES = 
SimiasServer_RESOURCES = $(srcdir)/App$(ICON_EXT)
SimiasServer_FLAGS = $(CSC_EXEFLAG)
SimiasServer_LIBS = System.dll System.Data.dll $(SYSTEM_XML) SimiasClient.dll
SimiasServer_LIBPATH = ../SimiasClient

PROJECT_FILES = $(srcdir)/SimiasServer.csproj

EXTRA_DIST = $(SimiasServer_CSFILES) $(PROJECT_FILES) App.ico simias.in simias.cmd

CUR_DIR := $(shell pwd)

all: SimiasServer.exe

SimiasServer.exe: $(SimiasServer_CSFILES) $(SimiasServer_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SimiasServer_FLAGS) $(SimiasServer_LIBS:%=/r:%) $(SimiasServer_LIBPATH:%=/lib:%) $(SimiasServer_CSFILES_CSC) /doc:SimiasServer.doc.xml
	
if DEBUG
if WINDOWS
DEBUG_FILES = SimiasServer.pdb
endif
endif

install-exec-local: SimiasServer.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) SimiasServer.exe $(DEBUG_FILES) $(DESTDIR)$(webbindir)
if WINDOWS
	$(INSTALL_PROGRAM) simias.cmd $(DESTDIR)$(bindir)
else
	$(INSTALL_PROGRAM) simias $(DESTDIR)$(bindir)
endif
if WINDOWS
if DEBUG	
	$(INSTALL_PROGRAM) $(PROJECT_FILES) $(DESTDIR)$(webbindir)
endif
endif
    
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f SimiasServer.exe $(DEBUG_FILES)
if WINDOWS
	cd $(DESTDIR)$(bindir); rm -f simias.cmd
else
	cd $(DESTDIR)$(bindir); rm -f simias
endif
if WINDOWS
if DEBUG	
	cd $(DESTDIR)$(webbindir); rm -f $(PROJECT_FILES:$(srcdir)/%=%)
endif
endif
	
CLEAN_FILES = SimiasServer.exe $(DEBUG_FILES) 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

