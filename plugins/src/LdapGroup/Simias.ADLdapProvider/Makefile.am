# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
ADLdapProvider_CSFILES = \
    $(srcdir)/AssemblyInfo.cs \
    $(srcdir)/ADLdapSync.cs \
    $(srcdir)/User.cs 
	
ADLdapProvider_CSFILES_CSC := $(subst /,$(SEP),$(ADLdapProvider_CSFILES))
ADLdapProvider_INCLUDES =
#ADLdapProvider_RESOURCES = $(srcdir)/App$(ICON_EXT)
ADLdapProvider_FLAGS = $(CSC_LIBFLAG)
ADLdapProvider_LIBPATH = ${webbindir} ../Simias.IdentitySync  ../Simias.LdapProvider 
ADLdapProvider_LIBS1 = \
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

ADLdapProvider_LIBS := $(ADLdapProvider_LIBS1)

EXTRA_DIST = $(ADLdapProvider_CSFILES) AssemblyInfo.cs.in 

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: Simias.Identity.ADLdapProvider.dll 

Simias.Identity.ADLdapProvider.dll: $(ADLdapProvider_CSFILES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(ADLdapProvider_FLAGS) $(SIMIAS_LIBS) $(ADLdapProvider_LIBS:%=/r:%) $(ADLdapProvider_LIBPATH:%=/lib:%) $(ADLdapProvider_CSFILES_CSC)
    
#IdentityManagement.dll: $(IdentityManagement_CSFILES) $(IdentityManagement_RESOURCES) 
#	$(CSC) /out:$@ $(CSCFLAGS) $(IdentityManagement_FLAGS) $(SIMIAS_LIBS) $(IdentityManagement_LIBS:%=/r:%) $(IdentityManagement_LIBPATH:%=/lib:%) $(IdentityManagement_RESOURCES:%=$(ICON_FLAG)%) $(IdentityManagement_CSFILES_CSC)

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) Simias.Identity.ADLdapProvider.dll $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f Simias.Identity.ADLdapProvider.dll

installcheck-local: install
	
CLEAN_FILES = Simias.Identity.ADLdapProvider.dll AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

