#=============================================================================
# Module:	Smtp Makefile
#
#=============================================================================

# configure makefile
include ../../config.mk

#=============================================================================
# Smtp variables
#=============================================================================
Smtp_SOURCES = Base64AttachmentEncoder.cs IAttachmentEncoder.cs \
	MailAddress.cs MailAddressCollection.cs MailAttachment.cs\
	MailEncoding.cs MailFormat.cs MailHeader.cs MailMessage.cs \
	MailMessageWrapper.cs MailPriority.cs MailUtil.cs \
	SmtpClient.cs SmtpException.cs SmtpMail.cs SmtpResponse.cs \
	SmtpStream.cs ToUUEncodingTransform.cs UUAttachmentEncoder.cs
Smtp_INCLUDES = 
Smtp_RESOURCES =
Smtp_FLAGS = /t:library
Smtp_LIBS = System.dll Configuration.dll $(SYSTEM_XML)
Smtp_LIBPATH = ../Configuration /opt/gnome2/lib /usr/lib
Smtp_LDFLAGS = 

#=============================================================================
# SmtpTests variables
#=============================================================================
SmtpTests_SOURCES = SmtpTests.cs
SmtpTests_INCLUDES = 
SmtpTests_FLAGS = /t:exe
SmtpTests_LIBS = System.dll System.Data.dll Configuration.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Smtp.dll
SmtpTests_LIBPATH = ../Configuration ../../tools/NUnit/bin
SmtpTests_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================
BIN_FILES =
LIB_FILES = Smtp.dll
DATA_FILES = 
TEST_BIN_FILES = SmtpTests.exe
TEST_LIB_FILES = 
TEST_DATA_FILES = smtptst1.gif smtptst2.gif

TARGETS = $(BIN_FILES) $(LIB_FILES) $(TEST_BIN_FILES) $(TEST_LIB_FILES) 
#TARGETS = Smtp.dll SmtpTests.exe
CLEAN_FILES = Smtp.xml SmtpTests.xml default.conf
NUNIT_TESTS = SmtpTests.exe
# for now we are including libegg.so and egg-sharp.dll until this gets
# distributed with gtk-sharp
STAGE_FILES = $(TARGETS) smtptst1.gif smtptst2.gif
STAGE_FILES := $(addprefix $(STAGE_DIR)$(SEP), $(STAGE_FILES))

#=============================================================================
# custom makefile
#=============================================================================
ifneq "$(wildcard custom.mk)" ""
	include custom.mk
endif

#=============================================================================
# global targets
#=============================================================================

# fix path seperator
CLEAN_FILES := $(subst /,$(SEP),$(CLEAN_FILES))
STAGE_FILES := $(subst /,$(SEP),$(STAGE_FILES))

# all
all: $(TARGETS)
#all: $(TARGETS) $(STAGE_FILES)

# clean
clean:
	$(RM) $(TARGETS)

# test
test:
	$(NUNIT) $(NUNIT_FLAGS) /xml:SmtpTests.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(CP) $(subst $(STAGE_DIR)$(SEP),,$@) $(STAGE_DIR)

#=============================================================================
# Smtp targets
#=============================================================================

# fix path seperator
Smtp_SOURCES := $(subst /,$(SEP), $(Smtp_SOURCES))
Smtp_INCLUDES := $(subst /,$(SEP), $(Smtp_INCLUDES))
Smtp_LIBPATH := $(subst /,$(SEP), $(Smtp_LIBPATH))

SmtpTests_SOURCES := $(subst /,$(SEP), $(SmtpTests_SOURCES))
SmtpTests_INCLUDES := $(subst /,$(SEP), $(SmtpTests_INCLUDES))
SmtpTests_LIBPATH := $(subst /,$(SEP), $(SmtpTests_LIBPATH))
# update VPATH

VPATH := $(VPATH) $(Smtp_LIBPATH) $(Smtp_INCLUDES) $(SmtpTests_LIBPATH) $(SmtpTests_INCLUDES)

Smtp.dll: $(Smtp_SOURCES) $(Smtp_RESOURCE) $(Smtp_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:Smtp.xml $(CSCFLAGS) $(Smtp_FLAGS) $(Smtp_LIBS:%=/r:%) $(Smtp_LIBPATH:%=/lib:%) $(Smtp_SOURCES)

SmtpTests.exe: $(SmtpTests_SOURCES) $(SmtpTests_RESOURCE) $(SmtpTests_LIBS) ../../config.mk
	$(CSC) /out:$@ /doc:SmtpTests.xml $(CSCFLAGS) $(SmtpTests_FLAGS) $(SmtpTests_LIBS:%=/r:%) $(SmtpTests_LIBPATH:%=/lib:%) $(SmtpTests_SOURCES)

include ../../common.mk
