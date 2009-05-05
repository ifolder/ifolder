# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
IdentityManagement_CSFILES = \
    $(srcdir)/AssemblyInfo.cs \
    $(srcdir)/Service.cs \
    $(srcdir)/Domain.cs \
    $(srcdir)/Authentication.cs \
    $(srcdir)/Exceptions.cs \
    $(srcdir)/ProvisionInfo.cs \
    $(srcdir)/HostLocationService.cs \
    $(srcdir)/HostProvider.cs \
    $(srcdir)/IdentitySyncService.cs \
    $(srcdir)/IIdentitySyncProvider.cs \
    $(srcdir)/IdentitySyncManager.asmx.cs \
    $(srcdir)/IProvisionUser.cs \
    $(srcdir)/Registration.asmx.cs \
    $(srcdir)/IUserProvider.cs \
    $(srcdir)/User.cs 
	
IdentityManagement_CSFILES_CSC := $(subst /,$(SEP),$(IdentityManagement_CSFILES))
IdentityManagement_INCLUDES =
#IdentityManagement_RESOURCES = $(srcdir)/App$(ICON_EXT)
IdentityManagement_FLAGS = $(CSC_LIBFLAG)
IdentityManagement_LIBPATH = ${webbindir} 
IdentityManagement_LIBS1 = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Web.dll \
	System.Web.Services.dll \
	SimiasLib.dll \
	SimiasClient.dll 

IdentityManagement_LIBS := $(IdentityManagement_LIBS1)

EXTRA_DIST = $(IdentityManagement_CSFILES) AssemblyInfo.cs.in 

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: IdentityManagement.dll 

IdentityManagement.dll: $(IdentityManagement_CSFILES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(IdentityManagement_FLAGS) $(SIMIAS_LIBS) $(IdentityManagement_LIBS:%=/r:%) $(IdentityManagement_LIBPATH:%=/lib:%) $(IdentityManagement_CSFILES_CSC)
    
#IdentityManagement.dll: $(IdentityManagement_CSFILES) $(IdentityManagement_RESOURCES) 
#	$(CSC) /out:$@ $(CSCFLAGS) $(IdentityManagement_FLAGS) $(SIMIAS_LIBS) $(IdentityManagement_LIBS:%=/r:%) $(IdentityManagement_LIBPATH:%=/lib:%) $(IdentityManagement_RESOURCES:%=$(ICON_FLAG)%) $(IdentityManagement_CSFILES_CSC)

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) IdentityManagement.dll $(DESTDIR)$(webbindir)

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f IdentityManagement.dll

installcheck-local: install
	
CLEAN_FILES = IdentityManagement.dll AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

