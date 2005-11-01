#Hacked up file to handle all of the platforms we deal with

SUFFIXES = .c .o .po .pox .gmo .mo

.c.o:
	$(COMPILE) $<

.po.pox:
	$(MAKE) $(PACKAGE).pot
	$(MSGMERGE) $< $(srcdir)/$(PACKAGE).pot -o $*.pox

.po.mo:
	$(MSGFMT) -o $@ $<

.po.gmo:
	file=$(srcdir)/`echo $* | sed 's,.*/,,'`.gmo \
	  && rm -f $$file && $(GMSGFMT) --statistics -o $$file $<

POFILES = $(srcdir)/pt_BR.po \
	 $(srcdir)/cs.po \
	 $(srcdir)/de.po \
	 $(srcdir)/en.po \
	 $(srcdir)/es.po \
	 $(srcdir)/fi.po \
	 $(srcdir)/fr.po \
	 $(srcdir)/it.po \
	 $(srcdir)/ja.po \
	 $(srcdir)/hu.po \
	 $(srcdir)/pl.po \
	 $(srcdir)/pt.po \
	 $(srcdir)/ru.po \
	 $(srcdir)/sk.po \
	 $(srcdir)/zh_CN.po \
	 $(srcdir)/zh_TW.po

CATALOGS := $(subst .po,.mo,$(POFILES))

EXTRA_DIST = $(POFILES)

update-po:
	cd ../../../; \
	echo Updating pot file: $(PACKAGE).pot; \
	xgettext --output-dir=src/LinuxClient/po/ \
			--files-from=src/LinuxClient/po/POTFILES \
			--language=C# \
			--join-existing \
			--from-code=UTF-8 \
			--output=$(srcdir)/ifolder3.pot \
			--keyword="Util.GS"; 

	echo updating CHARSET to UTF-8;
	cp ifolder3.pot ifolder3.bak;
	cat ifolder3.bak | sed "s/charset=CHARSET/charset=UTF-8/" > ifolder3.pot;
	rm -f ifolder3.bak;
	
	echo Updating po files from $(PACKAGE).pot;
	pofiles='$(POFILES)'; \
	for pofile in $$pofiles; do \
		pofile=`basename $$pofile`; \
		msginit -l UTF-8 -o $$pofile -i $(srcdir)/ifolder3.pot --no-translator; \
	done

all: $(CATALOGS)

install-exec-local: all
	catalogs='$(CATALOGS)'; \
	for cat in $$catalogs; do \
	  cat=`basename $$cat`; \
	  lang=`echo $$cat | sed 's/\.mo$$//'`; \
	  echo lang=$$lang; \
	  dir=$(datadir)/locale/$$lang/LC_MESSAGES; \
	  echo dir=$$dir; \
	  echo mkinstalldirs $(DESTDIR)$$dir; \
	  $(mkinstalldirs) $(DESTDIR)$$dir; \
	  if test -r $$cat; then \
	    $(INSTALL_DATA) $$cat $(DESTDIR)$$dir/$(PACKAGE).mo; \
	    echo "installing $$cat as $(DESTDIR)$$dir/$(PACKAGE).mo"; \
	  else \
	    $(INSTALL_DATA) $(srcdir)/$$cat $(DESTDIR)$$dir/$(PACKAGE).mo; \
	    echo "installing $(srcdir)/$$cat as" \
		 "$(DESTDIR)$$dir/$(PACKAGE).mo"; \
	  fi; \
	done


uninstall-local:
	catalogs='$(CATALOGS)'; \
	for cat in $$catalogs; do \
	  cat=`basename $$cat`; \
	  lang=`echo $$cat | sed 's/\.mo$$//'`; \
	  dir=$(datadir)/locale/$$lang/LC_MESSAGES; \
	  rm -f $(DESTDIR)$$dir/$(PACKAGE).mo; \
	done

CLEAN_FILES = *.mo

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -f ifolder

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)
