#######################################################################
#  $RCSfile$
#
#  Copyright (C) 2004 Novell, Inc.
#
#  This program is free software; you can redistribute it and/or
#  modify it under the terms of the GNU General Public
#  License as published by the Free Software Foundation; either
#  version 2 of the License, or (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
#  General Public License for more details.
#
#  You should have received a copy of the GNU General Public
#  License along with this program; if not, write to the Free
#  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#
#  Author: Paul Thomas <pthomas@novell.com>
#
#######################################################################
EXTRA_DIST = $(srcdir)/Makefile.am $(srcdir)/simias-install.sln $(srcdir)/simias-msi/simias-msi.vdproj $(srcdir)/simias-msm/simias-msm.vdproj

RELEASE = 1
CURDATE = $(shell date '+%Y%m%d')
MSI_FILE = $(PACKAGE)-$(VERSION).$(CURDATE)-$(RELEASE).msi
MSM_FILE = $(PACKAGE)-$(VERSION).$(CURDATE)-$(RELEASE).msm
MSI_LOG_FILE = devenv-simias-msi.log
MSM_LOG_FILE = devenv-simias-msm.log

.PHONY: package package-clean package-install package-uninstall simias

all clean:

package: $(MSI_FILE)

$(MSI_FILE): $(MSM_FILE) simias-install.sln simias-msi/simias-msi.vdproj
	@rm -f $(MSI_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" simias-install.sln /build $(DEVENV_CONFIGURATION) /project simias-msi /out $(MSI_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv simias-msi/$(DEVENV_CONFIGURATION)/simias.msi .; \
		ln simias.msi $@; \
	else \
		grep -a "ERROR:" $(MSI_LOG_FILE); \
	fi

$(MSM_FILE): simias simias-install.sln simias-msm/simias-msm.vdproj
	@rm -f $(MSM_LOG_FILE) $@
	@CMD='"$(VSINSTALLDIR)/devenv.exe" simias-install.sln /build $(DEVENV_CONFIGURATION) /project simias-msm /out $(MSM_LOG_FILE)'; \
	echo $$CMD; \
	if eval $$CMD; then \
		mv simias-msm/$(DEVENV_CONFIGURATION)/simias.msm .; \
		ln simias.msm $@; \
	else \
		grep -a "ERROR:" $(MSM_LOG_FILE); \
	fi

simias:
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
	rm -f Makefile simias.spec

maintainer-clean-local:
	rm -f Makefile.in

