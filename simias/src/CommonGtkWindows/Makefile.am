# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

CommonGtkWin_CSFILES =	$(srcdir)/GtkTraceWindow.cs
CommonGtkWin_CSFILES_CSC := $(subst /,$(SEP),$(CommonGtkWin_CSFILES))
CommonGtkWin_INCLUDES = 
CommonGtkWin_RESOURCES =
CommonGtkWin_FLAGS = $(CSC_LIBFLAG)
CommonGtkWin_LIBS = System.dll System.Data.dll System.Drawing.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll gdk-sharp.dll $(SYSTEM_XML)
CommonGtkWin_LIBPATH = /opt/gnome2/lib /usr/lib

NUNIT_TESTS := CommonGtkWinTests.exe

CommonGtkWinTests_CSFILES =	$(srcdir)/GtkTraceWindowTests.cs
CommonGtkWinTests_CSFILES_CSC := $(subst /,$(SEP),$(CommonGtkWinTests_CSFILES))
CommonGtkWinTests_INCLUDES = 
CommonGtkWinTests_RESOURCES =
CommonGtkWinTests_FLAGS = $(CSC_EXEFLAG)
CommonGtkWinTests_LIBS = System.dll System.Data.dll System.Drawing.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll gdk-sharp.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll CommonGtkWindows.dll Configuration.dll Common.dll
CommonGtkWinTests_LIBPATH = $(NUNIT_LIBPATH) ../Configuration ../Common

EXTRA_DIST = $(CommonGtkWin_CSFILES) $(CommonGtkWinTests_CSFILES)

CUR_DIR := $(shell pwd)

all: CommonGtkWindows.dll

CommonGtkWindows.dll: $(CommonGtkWin_CSFILES) $(CommonGtkWin_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CommonGtkWin_FLAGS) $(CommonGtkWin_LIBS:%=/r:%) $(CommonGtkWin_LIBPATH:%=/lib:%) $(CommonGtkWin_CSFILES_CSC) /doc:CommonGtkWindows.doc.xml

CommonGtkWinTests.exe: $(CommonGtkWinTests_CSFILES) $(CommonGtkWinTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CommonGtkWinTests_FLAGS) $(CommonGtkWinTests_LIBS:%=/r:%) $(CommonGtkWinTests_LIBPATH:%=/lib:%) $(CommonGtkWinTests_CSFILES_CSC)
	
install-exec-local: CommonGtkWindows.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) CommonGtkWindows.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f CommonGtkWindows.dll CommonGtkWinTests.test.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:CommonGtkWinTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = CommonGtkWindows.dll $(NUNIT_TESTS) CommonGtkWindows.doc.xml CommonGtkWinTests.test.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

