STOREPROVIDER_CSFILES = AssemblyInfo.cs CSPException.cs IProvider.cs IResultSet.cs Provider.cs Query.cs Record.cs
STOREPROVIDER_RESOURCES = Record.cs
STOREPROVIDER_LIB = /r:System.dll /r:System.Data.dll /r:Configuration.dll
STOREPROVIDER_LIBPATH = /lib:../Configuration

all: StoreProvider.dll 

StoreProvider.dll: $(STOREPROVIDER_CSFILES)
	$(CSC) $(CSCFLAGS) $(CSC_LIBFLAG) -out:$@ $(STOREPROVIDER_CSFILES) $(STOREPROVIDER_LIB) $(STOREPROVIDER_LIBPATH)

clean:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

install-exec-local: StoreProvider.dll $(STOREPROVIDER_RESOURCES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) StoreProvider.dll $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(STOREPROVIDER_RESOURCES) $(DESTDIR)$(bindir)
    
uninstall-local:
	rm -f $(DESTDIR)$(bindir)/StoreProvider.dll
	rm -f $(DESTDIR)$(bindir)/$(STOREPROVIDER_RESOURCES)
    
EXTRA_DIST = $(STOREPROVIDER_CSFILES)

CLEAN_FILES = StoreProvider.dll

