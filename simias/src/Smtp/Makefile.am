Smtp_CSFILES = $(srcdir)/Base64AttachmentEncoder.cs $(srcdir)/IAttachmentEncoder.cs $(srcdir)/MailAddress.cs $(srcdir)/MailAddressCollection.cs $(srcdir)/MailAttachment.cs $(srcdir)/MailEncoding.cs $(srcdir)/MailFormat.cs $(srcdir)/MailHeader.cs $(srcdir)/MailMessage.cs $(srcdir)/MailMessageWrapper.cs $(srcdir)/MailPriority.cs $(srcdir)/MailUtil.cs $(srcdir)/SmtpClient.cs $(srcdir)/SmtpException.cs $(srcdir)/SmtpMail.cs $(srcdir)/SmtpResponse.cs $(srcdir)/SmtpStream.cs $(srcdir)/ToUUEncodingTransform.cs $(srcdir)/UUAttachmentEncoder.cs
Smtp_CSFILES_CSC := $(subst /,$(SEP),$(Smtp_CSFILES))
Smtp_INCLUDES = 
Smtp_RESOURCES = 
Smtp_FLAGS = $(CSC_LIBFLAG)
Smtp_LIBS = System.dll $(SYSTEM_XML) Configuration.dll
Smtp_LIBPATH = ../Configuration

NUNIT_TESTS := SmtpTests.exe

SmtpTests_CSFILES = $(srcdir)/SmtpTests.cs
SmtpTests_CSFILES_CSC := $(subst /,$(SEP),$(SmtpTests_CSFILES))
SmtpTests_INCLUDES = 
SmtpTests_RESOURCES = $(srcdir)/smtptst1.gif $(srcdir)/smtptst2.gif
SmtpTests_FLAGS = $(CSC_EXEFLAG)
SmtpTests_LIBS = System.dll System.Data.dll Configuration.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Smtp.dll
SmtpTests_LIBPATH = ../Configuration $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(Smtp_CSFILES) $(SmtpTests_CSFILES) $(SmtpTests_RESOURCES)

CUR_DIR := $(shell pwd)

all: Smtp.dll

Smtp.dll: $(Smtp_CSFILES) $(Smtp_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Smtp_FLAGS) $(Smtp_LIBS:%=/r:%) $(Smtp_LIBPATH:%=/lib:%) $(Smtp_CSFILES_CSC) /doc:Smtp.xml

SmtpTests.exe: $(SmtpTests_CSFILES) $(SmtpTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SmtpTests_FLAGS) $(SmtpTests_LIBS:%=/r:%) $(SmtpTests_LIBPATH:%=/lib:%) $(SmtpTests_CSFILES_CSC)
	
install-exec-local: Smtp.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Smtp.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f Smtp.dll SmtpTests.xml $(NUNIT_TESTS) smtptst1.gif smtptst2.gif
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(SmtpTests_RESOURCES) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:SmtpTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Smtp.dll $(NUNIT_TESTS) Smtp.xml SmtpTests.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

