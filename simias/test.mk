.PHONY: test

test:
	$(NUNIT) $(NUNIT_FLAGS) /xml:Configuration.Test.xml $(NUNIT_TESTS)

