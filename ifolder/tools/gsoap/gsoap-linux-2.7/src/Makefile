#       gSOAP soapcpp2 Makefile by Robert van Engelen, Genivia inc.
#       Requires Flex (or Lex) and Bison
#
#	Mac OS X universal binary:
#	CC=gcc -arch i386 -arch ppc
CC=gcc
#       use LEX=flex -l or LEX=lex
LEX=flex -l
#       use YACC=bison or YACC=yacc
YACC=bison
#	For static linkage with Flex use:
ifndef (lib)
lib=lib
endif

LIBS=/usr/$(lib)/libfl.a
#	For static linkage with Lex use:
#	LIBS=/usr/lib/libl.a
COFLAGS=-O1
#       For static linking use:
#       COFLAGS=-O1 -static
#       For debugging use:
#	COFLAGS=-g
CWFLAGS=-Wall
CIFLAGS=
#       use -DWITH_BISON or -DWITH_YACC
#       use -DWITH_FLEX or -DWITH_LEX
CMFLAGS=-DWITH_BISON -DWITH_FLEX
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)
soapcpp2:	soapcpp2.h soapcpp2_yacc.tab.o symbol2.o error2.o lex.yy.o init2.o soapcpp2.o
		$(CC) $(CFLAGS) symbol2.o error2.o soapcpp2_yacc.tab.o lex.yy.o init2.o soapcpp2.o $(LIBS) -o $@
		cp -f soapcpp2 ../bin
soapcpp2_yacc.tab.c:	soapcpp2_yacc.y soapcpp2.h error2.h
		$(YACC) -d -v soapcpp2_yacc.y
lex.yy.c:	soapcpp2_yacc.tab.h soapcpp2_yacc.tab.c soapcpp2_lex.l
		$(LEX) soapcpp2_lex.l
.c.o:		soapcpp2.h soapcpp2_yacc.tab.h error2.h
		$(CC) $(CFLAGS) -c $<
clean:		
		rm -f *.o
distclean:		
		rm -f soapcpp2 *.o lex.yy.* soapcpp2_yacc.tab.h soapcpp2_yacc.tab.c y.tab.* *.output
