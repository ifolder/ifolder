
#DLLDEPS=
#EXEFLAGS=$(DLLDEPS)

#ASSEMBLY1 = Configuration.dll
#ASSEMBLY1_CSFILES =		 					\
#	$(srcdir)/AssemblyInfo.cs				\
#	$(srcdir)/Configuration.cs

#ASSEMBLIES = $(ASSEMBLY1)

bin_PROGRAMS = Configuration.dll
dist_Configuration_dll_SOURCES = Configuration.cs AssemblyInfo.cs
#Configuration_dll_OBJECTS =
#Configuration_dll_LDADD = $(LDADD)
#Configuration_dll_DEPENDENCIES = 
#Configuration_dll_LDFLAGS = 
Configuration.dll: $(Configuration_dll_SOURCES)
	$(CSC) $(CSCFLAGS) $(CSC_LIBFLAG) -out:$@ $<

#all: $(ASSEMBLIES)


#clean:
#	rm -f *.dbg *.exe *.dll

#distclean: clean

#dashboardlibdir = $(prefix)/lib/dashboard

#install-data-local: $(bin_PROGRAMS)
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_DATA) $(bin_PROGRAMS) $(DESTDIR)$(bindir)

