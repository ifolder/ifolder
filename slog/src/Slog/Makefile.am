# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SlogLib_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/Slog.cs $(srcdir)/SlogEntry.cs $(srcdir)/SlogManager.cs
SlogLib_CSFILES_CSC := $(subst /,$(SEP),$(SlogLib_CSFILES))
SlogLib_INCLUDES =
SlogLib_RESOURCES =
SlogLib_FLAGS = $(CSC_LIBFLAG)
SlogLib_LIBS = System.dll System.Data.dll $(SYSTEM_XML)
SlogLib_LIBPATH =

EXTRA_DIST = $(SlogLib_CSFILES)

CUR_DIR := $(shell pwd)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Slog.dll

Slog.dll: $(SlogLib_CSFILES) $(SlogLib_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_LIBS) $(SlogLib_FLAGS) $(SlogLib_LIBS:%=/r:%) $(SlogLib_LIBPATH:%=/lib:%) $(SlogLib_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Slog.pdb
endif
endif

install-exec-local: Slog.dll
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) Slog.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Slog.dll $(DEBUG_FILES)

CLEAN_FILES = Slog.dll $(DOC_FILE_CLEAN)

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

