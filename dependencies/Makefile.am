IFOLDER_TSA_CFLAGS = -D_REENTRANT -DGNU -DN_PLAT_GNU -DN_PLAT_UNIX -DLINUX -D_GNU_SOURCE -DVERSION=0x00030000
SMS_INCLUDES = -I./fs/ -I./fs/nss -I./fs/vfs -I./i18n/linux/ -I./tsa -I./tsa/linux -I./fs/cfs \
               -I./dependencies/external/fs64/include -I./dependencies/internal/smscomp/include -I./dependencies/internal/framework/include

INCLUDES = \
  $(IFOLDER_TSA_CFLAGS) \
  $(SMS_INCLUDES)

LIBDIR = ../../external/sms/deliver/linux

lib_LTLIBRARIES = libtsaif.la

libtsaif_la_LIBADD = \
	-L$(LIBDIR) -lsmsut -lsmdr -lpthread -lcrypt

libtsaif_la_LDFLAGS = \
	 -shared -export-dynamic -module -version-info 0:1:0

libtsaif_la_SOURCES = \
       ./fs/funpointer.c ./fs/nss/conversion.c ./fs/nss/fs_sidf.c ./fs/nss/fscreate.c ./fs/nss/fsmisc.c \
       ./fs/nss/fsopen.c ./fs/nss/fsscan.c ./fs/nss/timeconvc.c ./fs/nss/setfile.c ./fs/nss/convids.c \
       ./fs/nss/fs_trustee.c ./fs/nss/fsinterface.c ./fs/nss/fsmodifyinfo.c ./fs/nss/fsread.c \
       ./fs/nss/fswrite.c ./fs/vfs/fsvfs_modifyinfo.c ./fs/vfs/fsvfs_trustee.c ./fs/vfs/fsvfsmisc.c \
       ./fs/vfs/fsvfsread.c ./fs/vfs/fsvfswrite.c ./fs/vfs/fsvfs_sidf.c ./fs/vfs/fsvfscreate.c \
       ./fs/vfs/fsvfsopen.c ./fs/vfs/fsvfsscan.c ./i18n/linux/tsamsg.c \
       ./tsa/asyncread.c ./tsa/connect.c ./tsa/open.c ./tsa/sidfencode.c ./tsa/tsaname.c \
       ./tsa/asyncscan.c ./tsa/create.c ./tsa/openwtd.c ./tsa/sidfpreformat.c ./tsa/tsadset.c \
       ./tsa/tsaoptions.c ./tsa/asyncwrite.c ./tsa/restore.c ./tsa/tsainit.c ./tsa/tsaresources.c \
       ./tsa/closewtd.c ./tsa/incexc.c ./tsa/sidfdecode.c ./tsa/tsalib.c ./tsa/tsaunicode.c \
       ./tsa/close.c ./tsa/exclude.c ./tsa/rincexc.c ./tsa/tempfile.c ./tsa/tsajob.c ./tsa/tsasmdrif.c \
       ./tsa/linux/lunicode.c ./tsa/linux/osprimitives.c ./tsa/linux/dummyzapi.c ./tsa/linux/tsajob-linux.c \
       ./tsa/linux/resource.c ./tsa/linux/connect-linux.c ./tsa/linux/linktab.c

install-exec-local:
	$(mkinstalldirs) $(DESTDIR)$(bindir)

uninstall-local:

clean-local:
	rm -rf $(COMMON_CLEAN_FILES)

distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

