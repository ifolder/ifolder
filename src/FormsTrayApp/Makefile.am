# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

if WINDOWS
INSTALLUTIL := $(shell cygpath --unix "$(FRAMEWORKDIR)\v1.1*\InstallUtil.exe")
endif

iFolderApp_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/FormsTrayApp.cs $(srcdir)/GlobalProperties.cs $(srcdir)/iFolderAppInstallerClass.cs $(srcdir)/ServerInfo.cs $(srcdir)/ShellNotifyIcon.cs $(srcdir)/BalloonType.cs $(srcdir)/iFolderMigration.cs $(srcdir)/AcceptInvitation.cs $(srcdir)/DomainConnectEventArgs.cs $(srcdir)/DomainRemoveEventArgs.cs $(srcdir)/ServerDetails.cs $(srcdir)/SyncLog.cs $(srcdir)/Preferences.cs $(srcdir)/AdvancedSettings.cs $(srcdir)/Domain.cs $(srcdir)/iFolderObject.cs $(srcdir)/Security.cs $(srcdir)/About.cs $(srcdir)/Credits.cs $(srcdir)/ToolBarEx.cs $(srcdir)/RemoveAccount.cs
iFolderApp_CSFILES_CSC := $(subst /,$(SEP),$(iFolderApp_CSFILES))
iFolderApp_INCLUDES =
iFolderApp_RESOURCES = $(srcdir)/App$(ICON_EXT)
iFolderApp_RESOURCES_FILES = FormsTrayApp.resources GlobalProperties.resources ServerInfo.resources AcceptInvitation.resources ServerDetails.resources SyncLog.resources Preferences.resources AdvancedSettings.resources About.resources Credits.resources RemoveAccount.resources
iFolderApp_DE_RESOURCES_FILES = FormsTrayApp.de.resources GlobalProperties.de.resources ServerInfo.de.resources AcceptInvitation.de.resources ServerDetails.de.resources SyncLog.de.resources Preferences.de.resources AdvancedSettings.de.resources About.de.resources Credits.de.resources RemoveAccount.de.resources
iFolderApp_ES_RESOURCES_FILES = FormsTrayApp.es.resources GlobalProperties.es.resources ServerInfo.es.resources AcceptInvitation.es.resources ServerDetails.es.resources SyncLog.es.resources Preferences.es.resources AdvancedSettings.es.resources About.es.resources Credits.es.resources RemoveAccount.es.resources
iFolderApp_FR_RESOURCES_FILES = FormsTrayApp.fr.resources GlobalProperties.fr.resources ServerInfo.fr.resources AcceptInvitation.fr.resources ServerDetails.fr.resources SyncLog.fr.resources Preferences.fr.resources AdvancedSettings.fr.resources About.fr.resources Credits.fr.resources RemoveAccount.fr.resources
iFolderApp_HU_RESOURCES_FILES = FormsTrayApp.hu.resources GlobalProperties.hu.resources ServerInfo.hu.resources AcceptInvitation.hu.resources ServerDetails.hu.resources SyncLog.hu.resources Preferences.hu.resources AdvancedSettings.hu.resources About.hu.resources Credits.hu.resources RemoveAccount.hu.resources
iFolderApp_IT_RESOURCES_FILES = FormsTrayApp.it.resources GlobalProperties.it.resources ServerInfo.it.resources AcceptInvitation.it.resources ServerDetails.it.resources SyncLog.it.resources Preferences.it.resources AdvancedSettings.it.resources About.it.resources Credits.it.resources RemoveAccount.it.resources
iFolderApp_JA_RESOURCES_FILES = FormsTrayApp.ja.resources GlobalProperties.ja.resources ServerInfo.ja.resources AcceptInvitation.ja.resources ServerDetails.ja.resources SyncLog.ja.resources Preferences.ja.resources AdvancedSettings.ja.resources About.ja.resources Credits.ja.resources RemoveAccount.ja.resources
iFolderApp_PL_RESOURCES_FILES = FormsTrayApp.pl.resources GlobalProperties.pl.resources ServerInfo.pl.resources AcceptInvitation.pl.resources ServerDetails.pl.resources SyncLog.pl.resources Preferences.pl.resources AdvancedSettings.pl.resources About.pl.resources Credits.pl.resources RemoveAccount.pl.resources
iFolderApp_PT_RESOURCES_FILES = FormsTrayApp.pt.resources GlobalProperties.pt.resources ServerInfo.pt.resources AcceptInvitation.pt.resources ServerDetails.pt.resources SyncLog.pt.resources Preferences.pt.resources AdvancedSettings.pt.resources About.pt.resources Credits.pt.resources RemoveAccount.pt.resources
iFolderApp_RU_RESOURCES_FILES = FormsTrayApp.ru.resources GlobalProperties.ru.resources ServerInfo.ru.resources AcceptInvitation.ru.resources ServerDetails.ru.resources SyncLog.ru.resources Preferences.ru.resources AdvancedSettings.ru.resources About.ru.resources Credits.ru.resources RemoveAccount.ru.resources
iFolderApp_SK_RESOURCES_FILES = FormsTrayApp.sk.resources GlobalProperties.sk.resources ServerInfo.sk.resources AcceptInvitation.sk.resources ServerDetails.sk.resources SyncLog.sk.resources Preferences.sk.resources AdvancedSettings.sk.resources About.sk.resources Credits.sk.resources RemoveAccount.sk.resources
iFolderApp_ZH_CN_RESOURCES_FILES = FormsTrayApp.zh-CN.resources GlobalProperties.zh-CN.resources ServerInfo.zh-CN.resources AcceptInvitation.zh-CN.resources ServerDetails.zh-CN.resources SyncLog.zh-CN.resources Preferences.zh-CN.resources AdvancedSettings.zh-CN.resources About.zh-CN.resources Credits.zh-CN.resources RemoveAccount.zh-CN.resources
iFolderApp_ZH_TW_RESOURCES_FILES = FormsTrayApp.zh-TW.resources GlobalProperties.zh-TW.resources ServerInfo.zh-TW.resources AcceptInvitation.zh-TW.resources ServerDetails.zh-TW.resources SyncLog.zh-TW.resources Preferences.zh-TW.resources AdvancedSettings.zh-TW.resources About.zh-TW.resources Credits.zh-TW.resources RemoveAccount.zh-TW.resources
# iFolderApp_LANGUAGES = de es fr
# iFolderApp_LANGUAGE_RESOURCES_FILES = FormsTrayApp.$(iFolderApp_LANGUAGES).resources GlobalProperties.$(iFolderApp_LANGUAGES).resources ServerInfo.$(iFolderApp_LANGUAGES).resources AcceptInvitation.$(iFolderApp_LANGUAGES).resources ServerDetails.$(iFolderApp_LANGUAGES).resources SyncLog.$(iFolderApp_LANGUAGES).resources Preferences.$(iFolderApp_LANGUAGES).resources AdvancedSettings.$(iFolderApp_LANGUAGES).resources About.$(iFolderApp_LANGUAGES).resources Credits.$(iFolderApp_LANGUAGES).resources RemoveAccount.$(iFolderApp_LANGUAGES).resources

iFolderApp_FLAGS = $(CSC_WINEXEFLAG)
iFolderApp_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) Novell.iFolder.dll iFolderComponent.dll SimiasClient.dll
iFolderApp_LIBPATH = ../iFolderShell ../iFolder

DATA_FILES = App.ico ifolder_app.ico
RES_FILES = res/ifolder_loaded.ico res/ifolder_sync1.ico res/ifolder_sync2.ico res/ifolder_sync3.ico res/ifolder_sync4.ico res/ifolder_sync5.ico res/ifolder_sync6.ico res/ifolder_sync7.ico res/ifolder_sync8.ico res/ifolder_sync9.ico res/ifolder_sync10.ico $(srcdir)/../images/ifolder-banner.png $(srcdir)/../images/ifolder-banner-scaler.png res/ifolder_contact_card.ico res/inviteduser.ico res/ifolder_me_card.ico res/serverifolder.ico res/ifolderconflict.ico res/ifolder-startup.ico res/ifolder-shutdown.ico res/mtoolbar_nor.bmp res/mtoolbar_dis.bmp res/mtoolbar_hot.bmp res/ltoolbar_nor.bmp res/ltoolbar_dis.bmp res/ltoolbar_hot.bmp
RESX_FILES = FormsTrayApp.resx GlobalProperties.resx ServerInfo.resx AcceptInvitation.resx ServerDetails.resx SyncLog.resx Preferences.resx AdvancedSettings.resx About.resx Credits.resx RemoveAccount.resx

PROJECT_FILES = $(srcdir)/FormsTrayApp.csproj $(srcdir)/FormsTrayApp.sln

EXTRA_DIST = $(iFolderApp_CSFILES) $(DATA_FILES) $(RES_FILES) $(RESX_FILES) iFolderApp.exe.manifest iFolderApp.exe.config InstallUtil.exe.config $(PROJECT_FILES) AssemblyInfo.cs.in iFolderUpgrade.exe

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: languages iFolderApp.exe

iFolderApp.exe: $(iFolderApp_RESOURCES_FILES) $(iFolderApp_CSFILES) $(iFolderApp_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderApp_FLAGS) $(SIMIAS_CLIENT_LIBS) $(iFolderApp_LIBS:%=/r:%) $(iFolderApp_LIBPATH:%=/lib:%) $(iFolderApp_RESOURCES:%=$(ICON_FLAG)%) $(iFolderApp_RESOURCES_FILES:%=/res:Novell.FormsTrayApp.%) $(iFolderApp_CSFILES_CSC) $(DOC_FILE_CMD)

languages: de/iFolderApp.resources.dll es/iFolderApp.resources.dll fr/iFolderApp.resources.dll hu/iFolderApp.resources.dll it/iFolderApp.resources.dll ja/iFolderApp.resources.dll pl/iFolderApp.resources.dll pt/iFolderApp.resources.dll ru/iFolderApp.resources.dll sk/iFolderApp.resources.dll zh_CN/iFolderApp.resources.dll
	
%.resources : %.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	
%.de.resources : %.de.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir de
de/iFolderApp.resources.dll : $(iFolderApp_DE_RESOURCES_FILES)
	al /out:$@ /c:de $(iFolderApp_DE_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)

%.es.resources : %.es.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir es
es/iFolderApp.resources.dll : $(iFolderApp_ES_RESOURCES_FILES)
	al /out:$@ /c:es $(iFolderApp_ES_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.fr.resources : %.fr.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir fr
fr/iFolderApp.resources.dll : $(iFolderApp_FR_RESOURCES_FILES)
	al /out:$@ /c:fr $(iFolderApp_FR_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.hu.resources : %.hu.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir hu
hu/iFolderApp.resources.dll : $(iFolderApp_HU_RESOURCES_FILES)
	al /out:$@ /c:hu $(iFolderApp_HU_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.it.resources : %.it.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir it
it/iFolderApp.resources.dll : $(iFolderApp_IT_RESOURCES_FILES)
	al /out:$@ /c:it $(iFolderApp_IT_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.ja.resources : %.ja.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir ja
ja/iFolderApp.resources.dll : $(iFolderApp_JA_RESOURCES_FILES)
	al /out:$@ /c:ja $(iFolderApp_JA_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.pl.resources : %.pl.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir pl
pl/iFolderApp.resources.dll : $(iFolderApp_PL_RESOURCES_FILES)
	al /out:$@ /c:pl $(iFolderApp_PL_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.pt.resources : %.pt.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir pt
pt/iFolderApp.resources.dll : $(iFolderApp_PT_RESOURCES_FILES)
	al /out:$@ /c:pt $(iFolderApp_PT_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.ru.resources : %.ru.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir ru
ru/iFolderApp.resources.dll : $(iFolderApp_RU_RESOURCES_FILES)
	al /out:$@ /c:ru $(iFolderApp_RU_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.sk.resources : %.sk.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir sk
sk/iFolderApp.resources.dll : $(iFolderApp_SK_RESOURCES_FILES)
	al /out:$@ /c:sk $(iFolderApp_SK_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.zh-CN.resources : %.zh-CN.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir zh_CN
zh_CN/iFolderApp.resources.dll : $(iFolderApp_ZH_CN_RESOURCES_FILES)
	al /out:$@ /c:zh-CN $(iFolderApp_ZH_CN_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	
%.zh-TW.resources : %.zh-TW.resx
	$(RESG) $< Novell.FormsTrayApp.$@
	mkdir zh_TW
zh_TW/iFolderApp.resources.dll : $(iFolderApp_ZH_TW_RESOURCES_FILES)
	al /out:$@ /c:zh-TW $(iFolderApp_ZH_TW_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)
	


# %.$(iFolderApp_LANGUAGES).resources : %.$(iFolderApp_LANGUAGES).resx
#	$(RESG) $< Novell.FormsTrayApp.$@
	
# $(iFolderApp_LANGUAGES)/iFolderApp.resources.dll : $(iFolderApp_$(iFolderApp_LANGUAGES)_RESOURCES_FILES)
#	al /out:$@ /c:$(iFolderApp_LANGUAGES) $(iFolderApp_LANGUAGE_RESOURCES_FILES:%=/embed:Novell.FormsTrayApp.%)


if DEBUG
if WINDOWS
DEBUG_FILES = iFolderApp.pdb
endif
endif

install-util:
	if ! test -f "$(DESTDIR)$(bindir)/iFolderApp.exe"; then \
		echo "run 'make install' before 'make install-util'"; \
		exit 1; \
	fi
	$(INSTALLUTIL) /LogToConsole=false /ShowCallStack /LogFile=iFolderApp.InstallLog `cygpath --windows "$(DESTDIR)$(bindir)/iFolderApp.exe"`

uninstall-util:
	if test -f "$(DESTDIR)$(bindir)/iFolderApp.exe"; then \
		$(INSTALLUTIL) /uninstall /LogToConsole=false /ShowCallStack /LogFile=iFolderApp.UninstallLog `cygpath --windows "$(DESTDIR)$(bindir)/iFolderApp.exe"`; \
	fi

install-exec-local: iFolderApp.exe $(DATA_FILES) all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(bindir)/res
	$(INSTALL_PROGRAM) iFolderApp.exe iFolderApp.exe.manifest iFolderApp.exe.config iFolderUpgrade.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(DATA_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(RES_FILES) $(DESTDIR)$(bindir)/res
if WINDOWS
	$(mkinstalldirs) $(DESTDIR)$(bindir)/de
	$(mkinstalldirs) $(DESTDIR)$(bindir)/es
	$(mkinstalldirs) $(DESTDIR)$(bindir)/fr
	$(mkinstalldirs) $(DESTDIR)$(bindir)/hu
	$(mkinstalldirs) $(DESTDIR)$(bindir)/it
	$(mkinstalldirs) $(DESTDIR)$(bindir)/ja
	$(mkinstalldirs) $(DESTDIR)$(bindir)/pl
	$(mkinstalldirs) $(DESTDIR)$(bindir)/pt
	$(mkinstalldirs) $(DESTDIR)$(bindir)/ru
	$(mkinstalldirs) $(DESTDIR)$(bindir)/sk
	$(mkinstalldirs) $(DESTDIR)$(bindir)/zh_CN
	$(mkinstalldirs) $(DESTDIR)$(bindir)/zh_TW
	$(INSTALL_PROGRAM) de/iFolderApp.resources.dll $(DESTDIR)$(bindir)/de
	$(INSTALL_PROGRAM) es/iFolderApp.resources.dll $(DESTDIR)$(bindir)/es
	$(INSTALL_PROGRAM) fr/iFolderApp.resources.dll $(DESTDIR)$(bindir)/fr
	$(INSTALL_PROGRAM) hu/iFolderApp.resources.dll $(DESTDIR)$(bindir)/hu
	$(INSTALL_PROGRAM) it/iFolderApp.resources.dll $(DESTDIR)$(bindir)/it
	$(INSTALL_PROGRAM) ja/iFolderApp.resources.dll $(DESTDIR)$(bindir)/ja
	$(INSTALL_PROGRAM) pl/iFolderApp.resources.dll $(DESTDIR)$(bindir)/pl
	$(INSTALL_PROGRAM) pt/iFolderApp.resources.dll $(DESTDIR)$(bindir)/pt
	$(INSTALL_PROGRAM) ru/iFolderApp.resources.dll $(DESTDIR)$(bindir)/ru
	$(INSTALL_PROGRAM) sk/iFolderApp.resources.dll $(DESTDIR)$(bindir)/sk
	$(INSTALL_PROGRAM) zh_CN/iFolderApp.resources.dll $(DESTDIR)$(bindir)/zh_CN
	$(INSTALL_PROGRAM) de/iFolderApp.resources.dll $(DESTDIR)$(bindir)/zh_TW
if DEBUG	
	$(INSTALL_PROGRAM) $(PROJECT_FILES) $(DESTDIR)$(bindir)
endif
endif

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderApp.exe iFolderApp.exe.manifest iFolderApp.exe.config iFolderUpgrade.exe $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f $(DATA_FILES)
	cd $(DESTDIR)$(bindir)/res; rm -f $(RES_FILES:res/%=%) ifolder-banner.png ifolder-banner-scaler.png
if WINDOWS
if DEBUG	
	cd $(DESTDIR)$(bindir); rm -f $(PROJECT_FILES:$(srcdir)/%=%)
endif
endif

CLEAN_FILES = iFolderApp.exe $(DOC_FILE_CLEAN) $(iFolderApp_RESOURCES_FILES:%=Novell.FormsTrayApp.%) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	rm -f *Log

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

