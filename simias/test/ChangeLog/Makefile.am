# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

ChangeLogTest_CSFILES = $(srcdir)/ChangeLogTest.cs $(srcdir)/AssemblyInfo.cs
ChangeLogTest_CSFILES_CSC := $(subst /,$(SEP),$(ChangeLogTest_CSFILES))
ChangeLogTest_INCLUDES =
ChangeLogTest_RESOURCES =
ChangeLogTest_FLAGS = $(CSC_LIBFLAG)
ChangeLogTest_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll SimiasLib.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
ChangeLogTest_LIBPATH = ../../src/SimiasLib.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(ChangeLogTest_CSFILES)

NUNIT_TESTS := ChangeLogTest.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

ChangeLogTest.dll: $(ChangeLogTest_CSFILES) $(ChangeLogTest_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ChangeLogTest_FLAGS) $(ChangeLogTest_LIBS:%=/r:%) $(ChangeLogTest_LIBPATH:%=/lib:%) $(ChangeLogTest_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = ChangeLogTest.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(webbindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(webbindir); $(NUNIT) $(NUNIT_FLAGS) /xml:ChangeLogTest.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) ChangeLogTest.doc.xml ChangeLogTest.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

