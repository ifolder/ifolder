# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

NM_CSFILES = $(srcdir)/Device.cs $(srcdir)/Manager.cs $(srcdir)/Network.cs $(srcdir)/NetworkDetect.cs
NM_CSFILES_CSC := $(subst /,$(SEP),$(NM_CSFILES))
NM_INCLUDES =
NM_RESOURCES =
NM_FLAGS = $(CSC_LIBFLAG)
NM_LIBS = System.dll System.Drawing.dll System.Data.dll Mono.Posix.dll dbus-sharp.dll
NM_LIBPATH = $(srcdir)/../../TrayIcon $(srcdir)/../../iFolder $(srcdir)/../library

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

EXTRA_DIST = $(NM_CSFILES)

all: NetworkManager-sharp.dll

NetworkManager-sharp.dll: $(NM_CSFILES) $(NM_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(NM_FLAGS) $(DBUS_SHARP_LIBS) $(NM_LIBS:%=/r:%) $(NM_LIBPATH:%=/lib:%) $(NM_RESOURCES:%=$(ICON_FLAG)%) $(NM_CSFILES_CSC) $(DOC_FILE_CMD)

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) NetworkManager-sharp.dll $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f NetworkManager-sharp.dll
	cd $(DESTDIR)$(datadir)/applications; rm -f ifolder.desktop

CLEAN_FILES = NetworkManager-sharp.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -f ifolder
	rm -f ifolder.desktop

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
