CSC=mcs -g -warnaserror
#dashboard_exe=../engine/dashboard.exe
DLLDEPS=
LIBFLAGS=-target:library $(DLLDEPS)
EXEFLAGS=$(DLLDEPS)

# The backends to build by default.
assemblies =        \
	Configuration.dll

ASSEMBLY_CSFILES =		 					\
	$(srcdir)/AssemblyInfo.cs				\
	$(srcdir)/Configuration.cs

all: $(assemblies)

%.dll: %.cs
	$(CSC) $(LIBFLAGS) -out:$@ $<

clean:
	rm -f *.dbg *.exe *.dll

distclean: clean

dashboardlibdir = $(prefix)/lib/dashboard

install-data-local: $(assemblies)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_DATA) $(assemblies) $(DESTDIR)$(bindir)

