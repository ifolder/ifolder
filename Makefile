#
# The purpose of this Makefile is to simplify the coordinated building
# of the simias, addressbook, and ifolder projects.
#
.PHONY: all ag autogen agd autogen-debug

#
# By default, any target not defined elsewhere in this Makefile will be
# built for all projects.
#
all %:
	cd simias;      make $@
	cd addressbook; make $@
	cd ifolder;     make $@

#
# Run autogen.sh for all projects, with or without --enable-debug.
#
# PREFIX should be set to location where 'make install' should install files.
# If not already set in the environment, it defaults here to $HOME/stage.
#
# Short forms of target names:
# ag = autogen
# agd = autogen-debug
#
ifeq ($(PREFIX),)
export PREFIX = $(HOME)/stage
endif

AUTOGEN_CMD       = ./autogen.sh --prefix=$(PREFIX)
AUTOGEN_DEBUG_CMD = ./autogen.sh --prefix=$(PREFIX) --enable-debug

ag autogen:
	cd simias;      $(AUTOGEN_CMD)
	cd addressbook; $(AUTOGEN_CMD)
	cd ifolder;     $(AUTOGEN_CMD)

agd autogen-debug:
	cd simias;      $(AUTOGEN_DEBUG_CMD)
	cd addressbook; $(AUTOGEN_DEBUG_CMD)
	cd ifolder;     $(AUTOGEN_DEBUG_CMD)

