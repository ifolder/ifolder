EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/addressbook-install.sln $(srcdir)/addressbook-msi/addressbook-msi.vdproj $(srcdir)/addressbook-msm/addressbook-msm.vdproj

all: addressbook.msi

addressbook.msi: addressbook addressbook.msm addressbook-install.sln addressbook-msi/addressbook-msi.vdproj
	"$(VSINSTALLDIR)/devenv.exe" addressbook-install.sln /build $(DEVENV_CONFIGURATION) /project addressbook-msi /out devenv-addressbook-msi.log
	mv addressbook-msi/$(DEVENV_CONFIGURATION)/addressbook.msi $@

addressbook.msm: addressbook addressbook-install.sln addressbook-msm/addressbook-msm.vdproj
	"$(VSINSTALLDIR)/devenv.exe" addressbook-install.sln /build $(DEVENV_CONFIGURATION) /project addressbook-msm /out devenv-addressbook-msm.log
	mv addressbook-msm/$(DEVENV_CONFIGURATION)/addressbook.msm $@

.PHONY: addressbook
addressbook:
	make -C ../../.. all
	
install: addressbook.msi
	msiexec /i addressbook.msi /q /L* install-msi.log
	
uninstall:
	msiexec /x addressbook.msi /q /L* uninstall-msi.log

clean-local:
	rm -rf  *.msi *.msm Release Debug */Release */Debug *.log *.suo

maintainer-clean-local:
	rm -f Makefile.in

