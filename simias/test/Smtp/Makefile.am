# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH):$(DESTDIR)$(libdir)

SmtpTests_CSFILES = $(srcdir)/SmtpTests.cs $(srcdir)/AssemblyInfo.cs
SmtpTests_CSFILES_CSC := $(subst /,$(SEP),$(SmtpTests_CSFILES))
SmtpTests_INCLUDES =
SmtpTests_RESOURCES = smtptst1.gif smtptst2.gif
SmtpTests_FLAGS = $(CSC_LIBFLAG)
SmtpTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Runtime.Remoting.dll SimiasLib.dll Novell.Security.SecureSink.dll Novell.Security.SecureSink.SecurityProvider.RsaKeyStore.dll Novell.Security.SecureSink.SecurityProvider.dll Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider.dll nunit.framework.dll
SmtpTests_LIBPATH = ../../src/SimiasLib.dll $(srcdir)/../../external/SecureChannelSinks $(NUNIT_LIBPATH)

EXTRA_DIST = $(SmtpTests_CSFILES) $(SmtpTests_RESOURCES)

NUNIT_TESTS = SmtpTests.dll

CUR_DIR := $(shell pwd)

all: $(NUNIT_TESTS)

SmtpTests.dll: $(SmtpTests_CSFILES) $(SmtpTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SmtpTests_FLAGS) $(SmtpTests_LIBS:%=/r:%) $(SmtpTests_LIBPATH:%=/lib:%) $(SmtpTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_CHECK_FILES = SmtpTests.pdb
endif
endif

install-exec-local: all
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(SmtpTests_RESOURCES) $(DESTDIR)$(bindir)

uninstall-local:

installcheck-local: install
	cd $(DESTDIR)$(bindir); $(NUNIT) $(NUNIT_FLAGS) /xml:SmtpTests.test.xml $(NUNIT_TESTS)
	
.PHONY: test
test: installcheck

CLEAN_FILES = $(NUNIT_TESTS) SmtpTests.doc.xml SmtpTests.test.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

