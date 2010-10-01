# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

UILIB_CSFILES = $(srcdir)/iFolderHolder.cs $(srcdir)/iFolderConflictDialog.cs $(srcdir)/iFolderAcceptDialog.cs $(srcdir)/iFolderUserSelector.cs $(srcdir)/iFolderLoginDialog.cs $(srcdir)/iFolderPropSettingsPage.cs $(srcdir)/iFolderPropSharingPage.cs $(srcdir)/iFolderPropertiesDialog.cs $(srcdir)/iFolderCrashDialog.cs $(srcdir)/iFolderMsgDialog.cs $(srcdir)/iFolderWindow.cs $(srcdir)/iFolderCreationDialog.cs $(srcdir)/iFolderExceptionDialog.cs $(srcdir)/iFolderAccessDialog.cs $(srcdir)/iFolderTreeView.cs $(srcdir)/NotifyWindow.cs $(srcdir)/CompatFileChooserDialog.cs $(srcdir)/ClientConfig.cs $(srcdir)/SimiasEventBroker.cs $(srcdir)/CreateDialog.cs $(srcdir)/DragCreateDialog.cs $(srcdir)/LogWindow.cs $(srcdir)/PreferencesWindow.cs $(srcdir)/PrefsGeneralPage.cs $(srcdir)/PrefsAccountsPage.cs $(srcdir)/AccountDialog.cs $(srcdir)/RemoveAccountDialog.cs $(srcdir)/iFolderData.cs $(srcdir)/FileRenameDialog.cs $(srcdir)/list.cs $(srcdir)/elabel.cs $(srcdir)/GnomeHttpProxy.cs $(srcdir)/Events.cs $(srcdir)/DomainController.cs $(srcdir)/iFolderIconView.cs $(srcdir)/AddAccountWizard.cs $(srcdir)/iFolderViewGroup.cs $(srcdir)/iFolderViewItem.cs $(srcdir)/iFolderWaitDialog.cs $(srcdir)/IDomainProviderUI.cs $(srcdir)/DomainProviderUI.cs $(srcdir)/EnterpriseAccountDialog.cs $(srcdir)/MigrationPage.cs $(srcdir)/MigrationWizard.cs $(srcdir)/MigrateLocation.cs $(srcdir)/MigrationWindow.cs $(srcdir)/EnterPassPhraseDialog.cs $(srcdir)/VerifyPassPhraseDialog.cs $(srcdir)/ResetPassPhraseDialog.cs $(srcdir)/ExportKeysDialog.cs $(srcdir)/ImportKeysDialog.cs $(srcdir)/CertificateDialog.cs IiFolderLog.cs iFolderLog.cs iFolderLogManager.cs $(srcdir)/MigrationDialog.cs $(srcdir)/ResetPasswordDialog.cs $(srcdir)/KeyRecoveryWizard.cs $(srcdir)/CopyLocation.cs $(srcdir)/NotificationPreference.cs


UILIB_CSFILES_CSC := $(subst /,$(SEP),$(UILIB_CSFILES))
UILIB_INCLUDES =
UILIB_RESOURCES =
UILIB_FLAGS = $(CSC_LIBFLAG)
UILIB_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll Mono.Posix.dll SimiasLib.dll Mono.Security.dll System.Web.Services.dll
UILIB_LIBPATH = $(srcdir)/../iFolder $(top_srcdir)/src/common/iFolder $(webbindir)

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
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_CLIENT_LIBS) $(UILIB_FLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(GCONF_SHARP_LIBS) $(LOG4NET_LIBS) $(UILIB_LIBS:%=/r:%) $(UILIB_LIBPATH:%=/lib:%) $(UILIB_RESOURCES:%=$(ICON_FLAG)%) $(UILIB_CSFILES_CSC) Util.cs $(DOC_FILE_CMD)

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(mkinstalldirs) $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) Novell.iFolder.UI.gtk.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) UI.log4net $(DESTDIR)$(sysconfdir) 

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.UI.gtk.dll
	cd $(DESTDIR)$(sysconfdir); rm -f UI.log4net

CLEAN_FILES = Novell.iFolder.UI.gtk.dll $(DOC_FILE_CLEAN) Util.cs

clean-local:
	rm -rf *.dbg $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
