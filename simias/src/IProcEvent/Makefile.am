# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

IProcEvent_CSFILES     := $(srcdir)/AssemblyInfo.cs $(srcdir)/IProcEventClient.cs
IProcEvent_CSFILES_CSC := $(subst /,$(SEP),$(IProcEvent_CSFILES))
IProcEvent_INCLUDES =
IProcEvent_RESOURCES =
IProcEvent_FLAGS = $(CSC_LIBFLAG)
IProcEvent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) log4net.dll Simias.dll
IProcEvent_LIBPATH = ../Simias.dll $(LOG4NET_DIR)

EXTRA_DIST = $(IProcEvent_CSFILES)

CUR_DIR := $(shell pwd)

all: IProcEvent.dll

IProcEvent.dll: $(IProcEvent_CSFILES) $(IProcEvent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(IProcEvent_FLAGS) $(IProcEvent_LIBS:%=/r:%) $(IProcEvent_LIBPATH:%=/lib:%) $(IProcEvent_CSFILES_CSC) /doc:IProcEvent.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = IProcEvent.pdb
endif
endif

install-exec-local: IProcEvent.dll
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) IProcEvent.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f IProcEvent.dll $(DEBUG_FILES)

CLEAN_FILES = IProcEvent.dll IProcEvent.doc.xml $(DEBUG_FILES)

clean-local:
	rm -rf $(CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

