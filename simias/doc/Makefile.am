.PHONY: simias-install ndoc ndoc-nodeps ndoc-clean

DOC_TARBALL = $(PACKAGE)-doc-$(VERSION).tar.gz

all: ndoc

all-nodeps: ndoc-nodeps

clean-local: ndoc-clean

simias-install:
	$(MAKE) -C .. install

if NDOC

ndoc: simias-install ndoc-nodeps

ndoc-nodeps:
	rm -rf simias-ndoc
if WINDOWS
	"$(NDOC_CMD)" \
		`cygpath --windows "$(bindir)/Simias.dll"`,`cygpath --windows "../src/Simias.dll/Simias.doc.xml"` \
		`cygpath --windows "$(bindir)/Novell.AddressBook.dll"`,`cygpath --windows "../src/AddressBook/AddressBook.doc.xml"` \
		-documenter=MSDN -Title=Simias -OutputDirectory=simias-ndoc
else # NOT WINDOWS
	"$(NDOC_CMD)" \
		"$(bindir)/Simias.dll","../src/Simias.dll/Simias.doc.xml" \
		"$(bindir)/Novell.AddressBook.dll","../src/AddressBook/AddressBook.doc.xml" \
		-documenter=MSDN -Title=Simias -OutputDirectory=simias-ndoc
endif # WINDOWS
	tar -czf $(DOC_TARBALL) simias-ndoc

else # NDOC

ndoc ndoc-nodeps:
	@echo NDOC is not configured
	mkdir -p simias-ndoc
	touch simias-ndoc/sdk_doc_was_not_built

endif # NDOC

ndoc-clean:
	rm -rf simias-ndoc $(PACKAGE)-doc-*.tar.gz

maintainer-clean-local:
	rm -f Makefile.in
	

