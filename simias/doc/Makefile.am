.PHONY: simias-install ndoc ndoc-nodeps ndoc-clean

all: ndoc

clean-local: ndoc-clean

simias-install:
	$(MAKE) -C .. install

ASSEMBLIES = Common Storage StoreProvider StoreWatcher CollectionEvents CollectionStore CollectionWatcher AddressBook Agent CommonFormWindows InviteAgent Location Smtp SyncCommon Sync SyncLogicLite SyncManager Userservice

if WINDOWS
if NDOC

ndoc: simias-install ndoc-nodeps

ndoc-nodeps: ndoc-clean
	@echo "$(NDOC) ... -documenter=MSDN -Title=Simias -OutputDirectory=ndoc"
	@simias_dir=`cd .. && pwd`; \
	cd $(bindir); \
	for asmbly in $(ASSEMBLIES); do \
		doc_xml=`cygpath --windows "$$simias_dir/src/$$asmbly/$$asmbly.doc.xml"`; \
		asmbly=`cygpath --windows "$(bindir)/$$asmbly.dll"`; \
		asmbly_list="$$asmbly_list $$asmbly,$$doc_xml"; \
	done; \
	output_dir=`cygpath --windows "$$simias_dir/doc/ndoc"`; \
	"$(NDOC)" $$asmbly_list -documenter=MSDN -Title=Simias -OutputDirectory=$$output_dir

ndoc-clean:
	rm -rf ndoc ndoc.log

else # NDOC

ndoc ndoc-clean:
	@echo NDOC is not configured

endif # NDOC
else # WINDOWS

ndoc ndoc-clean:
	@echo ndoc target is currently only supported on Windows

endif # WINDOWS

maintainer-clean-local:
	rm -f Makefile.in

