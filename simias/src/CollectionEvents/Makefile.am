# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

EventService_CSFILES = $(srcdir)/EventService/AssemblyInfo.cs $(srcdir)/EventService/EventService.cs
EventService_CSFILES_CSC := $(subst /,$(SEP),$(EventService_CSFILES))
EventService_INCLUDES = 
EventService_RESOURCES =
EventService_FLAGS = $(CSC_EXEFLAG)
EventService_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Simias.dll
EventService_LIBPATH = ../Simias.dll

EXTRA_DIST = $(EventService_CSFILES)

CUR_DIR := $(shell pwd)

all: EventService.exe

EventService.exe: $(EventService_CSFILES) $(EventService_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(EventService_FLAGS) $(EventService_LIBS:%=/r:%) $(EventService_LIBPATH:%=/lib:%) $(EventService_CSFILES_CSC) /doc:EventService.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = EventService.pdb
DEBUG_CHECK_FILES = 
endif
endif

install-exec-local: EventService.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) --mode=644 EventService.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f EventService.exe $(DEBUG_FILES) $(DEBUG_CHECK_FILES)
    
CLEAN_FILES = EventService.exe $(NUNIT_TESTS) EventService.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

