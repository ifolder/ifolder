if WINDOWS
WINDIR_UNIX = $(shell cygpath --unix $$WINDIR)
REGSVR32 = $(shell cygpath --unix $$WINDIR\\system32\\Regsvr32.exe)
REGASM = $(shell cygpath --unix $$FRAMEWORKDIR\\v1.1*\\RegAsm.exe)
REGEDIT = regedit
endif

# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderComponent_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderAdvanced.cs $(srcdir)/ShareListMember.cs $(srcdir)/MyMessageBox.cs $(srcdir)/Win32Window.cs $(srcdir)/iFolderComponent.cs $(srcdir)/NewiFolder.cs $(srcdir)/UserProperties.cs $(srcdir)/iFolderInfo.cs $(srcdir)/GaugeChart.cs $(srcdir)/Picker.cs $(srcdir)/ConflictResolver.cs $(srcdir)/CreateiFolder.cs $(srcdir)/DomainItem.cs $(srcdir)/VirtualListView.cs $(srcdir)/Win32.cs $(srcdir)/Win32Security.cs
iFolderComponent_CSFILES_CSC := $(subst /,$(SEP),$(iFolderComponent_CSFILES))
iFolderComponent_INCLUDES =
iFolderComponent_RESOURCES =
iFolderComponent_RESOURCES_FILES = CreateiFolder.resources ConflictResolver.resources iFolderAdvanced.resources MyMessageBox.resources NewiFolder.resources Picker.resources UserProperties.resources GaugeChart.resources
iFolderComponent_DE_RESOURCES_FILES = CreateiFolder.de.resources ConflictResolver.de.resources iFolderAdvanced.de.resources MyMessageBox.de.resources NewiFolder.de.resources Picker.de.resources UserProperties.de.resources GaugeChart.de.resources
iFolderComponent_ES_RESOURCES_FILES = CreateiFolder.es.resources ConflictResolver.es.resources iFolderAdvanced.es.resources MyMessageBox.es.resources NewiFolder.es.resources Picker.es.resources UserProperties.es.resources GaugeChart.es.resources
iFolderComponent_FR_RESOURCES_FILES = CreateiFolder.fr.resources ConflictResolver.fr.resources iFolderAdvanced.fr.resources MyMessageBox.fr.resources NewiFolder.fr.resources Picker.fr.resources UserProperties.fr.resources GaugeChart.fr.resources
iFolderComponent_HU_RESOURCES_FILES = CreateiFolder.hu.resources ConflictResolver.hu.resources iFolderAdvanced.hu.resources MyMessageBox.hu.resources NewiFolder.hu.resources Picker.hu.resources UserProperties.hu.resources GaugeChart.hu.resources
iFolderComponent_IT_RESOURCES_FILES = CreateiFolder.it.resources ConflictResolver.it.resources iFolderAdvanced.it.resources MyMessageBox.it.resources NewiFolder.it.resources Picker.it.resources UserProperties.it.resources GaugeChart.it.resources
iFolderComponent_JA_RESOURCES_FILES = CreateiFolder.ja.resources ConflictResolver.ja.resources iFolderAdvanced.ja.resources MyMessageBox.ja.resources NewiFolder.ja.resources Picker.ja.resources UserProperties.ja.resources GaugeChart.ja.resources
iFolderComponent_PL_RESOURCES_FILES = CreateiFolder.pl.resources ConflictResolver.pl.resources iFolderAdvanced.pl.resources MyMessageBox.pl.resources NewiFolder.pl.resources Picker.pl.resources UserProperties.pl.resources GaugeChart.pl.resources
iFolderComponent_PT_RESOURCES_FILES = CreateiFolder.pt.resources ConflictResolver.pt.resources iFolderAdvanced.pt.resources MyMessageBox.pt.resources NewiFolder.pt.resources Picker.pt.resources UserProperties.pt.resources GaugeChart.pt.resources
iFolderComponent_RU_RESOURCES_FILES = CreateiFolder.ru.resources ConflictResolver.ru.resources iFolderAdvanced.ru.resources MyMessageBox.ru.resources NewiFolder.ru.resources Picker.ru.resources UserProperties.ru.resources GaugeChart.ru.resources
iFolderComponent_SK_RESOURCES_FILES = CreateiFolder.sk.resources ConflictResolver.sk.resources iFolderAdvanced.sk.resources MyMessageBox.sk.resources NewiFolder.sk.resources Picker.sk.resources UserProperties.sk.resources GaugeChart.sk.resources
iFolderComponent_ZH_CN_RESOURCES_FILES = CreateiFolder.zh_CN.resources ConflictResolver.zh_CN.resources iFolderAdvanced.zh_CN.resources MyMessageBox.zh_CN.resources NewiFolder.zh_CN.resources Picker.zh_CN.resources UserProperties.zh_CN.resources GaugeChart.zh_CN.resources
iFolderComponent_ZH_TW_RESOURCES_FILES = CreateiFolder.zh_TW.resources ConflictResolver.zh_TW.resources iFolderAdvanced.zh_TW.resources MyMessageBox.zh_TW.resources NewiFolder.zh_TW.resources Picker.zh_TW.resources UserProperties.zh_TW.resources GaugeChart.zh_TW.resources
iFolderComponent_FLAGS = $(CSC_LIBFLAG)
iFolderComponent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll System.Windows.Forms.dll Novell.iFolder.dll
iFolderComponent_LIBPATH = ../iFolder

iFolderShell_HFILES = $(srcdir)/iFolderShell/CommonUI.h $(srcdir)/iFolderShell/iFolderShell.h $(srcdir)/iFolderShell/Registry.h $(srcdir)/iFolderShell/resource.h $(srcdir)/iFolderShell/nifver.h $(srcdir)/iFolderShell/version.h
iFolderShell_SRCFILES = $(iFolderShell_HFILES) $(srcdir)/iFolderShell/ContextMenu.cpp $(srcdir)/iFolderShell/IconOverlay.cpp $(srcdir)/iFolderShell/iFolderShell.cpp $(srcdir)/iFolderShell/PropertySheet.cpp $(srcdir)/iFolderShell/Registry.cpp $(srcdir)/iFolderShell/iFolderShell.rc $(srcdir)/iFolderShell/iFolderShell.rc2
iFolderShell_OBJFILES = iFolderShell/ContextMenu.obj iFolderShell/IconOverlay.obj iFolderShell/iFolderShell.obj iFolderShell/PropertySheet.obj iFolderShell/Registry.obj iFolderShell/iFolderShell$(RES_EXT)
iFolderShell_INCLUDES = iFolderShell
iFolderShell_FLAGS = -D_USRDLL -DIFOLDERSHELL_EXPORTS
iFolderShell_CHARFLAGS = -DUNICODE -D_UNICODE
iFolderShell_CXXFLAGS = $(CXXFLAGS) $(iFolderShell_CHARFLAGS)
iFolderShell_LIBS = comctl32.lib
iFolderShell_LIBPATH = iFolderShell
iFolderShell_LIBFLAGS = $(SHARED_LIB_FLAG)
iFolderShell_DEF = $(srcdir)/iFolderShell/iFolderShell.Def

iFolderShellRes_SRCFILES = $(srcdir)/iFolderShell/iFolderShellRes/iFolderShellRes.cpp $(srcdir)/iFolderShell/iFolderShellRes/iFolderShell.rc $(srcdir)/iFolderShell/iFolderShellRes/iFolderShell.rc2 $(srcdir)/iFolderShell/iFolderShellRes/version.h $(srcdir)/iFolderShell/nifver.h
iFolderShellRes_OBJFILES = iFolderShell/iFolderShellRes/iFolderShellRes.obj iFolderShell/iFolderShellRes/iFolderShell$(RES_EXT)

DATA_FILES = $(srcdir)/iFolderShell/ifolder_emblem.ico $(srcdir)/iFolderShell/ifolder_conflict_emb.ico
RESX_FILES = $(srcdir)/CreateiFolder.resx $(srcdir)/ConflictResolver.resx $(srcdir)/iFolderAdvanced.resx $(srcdir)/MyMessageBox.resx $(srcdir)/NewiFolder.resx $(srcdir)/Picker.resx $(srcdir)/UserProperties.resx $(srcdir)/GaugeChart.resx
WINDOWS_FILES = $(srcdir)/explorer.exe.manifest $(srcdir)/explorer.exe.config

PROJECT_FILES = $(srcdir)/iFolderComponent.csproj $(srcdir)/iFolderComponent.sln

EXTRA_DIST = $(iFolderComponent_CSFILES) $(iFolderShell_SRCFILES) $(iFolderShellRes_SRCFILES) $(iFolderShell_DEF) $(DATA_FILES) $(RESX_FILES) $(WINDOWS_FILES) $(srcdir)/iFolderShell.reg  $(PROJECT_FILES) AssemblyInfo.cs.in de/iFolderComponent.resources.dll es/iFolderComponent.resources.dll fr/iFolderComponent.resources.dll it/iFolderComponent.resources.dll hu/iFolderComponent.resources.dll ja/iFolderComponent.resources.dll pl/iFolderComponent.resources.dll pt/iFolderComponent.resources.dll ru/iFolderComponent.resources.dll sk/iFolderComponent.resources.dll zh_CN/iFolderComponent.resources.dll zh_TW/iFolderComponent.resources.dll 

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

CUR_DIR := $(shell pwd)

all: iFolderShell.dll

iFolderShellRes.dll: $(iFolderShellRes_OBJFILES)
	$(LD) $(iFolderShell_LIBFLAGS) $(LDFLAGS) $(LDOUT)$@ $(iFolderShell_LIBPATH:%=$(LDINC)%) $(iFolderShellRes_OBJFILES) $(LIBS)
	
iFolderShell.dll: iFolderShellRes.dll iFolderComponent.tlb $(iFolderShell_OBJFILES)
	$(LD) $(iFolderShell_LIBFLAGS) $(LDFLAGS) $(LDOUT)$@ $(iFolderShell_LIBPATH:%=$(LDINC)%) $(iFolderShell_DEF:%=$(DEF)%) $(iFolderShell_OBJFILES) $(iFolderShell_LIBS:%=$(LIBFLAG)%) $(LIBS)

%.obj : %.c
	$(CC) $(COUT)$@ $(iFolderShell_FLAGS) $(iFolderShell_INCLUDES:%=$(CINC)%) $(CFLAGS) $<

%.obj : %.cpp
	$(CXX) $(COUT)$@ $(iFolderShell_FLAGS) $(iFolderShell_INCLUDES:%=$(CINC)%) $(iFolderShell_CXXFLAGS) $<

%$(RES_EXT) : %.rc
	$(RC) $(RCFLAGS) $(COUT)$@ $<

iFolderComponent.tlb: iFolderComponent.dll
	$(TLBX) $< $(LDOUT)$@

iFolderComponent.dll: $(iFolderComponent_RESOURCES_FILES) $(iFolderComponent_CSFILES) $(iFolderComponent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderComponent_FLAGS) $(SIMIAS_CLIENT_LIBS) $(iFolderComponent_LIBS:%=/r:%) $(iFolderComponent_LIBPATH:%=/lib:%) $(iFolderComponent_RESOURCES_FILES:%=/res:Novell.iFolderCom.%) $(iFolderComponent_CSFILES_CSC) $(DOC_FILE_CMD)
	
%.resources : %.resx
	$(RESG) $< Novell.iFolderCom.$@

%.de.resources : %.de.resx
	$(RESG) $< Novell.iFolderCom.$@
de/iFolderComponent.resources.dll : $(iFolderComponent_DE_RESOURCES_FILES)
	al /out:$@ /c:de $(iFolderComponent_DE_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.es.resources : %.es.resx
	$(RESG) $< Novell.iFolderCom.$@
es/iFolderComponent.resources.dll : $(iFolderComponent_ES_RESOURCES_FILES)
	al /out:$@ /c:es $(iFolderComponent_ES_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)
	
%.fr.resources : %.fr.resx
	$(RESG) $< Novell.iFolderCom.$@
fr/iFolderComponent.resources.dll : $(iFolderComponent_FR_RESOURCES_FILES)
	al /out:$@ /c:fr $(iFolderComponent_FR_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.hu.resources : %.hu.resx
	$(RESG) $< Novell.iFolderCom.$@
hu/iFolderComponent.resources.dll : $(iFolderComponent_HU_RESOURCES_FILES)
	al /out:$@ /c:hu $(iFolderComponent_HU_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.it.resources : %.it.resx
	$(RESG) $< Novell.iFolderCom.$@
it/iFolderComponent.resources.dll : $(iFolderComponent_IT_RESOURCES_FILES)
	al /out:$@ /c:it $(iFolderComponent_IT_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.ja.resources : %.ja.resx
	$(RESG) $< Novell.iFolderCom.$@
ja/iFolderComponent.resources.dll : $(iFolderComponent_JA_RESOURCES_FILES)
	al /out:$@ /c:ja $(iFolderComponent_JA_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.pl.resources : %.pl.resx
	$(RESG) $< Novell.iFolderCom.$@
pl/iFolderComponent.resources.dll : $(iFolderComponent_PL_RESOURCES_FILES)
	al /out:$@ /c:pl $(iFolderComponent_PL_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.pt.resources : %.pt.resx
	$(RESG) $< Novell.iFolderCom.$@
pt/iFolderComponent.resources.dll : $(iFolderComponent_PT_RESOURCES_FILES)
	al /out:$@ /c:pt $(iFolderComponent_PT_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.ru.resources : %.ru.resx
	$(RESG) $< Novell.iFolderCom.$@
ru/iFolderComponent.resources.dll : $(iFolderComponent_RU_RESOURCES_FILES)
	al /out:$@ /c:ru $(iFolderComponent_RU_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.sk.resources : %.sk.resx
	$(RESG) $< Novell.iFolderCom.$@
sk/iFolderComponent.resources.dll : $(iFolderComponent_SK_RESOURCES_FILES)
	al /out:$@ /c:sk $(iFolderComponent_SK_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.zh_CN.resources : %.zh_CN.resx
	$(RESG) $< Novell.iFolderCom.$@
zh_CN/iFolderComponent.resources.dll : $(iFolderComponent_ZH_CN_RESOURCES_FILES)
	al /out:$@ /c:zh_CN $(iFolderComponent_ZH_CN_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)

%.zh_TW.resources : %.zh_TW.resx
	$(RESG) $< Novell.iFolderCom.$@
zh_TW/iFolderComponent.resources.dll : $(iFolderComponent_ZH_TW_RESOURCES_FILES)
	al /out:$@ /c:zh_TW $(iFolderComponent_ZH_TW_RESOURCES_FILES:%=/embed:Novell.iFolderCom.%)
	
if DEBUG
if WINDOWS
DEBUG_FILES = iFolderComponent.pdb vc70.pdb
endif
endif

languages: de/iFolderComponent.resources.dll es/iFolderComponent.resources.dll fr/iFolderComponent.resources.dll hu/iFolderComponent.resources.dll it/iFolderComponent.resources.dll ja/iFolderComponent.resources.dll pl/iFolderComponent.resources.dll pt/iFolderComponent.resources.dll ru/iFolderComponent.resources.dll sk/iFolderComponent.resources.dll zh_CN/iFolderComponent.resources.dll zh_TW/iFolderComponent.resources.dll 

install-exec-local: iFolderComponent.dll iFolderShell.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) iFolderComponent.dll iFolderShell.dll $(srcdir)/iFolderShell.reg $(DEBUG_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(DATA_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(WINDOWS_FILES) $(DESTDIR)$(WINDIR_UNIX)
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
	$(INSTALL_PROGRAM) de/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/de
	$(INSTALL_PROGRAM) es/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/es
	$(INSTALL_PROGRAM) fr/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/fr
	$(INSTALL_PROGRAM) hu/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/hu
	$(INSTALL_PROGRAM) it/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/it
	$(INSTALL_PROGRAM) ja/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/ja
	$(INSTALL_PROGRAM) pl/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/pl
	$(INSTALL_PROGRAM) pt/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/pt
	$(INSTALL_PROGRAM) ru/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/ru
	$(INSTALL_PROGRAM) sk/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/sk
	$(INSTALL_PROGRAM) zh_CN/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/zh_CN
	$(INSTALL_PROGRAM) zh_TW/iFolderComponent.resources.dll $(DESTDIR)$(bindir)/zh_TW
if DEBUG	
	$(INSTALL_PROGRAM) $(PROJECT_FILES) $(DESTDIR)$(libdir)
endif
endif

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f iFolderComponent.dll iFolderShell.dll iFolderShell.reg $(DEBUG_FILES)
	cd $(DESTDIR)$(libdir); rm -f $(DATA_FILES:$(srcdir)/iFolderShell/%=%)
	cd $(DESTDIR)$(libdir); rm -f $(DATA_FILES:$(srcdir)/iFolderShell/%=%)
	cd $(DESTDIR)$(WINDIR_UNIX); rm -f $(WINDOWS_FILES)
if WINDOWS
if DEBUG	
	cd $(DESTDIR)$(libdir); rm -f $(PROJECT_FILES:$(srcdir)/%=%)
endif
endif

register:
	$(REGEDIT) /s `cygpath --windows $(DESTDIR)$(libdir)/iFolderShell.reg`
	$(REGSVR32) /s `cygpath --windows $(DESTDIR)$(libdir)/iFolderShell.dll`
	$(REGASM) `cygpath --windows $(DESTDIR)$(libdir)/iFolderComponent.dll` /codebase

unregister:
	-$(REGSVR32) /u /s `cygpath --windows $(DESTDIR)$(libdir)/iFolderShell.dll`
	$(REGASM) `cygpath --windows $(DESTDIR)$(libdir)/iFolderComponent.dll` /unregister

CLEAN_FILES = iFolderShell.dll iFolderComponent.tlb iFolderComponent.dll $(iFolderComponent_RESOURCES_FILES:%=Novell.iFolderCom.%) $(iFolderShell_OBJFILES) $(iFolderShellRes_OBJFILES) iFolderShell/ifoldercomponent.tlh iFolderShell/ifoldercomponent.tli $(DOC_FILE_CLEAN) iFolderShell.exp iFolderShell.lib AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

