# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

Slogger_CSFILES = $(srcdir)/SloggerApp.cs $(srcdir)/SlogEditor.cs $(srcdir)/Util.cs
Slogger_CSFILES_CSC := $(subst /,$(SEP),$(Slogger_CSFILES))
Slogger_INCLUDES = 
Slogger_RESOURCES =
Slogger_FLAGS = $(CSC_EXEFLAG)
Slogger_LIBS = System.dll System.Drawing.dll System.Data.dll Slog.dll Novell.AddressBook.UI.gtk.dll
Slogger_LIBPATH = $(srcdir)/../Slog

INSTALL_BIN_FILES = $(srcdir)/slogger

INSTALL_LIB_FILES =

GLADE_FILES = slogger.glade

IMAGE_FILES = 

MIME_FILES = 
APPLICATIONS_FILES = 

EXTRA_DIST = $(Slogger_CSFILES) $(GLADE_FILES) $(IMAGE_FILES) $(INSTALL_BIN_FILES) $(INSTALL_LIB_FILES) $(MIME_FILES) $(APPLICATIONS_FILES)

all: Slogger.exe

Slogger.exe: $(Slogger_CSFILES) $(Slogger_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Slogger_FLAGS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_LIBS) $(ADDRESSBOOK_LIBS) $(Slogger_LIBS:%=/r:%) $(Slogger_LIBPATH:%=/lib:%) $(Slogger_RESOURCES:%=$(ICON_FLAG)%) $(Slogger_CSFILES_CSC) /doc:Slogger.doc.xml

install-exec-local: all $(GLADE_FILES) $(IMAGE_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(libdir) $(DESTDIR)$(datadir)/slog/glade $(DESTDIR)$(datadir)/slog/images
	$(INSTALL_PROGRAM) $(GLADE_FILES) $(DESTDIR)$(datadir)/slog/glade
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) Slogger.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(datadir)/ifolder/glade; rm -f $(GLADE_FILES)
	cd $(DESTDIR)$(bindir); rm -f Slogger.exe $(notdir $(INSTALL_BIN_FILES))
    
CLEAN_FILES = Slogger.exe

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -f slogger

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

