/*

includes.h

--------------------------------------------------------------------------------
gSOAP XML Web services tools
Copyright (C) 2001-2004, Robert van Engelen, Genivia, Inc. All Rights Reserved.
This software is released under one of the following two licenses:
GPL or Genivia's license for commercial use.
--------------------------------------------------------------------------------
GPL license.

This program is free software; you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation; either version 2 of the License, or (at your option) any later
version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place, Suite 330, Boston, MA 02111-1307 USA

Author contact information:
engelen@genivia.com / engelen@acm.org
--------------------------------------------------------------------------------
A commercial use license is available from Genivia, Inc., contact@genivia.com
--------------------------------------------------------------------------------

*/

#ifndef INCLUDES_H
#define INCLUDES_H

#include "stdsoap2.h"

#undef VERSION
#define VERSION "1.1.4b"

#include <utility>
#include <iterator>
#include <set>
#include <map>

using namespace std;

struct ltstr
{ bool operator()(const char *s1, const char *s2) const
  { return strcmp(s1, s2) < 0;
  }
}; 

typedef set<const char*, ltstr> SetOfString;

typedef pair<const char*, const char*> Pair;

struct ltpair
{ bool operator()(Pair s1, Pair s2) const
  { int cmp = strcmp(s1.first, s2.first);
    if (cmp)
      return cmp < 0;
    return strcmp(s1.second, s2.second) < 0;
  }
};

typedef map<const char*, const char*, ltstr> MapOfStringToString;

typedef map<Pair, const char*, ltpair> MapOfPairToString;

typedef map<const char*, size_t, ltstr> MapOfStringToNum;

extern int cflag, eflag, fflag, lflag, mflag, pflag, sflag, vflag;

extern FILE *stream;

extern char *infile, *outfile, *mapfile, *proxy_host;

extern int proxy_port;

extern const char *prefix_name;

extern char elementformat[];
extern char pointerformat[];
extern char attributeformat[];
extern char vectorformat[];
extern char arrayformat[];
extern char sizeformat[];
extern char schemaformat[];
extern char serviceformat[];
extern char paraformat[];
extern char anonformat[];
extern char copyrightnotice[];
extern char licensenotice[];

extern void *emalloc(size_t size);
extern char *estrdup(const char *s);

#endif
