# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SqliteProvider_CSFILES := $(srcdir)/AssemblyInfo.cs $(srcdir)/SqliteProvider.cs $(srcdir)/SqliteResultSet.cs
SqliteProvider_CSFILES_CSC := $(subst /,$(SEP),$(SqliteProvider_CSFILES))
SqliteProvider_INCLUDES =
SqliteProvider_RESOURCES =
SqliteProvider_FLAGS = $(CSC_LIBFLAG)
SqliteProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll SimiasClient.dll SimiasLib.dll SqliteWrapper.dll
SqliteProvider_LIBPATH = ../SimiasLib.dll ../SimiasClient

SqliteWrapper_CSFILES := $(srcdir)/SqliteWrapper/AssemblyInfo.cs $(srcdir)/SqliteWrapper/SqliteCommand.cs $(srcdir)/SqliteWrapper/SqliteConnection.cs $(srcdir)/SqliteWrapper/SqliteDataReader.cs $(srcdir)/SqliteWrapper/SqliteParameter.cs $(srcdir)/SqliteWrapper/SqliteParameterCollection.cs $(srcdir)/SqliteWrapper/SqliteTransaction.cs
SqliteWrapper_CSFILES_CSC := $(subst /,$(SEP),$(SqliteWrapper_CSFILES))
SqliteWrapper_INCLUDES =
SqliteWrapper_RESOURCES =
SqliteWrapper_FLAGS = $(CSC_LIBFLAG) /unsafe
SqliteWrapper_LIBS = System.dll System.Data.dll $(SYSTEM_XML)
SqliteWrapper_LIBPATH =

NUNIT_TESTS := SqliteProviderTests.dll

SqliteProviderTests_CSFILES = $(srcdir)/SqliteProviderTests/AssemblyInfo.cs $(srcdir)/SqliteProviderTests/SqliteTests1.cs
SqliteProviderTests_CSFILES_CSC := $(subst /,$(SEP),$(SqliteProviderTests_CSFILES))
SqliteProviderTests_INCLUDES =
SqliteProviderTests_RESOURCES =
SqliteProviderTests_FLAGS = $(CSC_LIBFLAG)
SqliteProviderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll StoreProvider.dll ProviderTest.dll SqliteProvider.dll
SqliteProviderTests_LIBPATH = $(NUNIT_LIBPATH) ../StoreProvider

EXTRA_DIST = $(SqliteProvider_CSFILES) $(SqliteWrapper_CSFILES) $(SqliteProviderTests_CSFILES)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

CUR_DIR := $(shell pwd)

all: SqliteWrapper.dll SqliteProvider.dll

SqliteProvider.dll: $(SqliteProvider_CSFILES) $(SqliteProvider_RESOURCES) SqliteWrapper.dll
	$(CSC) /out:$@ $(CSCFLAGS) $(SqliteProvider_FLAGS) $(SqliteProvider_LIBS:%=/r:%) $(SqliteProvider_LIBPATH:%=/lib:%) $(SqliteProvider_CSFILES_CSC) $(DOC_FILE_CMD)

SqliteWrapper.dll: $(SqliteWrapper_CSFILES) $(SqliteWrapper_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SqliteWrapper_FLAGS) $(SqliteWrapper_LIBS:%=/r:%) $(SqliteWrapper_LIBPATH:%=/lib:%) $(SqliteWrapper_CSFILES_CSC) /doc:SqliteWrapper.doc.xml

SqliteProviderTests.dll: $(SqliteProviderTests_CSFILES) $(SqliteProviderTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SqliteProviderTests_FLAGS) $(SqliteProviderTests_LIBS:%=/r:%) $(SqliteProviderTests_LIBPATH:%=/lib:%) $(SqliteProviderTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = SqliteWrapper.pdb SqliteProvider.pdb
DEBUG_CHECK_FILES = SqliteProviderTests.pdb
endif
endif

install-exec-local: SqliteProvider.dll SqliteWrapper.dll $(srcdir)/$(STORELIB_PATH)/$(STORELIB)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) SqliteProvider.dll SqliteWrapper.dll $(srcdir)/$(STORELIB_PATH)/$(STORELIB) $(DEBUG_FILES) $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f SqliteProvider.dll SqliteWrapper.dll SqliteProviderTests.test.xml $(STORELIB) $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

CLEAN_FILES = SqliteProvider.dll SqliteWrapper.dll $(NUNIT_TESTS) $(DOC_FILE_CLEAN) SqliteProviderTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

