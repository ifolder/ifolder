export MONO_PATH=$(PWD)/external/SecureChannelSinks:$(PWD)/src/Configuration:$(PWD)/src/Common:$(PWD)/src/CollectionEvents:$(PWD)/src/SqliteWrapper:$(PWD)/src/StoreProvider:$(PWD)/src/SqliteProvider:$(PWD)/src/CollectionStore:$(PWD)/src/Agent:$(PWD)/src/SyncCommon:$(PWD)/src/CollectionWatcher:$(PWD)/src/AddressBook:$(PWD)/src/StoreWatcher:$(PWD)/src/UserService:$(PWD)/src/SyncLogicLite:$(PWD)/src/SyncManager:$(PWD)/src/InviteAgent:$(PWD)/src/Smtp:$(PWD)/src/FsWatcher:$(PWD)/src/Sync

SUBDIRS = src package

EXTRA_DIST = tools external

.PHONY: package package-clean package-install package-uninstall
package package-clean package-install package-uninstall:
	$(MAKE) -C package $@

clean-local:
	rm -rf stage *.suo

maintainer-clean-local:
	rm -f configure Makefile.in simias-*.tar.gz
	rm -f aclocal.m4
	rm -f config.guess config.sub
	rm -f ltmain.sh libtool
	rm -f install-sh install-sh.lnk INSTALL INSTALL.lnk
	rm -f missing missing.lnk mkinstalldirs mkinstalldir.lnk
	rm -rf autom4te.cache
	
uninstall-local:
	rm -rf $(DESTDIR)$(bindir)/.cstore
