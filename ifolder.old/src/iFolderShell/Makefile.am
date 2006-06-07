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
iFolderComponent_RESX_FILES := $(wildcard *.resx)
iFolderComponent_LANGUAGES := $(subst .,,$(sort $(suffix $(basename $(iFolderComponent_RESX_FILES)))))
iFolderComponent_RESOURCES_DLLS := $(iFolderComponent_LANGUAGES:%=$(srcdir)/%/iFolderComponent.resources.dll)
iFolderComponent_RESOURCES_FILES = Novell.iFolderCom.CreateiFolder.resources Novell.iFolderCom.ConflictResolver.resources Novell.iFolderCom.iFolderAdvanced.resources Novell.iFolderCom.MyMessageBox.resources Novell.iFolderCom.NewiFolder.resources Novell.iFolderCom.Picker.resources Novell.iFolderCom.UserProperties.resources Novell.iFolderCom.GaugeChart.resources
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
iFolderShell_RESOURCES_DLLS = $(srcdir)/cs/iFolderShellRes.dll $(srcdir)/de/iFolderShellRes.dll $(srcdir)/es/iFolderShellRes.dll $(srcdir)/fr/iFolderShellRes.dll $(srcdir)/hu/iFolderShellRes.dll $(srcdir)/it/iFolderShellRes.dll $(srcdir)/ja/iFolderShellRes.dll $(srcdir)/pl/iFolderShellRes.dll $(srcdir)/pt/iFolderShellRes.dll $(srcdir)/ru/iFolderShellRes.dll $(srcdir)/sk/iFolderShellRes.dll $(srcdir)/zh-CN/iFolderShellRes.dll $(srcdir)/zh-TW/iFolderShellRes.dll

iFolderShellRes_SRCFILES = $(srcdir)/iFolderShell/iFolderShellRes/iFolderShellRes.cpp $(srcdir)/iFolderShell/iFolderShellRes/iFolderShell.rc $(srcdir)/iFolderShell/iFolderShellRes/iFolderShell.rc2 $(srcdir)/iFolderShell/iFolderShellRes/version.h $(srcdir)/iFolderShell/nifver.h
iFolderShellRes_OBJFILES = iFolderShell/iFolderShellRes/iFolderShellRes.obj iFolderShell/iFolderShellRes/iFolderShell$(RES_EXT)

DATA_FILES = $(srcdir)/iFolderShell/ifolder_emblem.ico $(srcdir)/iFolderShell/ifolder_conflict_emb.ico
WINDOWS_FILES = $(srcdir)/explorer.exe.manifest $(srcdir)/explorer.exe.config

PROJECT_FILES = $(srcdir)/iFolderComponent.csproj $(srcdir)/iFolderComponent.sln

EXTRA_DIST = $(iFolderShell_RESOURCES_DLLS) $(iFolderComponent_CSFILES) $(iFolderShell_SRCFILES) $(iFolderShellRes_SRCFILES) $(iFolderShell_DEF) $(DATA_FILES) $(iFolderComponent_RESX_FILES) $(WINDOWS_FILES) $(srcdir)/iFolderShell.reg $(PROJECT_FILES) AssemblyInfo.cs.in iFolderShell/nifver.h.in 

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

CUR_DIR := $(shell pwd)

all: iFolderShell.dll iFolderComponent.dll $(iFolderComponent_RESOURCES_DLLS)

iFolderShellRes.dll: $(iFolderShellRes_OBJFILES)
	$(LD) $(iFolderShell_LIBFLAGS) $(LDFLAGS) $(LDOUT)$@ $(iFolderShell_LIBPATH:%=$(LDINC)%) $(iFolderShellRes_OBJFILES) $(LIBS)
	
iFolderShell.dll: iFolderShell/nifver.h iFolderShellRes.dll iFolderComponent.tlb $(iFolderShell_OBJFILES)
	$(LD) $(iFolderShell_LIBFLAGS) $(LDFLAGS) $(LDOUT)$@ $(iFolderShell_LIBPATH:%=$(LDINC)%) $(iFolderShell_DEF:%=$(DEF)%) $(iFolderShell_OBJFILES) $(iFolderShell_LIBS:%=$(LIBFLAG)%) $(LIBS)

iFolderShell/nifver.h: $(srcdir)/iFolderShell/nifver.h.in
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

%.obj : %.c
	$(CC) $(COUT)$@ $(iFolderShell_FLAGS) $(iFolderShell_INCLUDES:%=$(CINC)%) $(CFLAGS) $<

%.obj : %.cpp
	$(CXX) $(COUT)$@ $(iFolderShell_FLAGS) $(iFolderShell_INCLUDES:%=$(CINC)%) $(iFolderShell_CXXFLAGS) $<

%$(RES_EXT) : %.rc
	$(RC) $(RCFLAGS) $(COUT)$@ $<

iFolderComponent.tlb: iFolderComponent.dll
	$(TLBX) $< $(LDOUT)$@

iFolderComponent.dll: $(iFolderComponent_RESOURCES_FILES) $(iFolderComponent_CSFILES) $(iFolderComponent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderComponent_FLAGS) $(SIMIAS_CLIENT_LIBS) $(iFolderComponent_LIBS:%=/r:%) $(iFolderComponent_LIBPATH:%=/lib:%) $(iFolderComponent_RESOURCES_FILES:%=/res:%) $(iFolderComponent_CSFILES_CSC) $(DOC_FILE_CMD)
	
Novell.iFolderCom.%.resources: %.resx
	$(RESG) $< $@

%/iFolderComponent.resources.dll: Novell.iFolderCom.CreateiFolder.%.resources Novell.iFolderCom.ConflictResolver.%.resources Novell.iFolderCom.iFolderAdvanced.%.resources Novell.iFolderCom.MyMessageBox.%.resources Novell.iFolderCom.NewiFolder.%.resources Novell.iFolderCom.Picker.%.resources Novell.iFolderCom.UserProperties.%.resources Novell.iFolderCom.GaugeChart.%.resources
	$(mkinstalldirs) $*
	al /nologo /out:$@ /c:$* /template:iFolderComponent.dll $(patsubst %,/embed:%,$^)
	
if DEBUG
DEBUG_FILES = iFolderComponent.pdb vc70.pdb
endif

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) iFolderComponent.dll iFolderShell.dll $(srcdir)/iFolderShell.reg $(DEBUG_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(DATA_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(WINDOWS_FILES) $(DESTDIR)$(WINDIR_UNIX)
	$(mkinstalldirs) $(iFolderComponent_LANGUAGES:%=$(DESTDIR)$(bindir)/%)
	cp -r $(iFolderComponent_LANGUAGES) $(DESTDIR)$(bindir)
if DEBUG	
	$(INSTALL_PROGRAM) $(PROJECT_FILES) $(DESTDIR)$(libdir)
endif

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f iFolderComponent.dll iFolderShell.dll iFolderShell.reg $(DEBUG_FILES)
	cd $(DESTDIR)$(libdir); rm -f $(DATA_FILES:$(srcdir)/iFolderShell/%=%)
	cd $(DESTDIR)$(libdir); rm -f $(DATA_FILES:$(srcdir)/iFolderShell/%=%)
	cd $(DESTDIR)$(WINDIR_UNIX); rm -f $(WINDOWS_FILES)
	cd $(DESTDIR)$(bindir); rm -f $(iFolderComponent_RESOURCES_DLLS)
if DEBUG	
	cd $(DESTDIR)$(libdir); rm -f $(PROJECT_FILES:$(srcdir)/%=%)
endif

register:
	$(REGEDIT) /s `cygpath --windows $(DESTDIR)$(libdir)/iFolderShell.reg`
	$(REGSVR32) /s `cygpath --windows $(DESTDIR)$(libdir)/iFolderShell.dll`
	$(REGASM) `cygpath --windows $(DESTDIR)$(libdir)/iFolderComponent.dll` /codebase

unregister:
	-$(REGSVR32) /u /s `cygpath --windows $(DESTDIR)$(libdir)/iFolderShell.dll`
	$(REGASM) `cygpath --windows $(DESTDIR)$(libdir)/iFolderComponent.dll` /unregister

CLEAN_FILES = $(iFolderComponent_LANGUAGES) iFolderShell.dll iFolderComponent.tlb iFolderComponent.dll $(iFolderComponent_RESX_FILES:%.resx=Novell.iFolderCom.%.resources) $(iFolderShell_OBJFILES) $(iFolderShellRes_OBJFILES) iFolderShell/ifoldercomponent.tlh iFolderShell/ifoldercomponent.tli $(DOC_FILE_CLEAN) iFolderShell.exp iFolderShell.lib AssemblyInfo.cs iFolderShell/nifver.h

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

