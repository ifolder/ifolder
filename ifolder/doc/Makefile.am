.PHONY: ifolder-install ndoc ndoc-nodeps ndoc-clean

all: ndoc

clean-local: ndoc-clean

ifolder-install:
	$(MAKE) -C .. install

ASSEMBLIES = iFolder

if NDOC

ndoc: ifolder-install ndoc-nodeps

ndoc-nodeps: ndoc-clean
	@echo "$(NDOC_CMD) ... -documenter=MSDN -Title=iFolder -OutputDirectory=ndoc"
if WINDOWS
	@ifolder_dir=`cd .. && pwd`; \
	cd $(bindir); \
	for asmbly in $(ASSEMBLIES); do \
		doc_xml=`cygpath --windows "$$ifolder_dir/src/$$asmbly/$$asmbly.doc.xml"`; \
		asmbly=`cygpath --windows "$(bindir)/$$asmbly.dll"`; \
		asmbly_list="$$asmbly_list $$asmbly,$$doc_xml"; \
	done; \
	output_dir=`cygpath --windows "$$ifolder_dir/doc/ndoc"`; \
	"$(NDOC_CMD)" $$asmbly_list -documenter=MSDN -Title=iFolder -OutputDirectory=$$output_dir
else # NOT WINDOWS
	@ifolder_dir=`cd .. && pwd`; \
	cd $(bindir); \
	for asmbly in $(ASSEMBLIES); do \
		doc_xml="$$ifolder_dir/src/$$asmbly/$$asmbly.doc.xml"; \
		asmbly="$(bindir)/$$asmbly.dll"; \
		asmbly_list="$$asmbly_list $$asmbly,$$doc_xml"; \
	done; \
	output_dir="$$ifolder_dir/doc/ndoc"; \
	$(MONO) "$(NDOC_CMD)" $$asmbly_list -documenter=MSDN -Title=iFolder -OutputDirectory=$$output_dir
endif # WINDOWS

ndoc-clean:
	rm -rf ndoc

else # NDOC

ndoc ndoc-clean:
	@echo NDOC is not configured

endif # NDOC

maintainer-clean-local:
	rm -f Makefile.in

