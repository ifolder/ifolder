PREFIX=		/usr/local

ASSEMBLY=	TrayIcon
LIBRARY=	$(ASSEMBLY).dll
API=		libtrayicon-api.xml

INCLUDE_API=	$(PREFIX)/share/gapi/art-api.xml	\
		$(PREFIX)/share/gapi/pango-api.xml	\
		$(PREFIX)/share/gapi/atk-api.xml	\
		$(PREFIX)/share/gapi/gnome-api.xml	\
		$(PREFIX)/share/gapi/gtk-api.xml	\
		$(PREFIX)/share/gapi/gdk-api.xml	\
		$(PREFIX)/share/gapi/gdk-symbols.xml

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
	PERLLIB=/usr/local/share/perl5 gapi-parser libtrayicon-sources.xml

install:
	install -o root -g root -m 644 $(LIBRARY) $(PREFIX)/lib

uninstall:
	rm -f $(PREFIX)/lib/$(LIBRARY)

clean:
	rm -f generated-stamp
	rm -f $(LIBRARY)
	rm -rf generated
	rm -f $(API)
