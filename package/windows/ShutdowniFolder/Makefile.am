# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Shutdown_CSFILES = $(srcdir)/ShutdowniFolder.cs $(srcdir)/AssemblyInfo.cs
Shutdown_CSFILES_CSC := $(subst /,$(SEP),$(Shutdown_CSFILES))
Shutdown_INCLUDES = 
Shutdown_RESOURCES =
Shutdown_FLAGS = $(CSC_EXEFLAG)
Shutdown_LIBS = System.dll
Shutdown_LIBPATH = 

EXTRA_DIST = $(Shutdown_CSFILES) $(srcdir)/ShutdowniFolder.sln $(srcdir)/ShutdowniFolder.csproj $(srcdir)/App.ico

CUR_DIR := $(shell pwd)

all: ShutdowniFolder.exe

ShutdowniFolder.exe: $(Shutdown_CSFILES) $(Shutdown_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Shutdown_FLAGS) $(Shutdown_LIBS:%=/r:%) $(Shutdown_LIBPATH:%=/lib:%) $(Shutdown_CSFILES_CSC)

install-exec-local: ShutdowniFolder.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ShutdowniFolder.exe $(DESTDIR)$(bindir)
        
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f ShutdowniFolder.exe
	
CLEAN_FILES = ShutdowniFolder.exe

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

