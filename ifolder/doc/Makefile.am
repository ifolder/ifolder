.PHONY: $(PACKAGE)-install ndoc ndoc-nodeps ndoc-clean

DOC_TARBALL = $(PACKAGE)-doc-$(VERSION).tar.gz

all: ndoc

all-nodeps: ndoc-nodeps

clean-local: ndoc-clean

$(PACKAGE)-install:
	$(MAKE) -C .. install

if NDOC

ndoc: $(PACKAGE)-install ndoc-nodeps

ndoc-nodeps:
	rm -rf $(PACKAGE)-ndoc
if WINDOWS
	"$(NDOC_CMD)" `cygpath --windows "$(bindir)/Novell.iFolder.dll"`,`cygpath --windows "../src/iFolder/Novell.iFolder.doc.xml"` -documenter=MSDN -Title=iFolder -OutputDirectory=ifolder-ndoc
else # NOT WINDOWS
	"$(NDOC_CMD)" "$(bindir)/Novell.iFolder.dll","../src/iFolder/Novell.iFolder.doc.xml" -documenter=MSDN -Title=iFolder -OutputDirectory=ifolder-ndoc
endif # WINDOWS
	tar -czf $(DOC_TARBALL) $(PACKAGE)-ndoc

else # NDOC

ndoc ndoc-nodeps:
	@echo NDOC is not configured
	mkdir -p $(PACKAGE)-ndoc
	touch $(PACKAGE)-ndoc/sdk_doc_was_not_built

endif # NDOC

ndoc-clean:
	rm -rf $(PACKAGE)-ndoc $(PACKAGE)-doc-*.tar.gz

maintainer-clean-local:
	rm -f Makefile.in
	

