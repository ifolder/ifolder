CONFIGURATION_CSFILES = Configuration.cs AssemblyInfo.cs

all: Configuration.dll

Configuration.dll: $(CONFIGURATION_CSFILES)
	$(CSC) $(CSCFLAGS) $(CSC_LIBFLAG) -out:$@ $(CONFIGURATION_CSFILES)

clean:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

install-exec-local: Configuration.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Configuration.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	rm -f $(DESTDIR)$(bindir)/Configuration.dll
    
EXTRA_DIST = $(CONFIGURATION_CSFILES)

CLEAN_FILES = Configuration.dll

