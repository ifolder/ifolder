.PHONY: addressbook-install ndoc ndoc-nodeps ndoc-clean

DOC_TARBALL = $(PACKAGE)-doc-$(VERSION).tar.gz

all: ndoc

all-nodeps: ndoc-nodeps

clean-local: ndoc-clean

addressbook-install:
	$(MAKE) -C .. install

if NDOC

ndoc: addressbook-install ndoc-nodeps

ndoc-nodeps:
	rm -rf addressbook-ndoc
	
	@echo Novell.AddressBook.dll doc is in simias for now
	mkdir -p addressbook-ndoc
	touch addressbook-ndoc/sdk_doc_for_addressbook_in_simias_for_now
#if WINDOWS
#	"$(NDOC_CMD)" `cygpath --windows "$(bindir)/Novell.Addressbook.dll"`,`cygpath --windows "../src/Addressbook/Novell.Addressbook.doc.xml"` -documenter=MSDN -Title=AddressBook -OutputDirectory=addressbook-ndoc
#else # NOT WINDOWS
#	"$(NDOC_CMD)" "$(bindir)/Novell.Addressbook.dll","../src/Addressbook/Novell.Addressbook.doc.xml" -documenter=MSDN -Title=AddressBook -OutputDirectory=addressbook-ndoc
#endif # WINDOWS
	tar -czf $(DOC_TARBALL) addressbook-ndoc

else # NDOC

ndoc ndoc-nodeps:
	@echo NDOC is not configured
	mkdir -p addressbook-ndoc
	touch addressbook-ndoc/sdk_doc_was_not_built

endif # NDOC

ndoc-clean:
	rm -rf addressbook-ndoc $(PACKAGE)-doc-*.tar.gz

maintainer-clean-local:
	rm -f Makefile.in
	

