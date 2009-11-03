# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
UserAdd_CSFILES = \
    $(srcdir)/UserAdd.cs 
	
UserAdd_CSFILES_CSC := $(subst /,$(SEP),$(UserAdd_CSFILES))
UserAdd_INCLUDES =
#UserAdd_RESOURCES = $(srcdir)/App$(ICON_EXT)
#UserAdd_FLAGS = $(CSC_LIBFLAG)
UserAdd_FLAGS = 
UserAdd_LIBPATH = ${webbindir} 
UserAdd_LIBS1 = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Web.dll \
	System.Web.Services.dll \
	Novell.Directory.Ldap.dll \
	Simias.Server.dll \
	Mono.Security.dll \
	SimiasLib.dll \
	SimiasClient.dll \
	Simias.LdapProvider.dll

UserAdd_LIBS := $(UserAdd_LIBS1)

EXTRA_DIST = $(UserAdd_CSFILES) AssemblyInfo.cs.in iFolderLdapUserUpdate.sh.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: UserAdd.exe 

iFolderLdapUserUpdate.sh: iFolderLdapUserUpdate.sh.in
	sed -e 's,@_webbindir_@,@webbindir@,g' -e 's,@_bindir_@,@bindir@,g' $< > $@
	chmod +x $@

UserAdd.exe: $(UserAdd_CSFILES)iFolderLdapUserUpdate.sh 
	$(CSC) /out:$@ $(CSCFLAGS) $(UserAdd_FLAGS) $(SIMIAS_LIBS) $(UserAdd_LIBS:%=/r:%) $(UserAdd_LIBPATH:%=/lib:%) $(UserAdd_CSFILES_CSC)
    
install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) UserAdd.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderLdapUserUpdate.sh $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f UserAdd.exe iFolderLdapUserUpdate.sh

installcheck-local: install
	
CLEAN_FILES = UserAdd.exe AssemblyInfo.cs iFolderLdapUserUpdate.sh

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

