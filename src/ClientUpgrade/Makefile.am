# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

ClientUpgrade_CSFILES1 = $(srcdir)/AssemblyInfo.cs $(srcdir)/ClientUpgrade.cs $(srcdir)/ClientUpdate.cs
ClientUpgrade_CSFILES := $(subst /,$(SEP),$(ClientUpgrade_CSFILES1))
ClientUpgrade_INCLUDES =
ClientUpgrade_RESOURCES =
ClientUpgrade_FLAGS = $(CSC_LIBFLAG)
ClientUpgrade_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll System.Web.Services.dll
ClientUpgrade_LIBPATH =

EXTRA_DIST = $(ClientUpgrade_CSFILES1)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: ClientUpgrade.dll

ClientUpgrade.dll: $(ClientUpgrade_CSFILES) $(ClientUpgrade_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ClientUpgrade_FLAGS) $(SIMIAS_CLIENT_LIBS) $(ClientUpgrade_LIBS:%=/r:%) $(ClientUpgrade_LIBPATH:%=/lib:%) $(ClientUpgrade_RESOURCES:%=$(ICON_FLAG)%) $(ClientUpgrade_CSFILES) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = ClientUpgrade.pdb
endif
endif

install-exec-local: ClientUpgrade.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webdir)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) ClientUpgrade.dll $(DEBUG_FILES) $(DESTDIR)$(webbindir)
	
uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f ClientUpgrade.dll $(DEBUG_FILES)

installcheck-local: install
	
CLEAN_FILES = ClientUpgrade.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

