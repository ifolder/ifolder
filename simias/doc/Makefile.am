.PHONY: simias-install ndoc ndoc-nodeps ndoc-clean

all: ndoc

clean-local: ndoc-clean

simias-install:
	$(MAKE) -C .. install

ASSEMBLIES = Common Storage StoreProvider StoreWatcher CollectionEvents CollectionStore CollectionWatcher AddressBook Invitation Location Smtp SyncCommon Sync SyncLogicLite SyncManager UserService

if NDOC

ndoc: simias-install ndoc-nodeps

ndoc-nodeps: ndoc-clean
	@echo "$(NDOC_CMD) ... -documenter=MSDN -Title=Simias -OutputDirectory=ndoc"
if WINDOWS
	@simias_dir=`cd .. && pwd`; \
	cd $(bindir); \
	for asmbly in $(ASSEMBLIES); do \
		doc_xml=`cygpath --windows "$$simias_dir/src/$$asmbly/$$asmbly.doc.xml"`; \
		asmbly=`cygpath --windows "$(bindir)/$$asmbly.dll"`; \
		asmbly_list="$$asmbly_list $$asmbly,$$doc_xml"; \
	done; \
	output_dir=`cygpath --windows "$$simias_dir/doc/ndoc"`; \
	"$(NDOC_CMD)" $$asmbly_list -documenter=MSDN -Title=Simias -OutputDirectory=$$output_dir
else # NOT WINDOWS
	@simias_dir=`cd .. && pwd`; \
	cd $(bindir); \
	for asmbly in $(ASSEMBLIES); do \
		doc_xml="$$simias_dir/src/$$asmbly/$$asmbly.doc.xml"; \
		asmbly="$(bindir)/$$asmbly.dll"; \
		asmbly_list="$$asmbly_list $$asmbly,$$doc_xml"; \
	done; \
	output_dir="$$simias_dir/doc/ndoc"; \
	$(MONO) "$(NDOC_CMD)" $$asmbly_list -documenter=MSDN -Title=Simias -OutputDirectory=$$output_dir
endif # WINDOWS

ndoc-clean:
	rm -rf ndoc

else # NDOC

ndoc ndoc-clean:
	@echo NDOC is not configured

endif # NDOC

maintainer-clean-local:
	rm -f Makefile.in

