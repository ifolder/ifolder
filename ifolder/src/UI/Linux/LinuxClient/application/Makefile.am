# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderClient_CSFILES = $(srcdir)/iFolderApplication.cs $(srcdir)/CopyLocation.cs 
iFolderClient_CSFILES_CSC := $(subst /,$(SEP),$(iFolderClient_CSFILES))
iFolderClient_INCLUDES =
iFolderClient_RESOURCES =
iFolderClient_FLAGS = $(CSC_EXEFLAG)
#iFolderClient_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll TrayIcon.dll Novell.iFolder.UI.gtk.dll Mono.Posix.dll
iFolderClient_LIBPATH = $(srcdir)/../../TrayIcon $(top_srcdir)/src/common/iFolder $(srcdir)/../library 
iFolderClient_LIBS = System.dll System.Web.Services.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll TrayIcon.dll Novell.iFolder.UI.gtk.dll Mono.Posix.dll  
#iFolderClient_LIBPATH = $(srcdir)/../../TrayIcon $(srcdir)/../../iFolder $(srcdir)/../library $(srcdir)/../NetworkManager-sharp

EXTRA_DIST = $(iFolderClient_CSFILES) ifolder.in ifolder.desktop.in AssemblyInfo.cs.in

ifolder: $(srcdir)/ifolder.in
	sed -e 's,@_prefix_@,@prefix@,g' -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_casadir_@,@casadir@,g' -e 's,@_lib64_@,@LIB64@,g' $< > $@
	chmod +x $@

ifolder.desktop: $(srcdir)/ifolder.desktop.in
	sed -e 's,@_bindir_@,@bindir@,g' -e 's,@_datadir_@,@datadir@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: iFolderClient.exe ifolder ifolder.desktop

iFolderClient.exe: $(srcdir)/AssemblyInfo.cs $(iFolderClient_CSFILES) $(iFolderClient_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_CLIENT_LIBS) $(iFolderClient_FLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(iFolderClient_LIBS:%=/r:%) $(iFolderClient_LIBPATH:%=/lib:%) $(iFolderClient_RESOURCES:%=$(ICON_FLAG)%) $(srcdir)/AssemblyInfo.cs $(iFolderClient_CSFILES_CSC) $(DOC_FILE_CMD) 

# 	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_CLIENT_LIBS) $(iFolderClient_FLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(DBUS_SHARP_LIBS) $(iFolderClient_LIBS:%=/r:%) $(iFolderClient_LIBPATH:%=/lib:%) $(iFolderClient_RESOURCES:%=$(ICON_FLAG)%) $(iFolderClient_CSFILES_CSC) $(DOC_FILE_CMD)

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(datadir)/applications
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) iFolderClient.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(srcdir)/ifolder $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(srcdir)/ifolder.desktop $(DESTDIR)$(datadir)/applications

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderClient.exe ifolder
	cd $(DESTDIR)$(datadir)/applications; rm -f ifolder.desktop

CLEAN_FILES = iFolderClient.exe $(DOC_FILE_CLEAN) ifolder ifolder.desktop AssemblyInfo.cs

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -f ifolder
	rm -f ifolder.desktop
	rm -rf Makefile
	rm -rf AssemblyInfo.cs

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
	rm -rf AssemblyInfo.cs
