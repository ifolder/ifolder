# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolder_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolder.cs $(srcdir)/iFolderNode.cs $(srcdir)/iFolderManager.cs
iFolder_CSFILES_CSC := $(subst /,$(SEP),$(iFolder_CSFILES))
iFolder_INCLUDES =
iFolder_RESOURCES =
iFolder_FLAGS = $(CSC_LIBFLAG)
iFolder_LIBS = System.dll $(SYSTEM_XML)
iFolder_LIBPATH =

iFolderCmd_CSFILES = $(srcdir)/iFolderCmd.cs
iFolderCmd_CSFILES_CSC := $(subst /,$(SEP),$(iFolderCmd_CSFILES))
iFolderCmd_INCLUDES =
iFolderCmd_RESOURCES =
iFolderCmd_FLAGS = $(CSC_EXEFLAG)
iFolderCmd_LIBS = System.dll $(SYSTEM_XML) Novell.iFolder.dll
iFolderCmd_LIBPATH =

iFolderd_CSFILES = $(srcdir)/iFolderDaemon.cs
iFolderd_CSFILES_CSC := $(subst /,$(SEP),$(iFolderd_CSFILES))
iFolderd_INCLUDES =
iFolderd_RESOURCES =
iFolderd_FLAGS = $(CSC_EXEFLAG)
iFolderd_LIBS = System.dll $(SYSTEM_XML) Novell.iFolder.dll
iFolderd_LIBPATH =

EXTRA_DIST = ifolders $(srcdir)/iFolderCmd $(iFolder_CSFILES) $(iFolderCmd_CSFILES) $(iFolderd_CSFILES)

CUR_DIR := $(shell pwd)

all: Novell.iFolder.dll iFolderCmd.exe iFolderd.exe

Novell.iFolder.dll: $(iFolder_CSFILES) $(iFolder_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolder_FLAGS) $(SIMIAS_LIBS) $(iFolder_LIBS:%=/r:%) $(iFolder_LIBPATH:%=/lib:%) $(iFolder_CSFILES_CSC) /doc:Novell.iFolder.doc.xml

iFolderCmd.exe: $(iFolderCmd_CSFILES) $(iFolderCmd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderCmd_FLAGS) $(SIMIAS_LIBS) $(iFolderCmd_LIBS:%=/r:%) $(iFolderCmd_LIBPATH:%=/lib:%) $(iFolderCmd_CSFILES_CSC) /doc:iFolderCmd.doc.xml

iFolderd.exe: $(iFolderd_CSFILES) $(iFolderd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderd_FLAGS) $(SIMIAS_LIBS) $(iFolderd_LIBS:%=/r:%) $(iFolderd_LIBPATH:%=/lib:%) $(iFolderd_CSFILES_CSC) /doc:iFolderd.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolder.pdb iFolderCmd.pdb iFolderd.pdb
endif
endif

install-exec-local: Novell.iFolder.dll iFolderCmd.exe iFolderd.exe
	$(mkinstalldirs) $(DESTDIR)$(libdir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Novell.iFolder.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) iFolderCmd.exe iFolderd.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderCmd ifolders $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.dll $(DEBUG_FILES)
	cd $(DESTDIR)$(bindir); rm -f iFolderCmd.exe iFolderCmd iFolderd.exe ifolders

CLEAN_FILES = Novell.iFolder.dll iFolderCmd.exe iFolderd.exe Novell.iFolder.doc.xml iFolderCmd.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

