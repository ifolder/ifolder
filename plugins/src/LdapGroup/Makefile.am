
SUBDIRS = Simias.IdentitySync Simias.LdapProvider Simias.OpenLdapProvider Simias.ADLdapProvider

DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST = IdentityManagement.conf iFolderLdapPlugin.ldif iFolderLdapPlugin.schema iFolderLdapUserUpdate.sh

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(etcdir)/simias/bill/modules/
	$(INSTALL_PROGRAM) iFolderLdapPlugin.ldif $(DESTDIR)$(etcdir)
	$(INSTALL_PROGRAM) iFolderLdapPlugin.schema $(DESTDIR)$(etcdir)
	$(INSTALL_PROGRAM) iFolderLdapUserUpdate.sh $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) IdentityManagement.conf  $(DESTDIR)$(etcdir)/simias/bill/modules/

uninstall-local:

installcheck-summary:

clean-local:

distclean-local:

maintainer-clean-local:

