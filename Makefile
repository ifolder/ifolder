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
	cd simias;      make $@
	cd addressbook; make $@
	cd ifolder;     make $@

#
# Run autogen.sh for all projects, with or without --enable-debug.
#
# Short forms of target names:
# ag = autogen
# agd = autogen-debug
#
AUTOGEN_CMD       = ./autogen.sh --prefix=$(PREFIX)
AUTOGEN_DEBUG_CMD = ./autogen.sh --prefix=$(PREFIX) --enable-debug

ag autogen:
	rm -f */config.cache
	@echo PREFIX=$(PREFIX)
	cd simias;      $(AUTOGEN_CMD); make install
	cd addressbook; $(AUTOGEN_CMD); make install
	cd ifolder;     $(AUTOGEN_CMD)

agd autogen-debug:
	rm -f */config.cache
	@echo PREFIX=$(PREFIX)
	cd simias;      $(AUTOGEN_DEBUG_CMD); make install
	cd addressbook; $(AUTOGEN_DEBUG_CMD); make install
	cd ifolder;     $(AUTOGEN_DEBUG_CMD)

