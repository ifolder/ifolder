EXTRA_DIST =

bin_PROGRAMS = simias-shell
simias_shell_SOURCES = shell.c

simias_shell_INCLUDES = ../api/libsimias
simias_shell_LDADD = ../api/libsimias/libsimias2.la
simias_shell_CFLAGS = -I../api/libsimias

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
