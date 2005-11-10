# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

UILIB_CSFILES = $(srcdir)/iFolderConflictDialog.cs $(srcdir)/iFolderAcceptDialog.cs $(srcdir)/iFolderUserSelector.cs $(srcdir)/iFolderLoginDialog.cs $(srcdir)/iFolderPropSettingsPage.cs $(srcdir)/iFolderPropSharingPage.cs $(srcdir)/iFolderPropertiesDialog.cs $(srcdir)/iFolderCrashDialog.cs $(srcdir)/iFolderMsgDialog.cs $(srcdir)/iFolderWindow.cs $(srcdir)/iFolderCreationDialog.cs $(srcdir)/iFolderExceptionDialog.cs $(srcdir)/iFolderAccessDialog.cs $(srcdir)/iFolderTreeView.cs $(srcdir)/NotifyWindow.cs $(srcdir)/CompatFileChooserDialog.cs $(srcdir)/ClientConfig.cs $(srcdir)/SimiasEventBroker.cs $(srcdir)/CreateDialog.cs $(srcdir)/LogWindow.cs $(srcdir)/PreferencesWindow.cs $(srcdir)/PrefsGeneralPage.cs $(srcdir)/PrefsAccountsPage.cs $(srcdir)/AccountDialog.cs $(srcdir)/RemoveAccountDialog.cs $(srcdir)/iFolderData.cs $(srcdir)/FileRenameDialog.cs $(srcdir)/list.cs $(srcdir)/elabel.cs $(srcdir)/GnomeHttpProxy.cs $(srcdir)/Events.cs $(srcdir)/DomainController.cs $(srcdir)/iFolderController.cs $(srcdir)/iFolderIconView.cs


UILIB_CSFILES_CSC := $(subst /,$(SEP),$(UILIB_CSFILES))
UILIB_INCLUDES =
UILIB_RESOURCES =
UILIB_FLAGS = $(CSC_LIBFLAG)
UILIB_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll Mono.Posix.dll
UILIB_LIBPATH = $(srcdir)/../iFolder

EXTRA_DIST = $(UILIB_CSFILES) Util.cs.in 

Util.cs: $(srcdir)/Util.cs.in
	sed -e 's,@_prefix_@,@prefix@,g' -e 's,@_PACKAGE_@,@PACKAGE@,g' -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif


all: Novell.iFolder.UI.gtk.dll

Novell.iFolder.UI.gtk.dll: Util.cs $(UILIB_CSFILES) $(UILIB_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_CLIENT_LIBS) $(UILIB_FLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(GCONF_SHARP_LIBS) $(UILIB_LIBS:%=/r:%) $(UILIB_LIBPATH:%=/lib:%) $(UILIB_RESOURCES:%=$(ICON_FLAG)%) $(UILIB_CSFILES_CSC) Util.cs $(DOC_FILE_CMD)

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) Novell.iFolder.UI.gtk.dll $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.UI.gtk.dll

CLEAN_FILES = Novell.iFolder.UI.gtk.dll $(DOC_FILE_CLEAN) Util.cs

clean-local:
	rm -rf *.dbg $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
