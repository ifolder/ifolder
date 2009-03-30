# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
OpenLdapProvider_CSFILES = \
    $(srcdir)/AssemblyInfo.cs \
    $(srcdir)/OpenLdapSync.cs \
    $(srcdir)/User.cs 
	
OpenLdapProvider_CSFILES_CSC := $(subst /,$(SEP),$(OpenLdapProvider_CSFILES))
OpenLdapProvider_INCLUDES =
#OpenLdapProvider_RESOURCES = $(srcdir)/App$(ICON_EXT)
OpenLdapProvider_FLAGS = $(CSC_LIBFLAG)
OpenLdapProvider_LIBPATH = ${webbindir} ../Simias.IdentitySync  ../Simias.LdapProvider 
OpenLdapProvider_LIBS1 = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Web.dll \
	System.Web.Services.dll \
	Novell.Directory.Ldap.dll \
	IdentityManagement.dll \
	Simias.Identity.LdapProvider.dll \
	SimiasLib.dll \
	SimiasClient.dll 

OpenLdapProvider_LIBS := $(OpenLdapProvider_LIBS1)

EXTRA_DIST = $(OpenLdapProvider_CSFILES) AssemblyInfo.cs.in 

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: Simias.Identity.OpenLdapProvider.dll 

Simias.Identity.OpenLdapProvider.dll: $(OpenLdapProvider_CSFILES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(OpenLdapProvider_FLAGS) $(SIMIAS_LIBS) $(OpenLdapProvider_LIBS:%=/r:%) $(OpenLdapProvider_LIBPATH:%=/lib:%) $(OpenLdapProvider_CSFILES_CSC)
    
#IdentityManagement.dll: $(IdentityManagement_CSFILES) $(IdentityManagement_RESOURCES) 
#	$(CSC) /out:$@ $(CSCFLAGS) $(IdentityManagement_FLAGS) $(SIMIAS_LIBS) $(IdentityManagement_LIBS:%=/r:%) $(IdentityManagement_LIBPATH:%=/lib:%) $(IdentityManagement_RESOURCES:%=$(ICON_FLAG)%) $(IdentityManagement_CSFILES_CSC)

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.Identity.OpenLdapProvider.dll $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.Identity.OpenLdapProvider.dll

installcheck-local: install
	
CLEAN_FILES = Simias.Identity.OpenLdapProvider.dll AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

