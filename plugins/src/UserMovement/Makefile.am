# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
UserMovement_CSFILES = \
    $(srcdir)/AssemblyInfo.cs \
    $(srcdir)/Service.cs \
    $(srcdir)/iFolderDataMove.cs \
    $(srcdir)/iFolderUserMove.cs
	
UserMovement_CSFILES_CSC := $(subst /,$(SEP),$(UserMovement_CSFILES))
UserMovement_INCLUDES =
#UserMovement_RESOURCES = $(srcdir)/App$(ICON_EXT)
UserMovement_FLAGS = $(CSC_LIBFLAG)
UserMovement_LIBPATH = ${webbindir} 
UserMovement_LIBS1 = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Web.dll \
	System.Web.Services.dll \
	SimiasLib.dll \
	SimiasClient.dll \
	Simias.Server.dll 

UserMovement_LIBS := $(UserMovement_LIBS1)

EXTRA_DIST = $(UserMovement_CSFILES) AssemblyInfo.cs.in 

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@
	
all: UserMovement.dll 

UserMovement.dll: $(UserMovement_CSFILES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(UserMovement_FLAGS) $(SIMIAS_LIBS) $(UserMovement_LIBS:%=/r:%) $(UserMovement_LIBPATH:%=/lib:%) $(UserMovement_CSFILES_CSC)
    
#UserMovement.dll: $(UserMovement_CSFILES) $(UserMovement_RESOURCES) 
#	$(CSC) /out:$@ $(CSCFLAGS) $(UserMovement_FLAGS) $(SIMIAS_LIBS) $(UserMovement_LIBS:%=/r:%) $(UserMovement_LIBPATH:%=/lib:%) $(UserMovement_RESOURCES:%=$(ICON_FLAG)%) $(UserMovement_CSFILES_CSC)

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(webbindir)
	$(mkinstalldirs) $(DESTDIR)$(sysconfdir)/simias/bill/modules
	$(INSTALL_PROGRAM) UserMovement.dll $(DESTDIR)$(webbindir)
	$(INSTALL_PROGRAM) UserMovement.conf $(DESTDIR)$(sysconfdir)/simias/bill/modules/

uninstall-local:
	cd $(DESTDIR)$(webbindir); rm -f UserMovement.dll
	cd $(DESTDIR)$(sysconfdir)/simias/bill/modules/; rm -f UserMovement.conf

installcheck-local: install
	
CLEAN_FILES = UserMovement.dll AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

