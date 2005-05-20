# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolder_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderWebService.cs
iFolder_CSFILES_CSC := $(subst /,$(SEP),$(iFolder_CSFILES))
iFolder_INCLUDES =
iFolder_RESOURCES =
iFolder_FLAGS = $(CSC_LIBFLAG)
iFolder_LIBS = System.dll System.Web.Services.dll $(SYSTEM_XML)
iFolder_LIBPATH =

EXTRA_DIST = AssemblyInfo.cs.in

AssemblyInfo.cs: $(srcdir)/AssemblyInfo.cs.in
	sed -e 's,@_VERSION_@,@VERSION@,g' $< > $@
	chmod +x $@

CUR_DIR := $(shell pwd)

if MAKE_DOC_FILE
DOC_FILE = $(basename $@).doc.xml
DOC_FILE_CMD = /doc:$(DOC_FILE)
DOC_FILE_CLEAN = *.doc.xml
endif

all: Novell.iFolder.dll

$(srcdir)/iFolderWebService.cs: $(srcdir)/../WebService/iFolderWebService.wsdl
if WINDOWS
	wsdl $(srcdir)/../WebService/iFolderWebService.wsdl
endif
if LINUX
	wsdl -nologo -out:iFolderWebService.cs $(srcdir)/../WebService/iFolderWebService.wsdl
#	We used to have to do this because of the way the wdsl was being generated
#   But it all appears fine after switching to [SoapDocumentMethod]
#	cat iFolderWebService.cs | sed 's/iFolder1/iFolder/' | sed 's/iFolderUser1/iFolderUser/' | sed 's/DiskSpace1/DiskSpace/' > newsource.cs
#	mv newsource.cs iFolderWebService.cs
endif

Novell.iFolder.dll: $(iFolder_CSFILES) $(iFolder_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolder_FLAGS) $(iFolder_LIBS:%=/r:%) $(iFolder_LIBPATH:%=/lib:%) $(iFolder_CSFILES_CSC) $(DOC_FILE_CMD)

if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolder.pdb
endif
endif

install-exec-local: Novell.iFolder.dll
	$(mkinstalldirs) $(DESTDIR)$(libdir) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Novell.iFolder.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.dll $(DEBUG_FILES)

CLEAN_FILES = Novell.iFolder.dll $(DOC_FILE_CLEAN) AssemblyInfo.cs

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

