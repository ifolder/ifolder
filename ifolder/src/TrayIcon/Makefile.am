SUBDIRS =       libtrayicon

ASSEMBLY =	TrayIcon
LIBRARY =	$(ASSEMBLY).dll
API =		libtrayicon-api.xml

INCLUDE_API=	$(GTK_SHARP_PREFIX)/share/gapi/art-api.xml	\
		$(GTK_SHARP_PREFIX)/share/gapi/pango-api.xml	\
		$(GTK_SHARP_PREFIX)/share/gapi/atk-api.xml	\
		$(GTK_SHARP_PREFIX)/share/gapi/gnome-api.xml	\
		$(GTK_SHARP_PREFIX)/share/gapi/gtk-api.xml	\
		$(GTK_SHARP_PREFIX)/share/gapi/gdk-api.xml	\
		$(GTK_SHARP_PREFIX)/share/gapi/gdk-symbols.xml

ASSEMBLIES=	-r gtk-sharp.dll	\
		-r glib-sharp.dll	\
		-r gnome-sharp.dll	\
		-r gdk-sharp.dll	\
		-r pango-sharp.dll	\
		-r art-sharp.dll


all: $(LIBRARY)

$(LIBRARY): generated-stamp
	mcs --unsafe --target library $(ASSEMBLIES) --recurse '*.cs' -o $(LIBRARY)

generated-stamp: $(API)
	gapi-fixup --api=$(API) --metadata=Egg.metadata && \
	gapi-codegen --include $(INCLUDE_API) --generate $(API) \
	--outdir=generated --customdir=. --assembly-name=$(ASSEMBLY) \
	&& touch generated-stamp

$(API):
	PERLLIB=/usr/share/perl5 gapi-parser libtrayicon-sources.xml

install:
	install $(LIBRARY) $(prefix)/lib

uninstall:
	rm -f $(prefix)/lib/$(LIBRARY)

clean:
	rm -f generated-stamp
	rm -f $(LIBRARY)
	rm -rf generated
	rm -f $(API)
