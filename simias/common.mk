# Setup install targets

TEST_RUN_MAKEFILE := $(DESTDIR)$(bindir)$(SEP)test.mk

.PHONY: install
.PHONY: uninstall

.PHONY: bin_install
.PHONY: lib_install
.PHONY: data_install
.PHONY: test_bin_install
.PHONY: test_lib_install
.PHONY: test_data_install

.PHONY: install_test
.PHONY: uninstall_test

.PHONY: bin_uninstall
.PHONY: lib_uninstall
.PHONY: data_uninstall
.PHONY: test_bin_uninstall
.PHONY: test_lib_uninstall
.PHONY: test_data_uninstall

install: $(BIN_FILES) $(LIB_FILES) $(DATA_FILES) bin_install lib_install data_install

installtest: install $(TEST_BIN_FILES) $(TEST_LIB_FILES) $(TEST_DATA_FILES) test_bin_install test_lib_install test_data_install

uninstall: bin_uninstall lib_uninstall data_uninstall

uninstalltest: test_bin_uninstall test_lib_uninstall test_data_uninstall

$(DESTDIR)$(bindir):
	$(MKDIR_R) $(DESTDIR)$(bindir)

#$(DESTDIR)$(libdir):
#	$(MKDIR_R) $(DESTDIR)$(libdir)

#$(DESTDIR)$(libdir):
#	$(MKDIR_R) $(DESTDIR)$(datadir)

# this is the uglies piece of hud I have ever seen but to build on
# a windoZzzz box, you have to copy each file individually, LAME!
# what a luser OS!
ifdef BIN_FILES
UNINSTALL_BIN_FILES := $(addprefix $(DESTDIR)$(bindir)$(SEP), $(BIN_FILES))

bin_install: $(DESTDIR)$(bindir) $(UNINSTALL_BIN_FILES)

$(UNINSTALL_BIN_FILES): 
	$(CP) $(subst $(DESTDIR)$(bindir)$(SEP),,$@) $(DESTDIR)$(bindir)

bin_uninstall:
	$(RM) $(UNINSTALL_BIN_FILES)
endif



ifdef LIB_FILES
UNINSTALL_LIB_FILES := $(addprefix $(DESTDIR)$(libdir)$(SEP), $(LIB_FILES))

lib_install: $(DESTDIR)$(libdir) $(UNINSTALL_LIB_FILES)

$(UNINSTALL_LIB_FILES):
	$(CP) $(subst $(DESTDIR)$(libdir)$(SEP),,$@) $(DESTDIR)$(libdir)

lib_uninstall:
	$(RM) $(UNINSTALL_LIB_FILES)
endif



ifdef DATA_FILES
UNINSTALL_DATA_FILES := $(addprefix $(DESTDIR)$(datadir)$(SEP), $(DATA_FILES))

data_install: $(DESTDIR)$(datadir) $(UNINSTALL_DATA_FILES)

$(UNINSTALL_LIB_FILES):
	$(CP) $(subst $(DESTDIR)$(datadir)$(SEP),,$@) $(DESTDIR)$(datadir)

data_uninstall:
	$(RM) $(UNINSTALL_DATA_FILES)
endif



ifdef TEST_BIN_FILES
UNINSTALL_TEST_BIN_FILES := $(addprefix $(DESTDIR)$(bindir)$(SEP), $(TEST_BIN_FILES))

test_bin_install: $(DESTDIR)$(bindir) $(UNINSTALL_TEST_BIN_FILES)

$(UNINSTALL_TEST_BIN_FILES):
	$(CP) $(subst $(DESTDIR)$(bindir)$(SEP),,$@) $(DESTDIR)$(bindir)

test_bin_uninstall:
	$(RM) $(UNINSTALL_TEST_BIN_FILES)
endif


ifdef TEST_LIB_FILES
UNINSTALL_TEST_LIB_FILES := $(addprefix $(DESTDIR)$(libdir)$(SEP), $(TEST_LIB_FILES))

test_lib_install: $(DESTDIR)$(libdir) $(UNINSTALL_TEST_LIB_FILES)

$(UNINSTALL_TEST_LIB_FILES):
	$(CP) $(subst $(DESTDIR)$(libdir)$(SEP),,$@) $(DESTDIR)$(libdir)

test_lib_uninstall:
	$(RM) $(UNINSTALL_TEST_LIB_FILES)
endif

ifdef TEST_DATA_FILES
UNINSTALL_TEST_DATA_FILES := $(addprefix $(DESTDIR)$(datadir)$(SEP), $(TEST_DATA_FILES))

test_data_install: $(DESTDIR)$(datadir) $(UNINSTALL_TEST_DATA_FILES)

$(UNINSTALL_TEST_DATA_FILES):
	$(CP) $(subst $(DESTDIR)$(datadir)$(SEP),,$@) $(DESTDIR)$(datadir)

test_data_uninstall:
	$(RM) $(UNINSTALL_TEST_DATA_FILES)
endif


ifdef NUNIT_TESTS
export NUNIT_TESTS
test: installtest $(TEST_RUN_MAKEFILE)
	-$(MAKE) -C $(DESTDIR)$(bindir) -f test.mk test
else
test: installtest $(TEST_RUN_MAKEFILE)
	@echo "No Test cases to run"
endif

$(TEST_RUN_MAKEFILE):
	$(CP) $(ROOTDIR)$(SEP)test.mk $(TEST_RUN_MAKEFILE)

