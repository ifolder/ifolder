#	gSOAP 2 Makefile by Robert van Engelen, Genivia inc.
#	Requires Flex (or Lex) and Bison
#
CC=gcc
CPP=g++
#	use LEX=flex -l or LEX=lex
LEX=flex -l
#	use YACC=bison or YACC=yacc
YACC=bison
#	use LIBS=-lfl with flex or LIBS=-ll with lex
#	and use -ly with yacc
LIBS=-lfl
#COFLAGS=-g -DDEBUG
COFLAGS=-O1
CWFLAGS=
CIFLAGS=
#	use -DWITH_BISON or -DWITH_YACC
#	use -DWITH_FLEX or -DWITH_LEX
CMFLAGS=-DWITH_BISON -DWITH_FLEX
CFLAGS= $(CWFLAGS) $(COFLAGS) $(CIFLAGS) $(CMFLAGS)
soapcpp2:	soapcpp2.h soapcpp2_yacc.o symbol2.o error2.o lex.yy.o init2.o soapcpp2.o
		$(CC) symbol2.o error2.o soapcpp2_yacc.o lex.yy.o init2.o soapcpp2.o -static $(LIBS) -o $@
soapcpp2_yacc.c:	soapcpp2_yacc.y soapcpp2.h error2.h
		$(YACC) -d -v soapcpp2_yacc.y
		mv -f soapcpp2_yacc.tab.h soapcpp2_yacc.h
		mv -f soapcpp2_yacc.tab.c soapcpp2_yacc.c
lex.yy.c:	soapcpp2_yacc.h soapcpp2_yacc.c soapcpp2_lex.l
		$(LEX) soapcpp2_lex.l
.c.o:		soapcpp2.h soapcpp2_yacc.h error2.h
		$(CC) $(CFLAGS) -c $<
clean:		
		rm -f *.o
distclean:		
		rm -f soapcpp2 *.o lex.yy.* soapcpp2_yacc.h soapcpp2_yacc.c y.tab.* *.output
