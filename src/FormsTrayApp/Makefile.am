# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

if WINDOWS
INSTALLUTIL := $(shell cygpath --unix "$(FRAMEWORKDIR)\v1.1*\InstallUtil.exe")
endif

iFolderApp_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/FormsTrayApp.cs $(srcdir)/GlobalProperties.cs $(srcdir)/iFolderAppInstallerClass.cs $(srcdir)/ServerInfo.cs $(srcdir)/ShellNotifyIcon.cs $(srcdir)/BalloonType.cs $(srcdir)/iFolderMigration.cs $(srcdir)/AcceptInvitation.cs $(srcdir)/DomainConnectEventArgs.cs $(srcdir)/DomainRemoveEventArgs.cs $(srcdir)/ServerDetails.cs $(srcdir)/SyncLog.cs $(srcdir)/Preferences.cs $(srcdir)/AdvancedSettings.cs $(srcdir)/Domain.cs $(srcdir)/iFolderObject.cs $(srcdir)/Security.cs
iFolderApp_CSFILES_CSC := $(subst /,$(SEP),$(iFolderApp_CSFILES))
iFolderApp_INCLUDES =
iFolderApp_RESOURCES = $(srcdir)/App$(ICON_EXT)
iFolderApp_RESOURCES_FILES = FormsTrayApp.resources GlobalProperties.resources ServerInfo.resources AcceptInvitation.resources ServerDetails.resources SyncLog.resources Preferences.resources AdvancedSettings.resources
iFolderApp_FLAGS = $(CSC_WINEXEFLAG)
iFolderApp_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) Novell.iFolder.dll iFolderComponent.dll SimiasClient.dll ClientUpgrade.dll
iFolderApp_LIBPATH = ../iFolderShell ../iFolder ../ClientUpgrade

DATA_FILES = App.ico ifolder_app.ico
RES_FILES = res/ifolder_disconnect.ico res/ifolder_loaded.ico res/ifolder_message.ico res/ifolder_sync1.ico res/ifolder_sync2.ico res/ifolder_sync3.ico res/ifolder_sync4.ico res/ifolder_sync5.ico res/ifolder_sync6.ico res/ifolder_sync7.ico res/ifolder_sync8.ico res/ifolder_sync9.ico res/ifolder_sync10.ico $(srcdir)/../images/ifolder-banner.png $(srcdir)/../images/ifolder-banner-scaler.png res/ifolder_contact_card.ico res/inviteduser.ico res/ifolder_me_card.ico res/serverifolder.ico res/ifolderconflict.ico res/ifolder-startup.ico res/ifolder.ico $(srcdir)/../images/newifolder24.png $(srcdir)/../images/share24.png $(srcdir)/../images/sync24.png $(srcdir)/../images/setup24.png
RESX_FILES = FormsTrayApp.resx GlobalProperties.resx ServerInfo.resx AcceptInvitation.resx ServerDetails.resx SyncLog.resx Preferences.resx AdvancedSettings.resx

PROJECT_FILES = $(srcdir)/FormsTrayApp.csproj $(srcdir)/FormsTrayApp.sln

EXTRA_DIST = $(iFolderApp_CSFILES) $(DATA_FILES) $(RES_FILES) $(RESX_FILES) iFolderApp.exe.manifest iFolderApp.exe.config $(PROJECT_FILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: iFolderApp.exe

iFolderApp.exe: $(iFolderApp_RESOURCES_FILES) $(iFolderApp_CSFILES) $(iFolderApp_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderApp_FLAGS) $(SIMIAS_CLIENT_LIBS) $(iFolderApp_LIBS:%=/r:%) $(iFolderApp_LIBPATH:%=/lib:%) $(iFolderApp_RESOURCES:%=$(ICON_FLAG)%) $(iFolderApp_RESOURCES_FILES:%=/res:Novell.FormsTrayApp.%) $(iFolderApp_CSFILES_CSC) $(DOC_FILE_CMD)
	
%.resources : %.resx
	$(RESG) $< Novell.FormsTrayApp.$@

if DEBUG
if WINDOWS
DEBUG_FILES = iFolderApp.pdb
endif
endif

install-exec-local: iFolderApp.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(bindir)/res
	$(INSTALL_PROGRAM) iFolderApp.exe iFolderApp.exe.manifest iFolderApp.exe.config $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(DATA_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(RES_FILES) $(DESTDIR)$(bindir)/res
	if test -f "$(DESTDIR)$(bindir)/iFolderApp.exe"; then \
		$(INSTALLUTIL) /LogToConsole=false /ShowCallStack /LogFile=iFolderApp.InstallLog `cygpath --windows "$(DESTDIR)$(bindir)/iFolderApp.exe"`; \
	fi
if WINDOWS
if DEBUG	
	$(INSTALL_PROGRAM) $(PROJECT_FILES) $(DESTDIR)$(bindir)
endif
endif

uninstall-local:
	if test -f "$(DESTDIR)$(bindir)/iFolderApp.exe"; then \
		$(INSTALLUTIL) /uninstall /LogToConsole=false /ShowCallStack /LogFile=iFolderApp.UninstallLog `cygpath --windows "$(DESTDIR)$(bindir)/iFolderApp.exe"`; \
	fi
	cd $(DESTDIR)$(bindir); rm -f iFolderApp.exe iFolderApp.exe.manifest iFolderApp.exe.config $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f $(DATA_FILES)
	cd $(DESTDIR)$(bindir)/res; rm -f $(RES_FILES:res/%=%) ifolder-banner.png ifolder-banner-scaler.png newifolder24.png share24.png sync24.png setup24.png
if WINDOWS
if DEBUG	
	cd $(DESTDIR)$(bindir); rm -f $(PROJECT_FILES:$(srcdir)/%=%)
endif
endif

CLEAN_FILES = iFolderApp.exe $(DOC_FILE_CLEAN) $(iFolderApp_RESOURCES_FILES:%=Novell.FormsTrayApp.%)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	rm -f *Log

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

