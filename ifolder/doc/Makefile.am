.PHONY: ifolder-install ndoc ndoc-nodeps ndoc-clean

all: ndoc

clean-local: ndoc-clean

ifolder-install:
	$(MAKE) -C .. install

ASSEMBLIES = iFolder

if NDOC

ndoc: ifolder-install ndoc-nodeps

ndoc-nodeps: ndoc-clean
	@echo "$(NDOC) ... -documenter=MSDN -Title=iFolder -OutputDirectory=ndoc"
	@simias_dir=`cd .. && pwd`; \
	cd $(bindir); \
	for asmbly in $(ASSEMBLIES); do \
		doc_xml=`cygpath --windows "$$simias_dir/src/$$asmbly/$$asmbly.doc.xml"`; \
		asmbly=`cygpath --windows "$(bindir)/$$asmbly.dll"`; \
		asmbly_list="$$asmbly_list $$asmbly,$$doc_xml"; \
	done; \
	output_dir=`cygpath --windows "$$simias_dir/doc/ndoc"`; \
	"$(NDOC)" $$asmbly_list -documenter=MSDN -Title=iFolder -OutputDirectory=$$output_dir

ndoc-clean:
	rm -rf ndoc ndoc.log

else # NDOC

ndoc ndoc-clean:
	@echo NDOC is not configured

endif # NDOC

maintainer-clean-local:
	rm -f Makefile.in

