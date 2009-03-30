# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
LdapProvider_CSFILES = \
    $(srcdir)/AssemblyInfo.cs \
    $(srcdir)/LdapSettings.cs \
    $(srcdir)/LdapSync.cs \
    $(srcdir)/ProxyPassword.cs \
    $(srcdir)/User.cs 
	
LdapProvider_CSFILES_CSC := $(subst /,$(SEP),$(LdapProvider_CSFILES))
LdapProvider_INCLUDES =
#LdapProvider_RESOURCES = $(srcdir)/App$(ICON_EXT)
LdapProvider_FLAGS = $(CSC_LIBFLAG)
LdapProvider_LIBPATH = ${webbindir} ../Simias.IdentitySync 
LdapProvider_LIBS1 = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Web.dll \
	System.Web.Services.dll \
	Novell.Directory.Ldap.dll \
	IdentityManagement.dll \
	SimiasLib.dll \
	SimiasClient.dll 

LdapProvider_LIBS := $(LdapProvider_LIBS1)

EXTRA_DIST = $(LdapProvider_CSFILES) AssemblyInfo.cs.in 

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: Simias.Identity.LdapProvider.dll 

Simias.Identity.LdapProvider.dll: $(LdapProvider_CSFILES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(LdapProvider_FLAGS) $(SIMIAS_LIBS) $(LdapProvider_LIBS:%=/r:%) $(LdapProvider_LIBPATH:%=/lib:%) $(LdapProvider_CSFILES_CSC)
    
#IdentityManagement.dll: $(IdentityManagement_CSFILES) $(IdentityManagement_RESOURCES) 
#	$(CSC) /out:$@ $(CSCFLAGS) $(IdentityManagement_FLAGS) $(SIMIAS_LIBS) $(IdentityManagement_LIBS:%=/r:%) $(IdentityManagement_LIBPATH:%=/lib:%) $(IdentityManagement_RESOURCES:%=$(ICON_FLAG)%) $(IdentityManagement_CSFILES_CSC)

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.Identity.LdapProvider.dll $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.Identity.LdapProvider.dll

installcheck-local: install
	
CLEAN_FILES = Simias.Identity.LdapProvider.dll AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

