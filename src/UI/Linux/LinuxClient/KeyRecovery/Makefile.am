# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
DIST_SUBDIRS = $(SUBDIRS)
KeyRecovery_CSFILES = \
        $(srcdir)/utility.cs \
	$(srcdir)/Options.cs \
	$(srcdir)/Prompt.cs 

	
KeyRecovery_CSFILES_CSC := $(subst /,$(SEP),$(KeyRecovery_CSFILES))
KeyRecovery_FLAGS = $(CSC_EXEFLAG)
KeyRecovery_LIBS = Mono.Security.dll
EXTRA_DIST = iFolderUtilitySetup_CSFILES KeyRecovery.in

KeyRecovery: $(srcdir)/KeyRecovery.in
if !WINDOWS
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@
endif		
	
	
if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: KeyRecovery.exe

KeyRecovery.exe: $(iFolderUtilitySetup_CSFILES) KeyRecovery 
	$(CSC) /out:$@ $(CSCFLAGS) $(KeyRecovery_LIBS:%=/r:%) $(KeyRecovery_CSFILES_CSC) $(DOC_FILE_CMD)


install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) KeyRecovery.exe $(DEBUG_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) KeyRecovery $(DEBUG_FILES) $(DESTDIR)$(bindir)


uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f KeyRecovery.exe KeyRecovery

installcheck-local: install
	
CLEAN_FILES = KeyRecovery.exe KeyRecovery

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

