EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/simias-install.sln $(srcdir)/simias-msi/simias-msi.vdproj $(srcdir)/simias-msm/simias-msm.vdproj

all: simias.msi

simias.msi: simias simias.msm simias-install.sln simias-msi/simias-msi.vdproj
	"$(VSINSTALLDIR)/devenv.exe" simias-install.sln /build $(DEVENV_CONFIGURATION) /project simias-msi /out devenv-simias-msi.log
	mv simias-msi/$(DEVENV_CONFIGURATION)/simias.msi $@

simias.msm: simias simias-install.sln simias-msm/simias-msm.vdproj
	"$(VSINSTALLDIR)/devenv.exe" simias-install.sln /build $(DEVENV_CONFIGURATION) /project simias-msm /out devenv-simias-msm.log
	mv simias-msm/$(DEVENV_CONFIGURATION)/simias.msm $@

.PHONY: simias
simias:
	make -C ../../.. all
	
install: simias.msi
	msiexec /i simias.msi /q /L* install-msi.log
	
uninstall:
	msiexec /x simias.msi /q /L* uninstall-msi.log

clean-local:
	rm -rf  *.msi *.msm Release Debug */Release */Debug *.log *.suo

maintainer-clean-local:
	rm -f Makefile.in

