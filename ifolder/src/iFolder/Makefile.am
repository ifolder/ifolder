# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolder_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderWebService.cs
iFolder_CSFILES_CSC := $(subst /,$(SEP),$(iFolder_CSFILES))
iFolder_INCLUDES =
iFolder_RESOURCES =
iFolder_FLAGS = $(CSC_LIBFLAG)
iFolder_LIBS = System.dll System.Web.Services.dll $(SYSTEM_XML)
iFolder_LIBPATH =

EXTRA_DIST = $(iFolder_CSFILES)

CUR_DIR := $(shell pwd)

all: Novell.iFolder.dll

Novell.iFolder.dll: $(iFolder_CSFILES) $(iFolder_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolder_FLAGS) $(iFolder_LIBS:%=/r:%) $(iFolder_LIBPATH:%=/lib:%) $(iFolder_CSFILES_CSC) /doc:Novell.iFolder.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolder.pdb
endif
endif

install-exec-local: Novell.iFolder.dll
	$(mkinstalldirs) $(DESTDIR)$(libdir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Novell.iFolder.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.dll $(DEBUG_FILES)

CLEAN_FILES = Novell.iFolder.dll Novell.iFolder.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

