
INCLUDES = \
  $(EGG_TRAY_CFLAGS) \
  -I$(top_srcdir)/libegg/util \
  -DEGG_COMPILATION \
  -DGTK_DISABLE_DEPRECATED \
  -DGDK_DISABLE_DEPRECATED \
  -DG_DISABLE_DEPRECATED


inst_LTLIBRARIES = libtrayicon.la

libtrayicon_la_LIBADD = \
	$(EGG_TRAY_LIBS) \
	$(top_builddir)/libegg/util/libeggutil.la

libtrayicon_la_SOURCES = \
  eggtrayicon.c		\
  eggstatusicon.c	\
  eggtraymanager.c

noinst_HEADERS = \
  eggtrayicon.h	 \
  eggstatusicon.h\
  eggtraymanager.h


