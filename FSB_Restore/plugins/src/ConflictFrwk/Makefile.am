# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)
SUBDIRS = 
EnhancedConflictResolution_CSFILES = \
	$(srcdir)/iFolderEnhancedConflictDialog.cs \
	$(srcdir)/AssemblyInfo.cs 	
EnhancedConflictResolution_CSFILES_CSC := $(subst /,$(SEP),$(EnhancedConflictResolution_CSFILES))
EnhancedConflictResolution_INCLUDES =
EnhancedConflictResolution_FLAGS = $(CSC_LIBFLAG)
EnhancedConflictResolution_LIBPATH = ${libdir} $(webbindir) 
EnhancedConflictResolution_LIBS1 = \
	System.dll \
	System.Data.dll \
	$(SYSTEM_XML) \
	System.Web.dll \
	System.Web.Services.dll \
	SimiasClient.dll \
	Novell.iFolder.dll \
	Novell.iFolder.UI.gtk.dll 

EnhancedConflictResolution_LIBS := $(EnhancedConflictResolution_LIBS1)

EXTRA_DIST = $(EnhancedConflictResolution_CSFILES)  AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in Makefile
	sed -e 's,@_VERSION_@,@VERSION@,g' $<  > $@
	chmod +x $@

all: EnhancedConflictResolution.dll 

EnhancedConflictResolution.dll: $(EnhancedConflictResolution_CSFILES) 
	$(CSC) /out:$@ $(CSCFLAGS) $(EnhancedConflictResolution_FLAGS) $(EnhancedConflictResolution_LIBS:%=/r:%) $(EnhancedConflictResolution_LIBPATH:%=/lib:%) $(GTK_SHARP_LIBS) $(EnhancedConflictResolution_RESOURCES:%=$(ICON_FLAG)%) $(EnhancedConflictResolution_CSFILES_CSC)

install-exec-local: all $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)/plugins
	$(INSTALL_PROGRAM) EnhancedConflictResolution.dll $(DESTDIR)$(libdir)/plugins

uninstall-local:
	cd $(DESTDIR)$(libdir)/plugins; rm -f EnhancedConflictResolution.dll 

installcheck-local: install
	
CLEAN_FILES = EnhancedConflictResolution.dll AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
	
distclean: distclean-local

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

