# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
AutoAccount_CSFILES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/AutoAccount.cs \
	$(srcdir)/EnterPasswordDialog.cs \
	$(srcdir)/AutoAccountHelper.cs
	
AutoAccount_CSFILES_CSC := $(subst /,$(SEP),$(AutoAccount_CSFILES))
AutoAccount_INCLUDES =
#AutoAccount_RESOURCES = $(srcdir)/App$(ICON_EXT)
AutoAccount_FLAGS = $(CSC_LIBFLAG)
AutoAccount_LIBPATH = ${libdir} ${webbindir}
AutoAccount_LIBS1 = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Web.dll \
	System.Web.Services.dll \
	SimiasClient.dll \
	Novell.iFolder.dll \
	Mono.Posix.dll \
	Novell.iFolder.UI.gtk.dll 

AutoAccount_LIBS := $(AutoAccount_LIBS1)

EXTRA_DIST = $(AutoAccount_CSFILES) ../common/AutoAccount.xsd ../common/AutoAccount.xml AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile 
	sed -e 's,@_VERSION_@,@VERSION@,g' $<  > $@
	chmod +x $@

all: Novell.AutoAccount.AutoAccountCreator.dll 

Novell.AutoAccount.AutoAccountCreator.dll: $(AutoAccount_CSFILES) $(AutoAccount_RESOURCES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(AutoAccount_FLAGS) $(SIMIAS_LIBS) $(AutoAccount_LIBS:%=/r:%) $(AutoAccount_LIBPATH:%=/lib:%) $(GTK_SHARP_LIBS) $(AutoAccount_RESOURCES:%=$(ICON_FLAG)%) $(AutoAccount_CSFILES_CSC)

#Novell.AutoAccount.AutoAccountCreator.dll: $(AutoAccount_CSFILES) $(AutoAccount_RESOURCES) 
#	$(CSC) /out:$@ $(CSCFLAGS) $(AutoAccount_FLAGS) $(SIMIAS_LIBS) $(AutoAccount_LIBS:%=/r:%) $(AutoAccount_LIBPATH:%=/lib:%) $(AutoAccount_RESOURCES:%=$(ICON_FLAG)%) $(AutoAccount_CSFILES_CSC)

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)/plugins
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Novell.AutoAccount.AutoAccountCreator.dll $(DESTDIR)$(libdir)/plugins
	$(INSTALL_PROGRAM) ../common/AutoAccount.xsd $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ../common/AutoAccount.xml $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(libdir)/plugins; rm -f Novell.AutoAccount.AutoAccountCreator.dll 
	cd $(DESTDIR)$(bindir); rm -f AutoAccount.xsd AutoAccount.xml

installcheck-local: install
	
CLEAN_FILES = Novell.AutoAccount.AutoAccountCreator.dll AssemblyInfo.cs 

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

