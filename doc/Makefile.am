.PHONY: ifolder-install ndoc ndoc-nodeps ndoc-clean

all: ndoc

clean-local: ndoc-clean

ifolder-install:
	$(MAKE) -C .. install

if NDOC

ndoc: ifolder-install ndoc-nodeps

ndoc-nodeps:
	rm -rf ifolder-ndoc
if WINDOWS
	"$(NDOC_CMD)" `cygpath --windows "$(bindir)/Novell.iFolder.dll"`,`cygpath --windows "../src/iFolder/Novell.iFolder.doc.xml"` -documenter=MSDN -Title=iFolder -OutputDirectory=ifolder-ndoc
else # NOT WINDOWS
	"$(NDOC_CMD)" "$(bindir)/Novell.iFolder.dll","../src/iFolder/Novell.iFolder.doc.xml" -documenter=MSDN -Title=iFolder -OutputDirectory=ifolder-ndoc
endif # WINDOWS
	tar -czf ifolder-ndoc.tar.gz ifolder-ndoc

else # NDOC

ndoc:
	@echo NDOC is not configured
	mkdir -p ifolder-ndoc
	touch ifolder-ndoc/sdk_doc_was_not_built

endif # NDOC

clean-local:
	rm -rf ifolder-ndoc ifolder-ndoc.tar.gz

maintainer-clean-local:
	rm -f Makefile.in
	

