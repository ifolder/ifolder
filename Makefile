#
# The purpose of this Makefile is to simplify the coordinated building
# of the simias, addressbook, and ifolder projects.
#
# PREFIX should be set to location where 'make install' should install files.
# If not already set in the environment, it defaults here to $HOME/stage.
#
ifeq ($(PREFIX),)
export PREFIX := $(HOME)/stage
endif

.PHONY: all ag autogen agd autogen-debug

#
# By default, any target not defined elsewhere in this Makefile will be
# built for all projects.
#
all %:
	@echo PREFIX=$(PREFIX)
	$(MAKE) -C simias $@
	$(MAKE) -C addressbook $@
	$(MAKE) -C ifolder $@
	
#
# Support register/unregister targets for iFolder on Windows
#
register unregister:
	@-if `uname -o | grep -iq cygwin`; then \
		$(MAKE) -C ifolder $@; \
	fi

#
# Run autogen.sh for all projects, with or without --enable-debug.
#
# Short forms of target names:
# ag = autogen
# agd = autogen-debug
#
AUTOGEN_CMD       = ./autogen.sh --prefix=$(PREFIX)
AUTOGEN_DEBUG_CMD = ./autogen.sh --prefix=$(PREFIX) --enable-debug
AUTOGEN_SDK_CMD   = ./autogen.sh --prefix=$(PREFIX) --enable-merge-module --with-ndoc-path='c:/Program Files/NDoc/bin/.net-1.1'

ag autogen:
	rm -f */config.cache
	@echo PREFIX=$(PREFIX)
	cd simias; $(AUTOGEN_CMD)
	$(MAKE) -C simias install
	cd addressbook; $(AUTOGEN_CMD)
	$(MAKE) -C addressbook install
	cd ifolder; $(AUTOGEN_CMD)
	$(MAKE) -C ifolder install

agd autogen-debug:
	rm -f */config.cache
	@echo PREFIX=$(PREFIX)
	cd simias; $(AUTOGEN_DEBUG_CMD)
	$(MAKE) -C simias install
	cd addressbook; $(AUTOGEN_DEBUG_CMD)
	$(MAKE) -C addressbook install
	cd ifolder; $(AUTOGEN_DEBUG_CMD)
	$(MAKE) -C ifolder install

agsdk autogen-sdk:
	rm -f */config.cache
	@echo PREFIX=$(PREFIX)
	cd simias; $(AUTOGEN_SDK_CMD)
	$(MAKE) -C simias install
	cd addressbook; $(AUTOGEN_SDK_CMD)
	$(MAKE) -C addressbook install
	cd ifolder; $(AUTOGEN_SDK_CMD)
	$(MAKE) -C ifolder install

