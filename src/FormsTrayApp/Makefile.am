# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderApp_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/FormsTrayApp.cs $(srcdir)/Win32Window.cs
iFolderApp_CSFILES_CSC := $(subst /,$(SEP),$(iFolderApp_CSFILES))
iFolderApp_INCLUDES = 
iFolderApp_RESOURCES = $(srcdir)/App$(ICON_EXT)
iFolderApp_FLAGS = $(CSC_WINEXEFLAG)
iFolderApp_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) Sync.dll SyncManager.dll SyncCommon.dll CommonFormWindows.dll Common.dll SyncLogicLite.dll Configuration.dll CollectionEvents.dll UserService.dll
iFolderApp_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin

DATA_FILES = res/ifolder_disconnect.ico res/ifolder_loaded.ico res/ifolder_message.ico res/ifolder_sync1.ico res/ifolder_sync2.ico res/ifolder_sync3.ico res/ifolder_sync4.ico res/ifolder_sync5.ico res/ifolder_sync6.ico res/ifolder_sync7.ico res/ifolder_sync8.ico res/ifolder_sync9.ico res/ifolder_sync10.ico  

EXTRA_DIST = $(iFolderApp_CSFILES) $(DATA_FILES)

all: iFolderApp.exe

iFolderApp.exe: $(iFolderApp_CSFILES) $(iFolderApp_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderApp_FLAGS) $(iFolderApp_LIBS:%=/r:%) $(iFolderApp_LIBPATH:%=/lib:%) $(iFolderApp_RESOURCES:%=$(ICON_FLAG)%) $(iFolderApp_CSFILES_CSC) /doc:iFolderApp.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = iFolderApp.pdb
endif
endif

install-exec-local: iFolderApp.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(bindir)/res
	$(INSTALL_PROGRAM) iFolderApp.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(DATA_FILES) $(DESTDIR)$(bindir)/res
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderApp.exe $(DATA_FILES) $(DEBUG_FILES)
    
installcheck-local: install
	
CLEAN_FILES = iFolderApp.exe iFolderApp.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

