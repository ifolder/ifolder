# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Storage_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/Syntax.cs $(srcdir)/XmlTags.cs $(srcdir)/BaseSchema.cs $(srcdir)/Search.cs
Storage_CSFILES_CSC := $(subst /,$(SEP),$(Storage_CSFILES))
Storage_INCLUDES = 
Storage_RESOURCES =
Storage_FLAGS = $(CSC_LIBFLAG)
Storage_LIBS = System.dll System.Data.dll $(SYSTEM_XML) Common.dll CollectionEvents.dll
Storage_LIBPATH = /opt/gnome2/lib /usr/lib ../Common ../CollectionEvents


EXTRA_DIST = $(Storage_CSFILES) 

CUR_DIR := $(shell pwd)

all: Storage.dll

Storage.dll: $(Storage_CSFILES) $(Storage_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Storage_FLAGS) $(Storage_LIBS:%=/r:%) $(Storage_LIBPATH:%=/lib:%) $(Storage_CSFILES_CSC) /doc:Storage.doc.xml

if DEBUG
if WINDOWS
DEBUG_FILES = Storage.pdb
endif
endif

install-exec-local: Storage.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Storage.dll $(DEBUG_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Storage.dll $(NUNIT_TESTS) $(DEBUG_FILES)
    
installcheck-local: install $(NUNIT_TESTS)
	
CLEAN_FILES = Storage.dll $(NUNIT_TESTS) Storage.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

