if WINDOWS
WINDIR_UNIX = $(shell cygpath --unix $$WINDIR)
REGSVR32 = $(shell cygpath --unix $$WINDIR\\system32\\Regsvr32.exe)
REGASM = $(shell cygpath --unix $$FRAMEWORKDIR\\v1.1*\\RegAsm.exe)
REGEDIT = regedit
endif

# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderComponent_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderAdvanced.cs $(srcdir)/ShareListMember.cs $(srcdir)/MyMessageBox.cs $(srcdir)/Win32Window.cs $(srcdir)/iFolderComponent.cs $(srcdir)/NewiFolder.cs $(srcdir)/UserProperties.cs $(srcdir)/iFolderInfo.cs $(srcdir)/GaugeChart.cs $(srcdir)/Picker.cs $(srcdir)/ConflictResolver.cs $(srcdir)/CreateiFolder.cs $(srcdir)/DomainItem.cs $(srcdir)/VirtualListView.cs
iFolderComponent_CSFILES_CSC := $(subst /,$(SEP),$(iFolderComponent_CSFILES))
iFolderComponent_INCLUDES =
iFolderComponent_RESOURCES =
iFolderComponent_RESOURCES_FILES = CreateiFolder.resources ConflictResolver.resources iFolderAdvanced.resources MyMessageBox.resources NewiFolder.resources Picker.resources UserProperties.resources GaugeChart.resources
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

EXTRA_DIST = $(iFolderComponent_CSFILES) $(iFolderShell_SRCFILES) $(iFolderShellRes_SRCFILES) $(iFolderShell_DEF) $(DATA_FILES) $(RESX_FILES) $(WINDOWS_FILES) $(srcdir)/iFolderShell.reg  $(PROJECT_FILES) AssemblyInfo.cs.in

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

if DEBUG
if WINDOWS
DEBUG_FILES = iFolderComponent.pdb vc70.pdb
endif
endif

install-exec-local: iFolderComponent.dll iFolderShell.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) iFolderComponent.dll iFolderShell.dll $(srcdir)/iFolderShell.reg $(DEBUG_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(DATA_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(WINDOWS_FILES) $(DESTDIR)$(WINDIR_UNIX)
if WINDOWS
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

