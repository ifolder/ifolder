# Setup install targets

UNINSTALL_BIN_FILES := $(addprefix $(DESTDIR)$(bindir)$(SEP), $(BIN_FILES))
UNINSTALL_LIB_FILES := $(addprefix $(DESTDIR)$(libdir)$(SEP), $(LIB_FILES))
UNINSTALL_DATA_FILES := $(addprefix $(DESTDIR)$(datadir)$(SEP), $(DATA_FILES))
UNINSTALL_TEST_BIN_FILES := $(addprefix $(DESTDIR)$(bindir)$(SEP), $(TEST_BIN_FILES))
UNINSTALL_TEST_LIB_FILES := $(addprefix $(DESTDIR)$(libdir)$(SEP), $(TEST_LIB_FILES))
UNINSTALL_TEST_DATA_FILES := $(addprefix $(DESTDIR)$(datadir)$(SEP), $(TEST_DATA_FILES))

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


ifdef BIN_FILES
bin_install:
	$(MKDIR_R) $(DESTDIR)$(bindir)
	$(CP) $(BIN_FILES) $(DESTDIR)$(bindir)
bin_uninstall:
	$(RM) $(UNINSTALL_BIN_FILES)
endif

ifdef LIB_FILES
lib_install:
	$(MKDIR_R) $(DESTDIR)$(libdir)
	$(CP) $(LIB_FILES) $(DESTDIR)$(libdir)
lib_uninstall:
	$(RM) $(UNINSTALL_LIB_FILES)
endif

ifdef DATA_FILES
data_install:
	$(MKDIR_R) $(DESTDIR)$(datadir)
	$(CP) $(DATA_FILES) $(DESTDIR)$(datadir)
data_uninstall:
	$(RM) $(UNINSTALL_DATA_FILES)
endif

ifdef TEST_BIN_FILES
test_bin_install:
	$(MKDIR_R) $(DESTDIR)$(bindir)
	$(CP) $(TEST_BIN_FILES) $(DESTDIR)$(bindir)
test_bin_uninstall:
	$(RM) $(UNINSTALL_TEST_BIN_FILES)
endif

ifdef TEST_LIB_FILES
test_lib_install:
	$(MKDIR_R) $(DESTDIR)$(libdir)
	$(CP) $(TEST_LIB_FILES) $(DESTDIR)$(libdir)
test_lib_uninstall:
	$(RM) $(UNINSTALL_TEST_LIB_FILES)
endif

ifdef TEST_DATA_FILES
test_data_install:
	$(MKDIR_R) $(DESTDIR)$(datadir)
	$(CP) $(TEST_DATA_FILES) $(DESTDIR)$(datadir)
test_data_uninstall:
	$(RM) $(UNINSTALL_TEST_DATA_FILES)
endif


