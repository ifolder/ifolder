.PHONY: addressbook-install ndoc ndoc-nodeps ndoc-clean

all: ndoc

clean-local: ndoc-clean

addressbook-install:
	$(MAKE) -C .. install

ASSEMBLIES = FormsBookLib

if NDOC

ndoc: addressbook-install ndoc-nodeps

ndoc-nodeps: ndoc-clean
	@echo "$(NDOC) ... -documenter=MSDN -Title=\"Address Book\" -OutputDirectory=ndoc"
	@addressbook_dir=`cd .. && pwd`; \
	cd $(bindir); \
	for asmbly in $(ASSEMBLIES); do \
		doc_xml=`cygpath --windows "$$addressbook_dir/src/$$asmbly/$$asmbly.doc.xml"`; \
		asmbly=`cygpath --windows "$(bindir)/$$asmbly.dll"`; \
		asmbly_list="$$asmbly_list $$asmbly,$$doc_xml"; \
	done; \
	output_dir=`cygpath --windows "$$addressbook_dir/doc/ndoc"`; \
	"$(NDOC)" $$asmbly_list -documenter=MSDN -Title="Address Book" -OutputDirectory=$$output_dir

ndoc-clean:
	rm -rf ndoc ndoc.log

else # NDOC

ndoc ndoc-clean:
	@echo NDOC is not configured

endif # NDOC

maintainer-clean-local:
	rm -f Makefile.in

