
SUBDIRS = Simias.IdentitySync Simias.LdapProvider Simias.OpenLdapProvider Simias.ADLdapProvider Simias.UserAdd

DIST_SUBDIRS = $(SUBDIRS)

EXTRA_DIST = IdentityManagement.conf iFolderLdapPlugin.ldif iFolderLdapPlugin.schema iFolderADLdapPlugin.ldif iFolderLdapGroupExtend.ldif

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(mkinstalldirs) $(DESTDIR)$(sysconfdir)/simias/bill/modules/
	$(INSTALL_PROGRAM) iFolderLdapPlugin.ldif $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) iFolderLdapGroupExtend.ldif $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) iFolderLdapPlugin.schema $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) iFolderADLdapPlugin.ldif $(DESTDIR)$(sysconfdir)
	$(INSTALL_PROGRAM) IdentityManagement.conf  $(DESTDIR)$(sysconfdir)/simias/bill/modules/

uninstall-local:

installcheck-summary:

clean-local:

distclean-local:

maintainer-clean-local:

