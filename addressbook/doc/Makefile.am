.PHONY: addressbook-install ndoc ndoc-nodeps ndoc-clean

all clean-local:
	@echo There are currently no APIs to document for Address Book

#all: ndoc

#clean-local: ndoc-clean

addressbook-install:
	$(MAKE) -C .. install

ASSEMBLIES = FormsBookLib

if NDOC

ndoc: addressbook-install ndoc-nodeps

ndoc-nodeps: ndoc-clean
	@echo "$(NDOC) ... -documenter=MSDN -Title=\"Address Book\" -OutputDirectory=ndoc"
if WINDOWS
	@addressbook_dir=`cd .. && pwd`; \
	cd $(bindir); \
	for asmbly in $(ASSEMBLIES); do \
		doc_xml=`cygpath --windows "$$addressbook_dir/src/$$asmbly/$$asmbly.doc.xml"`; \
		asmbly=`cygpath --windows "$(bindir)/$$asmbly.dll"`; \
		asmbly_list="$$asmbly_list $$asmbly,$$doc_xml"; \
	done; \
	output_dir=`cygpath --windows "$$addressbook_dir/doc/ndoc"`; \
	"$(NDOC)" $$asmbly_list -documenter=MSDN -Title="Address Book" -OutputDirectory=$$output_dir
else # NOT WINDOWS
	@addressbook_dir=`cd .. && pwd`; \
	cd $(bindir); \
	for asmbly in $(ASSEMBLIES); do \
		doc_xml="$$addressbook_dir/src/$$asmbly/$$asmbly.doc.xml"; \
		asmbly="$(bindir)/$$asmbly.dll"; \
		asmbly_list="$$asmbly_list $$asmbly,$$doc_xml"; \
	done; \
	output_dir="$$addressbook_dir/doc/ndoc"; \
	$(MONO) "$(NDOC)" $$asmbly_list -documenter=MSDN -Title="Address Book" -OutputDirectory=$$output_dir
endif # WINDOWS

ndoc-clean:
	rm -rf ndoc

else # NDOC

ndoc ndoc-clean:
	@echo NDOC is not configured

endif # NDOC

maintainer-clean-local:
	rm -f Makefile.in

