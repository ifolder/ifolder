EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/addressbook-install.sln $(srcdir)/addressbook-msi/addressbook-msi.vdproj $(srcdir)/addressbook-msm/addressbook-msm.vdproj

RELEASE = 1
MSI_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).msi
MSM_FILE = $(PACKAGE)-$(VERSION)-$(RELEASE).msm
MSI_LOG_FILE = devenv-addressbook-msi.log
MSM_LOG_FILE = devenv-addressbook-msm.log

.PHONY: package package-clean package-install package-uninstall addressbook

all clean:

package: $(MSI_FILE)

$(MSI_FILE): $(MSM_FILE) addressbook-install.sln addressbook-msi/addressbook-msi.vdproj
	@rm -f $(MSI_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" addressbook-install.sln /build $(DEVENV_CONFIGURATION) /project addressbook-msi /out $(MSI_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv addressbook-msi/$(DEVENV_CONFIGURATION)/addressbook.msi .; \
		ln addressbook.msi $@; \
	else \
		grep -a "ERROR:" $(MSI_LOG_FILE); \
	fi

$(MSM_FILE): addressbook addressbook-install.sln addressbook-msm/addressbook-msm.vdproj
	@rm -f $(MSM_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" addressbook-install.sln /build $(DEVENV_CONFIGURATION) /project addressbook-msm /out $(MSM_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv addressbook-msm/$(DEVENV_CONFIGURATION)/addressbook.msm .; \
		ln addressbook.msm $@; \
	else \
		grep -a "ERROR:" $(MSM_LOG_FILE); \
	fi

addressbook:
	make -C ../.. all
	
package-install: $(MSI_FILE)
	@CMD='msiexec /i $(MSI_FILE) /qb+ /L* install-msi.log'; \
	echo $$CMD; \
	if ! eval $$CMD; then \
		grep -a "ERROR:" install-msi.log; \
	fi
	
package-uninstall:
	@CMD='msiexec /x $(MSI_FILE) /qb+ /L* uninstall-msi.log'; \
	echo $$CMD; \
	if ! eval $$CMD; then \
		grep -a "ERROR:" uninstall-msi.log; \
	fi

package-clean clean-local:
	rm -rf  *.msi *.msm Release Debug */Release */Debug *.log *.suo

distclean-local: package-clean
	rm -f Makefile addressbook.spec

maintainer-clean-local:
	rm -f Makefile.in

