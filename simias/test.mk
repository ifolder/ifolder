.PHONY: test

test: $(NUNIT_TESTS)

$(NUNIT_TESTS): FORCE
	$(NUNIT) $(NUNIT_FLAGS) /xml:$(addprefix $@,.Test.xml) $@

FORCE:
# $(NUNIT) $(NUNIT_FLAGS) /xml:$(addpstfix Configuration.Test.xml $(NUNIT_TESTS)

