#
# The purpose of this Makefile is to simplify the coordinated building
# of mulitple projects.
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
	$(MAKE) -C ifolder $@
	
#
# Support register/unregister targets for iFolder on Windows
#
register unregister:
	@-if `uname -o | grep -iq cygwin`; then \
		$(MAKE) -C ifolder $@; \
	fi

#
# Run autogen.sh for multiple projects, with or without --enable-debug.
#
AUTOGEN_CMD       = ./autogen.sh --prefix=$(PREFIX)
AUTOGEN_DEBUG_CMD = ./autogen.sh --prefix=$(PREFIX) --enable-debug
AUTOGEN_DOC_CMD   = ./autogen.sh --prefix=$(PREFIX) --with-ndoc-path='c:/Program Files/NDoc/bin/.net-1.1'

ag autogen:
	rm -f */config.cache
	@echo PREFIX=$(PREFIX)
	cd simias; $(AUTOGEN_CMD)
	$(MAKE) -C simias install
	cd ifolder; $(AUTOGEN_CMD)
	$(MAKE) -C ifolder install

agd autogen-debug:
	rm -f */config.cache
	@echo PREFIX=$(PREFIX)
	cd simias; $(AUTOGEN_DEBUG_CMD)
	$(MAKE) -C simias install
	cd ifolder; $(AUTOGEN_DEBUG_CMD)
	$(MAKE) -C ifolder install

agdoc autogen-doc:
	@if ! `uname -o | grep -iq cygwin`; then \
		echo "currently doc can only be built on Windows"; exit 1; \
	fi
	rm -f */config.cache
	@echo PREFIX=$(PREFIX)
	cd simias; $(AUTOGEN_DOC_CMD)
	$(MAKE) -C simias install
	cd ifolder; $(AUTOGEN_DOC_CMD)
	$(MAKE) -C ifolder install

