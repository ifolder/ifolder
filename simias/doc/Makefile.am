.PHONY: $(PACKAGE)-install ndoc ndoc-nodeps ndoc-clean

DOC_TARBALL = $(PACKAGE)-doc-$(VERSION).tar.gz

all: ndoc

all-nodeps: ndoc-nodeps

clean-local: ndoc-clean

$(PACKAGE)-install:
	$(MAKE) -C .. install

if NDOC

ndoc: $(PACKAGE)-install ndoc-nodeps

ndoc-nodeps:
	rm -rf $(PACKAGE)-ndoc
if WINDOWS
	"$(NDOC_CMD)" \
		`cygpath --windows "$(webbindir)/Simias.Web.dll"`,`cygpath --windows "../src/WebService/Simias.Web.doc.xml"` \
		`cygpath --windows "$(webbindir)/Simias.POBox.Web.dll"`,`cygpath --windows "../src/POBoxWS/Simias.POBox.Web.doc.xml"` \
		-documenter=MSDN -Title=Simias -OutputDirectory=$(PACKAGE)-ndoc
else # NOT WINDOWS
	"$(NDOC_CMD)" \
		"$(webbindir)/Simias.dll","../src/Simias.dll/Simias.doc.xml" \
		-documenter=MSDN -Title=Simias -OutputDirectory=$(PACKAGE)-ndoc
endif # WINDOWS
	tar -czf $(DOC_TARBALL) $(PACKAGE)-ndoc
	@test "x" = "x$(DOC_TARBALLS)" || \
		CMD='cp -f $(DOC_TARBALL) "$(DOC_TARBALLS)"'; \
		echo $$CMD; \
		eval $$CMD

else # NDOC

ndoc ndoc-nodeps:
	@echo NDOC is not configured
	mkdir -p $(PACKAGE)-ndoc
	touch $(PACKAGE)-ndoc/sdk_doc_was_not_built

endif # NDOC

ndoc-clean:
	rm -rf $(PACKAGE)-ndoc $(PACKAGE)-doc-*.tar.gz

maintainer-clean-local:
	rm -f Makefile.in
	

