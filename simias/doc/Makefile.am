.PHONY: simias-install ndoc ndoc-nodeps ndoc-clean

all: ndoc

clean-local: ndoc-clean

simias-install:
	$(MAKE) -C .. install

if NDOC

ndoc: simias-install ndoc-nodeps

ndoc-nodeps:
	rm -rf simias-ndoc
if WINDOWS
	"$(NDOC_CMD)" `cygpath --windows "$(bindir)/Simias.dll"`,`cygpath --windows "../src/Simias.dll/Simias.doc.xml"` -documenter=MSDN -Title=Simias -OutputDirectory=simias-ndoc
else # NOT WINDOWS
	"$(NDOC_CMD)" "$(bindir)/Simias.dll","../src/Simias.dll/Simias.doc.xml" -documenter=MSDN -Title=Simias -OutputDirectory=simias-ndoc
endif # WINDOWS
	tar -czf simias-ndoc.tar.gz simias-ndoc

else # NDOC

ndoc:
	@echo NDOC is not configured
	mkdir -p simias-ndoc
	touch simias-ndoc/sdk_doc_was_not_built

endif # NDOC

clean-local:
	rm -rf simias-ndoc simias-ndoc.tar.gz

maintainer-clean-local:
	rm -f Makefile.in
	

