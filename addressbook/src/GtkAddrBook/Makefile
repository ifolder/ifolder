#***********************************************************************
# *  $RCSfile$
# *
# *  Copyright (C) 2004 Novell, Inc.
# *
# *  This program is free software; you can redistribute it and/or
# *  modify it under the terms of the GNU General Public
# *  License as published by the Free Software Foundation; either
# *  version 2 of the License, or (at your option) any later version.
# *
# *  This program is distributed in the hope that it will be useful,
# *  but WITHOUT ANY WARRANTY; without even the implied warranty of
# *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# *  Library General Public License for more details.
# *
# *  You should have received a copy of the GNU General Public License
# *  along with this program; if not, write to the Free Software
# *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
# *
# *  Author: Calvin Gaisford <cgaisford@novell.com>
# *
# ***********************************************************************/

include ../config.mk

#=============================================================================
# Book variables
#=============================================================================
Book_SOURCES = Main.cs
Book_INCLUDES = 
Book_RESOURCES = 
Book_FLAGS = /t:exe
Book_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll pango-sharp.dll gnome-sharp.dll glib-sharp.dll gdk-sharp.dll glade-sharp.dll $(SYSTEM_XML) StoreProvider.dll Identity.dll CollectionStore.dll AddressBook.dll BookLib.dll
Book_LIBPATH = $(SIMIAS_ROOT)/src/stage
Book_LDFLAGS = 

#=============================================================================
# BookLib variables
#=============================================================================
BookLib_SOURCES = AddrBookWindow.cs ContactEditor.cs BookEditor.cs ContactPicker.cs
BookLib_INCLUDES = 
BookLib_RESOURCES = addressbook.glade
BookLib_FLAGS = /t:library
BookLib_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll pango-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) StoreProvider.dll Identity.dll CollectionStore.dll AddressBook.dll
BookLib_LIBPATH = $(SIMIAS_ROOT)/src/stage
BookLib_LDFLAGS = 

#=============================================================================
# globals
#=============================================================================

TARGETS = BookLib.dll Book.exe 
CLEAN_FILES = Check.xml iFolder.xml Check.xml
NUNIT_TESTS =
#STAGE_FILES = $(TARGETS) *.png
STAGE_FILES = $(TARGETS) book.png briefcase.png contact.png contact_me.png cellphone.png connect_to_url-16.xpm galeon.png globe.png house.png ico-mail.png blankhead.png addressbook.glade
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
all: $(TARGETS) $(STAGE_FILES)

# clean
clean:
	$(RM) $(TARGETS) $(CLEAN_FILES) $(STAGE_FILES)

# test
test:
	@echo No tests available
#	$(NUNIT) $(NUNIT_FLAGS) /xml:iFolder.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES):	$(TARGETS)
	$(CP) $(subst $(STAGE_DIR)$(SEP),,$@) $(STAGE_DIR)

#=============================================================================
# Book targets
#=============================================================================

# fix path seperator
Book_SOURCES := $(subst /,$(SEP), $(Book_SOURCES))
Book_INCLUDES := $(subst /,$(SEP), $(Book_INCLUDES))
Book_LIBPATH := $(subst /,$(SEP), $(Book_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(Book_LIBPATH) $(Book_INCLUDES)

Book.exe: $(Book_SOURCES) $(Book_RESOURCE) $(Book_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:Book.xml $(CSCFLAGS) $(Book_FLAGS) $(Book_LIBS:%=/r:%) $(Book_LIBPATH:%=/lib:%) $(Book_SOURCES)

#=============================================================================
# BookLib targets
#=============================================================================

# fix path seperator
BookLib_SOURCES := $(subst /,$(SEP), $(BookLib_SOURCES))
BookLib_INCLUDES := $(subst /,$(SEP), $(BookLib_INCLUDES))
BookLib_LIBPATH := $(subst /,$(SEP), $(BookLib_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(BookLib_LIBPATH) $(BookLib_INCLUDES)

BookLib.dll: $(BookLib_SOURCES) $(BookLib_RESOURCE) $(BookLib_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:BookLib.xml $(CSCFLAGS) $(BookLib_FLAGS) $(BookLib_LIBS:%=/r:%) $(BookLib_LIBPATH:%=/lib:%) $(BookLib_SOURCES)
