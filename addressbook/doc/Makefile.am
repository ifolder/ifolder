.PHONY: addressbook-install ndoc ndoc-nodeps ndoc-clean

all:
	@echo "Novell.AddressBook APIs are currently in Simias, but will move here shortly"

#all: ndoc

#clean-local: ndoc-clean

addressbook-install:
	$(MAKE) -C .. install

if NDOC

ndoc: addressbook-install ndoc-nodeps

ndoc-nodeps:
	rm -rf addressbook-ndoc
if WINDOWS
	"$(NDOC_CMD)" `cygpath --windows "$(bindir)/Novell.Addressbook.dll"`,`cygpath --windows "../src/Addressbook/Novell.Addressbook.doc.xml"` -documenter=MSDN -Title=AddressBook -OutputDirectory=addressbook-ndoc
else # NOT WINDOWS
	"$(NDOC_CMD)" "$(bindir)/Novell.Addressbook.dll","../src/Addressbook/Novell.Addressbook.doc.xml" -documenter=MSDN -Title=AddressBook -OutputDirectory=addressbook-ndoc
endif # WINDOWS
	tar -czf addressbook-ndoc.tar.gz addressbook-ndoc

else # NDOC

ndoc:
	@echo NDOC is not configured

endif # NDOC

clean-local:
	rm -rf addressbook-ndoc addressbook-ndoc.tar.gz

maintainer-clean-local:
	rm -f Makefile.in
	

