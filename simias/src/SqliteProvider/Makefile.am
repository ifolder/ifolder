# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

SqliteProvider_CSFILES := $(srcdir)/AssemblyInfo.cs $(srcdir)/SqliteProvider.cs $(srcdir)/SqliteResultSet.cs
SqliteProvider_CSFILES_CSC := $(subst /,$(SEP),$(SqliteProvider_CSFILES))
SqliteProvider_INCLUDES = 
SqliteProvider_RESOURCES =
SqliteProvider_FLAGS = $(CSC_LIBFLAG)
SqliteProvider_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll Configuration.dll Storage.dll StoreProvider.dll SqliteWrapper.dll 
SqliteProvider_LIBPATH = ../Configuration ../Storage ../StoreProvider

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
SqliteProviderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.framework.dll Configuration.dll StoreProvider.dll ProviderTest.dll SqliteProvider.dll
SqliteProviderTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin ../Configuration ../StoreProvider

EXTRA_DIST = $(SqliteProvider_CSFILES) $(SqliteWrapper_CSFILES) $(SqliteProviderTests_CSFILES)

CUR_DIR := $(shell pwd)

all: SqliteWrapper.dll SqliteProvider.dll

SqliteProvider.dll: $(SqliteProvider_CSFILES) $(SqliteProvider_RESOURCES) SqliteWrapper.dll
	$(CSC) /out:$@ $(CSCFLAGS) $(SqliteProvider_FLAGS) $(SqliteProvider_LIBS:%=/r:%) $(SqliteProvider_LIBPATH:%=/lib:%) $(SqliteProvider_CSFILES_CSC) /doc:SqliteProvider.doc.xml

SqliteWrapper.dll: $(SqliteWrapper_CSFILES) $(SqliteWrapper_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SqliteWrapper_FLAGS) $(SqliteWrapper_LIBS:%=/r:%) $(SqliteWrapper_LIBPATH:%=/lib:%) $(SqliteWrapper_CSFILES_CSC) /doc:SqliteWrapper.doc.xml

SqliteProviderTests.dll: $(SqliteProviderTests_CSFILES) $(SqliteProviderTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SqliteProviderTests_FLAGS) $(SqliteProviderTests_LIBS:%=/r:%) $(SqliteProviderTests_LIBPATH:%=/lib:%) $(SqliteProviderTests_CSFILES_CSC)
	
install-exec-local: SqliteProvider.dll SqliteWrapper.dll $(srcdir)/$(SQLITELIB_PATH)/$(SQLITELIB)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) SqliteProvider.dll SqliteWrapper.dll $(srcdir)/$(SQLITELIB_PATH)/$(SQLITELIB) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f SqliteProvider.dll SqliteWrapper.dll SqliteProviderTests.test.xml $(SQLITELIB) $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:SqliteProviderTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = SqliteProvider.dll SqliteWrapper.dll $(NUNIT_TESTS) SqliteProvider.doc.xml SqliteWrapper.doc.xml SqliteProviderTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

