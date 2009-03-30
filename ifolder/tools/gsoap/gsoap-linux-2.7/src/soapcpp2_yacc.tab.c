/* A Bison parser, made by GNU Bison 2.1.  */

/* Skeleton parser for Yacc-like parsing with Bison,
   Copyright (C) 1984, 1989, 1990, 2000, 2001, 2002, 2003, 2004, 2005 Free Software Foundation, Inc.

   This program is free software; you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation; either version 2, or (at your option)
   any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program; if not, write to the Free Software
   Foundation, Inc., 51 Franklin Street, Fifth Floor,
   Boston, MA 02110-1301, USA.  */

/* As a special exception, when this file is copied by Bison into a
   Bison output file, you may use that output file without restriction.
   This special exception was added by the Free Software Foundation
   in version 1.24 of Bison.  */

/* Written by Richard Stallman by simplifying the original so called
   ``semantic'' parser.  */

/* All symbols defined below should begin with yy or YY, to avoid
   infringing on user name space.  This should be done even for local
   variables, as they might otherwise be expanded by user macros.
   There are some unavoidable exceptions within include files to
   define necessary library symbols; they are noted "INFRINGES ON
   USER NAME SPACE" below.  */

/* Identify Bison output.  */
#define YYBISON 1

/* Bison version.  */
#define YYBISON_VERSION "2.1"

/* Skeleton name.  */
#define YYSKELETON_NAME "yacc.c"

/* Pure parsers.  */
#define YYPURE 0

/* Using locations.  */
#define YYLSP_NEEDED 0



/* Tokens.  */
#ifndef YYTOKENTYPE
# define YYTOKENTYPE
   /* Put the tokens into the symbol table, so that GDB and other debuggers
      know about them.  */
   enum yytokentype {
     PRAGMA = 258,
     AUTO = 259,
     DOUBLE = 260,
     INT = 261,
     STRUCT = 262,
     BREAK = 263,
     ELSE = 264,
     LONG = 265,
     SWITCH = 266,
     CASE = 267,
     ENUM = 268,
     REGISTER = 269,
     TYPEDEF = 270,
     CHAR = 271,
     EXTERN = 272,
     RETURN = 273,
     UNION = 274,
     CONST = 275,
     FLOAT = 276,
     SHORT = 277,
     UNSIGNED = 278,
     CONTINUE = 279,
     FOR = 280,
     SIGNED = 281,
     VOID = 282,
     DEFAULT = 283,
     GOTO = 284,
     SIZEOF = 285,
     VOLATILE = 286,
     DO = 287,
     IF = 288,
     STATIC = 289,
     WHILE = 290,
     CLASS = 291,
     PRIVATE = 292,
     PROTECTED = 293,
     PUBLIC = 294,
     VIRTUAL = 295,
     INLINE = 296,
     OPERATOR = 297,
     LLONG = 298,
     BOOL = 299,
     CFALSE = 300,
     CTRUE = 301,
     WCHAR = 302,
     TIME = 303,
     USING = 304,
     NAMESPACE = 305,
     ULLONG = 306,
     MUSTUNDERSTAND = 307,
     SIZE = 308,
     FRIEND = 309,
     TEMPLATE = 310,
     EXPLICIT = 311,
     TYPENAME = 312,
     RESTRICT = 313,
     null = 314,
     NONE = 315,
     ID = 316,
     LAB = 317,
     TYPE = 318,
     LNG = 319,
     DBL = 320,
     CHR = 321,
     STR = 322,
     RA = 323,
     LA = 324,
     OA = 325,
     XA = 326,
     AA = 327,
     MA = 328,
     DA = 329,
     TA = 330,
     NA = 331,
     PA = 332,
     OR = 333,
     AN = 334,
     NE = 335,
     EQ = 336,
     GE = 337,
     LE = 338,
     RS = 339,
     LS = 340,
     AR = 341,
     PP = 342,
     NN = 343
   };
#endif
/* Tokens.  */
#define PRAGMA 258
#define AUTO 259
#define DOUBLE 260
#define INT 261
#define STRUCT 262
#define BREAK 263
#define ELSE 264
#define LONG 265
#define SWITCH 266
#define CASE 267
#define ENUM 268
#define REGISTER 269
#define TYPEDEF 270
#define CHAR 271
#define EXTERN 272
#define RETURN 273
#define UNION 274
#define CONST 275
#define FLOAT 276
#define SHORT 277
#define UNSIGNED 278
#define CONTINUE 279
#define FOR 280
#define SIGNED 281
#define VOID 282
#define DEFAULT 283
#define GOTO 284
#define SIZEOF 285
#define VOLATILE 286
#define DO 287
#define IF 288
#define STATIC 289
#define WHILE 290
#define CLASS 291
#define PRIVATE 292
#define PROTECTED 293
#define PUBLIC 294
#define VIRTUAL 295
#define INLINE 296
#define OPERATOR 297
#define LLONG 298
#define BOOL 299
#define CFALSE 300
#define CTRUE 301
#define WCHAR 302
#define TIME 303
#define USING 304
#define NAMESPACE 305
#define ULLONG 306
#define MUSTUNDERSTAND 307
#define SIZE 308
#define FRIEND 309
#define TEMPLATE 310
#define EXPLICIT 311
#define TYPENAME 312
#define RESTRICT 313
#define null 314
#define NONE 315
#define ID 316
#define LAB 317
#define TYPE 318
#define LNG 319
#define DBL 320
#define CHR 321
#define STR 322
#define RA 323
#define LA 324
#define OA 325
#define XA 326
#define AA 327
#define MA 328
#define DA 329
#define TA 330
#define NA 331
#define PA 332
#define OR 333
#define AN 334
#define NE 335
#define EQ 336
#define GE 337
#define LE 338
#define RS 339
#define LS 340
#define AR 341
#define PP 342
#define NN 343




/* Copy the first part of user declarations.  */
#line 58 "soapcpp2_yacc.y"


#include "soapcpp2.h"

#ifdef WIN32
#ifndef __STDC__
#define __STDC__
#endif
#define YYINCLUDED_STDLIB_H
#ifdef WIN32_WITHOUT_SOLARIS_FLEX
extern int soapcpp2lex();
#else
extern int yylex();
#endif
#else
extern int yylex();
#endif

extern int is_XML(Tnode*);

#define MAXNEST 16	/* max. nesting depth of scopes */

struct Scope
{	Table	*table;
	Entry	*entry;
	Node	node;
	LONG64	val;
	int	offset;
	Bool	grow;	/* true if offset grows with declarations */
	Bool	mask;	/* true if enum is mask */
}	stack[MAXNEST],	/* stack of tables and offsets */
	*sp;		/* current scope stack pointer */

Table	*classtable = (Table*)0,
	*enumtable = (Table*)0,
	*typetable = (Table*)0,
	*booltable = (Table*)0,
	*templatetable = (Table*)0;

char	*namespaceid = NULL;
int	transient = 0;
int	permission = 0;
int	custom_header = 1;
int	custom_fault = 1;
Pragma	*pragmas = NULL;
Tnode	*qname = NULL;
Tnode	*xml = NULL;

/* function prototypes for support routine section */
static Entry	*undefined(Symbol*);
static Tnode	*mgtype(Tnode*, Tnode*);
static Node	op(const char*, Node, Node), iop(const char*, Node, Node), relop(const char*, Node, Node);
static void	mkscope(Table*, int), enterscope(Table*, int), exitscope();
static int	integer(Tnode*), real(Tnode*), numeric(Tnode*);
static void	add_XML(), add_qname(), add_header(Table*), add_fault(Table*), add_response(Entry*, Entry*), add_result(Tnode*);
extern char	*c_storage(Storage), *c_type(Tnode*), *c_ident(Tnode*);
extern int	is_primitive_or_string(Tnode*), is_stdstr(Tnode*), is_binary(Tnode*), is_external(Tnode*);

/* temporaries used in semantic rules */
int	i;
char	*s, *s1, *s2;
Symbol	*sym;
Entry	*p, *q;
Tnode	*t;
Node	tmp, c;
Pragma	**pp;



/* Enabling traces.  */
#ifndef YYDEBUG
# define YYDEBUG 0
#endif

/* Enabling verbose error messages.  */
#ifdef YYERROR_VERBOSE
# undef YYERROR_VERBOSE
# define YYERROR_VERBOSE 1
#else
# define YYERROR_VERBOSE 0
#endif

/* Enabling the token table.  */
#ifndef YYTOKEN_TABLE
# define YYTOKEN_TABLE 0
#endif

#if ! defined (YYSTYPE) && ! defined (YYSTYPE_IS_DECLARED)
#line 132 "soapcpp2_yacc.y"
typedef union YYSTYPE {	Symbol	*sym;
	LONG64	i;
	double	r;
	char	c;
	char	*s;
	Tnode	*typ;
	Storage	sto;
	Node	rec;
	Entry	*e;
} YYSTYPE;
/* Line 196 of yacc.c.  */
#line 341 "soapcpp2_yacc.tab.c"
# define yystype YYSTYPE /* obsolescent; will be withdrawn */
# define YYSTYPE_IS_DECLARED 1
# define YYSTYPE_IS_TRIVIAL 1
#endif



/* Copy the second part of user declarations.  */


/* Line 219 of yacc.c.  */
#line 353 "soapcpp2_yacc.tab.c"

#if ! defined (YYSIZE_T) && defined (__SIZE_TYPE__)
# define YYSIZE_T __SIZE_TYPE__
#endif
#if ! defined (YYSIZE_T) && defined (size_t)
# define YYSIZE_T size_t
#endif
#if ! defined (YYSIZE_T) && (defined (__STDC__) || defined (__cplusplus))
# include <stddef.h> /* INFRINGES ON USER NAME SPACE */
# define YYSIZE_T size_t
#endif
#if ! defined (YYSIZE_T)
# define YYSIZE_T unsigned int
#endif

#ifndef YY_
# if YYENABLE_NLS
#  if ENABLE_NLS
#   include <libintl.h> /* INFRINGES ON USER NAME SPACE */
#   define YY_(msgid) dgettext ("bison-runtime", msgid)
#  endif
# endif
# ifndef YY_
#  define YY_(msgid) msgid
# endif
#endif

#if ! defined (yyoverflow) || YYERROR_VERBOSE

/* The parser invokes alloca or malloc; define the necessary symbols.  */

# ifdef YYSTACK_USE_ALLOCA
#  if YYSTACK_USE_ALLOCA
#   ifdef __GNUC__
#    define YYSTACK_ALLOC __builtin_alloca
#   else
#    define YYSTACK_ALLOC alloca
#    if defined (__STDC__) || defined (__cplusplus)
#     include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
#     define YYINCLUDED_STDLIB_H
#    endif
#   endif
#  endif
# endif

# ifdef YYSTACK_ALLOC
   /* Pacify GCC's `empty if-body' warning. */
#  define YYSTACK_FREE(Ptr) do { /* empty */; } while (0)
#  ifndef YYSTACK_ALLOC_MAXIMUM
    /* The OS might guarantee only one guard page at the bottom of the stack,
       and a page size can be as small as 4096 bytes.  So we cannot safely
       invoke alloca (N) if N exceeds 4096.  Use a slightly smaller number
       to allow for a few compiler-allocated temporary stack slots.  */
#   define YYSTACK_ALLOC_MAXIMUM 4032 /* reasonable circa 2005 */
#  endif
# else
#  define YYSTACK_ALLOC YYMALLOC
#  define YYSTACK_FREE YYFREE
#  ifndef YYSTACK_ALLOC_MAXIMUM
#   define YYSTACK_ALLOC_MAXIMUM ((YYSIZE_T) -1)
#  endif
#  ifdef __cplusplus
extern "C" {
#  endif
#  ifndef YYMALLOC
#   define YYMALLOC malloc
#   if (! defined (malloc) && ! defined (YYINCLUDED_STDLIB_H) \
	&& (defined (__STDC__) || defined (__cplusplus)))
void *malloc (YYSIZE_T); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
#  ifndef YYFREE
#   define YYFREE free
#   if (! defined (free) && ! defined (YYINCLUDED_STDLIB_H) \
	&& (defined (__STDC__) || defined (__cplusplus)))
void free (void *); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
#  ifdef __cplusplus
}
#  endif
# endif
#endif /* ! defined (yyoverflow) || YYERROR_VERBOSE */


#if (! defined (yyoverflow) \
     && (! defined (__cplusplus) \
	 || (defined (YYSTYPE_IS_TRIVIAL) && YYSTYPE_IS_TRIVIAL)))

/* A type that is properly aligned for any stack member.  */
union yyalloc
{
  short int yyss;
  YYSTYPE yyvs;
  };

/* The size of the maximum gap between one aligned stack and the next.  */
# define YYSTACK_GAP_MAXIMUM (sizeof (union yyalloc) - 1)

/* The size of an array large to enough to hold all stacks, each with
   N elements.  */
# define YYSTACK_BYTES(N) \
     ((N) * (sizeof (short int) + sizeof (YYSTYPE))			\
      + YYSTACK_GAP_MAXIMUM)

/* Copy COUNT objects from FROM to TO.  The source and destination do
   not overlap.  */
# ifndef YYCOPY
#  if defined (__GNUC__) && 1 < __GNUC__
#   define YYCOPY(To, From, Count) \
      __builtin_memcpy (To, From, (Count) * sizeof (*(From)))
#  else
#   define YYCOPY(To, From, Count)		\
      do					\
	{					\
	  YYSIZE_T yyi;				\
	  for (yyi = 0; yyi < (Count); yyi++)	\
	    (To)[yyi] = (From)[yyi];		\
	}					\
      while (0)
#  endif
# endif

/* Relocate STACK from its old location to the new one.  The
   local variables YYSIZE and YYSTACKSIZE give the old and new number of
   elements in the stack, and YYPTR gives the new location of the
   stack.  Advance YYPTR to a properly aligned location for the next
   stack.  */
# define YYSTACK_RELOCATE(Stack)					\
    do									\
      {									\
	YYSIZE_T yynewbytes;						\
	YYCOPY (&yyptr->Stack, Stack, yysize);				\
	Stack = &yyptr->Stack;						\
	yynewbytes = yystacksize * sizeof (*Stack) + YYSTACK_GAP_MAXIMUM; \
	yyptr += yynewbytes / sizeof (*yyptr);				\
      }									\
    while (0)

#endif

#if defined (__STDC__) || defined (__cplusplus)
   typedef signed char yysigned_char;
#else
   typedef short int yysigned_char;
#endif

/* YYFINAL -- State number of the termination state. */
#define YYFINAL  3
/* YYLAST -- Last index in YYTABLE.  */
#define YYLAST   1011

/* YYNTOKENS -- Number of terminals. */
#define YYNTOKENS  113
/* YYNNTS -- Number of nonterminals. */
#define YYNNTS  61
/* YYNRULES -- Number of rules. */
#define YYNRULES  228
/* YYNRULES -- Number of states. */
#define YYNSTATES  355

/* YYTRANSLATE(YYLEX) -- Bison symbol number corresponding to YYLEX.  */
#define YYUNDEFTOK  2
#define YYMAXUTOK   343

#define YYTRANSLATE(YYX)						\
  ((unsigned int) (YYX) <= YYMAXUTOK ? yytranslate[YYX] : YYUNDEFTOK)

/* YYTRANSLATE[YYLEX] -- Bison symbol number corresponding to YYLEX.  */
static const unsigned char yytranslate[] =
{
       0,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,   108,     2,     2,     2,    99,    86,     2,
     110,   111,    97,    95,    68,    96,     2,    98,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,    81,   105,
      89,    69,    90,    80,   112,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,   106,     2,   107,    85,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,   103,    84,   104,   109,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     1,     2,     3,     4,
       5,     6,     7,     8,     9,    10,    11,    12,    13,    14,
      15,    16,    17,    18,    19,    20,    21,    22,    23,    24,
      25,    26,    27,    28,    29,    30,    31,    32,    33,    34,
      35,    36,    37,    38,    39,    40,    41,    42,    43,    44,
      45,    46,    47,    48,    49,    50,    51,    52,    53,    54,
      55,    56,    57,    58,    59,    60,    61,    62,    63,    64,
      65,    66,    67,    70,    71,    72,    73,    74,    75,    76,
      77,    78,    79,    82,    83,    87,    88,    91,    92,    93,
      94,   100,   101,   102
};

#if YYDEBUG
/* YYPRHS[YYN] -- Index of the first RHS symbol of rule number YYN in
   YYRHS.  */
static const unsigned short int yyprhs[] =
{
       0,     0,     3,     6,     7,    13,    15,    16,    19,    22,
      24,    27,    29,    31,    33,    34,    38,    43,    48,    53,
      58,    60,    62,    63,    64,    65,    67,    70,    74,    77,
      80,    84,    89,    95,    98,   100,   102,   104,   107,   110,
     113,   116,   119,   122,   125,   128,   131,   134,   137,   140,
     143,   146,   149,   152,   155,   158,   161,   164,   167,   170,
     173,   176,   179,   182,   185,   188,   191,   194,   197,   200,
     203,   206,   210,   214,   217,   219,   223,   231,   232,   233,
     235,   237,   241,   248,   249,   251,   255,   260,   261,   264,
     267,   269,   271,   274,   277,   279,   281,   283,   285,   287,
     289,   291,   293,   295,   297,   299,   301,   303,   305,   307,
     315,   321,   327,   335,   337,   341,   347,   353,   356,   359,
     365,   372,   375,   378,   385,   392,   401,   404,   407,   409,
     414,   417,   420,   423,   425,   427,   430,   433,   436,   438,
     441,   442,   443,   444,   445,   447,   448,   450,   452,   454,
     456,   458,   460,   462,   464,   466,   468,   470,   472,   474,
     476,   477,   479,   480,   483,   484,   486,   487,   490,   493,
     494,   499,   503,   505,   506,   509,   511,   514,   518,   523,
     527,   528,   530,   532,   535,   539,   541,   547,   549,   551,
     555,   557,   559,   563,   565,   567,   571,   575,   579,   583,
     587,   591,   595,   599,   603,   607,   611,   615,   619,   623,
     627,   631,   633,   636,   639,   642,   645,   648,   651,   656,
     658,   662,   664,   666,   668,   670,   672,   674,   676
};

/* YYRHS -- A `-1'-separated list of the rules' RHS. */
static const short int yyrhs[] =
{
     114,     0,    -1,   115,   116,    -1,    -1,    50,    61,   103,
     117,   104,    -1,   117,    -1,    -1,   117,   118,    -1,   126,
     105,    -1,   119,    -1,     1,   105,    -1,   121,    -1,   122,
      -1,     3,    -1,    -1,   126,   105,   120,    -1,    37,    81,
     123,   120,    -1,    38,    81,   124,   120,    -1,    39,    81,
     125,   120,    -1,   121,   120,   122,   120,    -1,   106,    -1,
     107,    -1,    -1,    -1,    -1,   140,    -1,   140,   127,    -1,
     140,   128,   133,    -1,   131,   133,    -1,   132,   133,    -1,
     126,    68,   127,    -1,   126,    68,   128,   133,    -1,   157,
      61,   159,   161,   160,    -1,   157,   130,    -1,    61,    -1,
      63,    -1,    61,    -1,    42,   108,    -1,    42,   109,    -1,
      42,    69,    -1,    42,    79,    -1,    42,    78,    -1,    42,
      77,    -1,    42,    76,    -1,    42,    75,    -1,    42,    74,
      -1,    42,    73,    -1,    42,    72,    -1,    42,    71,    -1,
      42,    70,    -1,    42,    82,    -1,    42,    83,    -1,    42,
      84,    -1,    42,    85,    -1,    42,    86,    -1,    42,    88,
      -1,    42,    87,    -1,    42,    89,    -1,    42,    92,    -1,
      42,    90,    -1,    42,    91,    -1,    42,    94,    -1,    42,
      93,    -1,    42,    95,    -1,    42,    96,    -1,    42,    97,
      -1,    42,    98,    -1,    42,    99,    -1,    42,   101,    -1,
      42,   102,    -1,    42,   100,    -1,    42,   106,   107,    -1,
      42,   110,   111,    -1,    42,   139,    -1,    63,    -1,   156,
     109,    63,    -1,   134,   110,   152,   135,   111,   154,   155,
      -1,    -1,    -1,   136,    -1,   137,    -1,   137,    68,   136,
      -1,   141,   157,   138,   159,   161,   160,    -1,    -1,    61,
      -1,   141,   157,   158,    -1,   141,   157,    61,   158,    -1,
      -1,   153,   140,    -1,   142,   140,    -1,   153,    -1,   142,
      -1,   153,   141,    -1,   142,   141,    -1,    27,    -1,    44,
      -1,    16,    -1,    47,    -1,    22,    -1,     6,    -1,    10,
      -1,    43,    -1,    51,    -1,    53,    -1,    21,    -1,     5,
      -1,    26,    -1,    23,    -1,    48,    -1,    55,    89,   146,
     129,    90,    36,   129,    -1,    36,   103,   148,   120,   104,
      -1,   144,   103,   148,   120,   104,    -1,   144,    81,   147,
     103,   148,   120,   104,    -1,   144,    -1,   144,    81,   147,
      -1,     7,   103,   148,   120,   104,    -1,   143,   103,   148,
     120,   104,    -1,     7,    61,    -1,     7,    63,    -1,    19,
     103,   149,   120,   104,    -1,    19,   129,   103,   149,   120,
     104,    -1,    19,    61,    -1,    19,    63,    -1,    13,   103,
     148,   126,   151,   104,    -1,   145,   103,   148,   126,   151,
     104,    -1,    13,    97,   129,   103,   150,   126,   151,   104,
      -1,    13,    61,    -1,    13,    63,    -1,    63,    -1,    63,
      89,   139,    90,    -1,     7,   129,    -1,    36,   129,    -1,
      13,   129,    -1,    36,    -1,    57,    -1,    38,   147,    -1,
      37,   147,    -1,    39,   147,    -1,    63,    -1,     7,    61,
      -1,    -1,    -1,    -1,    -1,    68,    -1,    -1,     4,    -1,
      14,    -1,    34,    -1,    56,    -1,    17,    -1,    15,    -1,
      40,    -1,    20,    -1,    54,    -1,    41,    -1,    52,    -1,
      18,    -1,   112,    -1,    31,    -1,    -1,    20,    -1,    -1,
      69,    64,    -1,    -1,    40,    -1,    -1,   157,    97,    -1,
     157,    86,    -1,    -1,   106,   165,   107,   158,    -1,   106,
     107,   158,    -1,   158,    -1,    -1,    69,   165,    -1,   162,
      -1,   162,   163,    -1,   162,   163,    81,    -1,   162,   163,
      81,   163,    -1,   162,    81,   163,    -1,    -1,    67,    -1,
      64,    -1,    96,    64,    -1,   164,    68,   164,    -1,   165,
      -1,   168,    80,   166,    81,   165,    -1,   167,    -1,   164,
      -1,   168,    82,   169,    -1,   169,    -1,   167,    -1,   170,
      83,   171,    -1,   171,    -1,   169,    -1,   171,    84,   171,
      -1,   171,    85,   171,    -1,   171,    86,   171,    -1,   171,
      88,   171,    -1,   171,    87,   171,    -1,   171,    89,   171,
      -1,   171,    92,   171,    -1,   171,    90,   171,    -1,   171,
      91,   171,    -1,   171,    94,   171,    -1,   171,    93,   171,
      -1,   171,    95,   171,    -1,   171,    96,   171,    -1,   171,
      97,   171,    -1,   171,    98,   171,    -1,   171,    99,   171,
      -1,   172,    -1,   108,   172,    -1,   109,   172,    -1,    96,
     172,    -1,    95,   172,    -1,    97,   172,    -1,    86,   172,
      -1,    30,   110,   139,   111,    -1,   173,    -1,   110,   164,
     111,    -1,    61,    -1,    64,    -1,    59,    -1,    65,    -1,
      66,    -1,    67,    -1,    45,    -1,    46,    -1
};

/* YYRLINE[YYN] -- source line where rule number YYN was defined.  */
static const unsigned short int yyrline[] =
{
       0,   205,   205,   221,   235,   237,   239,   242,   244,   245,
     246,   253,   254,   256,   279,   282,   284,   286,   288,   290,
     293,   296,   299,   302,   305,   308,   309,   310,   312,   313,
     314,   315,   318,   425,   441,   442,   444,   445,   446,   447,
     448,   449,   450,   451,   452,   453,   454,   455,   456,   457,
     458,   459,   460,   461,   462,   463,   464,   465,   466,   467,
     468,   469,   470,   471,   472,   473,   474,   475,   476,   477,
     478,   479,   480,   481,   492,   502,   519,   577,   579,   580,
     582,   583,   585,   664,   671,   686,   688,   691,   695,   705,
     752,   758,   762,   772,   819,   820,   821,   822,   823,   824,
     825,   826,   827,   828,   829,   830,   831,   832,   833,   834,
     842,   864,   875,   894,   897,   910,   932,   946,   961,   976,
     999,  1019,  1034,  1049,  1071,  1090,  1109,  1117,  1125,  1143,
    1153,  1168,  1185,  1202,  1203,  1205,  1206,  1207,  1208,  1215,
    1217,  1224,  1232,  1238,  1239,  1241,  1249,  1250,  1251,  1252,
    1253,  1254,  1255,  1256,  1257,  1258,  1259,  1260,  1261,  1265,
    1267,  1268,  1270,  1271,  1273,  1274,  1276,  1277,  1284,  1289,
    1291,  1304,  1308,  1316,  1317,  1328,  1333,  1338,  1343,  1348,
    1354,  1355,  1357,  1358,  1367,  1368,  1371,  1376,  1379,  1382,
    1385,  1387,  1390,  1393,  1395,  1398,  1399,  1400,  1401,  1402,
    1403,  1404,  1405,  1406,  1407,  1408,  1409,  1410,  1411,  1412,
    1413,  1414,  1417,  1422,  1427,  1437,  1438,  1445,  1449,  1454,
    1457,  1458,  1465,  1469,  1473,  1477,  1481,  1485,  1489
};
#endif

#if YYDEBUG || YYERROR_VERBOSE || YYTOKEN_TABLE
/* YYTNAME[SYMBOL-NUM] -- String name of the symbol SYMBOL-NUM.
   First, the terminals, then, starting at YYNTOKENS, nonterminals. */
static const char *const yytname[] =
{
  "$end", "error", "$undefined", "PRAGMA", "AUTO", "DOUBLE", "INT",
  "STRUCT", "BREAK", "ELSE", "LONG", "SWITCH", "CASE", "ENUM", "REGISTER",
  "TYPEDEF", "CHAR", "EXTERN", "RETURN", "UNION", "CONST", "FLOAT",
  "SHORT", "UNSIGNED", "CONTINUE", "FOR", "SIGNED", "VOID", "DEFAULT",
  "GOTO", "SIZEOF", "VOLATILE", "DO", "IF", "STATIC", "WHILE", "CLASS",
  "PRIVATE", "PROTECTED", "PUBLIC", "VIRTUAL", "INLINE", "OPERATOR",
  "LLONG", "BOOL", "CFALSE", "CTRUE", "WCHAR", "TIME", "USING",
  "NAMESPACE", "ULLONG", "MUSTUNDERSTAND", "SIZE", "FRIEND", "TEMPLATE",
  "EXPLICIT", "TYPENAME", "RESTRICT", "null", "NONE", "ID", "LAB", "TYPE",
  "LNG", "DBL", "CHR", "STR", "','", "'='", "RA", "LA", "OA", "XA", "AA",
  "MA", "DA", "TA", "NA", "PA", "'?'", "':'", "OR", "AN", "'|'", "'^'",
  "'&'", "NE", "EQ", "'<'", "'>'", "GE", "LE", "RS", "LS", "'+'", "'-'",
  "'*'", "'/'", "'%'", "AR", "PP", "NN", "'{'", "'}'", "';'", "'['", "']'",
  "'!'", "'~'", "'('", "')'", "'@'", "$accept", "prog", "s1", "exts",
  "exts1", "ext", "pragma", "decls", "t1", "t2", "t3", "t4", "t5", "dclrs",
  "dclr", "fdclr", "id", "name", "constr", "destr", "func", "fname",
  "fargso", "fargs", "farg", "arg", "texp", "spec", "tspec", "type",
  "struct", "class", "enum", "tname", "base", "s2", "s3", "s4", "s5", "s6",
  "store", "constobj", "abstract", "virtual", "ptrs", "array", "arrayck",
  "init", "occurs", "patt", "cint", "expr", "cexp", "qexp", "oexp", "obex",
  "aexp", "abex", "rexp", "lexp", "pexp", 0
};
#endif

# ifdef YYPRINT
/* YYTOKNUM[YYLEX-NUM] -- Internal token number corresponding to
   token YYLEX-NUM.  */
static const unsigned short int yytoknum[] =
{
       0,   256,   257,   258,   259,   260,   261,   262,   263,   264,
     265,   266,   267,   268,   269,   270,   271,   272,   273,   274,
     275,   276,   277,   278,   279,   280,   281,   282,   283,   284,
     285,   286,   287,   288,   289,   290,   291,   292,   293,   294,
     295,   296,   297,   298,   299,   300,   301,   302,   303,   304,
     305,   306,   307,   308,   309,   310,   311,   312,   313,   314,
     315,   316,   317,   318,   319,   320,   321,   322,    44,    61,
     323,   324,   325,   326,   327,   328,   329,   330,   331,   332,
      63,    58,   333,   334,   124,    94,    38,   335,   336,    60,
      62,   337,   338,   339,   340,    43,    45,    42,    47,    37,
     341,   342,   343,   123,   125,    59,    91,    93,    33,   126,
      40,    41,    64
};
# endif

/* YYR1[YYN] -- Symbol number of symbol that rule YYN derives.  */
static const unsigned char yyr1[] =
{
       0,   113,   114,   115,   116,   116,   117,   117,   118,   118,
     118,   118,   118,   119,   120,   120,   120,   120,   120,   120,
     121,   122,   123,   124,   125,   126,   126,   126,   126,   126,
     126,   126,   127,   128,   129,   129,   130,   130,   130,   130,
     130,   130,   130,   130,   130,   130,   130,   130,   130,   130,
     130,   130,   130,   130,   130,   130,   130,   130,   130,   130,
     130,   130,   130,   130,   130,   130,   130,   130,   130,   130,
     130,   130,   130,   130,   131,   132,   133,   134,   135,   135,
     136,   136,   137,   138,   138,   139,   139,   140,   140,   140,
     141,   141,   141,   141,   142,   142,   142,   142,   142,   142,
     142,   142,   142,   142,   142,   142,   142,   142,   142,   142,
     142,   142,   142,   142,   142,   142,   142,   142,   142,   142,
     142,   142,   142,   142,   142,   142,   142,   142,   142,   142,
     143,   144,   145,   146,   146,   147,   147,   147,   147,   147,
     148,   149,   150,   151,   151,   152,   153,   153,   153,   153,
     153,   153,   153,   153,   153,   153,   153,   153,   153,   153,
     154,   154,   155,   155,   156,   156,   157,   157,   157,   158,
     158,   158,   159,   160,   160,   161,   161,   161,   161,   161,
     162,   162,   163,   163,   164,   164,   165,   165,   166,   167,
     167,   168,   169,   169,   170,   171,   171,   171,   171,   171,
     171,   171,   171,   171,   171,   171,   171,   171,   171,   171,
     171,   171,   172,   172,   172,   172,   172,   172,   172,   172,
     173,   173,   173,   173,   173,   173,   173,   173,   173
};

/* YYR2[YYN] -- Number of symbols composing right hand side of rule YYN.  */
static const unsigned char yyr2[] =
{
       0,     2,     2,     0,     5,     1,     0,     2,     2,     1,
       2,     1,     1,     1,     0,     3,     4,     4,     4,     4,
       1,     1,     0,     0,     0,     1,     2,     3,     2,     2,
       3,     4,     5,     2,     1,     1,     1,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     3,     3,     2,     1,     3,     7,     0,     0,     1,
       1,     3,     6,     0,     1,     3,     4,     0,     2,     2,
       1,     1,     2,     2,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     7,
       5,     5,     7,     1,     3,     5,     5,     2,     2,     5,
       6,     2,     2,     6,     6,     8,     2,     2,     1,     4,
       2,     2,     2,     1,     1,     2,     2,     2,     1,     2,
       0,     0,     0,     0,     1,     0,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       0,     1,     0,     2,     0,     1,     0,     2,     2,     0,
       4,     3,     1,     0,     2,     1,     2,     3,     4,     3,
       0,     1,     1,     2,     3,     1,     5,     1,     1,     3,
       1,     1,     3,     1,     1,     3,     3,     3,     3,     3,
       3,     3,     3,     3,     3,     3,     3,     3,     3,     3,
       3,     1,     2,     2,     2,     2,     2,     2,     4,     1,
       3,     1,     1,     1,     1,     1,     1,     1,     1
};

/* YYDEFACT[STATE-NAME] -- Default rule to reduce with in state
   STATE-NUM when YYTABLE doesn't specify something else to do.  Zero
   means the default is an error.  */
static const unsigned char yydefact[] =
{
       3,     0,     6,     1,     0,     2,     0,     0,     0,    13,
     146,   105,    99,     0,   100,     0,   147,   151,    96,   150,
     157,     0,   153,   104,    98,   107,   106,    94,   159,   148,
       0,   152,   155,   101,    95,    97,   108,   102,   156,   103,
     154,     0,   149,   128,    20,    21,   158,     7,     9,    11,
      12,     0,    77,    77,   166,    87,     0,   113,     0,    87,
       0,     6,    10,   117,   118,   140,   130,   126,   127,     0,
     140,   132,   121,   122,   141,     0,    34,    35,   140,   131,
       0,     0,   166,     8,    28,     0,    29,    26,    77,     0,
     152,   128,    89,   140,     0,   140,   140,    88,     0,     0,
      87,     0,    87,    87,   141,    87,   133,   134,     0,     0,
     166,    91,    90,    30,    77,   145,    27,     0,   169,   168,
     167,    33,    87,     0,     0,     0,     0,   138,   114,    87,
      87,    75,     4,     0,     0,     0,     0,    87,     0,   142,
     143,     0,    87,     0,     0,   129,   169,    93,    92,    31,
      78,    39,    49,    48,    47,    46,    45,    44,    43,    42,
      41,    40,    50,    51,    52,    53,    54,    56,    55,    57,
      59,    60,    58,    62,    61,    63,    64,    65,    66,    67,
      70,    68,    69,     0,    37,    38,     0,    73,     0,   172,
     180,     0,   139,   136,   135,   137,   140,     0,   143,    22,
      23,    24,   115,     0,    87,    87,   166,     0,   119,     0,
     110,     0,   169,    85,     0,    79,    80,   166,    71,    72,
       0,   227,   228,   223,   221,   222,   224,   225,   226,     0,
       0,     0,     0,   169,     0,     0,     0,     0,   187,     0,
     190,     0,   193,   211,   219,   181,   173,   175,   116,    87,
     111,     0,    87,    87,    87,    87,    15,   143,   123,   120,
       0,    86,   160,     0,    83,     0,   217,   215,   214,   216,
     171,   212,   213,     0,   185,   169,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,    32,   182,     0,     0,
     176,     0,   124,    16,    17,    18,    19,     0,   109,   161,
     162,    81,    84,   169,     0,     0,   220,   170,   188,     0,
     189,   192,   195,   196,   197,   199,   198,   200,   202,   203,
     201,   205,   204,   206,   207,   208,   209,   210,   174,   179,
     183,   177,   112,   125,     0,    76,   180,   218,   184,     0,
     178,   163,   173,   186,    82
};

/* YYDEFGOTO[NTERM-NUM]. */
static const short int yydefgoto[] =
{
      -1,     1,     2,     5,     6,    47,    48,   136,   137,    50,
     252,   253,   254,   138,   113,   114,    66,   121,    52,    53,
      84,    85,   214,   215,   216,   313,   109,    54,   110,    55,
      56,    57,    58,   108,   128,   100,   103,   205,   207,   150,
      59,   310,   345,    60,    89,   189,   190,   296,   246,   247,
     300,   273,   274,   319,   238,   239,   240,   241,   242,   243,
     244
};

/* YYPACT[STATE-NUM] -- Index in YYTABLE of the portion describing
   STATE-NUM.  */
#define YYPACT_NINF -280
static const short int yypact[] =
{
    -280,    20,   -29,  -280,   -35,  -280,   206,   -57,   -21,  -280,
    -280,  -280,  -280,    -3,  -280,     2,  -280,  -280,  -280,  -280,
    -280,     9,  -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,
      29,   -60,  -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,
    -280,     6,  -280,    14,  -280,  -280,  -280,  -280,  -280,  -280,
    -280,    -7,  -280,  -280,    15,   889,    19,   -58,    24,   889,
      47,  -280,  -280,    30,    31,  -280,  -280,    30,    31,   -32,
    -280,  -280,    30,    31,  -280,    58,  -280,  -280,  -280,  -280,
      79,   889,  -280,  -280,  -280,     1,  -280,  -280,  -280,   -18,
    -280,    76,  -280,  -280,    18,  -280,  -280,  -280,    99,   316,
     643,    82,   826,   643,  -280,   643,  -280,  -280,   -32,    93,
    -280,   889,   889,  -280,  -280,  -280,  -280,   425,   -62,  -280,
    -280,  -280,   643,   127,    18,    18,    18,  -280,   105,   643,
     826,  -280,  -280,   133,   134,   136,   114,   752,     8,  -280,
     162,   135,   643,   137,   141,  -280,   -33,  -280,  -280,  -280,
     889,  -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,
    -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,
    -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,
    -280,  -280,  -280,   131,  -280,  -280,   132,  -280,    21,  -280,
     177,   147,  -280,  -280,  -280,  -280,  -280,   148,   162,  -280,
    -280,  -280,  -280,   149,   534,   826,   151,   159,  -280,   160,
    -280,   229,   164,  -280,   155,  -280,   200,  -280,  -280,  -280,
     161,  -280,  -280,  -280,  -280,  -280,  -280,  -280,  -280,   901,
     901,   901,   901,   164,   901,   901,   901,   165,    22,    73,
     190,   208,   191,  -280,  -280,  -280,   228,    27,  -280,   643,
    -280,   196,   534,   534,   534,   534,  -280,   162,  -280,  -280,
     -32,  -280,   281,   889,    40,   889,  -280,  -280,  -280,  -280,
    -280,  -280,  -280,   -52,  -280,   164,   901,   901,   901,   901,
     901,   901,   901,   901,   901,   901,   901,   901,   901,   901,
     901,   901,   901,   901,   901,   901,  -280,  -280,    25,   238,
     223,   201,  -280,  -280,  -280,  -280,  -280,   202,  -280,  -280,
     239,  -280,  -280,   164,   198,   901,  -280,  -280,   242,   226,
     190,   191,   300,   512,   525,    52,    52,    97,    97,    97,
      97,   106,   106,   100,   100,  -280,  -280,  -280,  -280,  -280,
    -280,    25,  -280,  -280,   250,  -280,   177,  -280,  -280,   901,
    -280,  -280,   228,  -280,  -280
};

/* YYPGOTO[NTERM-NUM].  */
static const short int yypgoto[] =
{
    -280,  -280,  -280,  -280,   255,  -280,  -280,   -95,     7,   121,
    -280,  -280,  -280,    -5,   271,   273,   -15,  -280,  -280,  -280,
     -36,  -280,  -280,    65,  -280,  -280,  -113,    80,  -100,   -79,
    -280,  -280,  -280,  -280,   110,   -56,   236,  -280,  -180,  -280,
     -76,  -280,  -280,  -280,  -103,  -137,    28,    -8,    -1,  -280,
    -279,  -246,  -185,  -280,  -280,  -280,    69,  -280,  -112,    64,
    -280
};

/* YYTABLE[YYPACT[STATE-NUM]].  What to do in state STATE-NUM.  If
   positive, shift that token.  If negative, reduce the rule which
   number is the opposite.  If zero, do what YYDEFACT says.
   If YYTABLE_NINF, syntax error.  */
#define YYTABLE_NINF -195
static const short int yytable[] =
{
      71,    51,   111,   237,   187,   112,    75,   146,   141,   213,
     143,   147,   148,    49,   102,    79,   315,    86,   251,   339,
       3,     4,   105,    94,   117,   123,     7,   191,   212,    76,
     318,    77,   111,   111,   197,   112,   112,   122,   111,   129,
     130,   112,   203,   118,   188,    95,    61,   209,   -36,  -165,
     217,   220,   116,   119,   101,   124,   125,   126,    63,   316,
      64,    82,   350,    67,   120,    68,   221,   222,   119,   348,
      72,   111,    73,   188,   112,   261,    82,   307,   149,   120,
     223,   127,   224,   -25,    62,   225,   226,   227,   228,   297,
      76,   297,    77,   144,    51,    80,   270,   140,    83,    69,
      65,   312,  -191,    81,  -191,    70,    49,   229,   298,   256,
     338,   115,    74,   204,   264,   106,   230,   231,   232,   -25,
     -25,   299,    93,   299,   -74,   198,   119,    96,   233,   234,
     235,   236,    78,   -34,   -35,    92,   107,   120,   317,    97,
     249,   284,   285,   286,   287,   288,   289,   290,   291,   292,
     293,   294,   314,   276,   301,   277,    98,   303,   304,   305,
     306,   104,   131,   217,   353,    81,   321,   322,   323,   324,
     325,   326,   327,   328,   329,   330,   331,   332,   333,   334,
     335,   336,   337,   145,   111,   139,   111,   112,   192,   112,
     288,   289,   290,   291,   292,   293,   294,   292,   293,   294,
     257,   290,   291,   292,   293,   294,    -5,     8,   196,     9,
      10,    11,    12,    13,   199,   200,    14,   201,   202,    15,
      16,    17,    18,    19,    20,    21,    22,    23,    24,    25,
     206,   211,    26,    27,   193,   194,   195,    28,   218,   208,
      29,   210,    30,   219,   245,   308,    31,    32,   -87,    33,
      34,   248,   250,    35,    36,  -144,    45,    37,    38,    39,
      40,    41,    42,   258,   259,   260,   262,   -87,   263,    43,
     188,   265,   275,  -194,   -87,   279,   280,   281,   282,   283,
     284,   285,   286,   287,   288,   289,   290,   291,   292,   293,
     294,   278,   -87,   266,   267,   268,   269,   295,   271,   272,
     302,   309,   340,   -87,   341,   342,   343,   349,   344,   347,
     315,   -87,    44,    45,   351,  -164,    99,     8,    46,     9,
      10,    11,    12,    13,   255,    87,    14,    88,   311,    15,
      16,    17,    18,    19,    20,    21,    22,    23,    24,    25,
     142,   346,    26,    27,   354,   352,   320,    28,     0,     0,
      29,     0,    30,     0,     0,     0,    31,    32,   -87,    33,
      34,     0,     0,    35,    36,     0,     0,    37,    38,    39,
      40,    41,    42,     0,     0,     0,     0,   -87,     0,    43,
       0,     0,     0,     0,   -87,   280,   281,   282,   283,   284,
     285,   286,   287,   288,   289,   290,   291,   292,   293,   294,
       0,     0,   -87,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,   -87,     0,     0,     0,     0,     0,     0,
     132,   -87,    44,    45,     0,  -164,     0,     0,    46,    10,
      11,    12,    13,     0,     0,    14,     0,     0,    15,    16,
      17,    18,    19,    20,    21,    22,    23,    24,    25,     0,
       0,    26,    27,     0,     0,     0,    28,     0,     0,    29,
       0,    30,     0,     0,     0,    90,    32,     0,    33,    34,
       0,     0,    35,    36,     0,     0,    37,    38,    39,    40,
      41,    42,     0,     0,     0,     0,     0,     0,    91,     0,
       0,     0,     0,     0,   151,   152,   153,   154,   155,   156,
     157,   158,   159,   160,   161,     0,     0,   162,   163,   164,
     165,   166,   167,   168,   169,   170,   171,   172,   173,   174,
     175,   176,   177,   178,   179,   180,   181,   182,     0,     0,
       0,   183,     0,   184,   185,   186,     0,    46,    10,    11,
      12,    13,     0,     0,    14,     0,     0,    15,    16,    17,
      18,    19,    20,    21,    22,    23,    24,    25,     0,     0,
      26,    27,     0,     0,     0,    28,     0,     0,    29,     0,
      30,   133,   134,   135,    31,    32,     0,    33,    34,     0,
       0,    35,    36,     0,     0,    37,    38,    39,    40,    41,
      42,     0,     0,     0,     0,     0,     0,    43,   281,   282,
     283,   284,   285,   286,   287,   288,   289,   290,   291,   292,
     293,   294,   282,   283,   284,   285,   286,   287,   288,   289,
     290,   291,   292,   293,   294,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,   -14,     0,
      44,   -14,     0,  -164,     0,     0,    46,    10,    11,    12,
      13,     0,     0,    14,     0,     0,    15,    16,    17,    18,
      19,    20,    21,    22,    23,    24,    25,     0,     0,    26,
      27,     0,     0,     0,    28,     0,     0,    29,     0,    30,
     133,   134,   135,    31,    32,     0,    33,    34,     0,     0,
      35,    36,     0,     0,    37,    38,    39,    40,    41,    42,
       0,     0,     0,     0,     0,     0,    43,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,   -14,     0,    44,
       0,     0,  -164,     0,     0,    46,    10,    11,    12,    13,
       0,     0,    14,     0,     0,    15,    16,    17,    18,    19,
      20,    21,    22,    23,    24,    25,     0,     0,    26,    27,
       0,     0,     0,    28,     0,     0,    29,     0,    30,   133,
     134,   135,    31,    32,     0,    33,    34,     0,     0,    35,
      36,     0,     0,    37,    38,    39,    40,    41,    42,     0,
       0,     0,     0,     0,     0,    43,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
      10,    11,    12,    13,     0,     0,    14,     0,     0,    15,
      16,    17,    18,    19,    20,    21,    22,    23,    24,    25,
       0,     0,    26,    27,     0,     0,     0,    28,    44,   -14,
      29,  -164,    30,     0,    46,     0,    31,    32,     0,    33,
      34,     0,     0,    35,    36,     0,     0,    37,    38,    39,
      40,    41,    42,     0,     0,     0,     0,     0,     0,    43,
       0,     0,     0,    10,    11,    12,    13,     0,     0,    14,
       0,     0,    15,    16,    17,    18,    19,    20,    21,    22,
      23,    24,    25,     0,     0,    26,    27,     0,     0,     0,
      28,     0,     0,    29,     0,    30,     0,     0,     0,    90,
      32,   220,    33,    34,     0,  -164,    35,    36,    46,     0,
      37,    38,    39,    40,    41,    42,   221,   222,     0,     0,
       0,     0,    91,     0,     0,     0,     0,     0,     0,     0,
     223,     0,   224,     0,     0,   225,   226,   227,   228,     0,
       0,     0,     0,     0,     0,     0,     0,     0,     0,     0,
       0,     0,     0,     0,     0,     0,     0,   229,     0,     0,
       0,     0,     0,     0,     0,     0,   230,   231,   232,     0,
       0,    46,     0,     0,     0,     0,     0,     0,     0,   234,
     235,   236
};

static const short int yycheck[] =
{
      15,     6,    81,   188,   117,    81,    21,   110,   103,   146,
     105,   111,   112,     6,    70,    30,    68,    53,   198,   298,
       0,    50,    78,    81,    42,     7,    61,   122,    61,    61,
     276,    63,   111,   112,   129,   111,   112,    93,   117,    95,
      96,   117,   137,    61,   106,   103,   103,   142,   110,   109,
     150,    30,    88,    86,    69,    37,    38,    39,    61,   111,
      63,    68,   341,    61,    97,    63,    45,    46,    86,   315,
      61,   150,    63,   106,   150,   212,    68,   257,   114,    97,
      59,    63,    61,    68,   105,    64,    65,    66,    67,    64,
      61,    64,    63,   108,    99,    89,   233,   102,   105,    97,
     103,    61,    80,    89,    82,   103,    99,    86,    81,   204,
     295,   110,   103,   105,   217,    36,    95,    96,    97,   104,
     105,    96,   103,    96,   110,   130,    86,   103,   107,   108,
     109,   110,   103,   103,   103,    55,    57,    97,   275,    59,
     196,    89,    90,    91,    92,    93,    94,    95,    96,    97,
      98,    99,   265,    80,   249,    82,   109,   252,   253,   254,
     255,   103,    63,   263,   349,    89,   278,   279,   280,   281,
     282,   283,   284,   285,   286,   287,   288,   289,   290,   291,
     292,   293,   294,    90,   263,   103,   265,   263,    61,   265,
      93,    94,    95,    96,    97,    98,    99,    97,    98,    99,
     205,    95,    96,    97,    98,    99,     0,     1,   103,     3,
       4,     5,     6,     7,    81,    81,    10,    81,   104,    13,
      14,    15,    16,    17,    18,    19,    20,    21,    22,    23,
      68,    90,    26,    27,   124,   125,   126,    31,   107,   104,
      34,   104,    36,   111,    67,   260,    40,    41,    42,    43,
      44,   104,   104,    47,    48,   104,   107,    51,    52,    53,
      54,    55,    56,   104,   104,    36,   111,    61,    68,    63,
     106,   110,   107,    83,    68,    84,    85,    86,    87,    88,
      89,    90,    91,    92,    93,    94,    95,    96,    97,    98,
      99,    83,    86,   229,   230,   231,   232,    69,   234,   235,
     104,    20,    64,    97,    81,   104,   104,    81,    69,   111,
      68,   105,   106,   107,    64,   109,    61,     1,   112,     3,
       4,     5,     6,     7,   203,    54,    10,    54,   263,    13,
      14,    15,    16,    17,    18,    19,    20,    21,    22,    23,
     104,   313,    26,    27,   352,   346,   277,    31,    -1,    -1,
      34,    -1,    36,    -1,    -1,    -1,    40,    41,    42,    43,
      44,    -1,    -1,    47,    48,    -1,    -1,    51,    52,    53,
      54,    55,    56,    -1,    -1,    -1,    -1,    61,    -1,    63,
      -1,    -1,    -1,    -1,    68,    85,    86,    87,    88,    89,
      90,    91,    92,    93,    94,    95,    96,    97,    98,    99,
      -1,    -1,    86,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    97,    -1,    -1,    -1,    -1,    -1,    -1,
     104,   105,   106,   107,    -1,   109,    -1,    -1,   112,     4,
       5,     6,     7,    -1,    -1,    10,    -1,    -1,    13,    14,
      15,    16,    17,    18,    19,    20,    21,    22,    23,    -1,
      -1,    26,    27,    -1,    -1,    -1,    31,    -1,    -1,    34,
      -1,    36,    -1,    -1,    -1,    40,    41,    -1,    43,    44,
      -1,    -1,    47,    48,    -1,    -1,    51,    52,    53,    54,
      55,    56,    -1,    -1,    -1,    -1,    -1,    -1,    63,    -1,
      -1,    -1,    -1,    -1,    69,    70,    71,    72,    73,    74,
      75,    76,    77,    78,    79,    -1,    -1,    82,    83,    84,
      85,    86,    87,    88,    89,    90,    91,    92,    93,    94,
      95,    96,    97,    98,    99,   100,   101,   102,    -1,    -1,
      -1,   106,    -1,   108,   109,   110,    -1,   112,     4,     5,
       6,     7,    -1,    -1,    10,    -1,    -1,    13,    14,    15,
      16,    17,    18,    19,    20,    21,    22,    23,    -1,    -1,
      26,    27,    -1,    -1,    -1,    31,    -1,    -1,    34,    -1,
      36,    37,    38,    39,    40,    41,    -1,    43,    44,    -1,
      -1,    47,    48,    -1,    -1,    51,    52,    53,    54,    55,
      56,    -1,    -1,    -1,    -1,    -1,    -1,    63,    86,    87,
      88,    89,    90,    91,    92,    93,    94,    95,    96,    97,
      98,    99,    87,    88,    89,    90,    91,    92,    93,    94,
      95,    96,    97,    98,    99,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,   104,    -1,
     106,   107,    -1,   109,    -1,    -1,   112,     4,     5,     6,
       7,    -1,    -1,    10,    -1,    -1,    13,    14,    15,    16,
      17,    18,    19,    20,    21,    22,    23,    -1,    -1,    26,
      27,    -1,    -1,    -1,    31,    -1,    -1,    34,    -1,    36,
      37,    38,    39,    40,    41,    -1,    43,    44,    -1,    -1,
      47,    48,    -1,    -1,    51,    52,    53,    54,    55,    56,
      -1,    -1,    -1,    -1,    -1,    -1,    63,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,   104,    -1,   106,
      -1,    -1,   109,    -1,    -1,   112,     4,     5,     6,     7,
      -1,    -1,    10,    -1,    -1,    13,    14,    15,    16,    17,
      18,    19,    20,    21,    22,    23,    -1,    -1,    26,    27,
      -1,    -1,    -1,    31,    -1,    -1,    34,    -1,    36,    37,
      38,    39,    40,    41,    -1,    43,    44,    -1,    -1,    47,
      48,    -1,    -1,    51,    52,    53,    54,    55,    56,    -1,
      -1,    -1,    -1,    -1,    -1,    63,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
       4,     5,     6,     7,    -1,    -1,    10,    -1,    -1,    13,
      14,    15,    16,    17,    18,    19,    20,    21,    22,    23,
      -1,    -1,    26,    27,    -1,    -1,    -1,    31,   106,   107,
      34,   109,    36,    -1,   112,    -1,    40,    41,    -1,    43,
      44,    -1,    -1,    47,    48,    -1,    -1,    51,    52,    53,
      54,    55,    56,    -1,    -1,    -1,    -1,    -1,    -1,    63,
      -1,    -1,    -1,     4,     5,     6,     7,    -1,    -1,    10,
      -1,    -1,    13,    14,    15,    16,    17,    18,    19,    20,
      21,    22,    23,    -1,    -1,    26,    27,    -1,    -1,    -1,
      31,    -1,    -1,    34,    -1,    36,    -1,    -1,    -1,    40,
      41,    30,    43,    44,    -1,   109,    47,    48,   112,    -1,
      51,    52,    53,    54,    55,    56,    45,    46,    -1,    -1,
      -1,    -1,    63,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      59,    -1,    61,    -1,    -1,    64,    65,    66,    67,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    -1,    86,    -1,    -1,
      -1,    -1,    -1,    -1,    -1,    -1,    95,    96,    97,    -1,
      -1,   112,    -1,    -1,    -1,    -1,    -1,    -1,    -1,   108,
     109,   110
};

/* YYSTOS[STATE-NUM] -- The (internal number of the) accessing
   symbol of state STATE-NUM.  */
static const unsigned char yystos[] =
{
       0,   114,   115,     0,    50,   116,   117,    61,     1,     3,
       4,     5,     6,     7,    10,    13,    14,    15,    16,    17,
      18,    19,    20,    21,    22,    23,    26,    27,    31,    34,
      36,    40,    41,    43,    44,    47,    48,    51,    52,    53,
      54,    55,    56,    63,   106,   107,   112,   118,   119,   121,
     122,   126,   131,   132,   140,   142,   143,   144,   145,   153,
     156,   103,   105,    61,    63,   103,   129,    61,    63,    97,
     103,   129,    61,    63,   103,   129,    61,    63,   103,   129,
      89,    89,    68,   105,   133,   134,   133,   127,   128,   157,
      40,    63,   140,   103,    81,   103,   103,   140,   109,   117,
     148,   129,   148,   149,   103,   148,    36,    57,   146,   139,
     141,   142,   153,   127,   128,   110,   133,    42,    61,    86,
      97,   130,   148,     7,    37,    38,    39,    63,   147,   148,
     148,    63,   104,    37,    38,    39,   120,   121,   126,   103,
     126,   120,   149,   120,   129,    90,   157,   141,   141,   133,
     152,    69,    70,    71,    72,    73,    74,    75,    76,    77,
      78,    79,    82,    83,    84,    85,    86,    87,    88,    89,
      90,    91,    92,    93,    94,    95,    96,    97,    98,    99,
     100,   101,   102,   106,   108,   109,   110,   139,   106,   158,
     159,   120,    61,   147,   147,   147,   103,   120,   126,    81,
      81,    81,   104,   120,   105,   150,    68,   151,   104,   120,
     104,    90,    61,   158,   135,   136,   137,   141,   107,   111,
      30,    45,    46,    59,    61,    64,    65,    66,    67,    86,
      95,    96,    97,   107,   108,   109,   110,   165,   167,   168,
     169,   170,   171,   172,   173,    67,   161,   162,   104,   148,
     104,   151,   123,   124,   125,   122,   120,   126,   104,   104,
      36,   158,   111,    68,   157,   110,   172,   172,   172,   172,
     158,   172,   172,   164,   165,   107,    80,    82,    83,    84,
      85,    86,    87,    88,    89,    90,    91,    92,    93,    94,
      95,    96,    97,    98,    99,    69,   160,    64,    81,    96,
     163,   120,   104,   120,   120,   120,   120,   151,   129,    20,
     154,   136,    61,   138,   139,    68,   111,   158,   164,   166,
     169,   171,   171,   171,   171,   171,   171,   171,   171,   171,
     171,   171,   171,   171,   171,   171,   171,   171,   165,   163,
      64,    81,   104,   104,    69,   155,   159,   111,   164,    81,
     163,    64,   161,   165,   160
};

#define yyerrok		(yyerrstatus = 0)
#define yyclearin	(yychar = YYEMPTY)
#define YYEMPTY		(-2)
#define YYEOF		0

#define YYACCEPT	goto yyacceptlab
#define YYABORT		goto yyabortlab
#define YYERROR		goto yyerrorlab


/* Like YYERROR except do call yyerror.  This remains here temporarily
   to ease the transition to the new meaning of YYERROR, for GCC.
   Once GCC version 2 has supplanted version 1, this can go.  */

#define YYFAIL		goto yyerrlab

#define YYRECOVERING()  (!!yyerrstatus)

#define YYBACKUP(Token, Value)					\
do								\
  if (yychar == YYEMPTY && yylen == 1)				\
    {								\
      yychar = (Token);						\
      yylval = (Value);						\
      yytoken = YYTRANSLATE (yychar);				\
      YYPOPSTACK;						\
      goto yybackup;						\
    }								\
  else								\
    {								\
      yyerror (YY_("syntax error: cannot back up")); \
      YYERROR;							\
    }								\
while (0)


#define YYTERROR	1
#define YYERRCODE	256


/* YYLLOC_DEFAULT -- Set CURRENT to span from RHS[1] to RHS[N].
   If N is 0, then set CURRENT to the empty location which ends
   the previous symbol: RHS[0] (always defined).  */

#define YYRHSLOC(Rhs, K) ((Rhs)[K])
#ifndef YYLLOC_DEFAULT
# define YYLLOC_DEFAULT(Current, Rhs, N)				\
    do									\
      if (N)								\
	{								\
	  (Current).first_line   = YYRHSLOC (Rhs, 1).first_line;	\
	  (Current).first_column = YYRHSLOC (Rhs, 1).first_column;	\
	  (Current).last_line    = YYRHSLOC (Rhs, N).last_line;		\
	  (Current).last_column  = YYRHSLOC (Rhs, N).last_column;	\
	}								\
      else								\
	{								\
	  (Current).first_line   = (Current).last_line   =		\
	    YYRHSLOC (Rhs, 0).last_line;				\
	  (Current).first_column = (Current).last_column =		\
	    YYRHSLOC (Rhs, 0).last_column;				\
	}								\
    while (0)
#endif


/* YY_LOCATION_PRINT -- Print the location on the stream.
   This macro was not mandated originally: define only if we know
   we won't break user code: when these are the locations we know.  */

#ifndef YY_LOCATION_PRINT
# if YYLTYPE_IS_TRIVIAL
#  define YY_LOCATION_PRINT(File, Loc)			\
     fprintf (File, "%d.%d-%d.%d",			\
              (Loc).first_line, (Loc).first_column,	\
              (Loc).last_line,  (Loc).last_column)
# else
#  define YY_LOCATION_PRINT(File, Loc) ((void) 0)
# endif
#endif


/* YYLEX -- calling `yylex' with the right arguments.  */

#ifdef YYLEX_PARAM
# define YYLEX yylex (YYLEX_PARAM)
#else
# define YYLEX yylex ()
#endif

/* Enable debugging if requested.  */
#if YYDEBUG

# ifndef YYFPRINTF
#  include <stdio.h> /* INFRINGES ON USER NAME SPACE */
#  define YYFPRINTF fprintf
# endif

# define YYDPRINTF(Args)			\
do {						\
  if (yydebug)					\
    YYFPRINTF Args;				\
} while (0)

# define YY_SYMBOL_PRINT(Title, Type, Value, Location)		\
do {								\
  if (yydebug)							\
    {								\
      YYFPRINTF (stderr, "%s ", Title);				\
      yysymprint (stderr,					\
                  Type, Value);	\
      YYFPRINTF (stderr, "\n");					\
    }								\
} while (0)

/*------------------------------------------------------------------.
| yy_stack_print -- Print the state stack from its BOTTOM up to its |
| TOP (included).                                                   |
`------------------------------------------------------------------*/

#if defined (__STDC__) || defined (__cplusplus)
static void
yy_stack_print (short int *bottom, short int *top)
#else
static void
yy_stack_print (bottom, top)
    short int *bottom;
    short int *top;
#endif
{
  YYFPRINTF (stderr, "Stack now");
  for (/* Nothing. */; bottom <= top; ++bottom)
    YYFPRINTF (stderr, " %d", *bottom);
  YYFPRINTF (stderr, "\n");
}

# define YY_STACK_PRINT(Bottom, Top)				\
do {								\
  if (yydebug)							\
    yy_stack_print ((Bottom), (Top));				\
} while (0)


/*------------------------------------------------.
| Report that the YYRULE is going to be reduced.  |
`------------------------------------------------*/

#if defined (__STDC__) || defined (__cplusplus)
static void
yy_reduce_print (int yyrule)
#else
static void
yy_reduce_print (yyrule)
    int yyrule;
#endif
{
  int yyi;
  unsigned long int yylno = yyrline[yyrule];
  YYFPRINTF (stderr, "Reducing stack by rule %d (line %lu), ",
             yyrule - 1, yylno);
  /* Print the symbols being reduced, and their result.  */
  for (yyi = yyprhs[yyrule]; 0 <= yyrhs[yyi]; yyi++)
    YYFPRINTF (stderr, "%s ", yytname[yyrhs[yyi]]);
  YYFPRINTF (stderr, "-> %s\n", yytname[yyr1[yyrule]]);
}

# define YY_REDUCE_PRINT(Rule)		\
do {					\
  if (yydebug)				\
    yy_reduce_print (Rule);		\
} while (0)

/* Nonzero means print parse trace.  It is left uninitialized so that
   multiple parsers can coexist.  */
int yydebug;
#else /* !YYDEBUG */
# define YYDPRINTF(Args)
# define YY_SYMBOL_PRINT(Title, Type, Value, Location)
# define YY_STACK_PRINT(Bottom, Top)
# define YY_REDUCE_PRINT(Rule)
#endif /* !YYDEBUG */


/* YYINITDEPTH -- initial size of the parser's stacks.  */
#ifndef	YYINITDEPTH
# define YYINITDEPTH 200
#endif

/* YYMAXDEPTH -- maximum size the stacks can grow to (effective only
   if the built-in stack extension method is used).

   Do not make this value too large; the results are undefined if
   YYSTACK_ALLOC_MAXIMUM < YYSTACK_BYTES (YYMAXDEPTH)
   evaluated with infinite-precision integer arithmetic.  */

#ifndef YYMAXDEPTH
# define YYMAXDEPTH 10000
#endif



#if YYERROR_VERBOSE

# ifndef yystrlen
#  if defined (__GLIBC__) && defined (_STRING_H)
#   define yystrlen strlen
#  else
/* Return the length of YYSTR.  */
static YYSIZE_T
#   if defined (__STDC__) || defined (__cplusplus)
yystrlen (const char *yystr)
#   else
yystrlen (yystr)
     const char *yystr;
#   endif
{
  const char *yys = yystr;

  while (*yys++ != '\0')
    continue;

  return yys - yystr - 1;
}
#  endif
# endif

# ifndef yystpcpy
#  if defined (__GLIBC__) && defined (_STRING_H) && defined (_GNU_SOURCE)
#   define yystpcpy stpcpy
#  else
/* Copy YYSRC to YYDEST, returning the address of the terminating '\0' in
   YYDEST.  */
static char *
#   if defined (__STDC__) || defined (__cplusplus)
yystpcpy (char *yydest, const char *yysrc)
#   else
yystpcpy (yydest, yysrc)
     char *yydest;
     const char *yysrc;
#   endif
{
  char *yyd = yydest;
  const char *yys = yysrc;

  while ((*yyd++ = *yys++) != '\0')
    continue;

  return yyd - 1;
}
#  endif
# endif

# ifndef yytnamerr
/* Copy to YYRES the contents of YYSTR after stripping away unnecessary
   quotes and backslashes, so that it's suitable for yyerror.  The
   heuristic is that double-quoting is unnecessary unless the string
   contains an apostrophe, a comma, or backslash (other than
   backslash-backslash).  YYSTR is taken from yytname.  If YYRES is
   null, do not copy; instead, return the length of what the result
   would have been.  */
static YYSIZE_T
yytnamerr (char *yyres, const char *yystr)
{
  if (*yystr == '"')
    {
      size_t yyn = 0;
      char const *yyp = yystr;

      for (;;)
	switch (*++yyp)
	  {
	  case '\'':
	  case ',':
	    goto do_not_strip_quotes;

	  case '\\':
	    if (*++yyp != '\\')
	      goto do_not_strip_quotes;
	    /* Fall through.  */
	  default:
	    if (yyres)
	      yyres[yyn] = *yyp;
	    yyn++;
	    break;

	  case '"':
	    if (yyres)
	      yyres[yyn] = '\0';
	    return yyn;
	  }
    do_not_strip_quotes: ;
    }

  if (! yyres)
    return yystrlen (yystr);

  return yystpcpy (yyres, yystr) - yyres;
}
# endif

#endif /* YYERROR_VERBOSE */



#if YYDEBUG
/*--------------------------------.
| Print this symbol on YYOUTPUT.  |
`--------------------------------*/

#if defined (__STDC__) || defined (__cplusplus)
static void
yysymprint (FILE *yyoutput, int yytype, YYSTYPE *yyvaluep)
#else
static void
yysymprint (yyoutput, yytype, yyvaluep)
    FILE *yyoutput;
    int yytype;
    YYSTYPE *yyvaluep;
#endif
{
  /* Pacify ``unused variable'' warnings.  */
  (void) yyvaluep;

  if (yytype < YYNTOKENS)
    YYFPRINTF (yyoutput, "token %s (", yytname[yytype]);
  else
    YYFPRINTF (yyoutput, "nterm %s (", yytname[yytype]);


# ifdef YYPRINT
  if (yytype < YYNTOKENS)
    YYPRINT (yyoutput, yytoknum[yytype], *yyvaluep);
# endif
  switch (yytype)
    {
      default:
        break;
    }
  YYFPRINTF (yyoutput, ")");
}

#endif /* ! YYDEBUG */
/*-----------------------------------------------.
| Release the memory associated to this symbol.  |
`-----------------------------------------------*/

#if defined (__STDC__) || defined (__cplusplus)
static void
yydestruct (const char *yymsg, int yytype, YYSTYPE *yyvaluep)
#else
static void
yydestruct (yymsg, yytype, yyvaluep)
    const char *yymsg;
    int yytype;
    YYSTYPE *yyvaluep;
#endif
{
  /* Pacify ``unused variable'' warnings.  */
  (void) yyvaluep;

  if (!yymsg)
    yymsg = "Deleting";
  YY_SYMBOL_PRINT (yymsg, yytype, yyvaluep, yylocationp);

  switch (yytype)
    {

      default:
        break;
    }
}


/* Prevent warnings from -Wmissing-prototypes.  */

#ifdef YYPARSE_PARAM
# if defined (__STDC__) || defined (__cplusplus)
int yyparse (void *YYPARSE_PARAM);
# else
int yyparse ();
# endif
#else /* ! YYPARSE_PARAM */
#if defined (__STDC__) || defined (__cplusplus)
int yyparse (void);
#else
int yyparse ();
#endif
#endif /* ! YYPARSE_PARAM */



/* The look-ahead symbol.  */
int yychar;

/* The semantic value of the look-ahead symbol.  */
YYSTYPE yylval;

/* Number of syntax errors so far.  */
int yynerrs;



/*----------.
| yyparse.  |
`----------*/

#ifdef YYPARSE_PARAM
# if defined (__STDC__) || defined (__cplusplus)
int yyparse (void *YYPARSE_PARAM)
# else
int yyparse (YYPARSE_PARAM)
  void *YYPARSE_PARAM;
# endif
#else /* ! YYPARSE_PARAM */
#if defined (__STDC__) || defined (__cplusplus)
int
yyparse (void)
#else
int
yyparse ()
    ;
#endif
#endif
{
  
  int yystate;
  int yyn;
  int yyresult;
  /* Number of tokens to shift before error messages enabled.  */
  int yyerrstatus;
  /* Look-ahead token as an internal (translated) token number.  */
  int yytoken = 0;

  /* Three stacks and their tools:
     `yyss': related to states,
     `yyvs': related to semantic values,
     `yyls': related to locations.

     Refer to the stacks thru separate pointers, to allow yyoverflow
     to reallocate them elsewhere.  */

  /* The state stack.  */
  short int yyssa[YYINITDEPTH];
  short int *yyss = yyssa;
  short int *yyssp;

  /* The semantic value stack.  */
  YYSTYPE yyvsa[YYINITDEPTH];
  YYSTYPE *yyvs = yyvsa;
  YYSTYPE *yyvsp;



#define YYPOPSTACK   (yyvsp--, yyssp--)

  YYSIZE_T yystacksize = YYINITDEPTH;

  /* The variables used to return semantic value and location from the
     action routines.  */
  YYSTYPE yyval;


  /* When reducing, the number of symbols on the RHS of the reduced
     rule.  */
  int yylen;

  YYDPRINTF ((stderr, "Starting parse\n"));

  yystate = 0;
  yyerrstatus = 0;
  yynerrs = 0;
  yychar = YYEMPTY;		/* Cause a token to be read.  */

  /* Initialize stack pointers.
     Waste one element of value and location stack
     so that they stay on the same level as the state stack.
     The wasted elements are never initialized.  */

  yyssp = yyss;
  yyvsp = yyvs;

  goto yysetstate;

/*------------------------------------------------------------.
| yynewstate -- Push a new state, which is found in yystate.  |
`------------------------------------------------------------*/
 yynewstate:
  /* In all cases, when you get here, the value and location stacks
     have just been pushed. so pushing a state here evens the stacks.
     */
  yyssp++;

 yysetstate:
  *yyssp = yystate;

  if (yyss + yystacksize - 1 <= yyssp)
    {
      /* Get the current used size of the three stacks, in elements.  */
      YYSIZE_T yysize = yyssp - yyss + 1;

#ifdef yyoverflow
      {
	/* Give user a chance to reallocate the stack. Use copies of
	   these so that the &'s don't force the real ones into
	   memory.  */
	YYSTYPE *yyvs1 = yyvs;
	short int *yyss1 = yyss;


	/* Each stack pointer address is followed by the size of the
	   data in use in that stack, in bytes.  This used to be a
	   conditional around just the two extra args, but that might
	   be undefined if yyoverflow is a macro.  */
	yyoverflow (YY_("memory exhausted"),
		    &yyss1, yysize * sizeof (*yyssp),
		    &yyvs1, yysize * sizeof (*yyvsp),

		    &yystacksize);

	yyss = yyss1;
	yyvs = yyvs1;
      }
#else /* no yyoverflow */
# ifndef YYSTACK_RELOCATE
      goto yyexhaustedlab;
# else
      /* Extend the stack our own way.  */
      if (YYMAXDEPTH <= yystacksize)
	goto yyexhaustedlab;
      yystacksize *= 2;
      if (YYMAXDEPTH < yystacksize)
	yystacksize = YYMAXDEPTH;

      {
	short int *yyss1 = yyss;
	union yyalloc *yyptr =
	  (union yyalloc *) YYSTACK_ALLOC (YYSTACK_BYTES (yystacksize));
	if (! yyptr)
	  goto yyexhaustedlab;
	YYSTACK_RELOCATE (yyss);
	YYSTACK_RELOCATE (yyvs);

#  undef YYSTACK_RELOCATE
	if (yyss1 != yyssa)
	  YYSTACK_FREE (yyss1);
      }
# endif
#endif /* no yyoverflow */

      yyssp = yyss + yysize - 1;
      yyvsp = yyvs + yysize - 1;


      YYDPRINTF ((stderr, "Stack size increased to %lu\n",
		  (unsigned long int) yystacksize));

      if (yyss + yystacksize - 1 <= yyssp)
	YYABORT;
    }

  YYDPRINTF ((stderr, "Entering state %d\n", yystate));

  goto yybackup;

/*-----------.
| yybackup.  |
`-----------*/
yybackup:

/* Do appropriate processing given the current state.  */
/* Read a look-ahead token if we need one and don't already have one.  */
/* yyresume: */

  /* First try to decide what to do without reference to look-ahead token.  */

  yyn = yypact[yystate];
  if (yyn == YYPACT_NINF)
    goto yydefault;

  /* Not known => get a look-ahead token if don't already have one.  */

  /* YYCHAR is either YYEMPTY or YYEOF or a valid look-ahead symbol.  */
  if (yychar == YYEMPTY)
    {
      YYDPRINTF ((stderr, "Reading a token: "));
      yychar = YYLEX;
    }

  if (yychar <= YYEOF)
    {
      yychar = yytoken = YYEOF;
      YYDPRINTF ((stderr, "Now at end of input.\n"));
    }
  else
    {
      yytoken = YYTRANSLATE (yychar);
      YY_SYMBOL_PRINT ("Next token is", yytoken, &yylval, &yylloc);
    }

  /* If the proper action on seeing token YYTOKEN is to reduce or to
     detect an error, take that action.  */
  yyn += yytoken;
  if (yyn < 0 || YYLAST < yyn || yycheck[yyn] != yytoken)
    goto yydefault;
  yyn = yytable[yyn];
  if (yyn <= 0)
    {
      if (yyn == 0 || yyn == YYTABLE_NINF)
	goto yyerrlab;
      yyn = -yyn;
      goto yyreduce;
    }

  if (yyn == YYFINAL)
    YYACCEPT;

  /* Shift the look-ahead token.  */
  YY_SYMBOL_PRINT ("Shifting", yytoken, &yylval, &yylloc);

  /* Discard the token being shifted unless it is eof.  */
  if (yychar != YYEOF)
    yychar = YYEMPTY;

  *++yyvsp = yylval;


  /* Count tokens shifted since error; after three, turn off error
     status.  */
  if (yyerrstatus)
    yyerrstatus--;

  yystate = yyn;
  goto yynewstate;


/*-----------------------------------------------------------.
| yydefault -- do the default action for the current state.  |
`-----------------------------------------------------------*/
yydefault:
  yyn = yydefact[yystate];
  if (yyn == 0)
    goto yyerrlab;
  goto yyreduce;


/*-----------------------------.
| yyreduce -- Do a reduction.  |
`-----------------------------*/
yyreduce:
  /* yyn is the number of a rule to reduce with.  */
  yylen = yyr2[yyn];

  /* If YYLEN is nonzero, implement the default value of the action:
     `$$ = $1'.

     Otherwise, the following line sets YYVAL to garbage.
     This behavior is undocumented and Bison
     users should not rely upon it.  Assigning to YYVAL
     unconditionally makes the parser a bit smaller, and it avoids a
     GCC warning that YYVAL may be used uninitialized.  */
  yyval = yyvsp[1-yylen];


  YY_REDUCE_PRINT (yyn);
  switch (yyn)
    {
        case 2:
#line 205 "soapcpp2_yacc.y"
    { if (lflag)
    			  {	custom_header = 0;
    			  	custom_fault = 0;
			  }
			  else
			  {	add_header(sp->table);
			  	add_fault(sp->table);
			  }
			  compile(sp->table);
			  freetable(classtable);
			  freetable(enumtable);
			  freetable(typetable);
			  freetable(booltable);
			  freetable(templatetable);
			;}
    break;

  case 3:
#line 221 "soapcpp2_yacc.y"
    { classtable = mktable((Table*)0);
			  enumtable = mktable((Table*)0);
			  typetable = mktable((Table*)0);
			  booltable = mktable((Table*)0);
			  templatetable = mktable((Table*)0);
			  p = enter(booltable, lookup("false"));
			  p->info.typ = mkint();
			  p->info.val.i = 0;
			  p = enter(booltable, lookup("true"));
			  p->info.typ = mkint();
			  p->info.val.i = 1;
			  mkscope(mktable(mktable((Table*)0)), 0);
			;}
    break;

  case 4:
#line 236 "soapcpp2_yacc.y"
    { namespaceid = (yyvsp[-3].sym)->name; ;}
    break;

  case 5:
#line 237 "soapcpp2_yacc.y"
    { namespaceid = NULL; ;}
    break;

  case 6:
#line 239 "soapcpp2_yacc.y"
    { add_XML();
			  add_qname();
			;}
    break;

  case 7:
#line 242 "soapcpp2_yacc.y"
    { ;}
    break;

  case 8:
#line 244 "soapcpp2_yacc.y"
    { ;}
    break;

  case 9:
#line 245 "soapcpp2_yacc.y"
    { ;}
    break;

  case 10:
#line 246 "soapcpp2_yacc.y"
    { synerror("input before ; skipped");
			  while (sp > stack)
			  {	freetable(sp->table);
			  	exitscope();
			  }
			  yyerrok;
			;}
    break;

  case 11:
#line 253 "soapcpp2_yacc.y"
    { ;}
    break;

  case 12:
#line 254 "soapcpp2_yacc.y"
    { ;}
    break;

  case 13:
#line 256 "soapcpp2_yacc.y"
    { if ((yyvsp[0].s)[1] >= 'a' && (yyvsp[0].s)[1] <= 'z')
			  {	for (pp = &pragmas; *pp; pp = &(*pp)->next)
			          ;
				*pp = (Pragma*)emalloc(sizeof(Pragma));
				(*pp)->pragma = (char*)emalloc(strlen((yyvsp[0].s))+1);
				strcpy((*pp)->pragma, (yyvsp[0].s));
				(*pp)->next = NULL;
			  }
			  else if ((i = atoi((yyvsp[0].s)+2)) > 0)
				yylineno = i;
			  else
			  {	sprintf(errbuf, "directive '%s' ignored (use #import to import files and/or use option -i)", (yyvsp[0].s));
			  	semwarn(errbuf);
			  }
			;}
    break;

  case 14:
#line 279 "soapcpp2_yacc.y"
    { transient &= ~6;
			  permission = 0;
			;}
    break;

  case 15:
#line 283 "soapcpp2_yacc.y"
    { ;}
    break;

  case 16:
#line 285 "soapcpp2_yacc.y"
    { ;}
    break;

  case 17:
#line 287 "soapcpp2_yacc.y"
    { ;}
    break;

  case 18:
#line 289 "soapcpp2_yacc.y"
    { ;}
    break;

  case 19:
#line 291 "soapcpp2_yacc.y"
    { ;}
    break;

  case 20:
#line 293 "soapcpp2_yacc.y"
    { transient |= 1;
			;}
    break;

  case 21:
#line 296 "soapcpp2_yacc.y"
    { transient &= ~1;
			;}
    break;

  case 22:
#line 299 "soapcpp2_yacc.y"
    { permission = Sprivate;
			;}
    break;

  case 23:
#line 302 "soapcpp2_yacc.y"
    { permission = Sprotected;
			;}
    break;

  case 24:
#line 305 "soapcpp2_yacc.y"
    { permission = 0;
			;}
    break;

  case 25:
#line 308 "soapcpp2_yacc.y"
    { ;}
    break;

  case 26:
#line 309 "soapcpp2_yacc.y"
    { ;}
    break;

  case 27:
#line 311 "soapcpp2_yacc.y"
    { ;}
    break;

  case 28:
#line 312 "soapcpp2_yacc.y"
    { ;}
    break;

  case 29:
#line 313 "soapcpp2_yacc.y"
    { ;}
    break;

  case 30:
#line 314 "soapcpp2_yacc.y"
    { ;}
    break;

  case 31:
#line 316 "soapcpp2_yacc.y"
    { ;}
    break;

  case 32:
#line 319 "soapcpp2_yacc.y"
    { if (((yyvsp[-2].rec).sto & Stypedef) && sp->table->level == GLOBAL)
			  {	if (((yyvsp[-2].rec).typ->type != Tstruct && (yyvsp[-2].rec).typ->type != Tunion && (yyvsp[-2].rec).typ->type != Tenum) || strcmp((yyvsp[-3].sym)->name, (yyvsp[-2].rec).typ->id->name))
				{	p = enter(typetable, (yyvsp[-3].sym));
					p->info.typ = mksymtype((yyvsp[-2].rec).typ, (yyvsp[-3].sym));
			  		if ((yyvsp[-2].rec).sto & Sextern)
						p->info.typ->transient = -1;
					else
						p->info.typ->transient = (yyvsp[-2].rec).typ->transient;
			  		p->info.sto = (yyvsp[-2].rec).sto;
					p->info.typ->pattern = (yyvsp[-1].rec).pattern;
					if ((yyvsp[-1].rec).minOccurs != -1)
					{	p->info.typ->minLength = (yyvsp[-1].rec).minOccurs;
					}
					if ((yyvsp[-1].rec).maxOccurs > 1)
						p->info.typ->maxLength = (yyvsp[-1].rec).maxOccurs;
				}
				(yyvsp[-3].sym)->token = TYPE;
			  }
			  else
			  {	p = enter(sp->table, (yyvsp[-3].sym));
			  	p->info.typ = (yyvsp[-2].rec).typ;
			  	p->info.sto = ((yyvsp[-2].rec).sto | permission);
				if ((yyvsp[0].rec).hasval)
				{	p->info.hasval = True;
					switch ((yyvsp[-2].rec).typ->type)
					{	case Tchar:
						case Tuchar:
						case Tshort:
						case Tushort:
						case Tint:
						case Tuint:
						case Tlong:
						case Tulong:
						case Tllong:
						case Tullong:
						case Tenum:
						case Ttime:
							if ((yyvsp[0].rec).typ->type == Tint || (yyvsp[0].rec).typ->type == Tchar || (yyvsp[0].rec).typ->type == Tenum)
								sp->val = p->info.val.i = (yyvsp[0].rec).val.i;
							else
							{	semerror("type error in initialization constant");
								p->info.hasval = False;
							}
							break;
						case Tfloat:
						case Tdouble:
						case Tldouble:
							if ((yyvsp[0].rec).typ->type == Tfloat || (yyvsp[0].rec).typ->type == Tdouble || (yyvsp[0].rec).typ->type == Tldouble)
								p->info.val.r = (yyvsp[0].rec).val.r;
							else if ((yyvsp[0].rec).typ->type == Tint)
								p->info.val.r = (double)(yyvsp[0].rec).val.i;
							else
							{	semerror("type error in initialization constant");
								p->info.hasval = False;
							}
							break;
						default:
							if ((yyvsp[-2].rec).typ->type == Tpointer
							 && ((Tnode*)(yyvsp[-2].rec).typ->ref)->type == Tchar
							 && (yyvsp[0].rec).typ->type == Tpointer
							 && ((Tnode*)(yyvsp[0].rec).typ->ref)->type == Tchar)
								p->info.val.s = (yyvsp[0].rec).val.s;
							else if ((yyvsp[-2].rec).typ->type == Tpointer
							      && ((Tnode*)(yyvsp[-2].rec).typ->ref)->id == lookup("std::string"))
							      	p->info.val.s = (yyvsp[0].rec).val.s;
							else if ((yyvsp[-2].rec).typ->id == lookup("std::string"))
							      	p->info.val.s = (yyvsp[0].rec).val.s;
							else if ((yyvsp[-2].rec).typ->type == Tpointer
							      && (yyvsp[0].rec).typ->type == Tint
							      && (yyvsp[0].rec).val.i == 0)
								p->info.val.i = 0;
							else
							{	semerror("type error in initialization constant");
								p->info.hasval = False;
							}
							break;
					}
				}
				else
					p->info.val.i = sp->val;
			        if ((yyvsp[-1].rec).minOccurs < 0)
			        {	if (((yyvsp[-2].rec).sto & Sattribute) || (yyvsp[-2].rec).typ->type == Tpointer || (yyvsp[-2].rec).typ->type == Ttemplate || !strncmp((yyvsp[-3].sym)->name, "__size", 6))
			        		p->info.minOccurs = 0;
			        	else
			        		p->info.minOccurs = 1;
				}
				else
					p->info.minOccurs = (yyvsp[-1].rec).minOccurs;
				p->info.maxOccurs = (yyvsp[-1].rec).maxOccurs;
				if (sp->mask)
					sp->val <<= 1;
				else
					sp->val++;
			  	p->info.offset = sp->offset;
				if ((yyvsp[-2].rec).sto & Sextern)
					p->level = GLOBAL;
				else if ((yyvsp[-2].rec).sto & Stypedef)
					;
			  	else if (sp->grow)
					sp->offset += p->info.typ->width;
				else if (p->info.typ->width > sp->offset)
					sp->offset = p->info.typ->width;
			  }
			  sp->entry = p;
			;}
    break;

  case 33:
#line 425 "soapcpp2_yacc.y"
    { if ((yyvsp[-1].rec).sto & Stypedef)
			  {	sprintf(errbuf, "invalid typedef qualifier for '%s'", (yyvsp[0].sym)->name);
				semwarn(errbuf);
			  }
			  p = enter(sp->table, (yyvsp[0].sym));
			  p->info.typ = (yyvsp[-1].rec).typ;
			  p->info.sto = (yyvsp[-1].rec).sto;
			  p->info.hasval = False;
			  p->info.offset = sp->offset;
			  if (sp->grow)
				sp->offset += p->info.typ->width;
			  else if (p->info.typ->width > sp->offset)
				sp->offset = p->info.typ->width;
			  sp->entry = p;
			;}
    break;

  case 34:
#line 441 "soapcpp2_yacc.y"
    { (yyval.sym) = (yyvsp[0].sym); ;}
    break;

  case 35:
#line 442 "soapcpp2_yacc.y"
    { (yyval.sym) = (yyvsp[0].sym); ;}
    break;

  case 36:
#line 444 "soapcpp2_yacc.y"
    { (yyval.sym) = (yyvsp[0].sym); ;}
    break;

  case 37:
#line 445 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator!"); ;}
    break;

  case 38:
#line 446 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator~"); ;}
    break;

  case 39:
#line 447 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator="); ;}
    break;

  case 40:
#line 448 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator+="); ;}
    break;

  case 41:
#line 449 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator-="); ;}
    break;

  case 42:
#line 450 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator*="); ;}
    break;

  case 43:
#line 451 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator/="); ;}
    break;

  case 44:
#line 452 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator%="); ;}
    break;

  case 45:
#line 453 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator&="); ;}
    break;

  case 46:
#line 454 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator^="); ;}
    break;

  case 47:
#line 455 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator|="); ;}
    break;

  case 48:
#line 456 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator<<="); ;}
    break;

  case 49:
#line 457 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator>>="); ;}
    break;

  case 50:
#line 458 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator||"); ;}
    break;

  case 51:
#line 459 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator&&"); ;}
    break;

  case 52:
#line 460 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator|"); ;}
    break;

  case 53:
#line 461 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator^"); ;}
    break;

  case 54:
#line 462 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator&"); ;}
    break;

  case 55:
#line 463 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator=="); ;}
    break;

  case 56:
#line 464 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator!="); ;}
    break;

  case 57:
#line 465 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator<"); ;}
    break;

  case 58:
#line 466 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator<="); ;}
    break;

  case 59:
#line 467 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator>"); ;}
    break;

  case 60:
#line 468 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator>="); ;}
    break;

  case 61:
#line 469 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator<<"); ;}
    break;

  case 62:
#line 470 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator>>"); ;}
    break;

  case 63:
#line 471 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator+"); ;}
    break;

  case 64:
#line 472 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator-"); ;}
    break;

  case 65:
#line 473 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator*"); ;}
    break;

  case 66:
#line 474 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator/"); ;}
    break;

  case 67:
#line 475 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator%"); ;}
    break;

  case 68:
#line 476 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator++"); ;}
    break;

  case 69:
#line 477 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator--"); ;}
    break;

  case 70:
#line 478 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator->"); ;}
    break;

  case 71:
#line 479 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator[]"); ;}
    break;

  case 72:
#line 480 "soapcpp2_yacc.y"
    { (yyval.sym) = lookup("operator()"); ;}
    break;

  case 73:
#line 481 "soapcpp2_yacc.y"
    { s1 = c_storage((yyvsp[0].rec).sto);
			  s2 = c_type((yyvsp[0].rec).typ);
			  s = (char*)emalloc(strlen(s1) + strlen(s2) + 10);
			  strcpy(s, "operator ");
			  strcat(s, s1);
			  strcat(s, s2);
			  (yyval.sym) = lookup(s);
			  if (!(yyval.sym))
				(yyval.sym) = install(s, ID);
			;}
    break;

  case 74:
#line 492 "soapcpp2_yacc.y"
    { if (!(p = entry(classtable, (yyvsp[0].sym))))
			  	semerror("invalid constructor");
			  sp->entry = enter(sp->table, (yyvsp[0].sym));
			  sp->entry->info.typ = mknone();
			  sp->entry->info.sto = Snone;
			  sp->entry->info.offset = sp->offset;
			  sp->node.typ = mkvoid();
			  sp->node.sto = Snone;
			;}
    break;

  case 75:
#line 503 "soapcpp2_yacc.y"
    { if (!(p = entry(classtable, (yyvsp[0].sym))))
			  	semerror("invalid destructor");
			  s = (char*)emalloc(strlen((yyvsp[0].sym)->name) + 2);
			  strcpy(s, "~");
			  strcat(s, (yyvsp[0].sym)->name);
			  sym = lookup(s);
			  if (!sym)
				sym = install(s, ID);
			  sp->entry = enter(sp->table, sym);
			  sp->entry->info.typ = mknone();
			  sp->entry->info.sto = (yyvsp[-2].sto);
			  sp->entry->info.offset = sp->offset;
			  sp->node.typ = mkvoid();
			  sp->node.sto = Snone;
			;}
    break;

  case 76:
#line 520 "soapcpp2_yacc.y"
    { if ((yyvsp[-6].e)->level == GLOBAL)
			  {	if (!((yyvsp[-6].e)->info.sto & Sextern) && sp->entry && sp->entry->info.typ->type == Tpointer && ((Tnode*)sp->entry->info.typ->ref)->type == Tchar)
			  	{	sprintf(errbuf, "last output parameter of remote method function prototype '%s' is a pointer to a char which will only return one byte: use char** instead to return a string", (yyvsp[-6].e)->sym->name);
					semwarn(errbuf);
				}
				if ((yyvsp[-6].e)->info.sto & Sextern)
				 	(yyvsp[-6].e)->info.typ = mkmethod((yyvsp[-6].e)->info.typ, sp->table);
			  	else if (sp->entry && (sp->entry->info.typ->type == Tpointer || sp->entry->info.typ->type == Treference || sp->entry->info.typ->type == Tarray || is_transient(sp->entry->info.typ)))
				{	if ((yyvsp[-6].e)->info.typ->type == Tint)
					{	sp->entry->info.sto = (Storage)((int)sp->entry->info.sto | (int)Sreturn);
						(yyvsp[-6].e)->info.typ = mkfun(sp->entry);
						(yyvsp[-6].e)->info.typ->id = (yyvsp[-6].e)->sym;
						if (!is_transient(sp->entry->info.typ))
						{	if (!is_response(sp->entry->info.typ))
							{	if (!is_XML(sp->entry->info.typ))
									add_response((yyvsp[-6].e), sp->entry);
							}
							else
								add_result(sp->entry->info.typ);
						}
					}
					else
					{	sprintf(errbuf, "return type of remote method function prototype '%s' must be integer", (yyvsp[-6].e)->sym->name);
						semerror(errbuf);
					}
				}
			  	else
			  	{	sprintf(errbuf, "last output parameter of remote method function prototype '%s' is a return parameter and must be a pointer or reference", (yyvsp[-6].e)->sym->name);
					semerror(errbuf);
			  	}
				if (!((yyvsp[-6].e)->info.sto & Sextern))
			  	{	unlinklast(sp->table);
			  		if ((p = entry(classtable, (yyvsp[-6].e)->sym)))
					{	if (p->info.typ->ref)
						{	sprintf(errbuf, "remote method name clash: struct/class '%s' already declared at line %d", (yyvsp[-6].e)->sym->name, p->lineno);
							semerror(errbuf);
						}
						else
						{	p->info.typ->ref = sp->table;
							p->info.typ->width = sp->offset;
						}
					}
			  		else
			  		{	p = enter(classtable, (yyvsp[-6].e)->sym);
						p->info.typ = mkstruct(sp->table, sp->offset);
						p->info.typ->id = (yyvsp[-6].e)->sym;
			  		}
			  	}
			  }
			  else if ((yyvsp[-6].e)->level == INTERNAL)
			  {	(yyvsp[-6].e)->info.typ = mkmethod((yyvsp[-6].e)->info.typ, sp->table);
				(yyvsp[-6].e)->info.sto = (Storage)((int)(yyvsp[-6].e)->info.sto | (int)(yyvsp[-1].sto) | (int)(yyvsp[0].sto));
			  	transient &= ~1;
			  }
			  exitscope();
			;}
    break;

  case 77:
#line 577 "soapcpp2_yacc.y"
    { (yyval.e) = sp->entry; ;}
    break;

  case 78:
#line 579 "soapcpp2_yacc.y"
    { ;}
    break;

  case 79:
#line 580 "soapcpp2_yacc.y"
    { ;}
    break;

  case 80:
#line 582 "soapcpp2_yacc.y"
    { ;}
    break;

  case 81:
#line 583 "soapcpp2_yacc.y"
    { ;}
    break;

  case 82:
#line 586 "soapcpp2_yacc.y"
    { if ((yyvsp[-2].rec).sto & Stypedef)
			  	semwarn("typedef in function argument");
			  p = enter(sp->table, (yyvsp[-3].sym));
			  p->info.typ = (yyvsp[-2].rec).typ;
			  p->info.sto = (yyvsp[-2].rec).sto;
			  if ((yyvsp[-1].rec).minOccurs < 0)
			  {	if (((yyvsp[-2].rec).sto & Sattribute) || (yyvsp[-2].rec).typ->type == Tpointer)
			        	p->info.minOccurs = 0;
			       	else
			        	p->info.minOccurs = 1;
			  }
			  else
				p->info.minOccurs = (yyvsp[-1].rec).minOccurs;
			  p->info.maxOccurs = (yyvsp[-1].rec).maxOccurs;
			  if ((yyvsp[0].rec).hasval)
			  {	p->info.hasval = True;
				switch ((yyvsp[-2].rec).typ->type)
				{	case Tchar:
					case Tuchar:
					case Tshort:
					case Tushort:
					case Tint:
					case Tuint:
					case Tlong:
					case Tulong:
					case Tenum:
					case Ttime:
						if ((yyvsp[0].rec).typ->type == Tint || (yyvsp[0].rec).typ->type == Tchar || (yyvsp[0].rec).typ->type == Tenum)
							sp->val = p->info.val.i = (yyvsp[0].rec).val.i;
						else
						{	semerror("type error in initialization constant");
							p->info.hasval = False;
						}
						break;
					case Tfloat:
					case Tdouble:
					case Tldouble:
						if ((yyvsp[0].rec).typ->type == Tfloat || (yyvsp[0].rec).typ->type == Tdouble || (yyvsp[0].rec).typ->type == Tldouble)
							p->info.val.r = (yyvsp[0].rec).val.r;
						else if ((yyvsp[0].rec).typ->type == Tint)
							p->info.val.r = (double)(yyvsp[0].rec).val.i;
						else
						{	semerror("type error in initialization constant");
							p->info.hasval = False;
						}
						break;
					default:
						if ((yyvsp[-2].rec).typ->type == Tpointer
						 && ((Tnode*)(yyvsp[-2].rec).typ->ref)->type == Tchar
						 && (yyvsp[0].rec).typ->type == Tpointer
						 && ((Tnode*)(yyvsp[0].rec).typ->ref)->type == Tchar)
							p->info.val.s = (yyvsp[0].rec).val.s;
						else if ((yyvsp[-2].rec).typ->type == Tpointer
						      && ((Tnode*)(yyvsp[-2].rec).typ->ref)->id == lookup("std::string"))
						      	p->info.val.s = (yyvsp[0].rec).val.s;
						else if ((yyvsp[-2].rec).typ->id == lookup("std::string"))
						      	p->info.val.s = (yyvsp[0].rec).val.s;
						else if ((yyvsp[-2].rec).typ->type == Tpointer
						      && (yyvsp[0].rec).typ->type == Tint
						      && (yyvsp[0].rec).val.i == 0)
							p->info.val.i = 0;
						else
						{	semerror("type error in initialization constant");
							p->info.hasval = False;
						}
						break;
				}
			  }
			  p->info.offset = sp->offset;
			  if ((yyvsp[-2].rec).sto & Sextern)
				p->level = GLOBAL;
			  else if (sp->grow)
				sp->offset += p->info.typ->width;
			  else if (p->info.typ->width > sp->offset)
				sp->offset = p->info.typ->width;
			  sp->entry = p;
			;}
    break;

  case 83:
#line 664 "soapcpp2_yacc.y"
    { if (sp->table->level != PARAM)
			    (yyval.sym) = gensymidx("param", (int)++sp->val);
			  else if (eflag)
				(yyval.sym) = gensymidx("_param", (int)++sp->val);
			  else
				(yyval.sym) = gensym("_param");
			;}
    break;

  case 84:
#line 671 "soapcpp2_yacc.y"
    { if (vflag != 1 && *(yyvsp[0].sym)->name == '_' && sp->table->level == GLOBAL)
			  { sprintf(errbuf, "SOAP 1.2 does not support anonymous parameters '%s'", (yyvsp[0].sym)->name);
			    semwarn(errbuf);
			  }
			  (yyval.sym) = (yyvsp[0].sym);
			;}
    break;

  case 85:
#line 687 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 86:
#line 689 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 87:
#line 691 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkint();
			  (yyval.rec).sto = Snone;
			  sp->node = (yyval.rec);
			;}
    break;

  case 88:
#line 695 "soapcpp2_yacc.y"
    { (yyval.rec).typ = (yyvsp[0].rec).typ;
			  (yyval.rec).sto = (Storage)((int)(yyvsp[-1].sto) | (int)(yyvsp[0].rec).sto);
			  if (((yyval.rec).sto & Sattribute) && !is_primitive_or_string((yyvsp[0].rec).typ) && !is_stdstr((yyvsp[0].rec).typ) && !is_binary((yyvsp[0].rec).typ) && !is_external((yyvsp[0].rec).typ))
			  {	semwarn("invalid attribute type");
			  	(yyval.rec).sto &= ~Sattribute;
			  }
			  sp->node = (yyval.rec);
			  if ((yyvsp[-1].sto) & Sextern)
				transient = 0;
			;}
    break;

  case 89:
#line 705 "soapcpp2_yacc.y"
    { if ((yyvsp[-1].typ)->type == Tint)
				switch ((yyvsp[0].rec).typ->type)
				{ case Tchar:	(yyval.rec).typ = (yyvsp[0].rec).typ; break;
				  case Tshort:	(yyval.rec).typ = (yyvsp[0].rec).typ; break;
				  case Tint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tlong:	(yyval.rec).typ = (yyvsp[0].rec).typ; break;
				  case Tllong:	(yyval.rec).typ = (yyvsp[0].rec).typ; break;
				  default:	semwarn("illegal use of 'signed'");
						(yyval.rec).typ = (yyvsp[0].rec).typ;
				}
			  else if ((yyvsp[-1].typ)->type == Tuint)
				switch ((yyvsp[0].rec).typ->type)
				{ case Tchar:	(yyval.rec).typ = mkuchar(); break;
				  case Tshort:	(yyval.rec).typ = mkushort(); break;
				  case Tint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tlong:	(yyval.rec).typ = mkulong(); break;
				  case Tllong:	(yyval.rec).typ = mkullong(); break;
				  default:	semwarn("illegal use of 'unsigned'");
						(yyval.rec).typ = (yyvsp[0].rec).typ;
				}
			  else if ((yyvsp[-1].typ)->type == Tlong)
				switch ((yyvsp[0].rec).typ->type)
				{ case Tint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tlong:	(yyval.rec).typ = mkllong(); break;
				  case Tuint:	(yyval.rec).typ = mkulong(); break;
				  case Tulong:	(yyval.rec).typ = mkullong(); break;
				  case Tdouble:	(yyval.rec).typ = mkldouble(); break;
				  default:	semwarn("illegal use of 'long'");
						(yyval.rec).typ = (yyvsp[0].rec).typ;
				}
			  else if ((yyvsp[-1].typ)->type == Tulong)
				switch ((yyvsp[0].rec).typ->type)
				{ case Tint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tlong:	(yyval.rec).typ = mkullong(); break;
				  case Tuint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tulong:	(yyval.rec).typ = mkullong(); break;
				  default:	semwarn("illegal use of 'long'");
						(yyval.rec).typ = (yyvsp[0].rec).typ;
				}
			  else if ((yyvsp[0].rec).typ->type == Tint)
				(yyval.rec).typ = (yyvsp[-1].typ);
			  else
			  	semwarn("invalid type (missing ';'?)");
			  (yyval.rec).sto = (yyvsp[0].rec).sto;
			  sp->node = (yyval.rec);
			;}
    break;

  case 90:
#line 752 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkint();
			  (yyval.rec).sto = (yyvsp[0].sto);
			  sp->node = (yyval.rec);
			  if ((yyvsp[0].sto) & Sextern)
				transient = 0;
			;}
    break;

  case 91:
#line 758 "soapcpp2_yacc.y"
    { (yyval.rec).typ = (yyvsp[0].typ);
			  (yyval.rec).sto = Snone;
			  sp->node = (yyval.rec);
			;}
    break;

  case 92:
#line 762 "soapcpp2_yacc.y"
    { (yyval.rec).typ = (yyvsp[0].rec).typ;
			  (yyval.rec).sto = (Storage)((int)(yyvsp[-1].sto) | (int)(yyvsp[0].rec).sto);
			  if (((yyval.rec).sto & Sattribute) && !is_primitive_or_string((yyvsp[0].rec).typ) && !is_stdstr((yyvsp[0].rec).typ) && !is_binary((yyvsp[0].rec).typ) && !is_external((yyvsp[0].rec).typ))
			  {	semwarn("invalid attribute type");
			  	(yyval.rec).sto &= ~Sattribute;
			  }
			  sp->node = (yyval.rec);
			  if ((yyvsp[-1].sto) & Sextern)
				transient = 0;
			;}
    break;

  case 93:
#line 772 "soapcpp2_yacc.y"
    { if ((yyvsp[-1].typ)->type == Tint)
				switch ((yyvsp[0].rec).typ->type)
				{ case Tchar:	(yyval.rec).typ = (yyvsp[0].rec).typ; break;
				  case Tshort:	(yyval.rec).typ = (yyvsp[0].rec).typ; break;
				  case Tint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tlong:	(yyval.rec).typ = (yyvsp[0].rec).typ; break;
				  case Tllong:	(yyval.rec).typ = (yyvsp[0].rec).typ; break;
				  default:	semwarn("illegal use of 'signed'");
						(yyval.rec).typ = (yyvsp[0].rec).typ;
				}
			  else if ((yyvsp[-1].typ)->type == Tuint)
				switch ((yyvsp[0].rec).typ->type)
				{ case Tchar:	(yyval.rec).typ = mkuchar(); break;
				  case Tshort:	(yyval.rec).typ = mkushort(); break;
				  case Tint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tlong:	(yyval.rec).typ = mkulong(); break;
				  case Tllong:	(yyval.rec).typ = mkullong(); break;
				  default:	semwarn("illegal use of 'unsigned'");
						(yyval.rec).typ = (yyvsp[0].rec).typ;
				}
			  else if ((yyvsp[-1].typ)->type == Tlong)
				switch ((yyvsp[0].rec).typ->type)
				{ case Tint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tlong:	(yyval.rec).typ = mkllong(); break;
				  case Tuint:	(yyval.rec).typ = mkulong(); break;
				  case Tulong:	(yyval.rec).typ = mkullong(); break;
				  case Tdouble:	(yyval.rec).typ = mkldouble(); break;
				  default:	semwarn("illegal use of 'long'");
						(yyval.rec).typ = (yyvsp[0].rec).typ;
				}
			  else if ((yyvsp[-1].typ)->type == Tulong)
				switch ((yyvsp[0].rec).typ->type)
				{ case Tint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tlong:	(yyval.rec).typ = mkullong(); break;
				  case Tuint:	(yyval.rec).typ = (yyvsp[-1].typ); break;
				  case Tulong:	(yyval.rec).typ = mkullong(); break;
				  default:	semwarn("illegal use of 'long'");
						(yyval.rec).typ = (yyvsp[0].rec).typ;
				}
			  else if ((yyvsp[0].rec).typ->type == Tint)
				(yyval.rec).typ = (yyvsp[-1].typ);
			  else
			  	semwarn("invalid type");
			  (yyval.rec).sto = (yyvsp[0].rec).sto;
			  sp->node = (yyval.rec);
			;}
    break;

  case 94:
#line 819 "soapcpp2_yacc.y"
    { (yyval.typ) = mkvoid(); ;}
    break;

  case 95:
#line 820 "soapcpp2_yacc.y"
    { (yyval.typ) = mkbool(); ;}
    break;

  case 96:
#line 821 "soapcpp2_yacc.y"
    { (yyval.typ) = mkchar(); ;}
    break;

  case 97:
#line 822 "soapcpp2_yacc.y"
    { (yyval.typ) = mkwchart(); ;}
    break;

  case 98:
#line 823 "soapcpp2_yacc.y"
    { (yyval.typ) = mkshort(); ;}
    break;

  case 99:
#line 824 "soapcpp2_yacc.y"
    { (yyval.typ) = mkint(); ;}
    break;

  case 100:
#line 825 "soapcpp2_yacc.y"
    { (yyval.typ) = mklong(); ;}
    break;

  case 101:
#line 826 "soapcpp2_yacc.y"
    { (yyval.typ) = mkllong(); ;}
    break;

  case 102:
#line 827 "soapcpp2_yacc.y"
    { (yyval.typ) = mkullong(); ;}
    break;

  case 103:
#line 828 "soapcpp2_yacc.y"
    { (yyval.typ) = mkulong(); ;}
    break;

  case 104:
#line 829 "soapcpp2_yacc.y"
    { (yyval.typ) = mkfloat(); ;}
    break;

  case 105:
#line 830 "soapcpp2_yacc.y"
    { (yyval.typ) = mkdouble(); ;}
    break;

  case 106:
#line 831 "soapcpp2_yacc.y"
    { (yyval.typ) = mkint(); ;}
    break;

  case 107:
#line 832 "soapcpp2_yacc.y"
    { (yyval.typ) = mkuint(); ;}
    break;

  case 108:
#line 833 "soapcpp2_yacc.y"
    { (yyval.typ) = mktimet(); ;}
    break;

  case 109:
#line 835 "soapcpp2_yacc.y"
    { if (!(p = entry(templatetable, (yyvsp[0].sym))))
			  {	p = enter(templatetable, (yyvsp[0].sym));
			  	p->info.typ = mktemplate(NULL, (yyvsp[0].sym));
			  	(yyvsp[0].sym)->token = TYPE;
			  }
			  (yyval.typ) = p->info.typ;
			;}
    break;

  case 110:
#line 843 "soapcpp2_yacc.y"
    { sym = gensym("_Struct");
			  sprintf(errbuf, "anonymous class will be named '%s'", sym->name);
			  semwarn(errbuf);
			  if ((p = entry(classtable, sym)))
			  {	if (p->info.typ->ref || p->info.typ->type != Tclass)
				{	sprintf(errbuf, "class '%s' already declared at line %d", sym->name, p->lineno);
					semerror(errbuf);
				}
			  }
			  else
			  {	p = enter(classtable, sym);
				p->info.typ = mkclass((Table*)0, 0);
			  }
			  sym->token = TYPE;
			  sp->table->sym = sym;
			  p->info.typ->ref = sp->table;
			  p->info.typ->width = sp->offset;
			  p->info.typ->id = sym;
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 111:
#line 865 "soapcpp2_yacc.y"
    { p = reenter(classtable, (yyvsp[-4].e)->sym);
			  sp->table->sym = p->sym;
			  p->info.typ->ref = sp->table;
			  p->info.typ->width = sp->offset;
			  p->info.typ->id = p->sym;
			  if (p->info.typ->base)
			  	sp->table->prev = (Table*)entry(classtable, p->info.typ->base)->info.typ->ref;
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 112:
#line 876 "soapcpp2_yacc.y"
    { p = reenter(classtable, (yyvsp[-6].e)->sym);
			  sp->table->sym = p->sym;
			  if (!(yyvsp[-4].e))
				semerror("invalid base class");
			  else
			  {	sp->table->prev = (Table*)(yyvsp[-4].e)->info.typ->ref;
				if (!sp->table->prev && !(yyvsp[-4].e)->info.typ->transient)
				{	sprintf(errbuf, "class '%s' has incomplete type", (yyvsp[-4].e)->sym->name);
					semerror(errbuf);
				}
			  	p->info.typ->base = (yyvsp[-4].e)->info.typ->id;
			  }
			  p->info.typ->ref = sp->table;
			  p->info.typ->width = sp->offset;
			  p->info.typ->id = p->sym;
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 113:
#line 894 "soapcpp2_yacc.y"
    { (yyvsp[0].e)->info.typ->id = (yyvsp[0].e)->sym;
			  (yyval.typ) = (yyvsp[0].e)->info.typ;
			;}
    break;

  case 114:
#line 898 "soapcpp2_yacc.y"
    { if (!(yyvsp[0].e))
				semerror("invalid base class");
			  else
			  {	if (!(yyvsp[0].e)->info.typ->ref && !(yyvsp[0].e)->info.typ->transient)
				{	sprintf(errbuf, "class '%s' has incomplete type", (yyvsp[0].e)->sym->name);
					semerror(errbuf);
				}
			  	(yyvsp[-2].e)->info.typ->base = (yyvsp[0].e)->info.typ->id;
			  }
			  (yyvsp[-2].e)->info.typ->id = (yyvsp[-2].e)->sym;
			  (yyval.typ) = (yyvsp[-2].e)->info.typ;
			;}
    break;

  case 115:
#line 911 "soapcpp2_yacc.y"
    { sym = gensym("_Struct");
			  sprintf(errbuf, "anonymous struct will be named '%s'", sym->name);
			  semwarn(errbuf);
			  if ((p = entry(classtable, sym)))
			  {	if (p->info.typ->ref || p->info.typ->type != Tstruct)
				{	sprintf(errbuf, "struct '%s' already declared at line %d", sym->name, p->lineno);
					semerror(errbuf);
				}
				else
				{	p->info.typ->ref = sp->table;
					p->info.typ->width = sp->offset;
				}
			  }
			  else
			  {	p = enter(classtable, sym);
				p->info.typ = mkstruct(sp->table, sp->offset);
			  }
			  p->info.typ->id = sym;
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 116:
#line 933 "soapcpp2_yacc.y"
    { if ((p = entry(classtable, (yyvsp[-4].e)->sym)) && p->info.typ->ref)
			  {	sprintf(errbuf, "struct '%s' already declared at line %d", (yyvsp[-4].e)->sym->name, p->lineno);
				semerror(errbuf);
			  }
			  else
			  {	p = reenter(classtable, (yyvsp[-4].e)->sym);
			  	p->info.typ->ref = sp->table;
			  	p->info.typ->width = sp->offset;
			  	p->info.typ->id = p->sym;
			  }
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 117:
#line 946 "soapcpp2_yacc.y"
    { if ((p = entry(classtable, (yyvsp[0].sym))))
			  {	if (p->info.typ->type == Tstruct)
			  		(yyval.typ) = p->info.typ;
			  	else
				{	sprintf(errbuf, "'struct %s' redeclaration (line %d)", (yyvsp[0].sym)->name, p->lineno);
			  		semerror(errbuf);
			  		(yyval.typ) = mkint();
				}
			  }
			  else
			  {	p = enter(classtable, (yyvsp[0].sym));
			  	(yyval.typ) = p->info.typ = mkstruct((Table*)0, 0);
				p->info.typ->id = (yyvsp[0].sym);
			  }
			;}
    break;

  case 118:
#line 961 "soapcpp2_yacc.y"
    { if ((p = entry(classtable, (yyvsp[0].sym))))
			  {	if (p->info.typ->type == Tstruct)
					(yyval.typ) = p->info.typ;
			  	else
				{	sprintf(errbuf, "'struct %s' redeclaration (line %d)", (yyvsp[0].sym)->name, p->lineno);
			  		semerror(errbuf);
			  		(yyval.typ) = mkint();
				}
			  }
			  else
			  {	p = enter(classtable, (yyvsp[0].sym));
			  	(yyval.typ) = p->info.typ = mkstruct((Table*)0, 0);
				p->info.typ->id = (yyvsp[0].sym);
			  }
			;}
    break;

  case 119:
#line 977 "soapcpp2_yacc.y"
    { sym = gensym("_Union");
			  sprintf(errbuf, "anonymous union will be named '%s'", sym->name);
			  semwarn(errbuf);
			  (yyval.typ) = mkunion(sp->table, sp->offset);
			  if ((p = entry(classtable, sym)))
			  {	if ((Table*) p->info.typ->ref)
				{	sprintf(errbuf, "union or struct '%s' already declared at line %d", sym->name, p->lineno);
					semerror(errbuf);
				}
				else
				{	p->info.typ->ref = sp->table;
					p->info.typ->width = sp->offset;
				}
			  }
			  else
			  {	p = enter(classtable, sym);
				p->info.typ = mkunion(sp->table, sp->offset);
			  }
			  p->info.typ->id = sym;
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 120:
#line 1000 "soapcpp2_yacc.y"
    { if ((p = entry(classtable, (yyvsp[-4].sym))))
			  {	if (p->info.typ->ref || p->info.typ->type != Tunion)
			  	{	sprintf(errbuf, "union '%s' already declared at line %d", (yyvsp[-4].sym)->name, p->lineno);
					semerror(errbuf);
				}
				else
				{	p = reenter(classtable, (yyvsp[-4].sym));
					p->info.typ->ref = sp->table;
					p->info.typ->width = sp->offset;
				}
			  }
			  else
			  {	p = enter(classtable, (yyvsp[-4].sym));
				p->info.typ = mkunion(sp->table, sp->offset);
			  }
			  p->info.typ->id = (yyvsp[-4].sym);
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 121:
#line 1019 "soapcpp2_yacc.y"
    { if ((p = entry(classtable, (yyvsp[0].sym))))
			  {	if (p->info.typ->type == Tunion)
					(yyval.typ) = p->info.typ;
			  	else
				{	sprintf(errbuf, "'union %s' redeclaration (line %d)", (yyvsp[0].sym)->name, p->lineno);
			  		semerror(errbuf);
			  		(yyval.typ) = mkint();
				}
			  }
			  else
			  {	p = enter(classtable, (yyvsp[0].sym));
			  	(yyval.typ) = p->info.typ = mkunion((Table*) 0, 0);
				p->info.typ->id = (yyvsp[0].sym);
			  }
			;}
    break;

  case 122:
#line 1034 "soapcpp2_yacc.y"
    { if ((p = entry(classtable, (yyvsp[0].sym))))
			  {	if (p->info.typ->type == Tunion)
					(yyval.typ) = p->info.typ;
			  	else
				{	sprintf(errbuf, "'union %s' redeclaration (line %d)", (yyvsp[0].sym)->name, p->lineno);
			  		semerror(errbuf);
			  		(yyval.typ) = mkint();
				}
			  }
			  else
			  {	p = enter(classtable, (yyvsp[0].sym));
			  	(yyval.typ) = p->info.typ = mkunion((Table*) 0, 0);
				p->info.typ->id = (yyvsp[0].sym);
			  }
			;}
    break;

  case 123:
#line 1050 "soapcpp2_yacc.y"
    { sym = gensym("_Enum");
			  sprintf(errbuf, "anonymous enum will be named '%s'", sym->name);
			  semwarn(errbuf);
			  if ((p = entry(enumtable, sym)))
			  {	if ((Table*) p->info.typ->ref)
				{	sprintf(errbuf, "enum '%s' already declared at line %d", sym->name, p->lineno);
					semerror(errbuf);
				}
				else
				{	p->info.typ->ref = sp->table;
					p->info.typ->width = 4; /* 4 = enum */
				}
			  }
			  else
			  {	p = enter(enumtable, sym);
				p->info.typ = mkenum(sp->table);
			  }
			  p->info.typ->id = sym;
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 124:
#line 1072 "soapcpp2_yacc.y"
    { if ((p = entry(enumtable, (yyvsp[-5].e)->sym)))
			  {	if ((Table*) p->info.typ->ref)
				{	sprintf(errbuf, "enum '%s' already declared at line %d", (yyvsp[-5].e)->sym->name, p->lineno);
					semerror(errbuf);
				}
				else
				{	p->info.typ->ref = sp->table;
					p->info.typ->width = 4; /* 4 = enum */
				}
			  }
			  else
			  {	p = enter(enumtable, (yyvsp[-5].e)->sym);
				p->info.typ = mkenum(sp->table);
			  }
			  p->info.typ->id = (yyvsp[-5].e)->sym;
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 125:
#line 1091 "soapcpp2_yacc.y"
    { if ((p = entry(enumtable, (yyvsp[-5].sym))))
			  {	if (p->info.typ->ref)
				{	sprintf(errbuf, "enum '%s' already declared at line %d", (yyvsp[-5].sym)->name, p->lineno);
					semerror(errbuf);
				}
				else
				{	p->info.typ->ref = sp->table;
					p->info.typ->width = 8; /* 8 = mask */
				}
			  }
			  else
			  {	p = enter(enumtable, (yyvsp[-5].sym));
				p->info.typ = mkmask(sp->table);
			  }
			  p->info.typ->id = (yyvsp[-5].sym);
			  (yyval.typ) = p->info.typ;
			  exitscope();
			;}
    break;

  case 126:
#line 1109 "soapcpp2_yacc.y"
    { if ((p = entry(enumtable, (yyvsp[0].sym))))
			  	(yyval.typ) = p->info.typ;
			  else
			  {	p = enter(enumtable, (yyvsp[0].sym));
			  	(yyval.typ) = p->info.typ = mkenum((Table*)0);
				p->info.typ->id = (yyvsp[0].sym);
			  }
			;}
    break;

  case 127:
#line 1117 "soapcpp2_yacc.y"
    { if ((p = entry(enumtable, (yyvsp[0].sym))))
				(yyval.typ) = p->info.typ;
			  else
			  {	p = enter(enumtable, (yyvsp[0].sym));
			  	(yyval.typ) = p->info.typ = mkenum((Table*)0);
				p->info.typ->id = (yyvsp[0].sym);
			  }
			;}
    break;

  case 128:
#line 1125 "soapcpp2_yacc.y"
    { if ((p = entry(typetable, (yyvsp[0].sym))))
				(yyval.typ) = p->info.typ;
			  else if ((p = entry(classtable, (yyvsp[0].sym))))
			  	(yyval.typ) = p->info.typ;
			  else if ((p = entry(enumtable, (yyvsp[0].sym))))
			  	(yyval.typ) = p->info.typ;
			  else if ((yyvsp[0].sym) == lookup("std::string") || (yyvsp[0].sym) == lookup("std::wstring"))
			  {	p = enter(classtable, (yyvsp[0].sym));
				(yyval.typ) = p->info.typ = mkclass((Table*)0, 0);
			  	p->info.typ->id = (yyvsp[0].sym);
			  	p->info.typ->transient = -2;
			  }
			  else
			  {	sprintf(errbuf, "unknown type '%s'", (yyvsp[0].sym)->name);
				semerror(errbuf);
				(yyval.typ) = mkint();
			  }
			;}
    break;

  case 129:
#line 1144 "soapcpp2_yacc.y"
    { if ((p = entry(templatetable, (yyvsp[-3].sym))))
				(yyval.typ) = mktemplate((yyvsp[-1].rec).typ, (yyvsp[-3].sym));
			  else
			  {	sprintf(errbuf, "invalid template '%s'", (yyvsp[-3].sym)->name);
				semerror(errbuf);
				(yyval.typ) = mkint();
			  }
			;}
    break;

  case 130:
#line 1153 "soapcpp2_yacc.y"
    { if ((p = entry(classtable, (yyvsp[0].sym))))
			  {	if (p->info.typ->ref)
				{	sprintf(errbuf, "struct '%s' already declared at line %d", (yyvsp[0].sym)->name, p->lineno);
					semerror(errbuf);
				}
				else
					p = reenter(classtable, (yyvsp[0].sym));
			  }
			  else
			  {	p = enter(classtable, (yyvsp[0].sym));
				p->info.typ = mkstruct((Table*)0, 0);
			  }
			  (yyval.e) = p;
			;}
    break;

  case 131:
#line 1168 "soapcpp2_yacc.y"
    { if ((p = entry(classtable, (yyvsp[0].sym))))
			  {	if (p->info.typ->ref)
				{	sprintf(errbuf, "class '%s' already declared at line %d", (yyvsp[0].sym)->name, p->lineno);
					semerror(errbuf);
				}
				else
					p = reenter(classtable, (yyvsp[0].sym));
			  }
			  else
			  {	p = enter(classtable, (yyvsp[0].sym));
				p->info.typ = mkclass((Table*)0, 0);
				p->info.typ->id = p->sym;
			  }
			  (yyvsp[0].sym)->token = TYPE;
			  (yyval.e) = p;
			;}
    break;

  case 132:
#line 1185 "soapcpp2_yacc.y"
    { if ((p = entry(enumtable, (yyvsp[0].sym))))
			  {	if (p->info.typ->ref)
				{	sprintf(errbuf, "enum '%s' already declared at line %d", (yyvsp[0].sym)->name, p->lineno);
					semerror(errbuf);
				}
				/*
				else
					p = reenter(classtable, $2);
			  	*/
			  }
			  else
			  {	p = enter(enumtable, (yyvsp[0].sym));
				p->info.typ = mkenum(0);
			  }
			  (yyval.e) = p;
			;}
    break;

  case 133:
#line 1202 "soapcpp2_yacc.y"
    { ;}
    break;

  case 134:
#line 1203 "soapcpp2_yacc.y"
    { ;}
    break;

  case 135:
#line 1205 "soapcpp2_yacc.y"
    { (yyval.e) = (yyvsp[0].e); ;}
    break;

  case 136:
#line 1206 "soapcpp2_yacc.y"
    { (yyval.e) = (yyvsp[0].e); ;}
    break;

  case 137:
#line 1207 "soapcpp2_yacc.y"
    { (yyval.e) = (yyvsp[0].e); ;}
    break;

  case 138:
#line 1208 "soapcpp2_yacc.y"
    { (yyval.e) = entry(classtable, (yyvsp[0].sym));
			  if (!(yyval.e))
			  {	p = entry(typetable, (yyvsp[0].sym));
			  	if (p && (p->info.typ->type == Tclass || p->info.typ->type == Tstruct))
					(yyval.e) = p;
			  }
			;}
    break;

  case 139:
#line 1215 "soapcpp2_yacc.y"
    { (yyval.e) = entry(classtable, (yyvsp[0].sym)); ;}
    break;

  case 140:
#line 1217 "soapcpp2_yacc.y"
    { if (transient == -2)
			  	transient = 0;
			  permission = 0;
			  enterscope(mktable(NULL), 0);
			  sp->entry = NULL;
			;}
    break;

  case 141:
#line 1224 "soapcpp2_yacc.y"
    { if (transient == -2)
			  	transient = 0;
			  permission = 0;
			  enterscope(mktable(NULL), 0);
			  sp->entry = NULL;
			  sp->grow = False;
			;}
    break;

  case 142:
#line 1232 "soapcpp2_yacc.y"
    { enterscope(mktable(NULL), 0);
			  sp->entry = NULL;
			  sp->mask = True;
			  sp->val = 1;
			;}
    break;

  case 143:
#line 1238 "soapcpp2_yacc.y"
    { ;}
    break;

  case 144:
#line 1239 "soapcpp2_yacc.y"
    { ;}
    break;

  case 145:
#line 1241 "soapcpp2_yacc.y"
    { if (sp->table->level == INTERNAL)
			  	transient |= 1;
			  permission = 0;
			  enterscope(mktable(NULL), 0);
			  sp->entry = NULL;
			  sp->table->level = PARAM;
			;}
    break;

  case 146:
#line 1249 "soapcpp2_yacc.y"
    { (yyval.sto) = Sauto; ;}
    break;

  case 147:
#line 1250 "soapcpp2_yacc.y"
    { (yyval.sto) = Sregister; ;}
    break;

  case 148:
#line 1251 "soapcpp2_yacc.y"
    { (yyval.sto) = Sstatic; ;}
    break;

  case 149:
#line 1252 "soapcpp2_yacc.y"
    { (yyval.sto) = Sexplicit; ;}
    break;

  case 150:
#line 1253 "soapcpp2_yacc.y"
    { (yyval.sto) = Sextern; transient = 1; ;}
    break;

  case 151:
#line 1254 "soapcpp2_yacc.y"
    { (yyval.sto) = Stypedef; ;}
    break;

  case 152:
#line 1255 "soapcpp2_yacc.y"
    { (yyval.sto) = Svirtual; ;}
    break;

  case 153:
#line 1256 "soapcpp2_yacc.y"
    { (yyval.sto) = Sconst; ;}
    break;

  case 154:
#line 1257 "soapcpp2_yacc.y"
    { (yyval.sto) = Sfriend; ;}
    break;

  case 155:
#line 1258 "soapcpp2_yacc.y"
    { (yyval.sto) = Sinline; ;}
    break;

  case 156:
#line 1259 "soapcpp2_yacc.y"
    { (yyval.sto) = SmustUnderstand; ;}
    break;

  case 157:
#line 1260 "soapcpp2_yacc.y"
    { (yyval.sto) = Sreturn; ;}
    break;

  case 158:
#line 1261 "soapcpp2_yacc.y"
    { (yyval.sto) = Sattribute;
			  if (eflag)
			   	semwarn("SOAP RPC encoding does not support XML attributes");
			;}
    break;

  case 159:
#line 1265 "soapcpp2_yacc.y"
    { (yyval.sto) = Sextern; transient = -2; ;}
    break;

  case 160:
#line 1267 "soapcpp2_yacc.y"
    { (yyval.sto) = Snone; ;}
    break;

  case 161:
#line 1268 "soapcpp2_yacc.y"
    { (yyval.sto) = Sconstobj; ;}
    break;

  case 162:
#line 1270 "soapcpp2_yacc.y"
    { (yyval.sto) = Snone; ;}
    break;

  case 163:
#line 1271 "soapcpp2_yacc.y"
    { (yyval.sto) = Sabstract; ;}
    break;

  case 164:
#line 1273 "soapcpp2_yacc.y"
    { (yyval.sto) = Snone; ;}
    break;

  case 165:
#line 1274 "soapcpp2_yacc.y"
    { (yyval.sto) = Svirtual; ;}
    break;

  case 166:
#line 1276 "soapcpp2_yacc.y"
    { (yyval.rec) = tmp = sp->node; ;}
    break;

  case 167:
#line 1277 "soapcpp2_yacc.y"
    { /* handle const pointers, such as const char* */
			  if (/*tmp.typ->type == Tchar &&*/ (tmp.sto & Sconst))
			  	tmp.sto = (tmp.sto & ~Sconst) | Sconstptr;
			  tmp.typ = mkpointer(tmp.typ);
			  tmp.typ->transient = transient;
			  (yyval.rec) = tmp;
			;}
    break;

  case 168:
#line 1284 "soapcpp2_yacc.y"
    { tmp.typ = mkreference(tmp.typ);
			  tmp.typ->transient = transient;
			  (yyval.rec) = tmp;
			;}
    break;

  case 169:
#line 1289 "soapcpp2_yacc.y"
    { (yyval.rec) = tmp;	/* tmp is inherited */
			;}
    break;

  case 170:
#line 1292 "soapcpp2_yacc.y"
    { if ((yyvsp[0].rec).typ->type == Tchar)
			  {	sprintf(errbuf, "char["SOAP_LONG_FORMAT"] will be encoded as an array of "SOAP_LONG_FORMAT" bytes: use char* for strings", (yyvsp[-2].rec).val.i, (yyvsp[-2].rec).val.i);
			  	semwarn(errbuf);
			  }
			  if ((yyvsp[-2].rec).hasval && (yyvsp[-2].rec).typ->type == Tint && (yyvsp[-2].rec).val.i > 0 && (yyvsp[0].rec).typ->width > 0)
				(yyval.rec).typ = mkarray((yyvsp[0].rec).typ, (int) (yyvsp[-2].rec).val.i * (yyvsp[0].rec).typ->width);
			  else
			  {	(yyval.rec).typ = mkarray((yyvsp[0].rec).typ, 0);
			  	semerror("undetermined array size");
			  }
			  (yyval.rec).sto = (yyvsp[0].rec).sto;
			;}
    break;

  case 171:
#line 1304 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkpointer((yyvsp[0].rec).typ); /* zero size array = pointer */
			  (yyval.rec).sto = (yyvsp[0].rec).sto;
			;}
    break;

  case 172:
#line 1308 "soapcpp2_yacc.y"
    { if ((yyvsp[0].rec).typ->type == Tstruct || (yyvsp[0].rec).typ->type == Tclass)
				if (!(yyvsp[0].rec).typ->ref && !(yyvsp[0].rec).typ->transient && !((yyvsp[0].rec).sto & Stypedef))
			   	{	sprintf(errbuf, "struct/class '%s' has incomplete type", (yyvsp[0].rec).typ->id->name);
					semerror(errbuf);
				}
			  (yyval.rec) = (yyvsp[0].rec);
			;}
    break;

  case 173:
#line 1316 "soapcpp2_yacc.y"
    { (yyval.rec).hasval = False; ;}
    break;

  case 174:
#line 1317 "soapcpp2_yacc.y"
    { if ((yyvsp[0].rec).hasval)
			  {	(yyval.rec).typ = (yyvsp[0].rec).typ;
				(yyval.rec).hasval = True;
				(yyval.rec).val = (yyvsp[0].rec).val;
			  }
			  else
			  {	(yyval.rec).hasval = False;
				semerror("initialization expression not constant");
			  }
			;}
    break;

  case 175:
#line 1329 "soapcpp2_yacc.y"
    { (yyval.rec).minOccurs = -1;
			  (yyval.rec).maxOccurs = 1;
			  (yyval.rec).pattern = (yyvsp[0].s);
			;}
    break;

  case 176:
#line 1334 "soapcpp2_yacc.y"
    { (yyval.rec).minOccurs = (long)(yyvsp[0].i);
			  (yyval.rec).maxOccurs = 1;
			  (yyval.rec).pattern = (yyvsp[-1].s);
			;}
    break;

  case 177:
#line 1339 "soapcpp2_yacc.y"
    { (yyval.rec).minOccurs = (long)(yyvsp[-1].i);
			  (yyval.rec).maxOccurs = 1;
			  (yyval.rec).pattern = (yyvsp[-2].s);
			;}
    break;

  case 178:
#line 1344 "soapcpp2_yacc.y"
    { (yyval.rec).minOccurs = (long)(yyvsp[-2].i);
			  (yyval.rec).maxOccurs = (long)(yyvsp[0].i);
			  (yyval.rec).pattern = (yyvsp[-3].s);
			;}
    break;

  case 179:
#line 1349 "soapcpp2_yacc.y"
    { (yyval.rec).minOccurs = -1;
			  (yyval.rec).maxOccurs = (long)(yyvsp[0].i);
			  (yyval.rec).pattern = (yyvsp[-2].s);
			;}
    break;

  case 180:
#line 1354 "soapcpp2_yacc.y"
    { (yyval.s) = NULL; ;}
    break;

  case 181:
#line 1355 "soapcpp2_yacc.y"
    { (yyval.s) = (yyvsp[0].s); ;}
    break;

  case 182:
#line 1357 "soapcpp2_yacc.y"
    { (yyval.i) = (yyvsp[0].i); ;}
    break;

  case 183:
#line 1358 "soapcpp2_yacc.y"
    { (yyval.i) = -(yyvsp[0].i); ;}
    break;

  case 184:
#line 1367 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 185:
#line 1368 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 186:
#line 1372 "soapcpp2_yacc.y"
    { (yyval.rec).typ = (yyvsp[-2].rec).typ;
			  (yyval.rec).sto = Snone;
			  (yyval.rec).hasval = False;
			;}
    break;

  case 188:
#line 1379 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 189:
#line 1382 "soapcpp2_yacc.y"
    { (yyval.rec).hasval = False;
			  (yyval.rec).typ = mkint();
			;}
    break;

  case 190:
#line 1385 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 191:
#line 1387 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 192:
#line 1390 "soapcpp2_yacc.y"
    { (yyval.rec).hasval = False;
			  (yyval.rec).typ = mkint();
			;}
    break;

  case 193:
#line 1393 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 194:
#line 1395 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 195:
#line 1398 "soapcpp2_yacc.y"
    { (yyval.rec) = iop("|", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 196:
#line 1399 "soapcpp2_yacc.y"
    { (yyval.rec) = iop("^", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 197:
#line 1400 "soapcpp2_yacc.y"
    { (yyval.rec) = iop("&", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 198:
#line 1401 "soapcpp2_yacc.y"
    { (yyval.rec) = relop("==", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 199:
#line 1402 "soapcpp2_yacc.y"
    { (yyval.rec) = relop("!=", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 200:
#line 1403 "soapcpp2_yacc.y"
    { (yyval.rec) = relop("<", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 201:
#line 1404 "soapcpp2_yacc.y"
    { (yyval.rec) = relop("<=", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 202:
#line 1405 "soapcpp2_yacc.y"
    { (yyval.rec) = relop(">", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 203:
#line 1406 "soapcpp2_yacc.y"
    { (yyval.rec) = relop(">=", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 204:
#line 1407 "soapcpp2_yacc.y"
    { (yyval.rec) = iop("<<", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 205:
#line 1408 "soapcpp2_yacc.y"
    { (yyval.rec) = iop(">>", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 206:
#line 1409 "soapcpp2_yacc.y"
    { (yyval.rec) = op("+", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 207:
#line 1410 "soapcpp2_yacc.y"
    { (yyval.rec) = op("-", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 208:
#line 1411 "soapcpp2_yacc.y"
    { (yyval.rec) = op("*", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 209:
#line 1412 "soapcpp2_yacc.y"
    { (yyval.rec) = op("/", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 210:
#line 1413 "soapcpp2_yacc.y"
    { (yyval.rec) = iop("%", (yyvsp[-2].rec), (yyvsp[0].rec)); ;}
    break;

  case 211:
#line 1414 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 212:
#line 1417 "soapcpp2_yacc.y"
    { if ((yyvsp[0].rec).hasval)
				(yyval.rec).val.i = !(yyvsp[0].rec).val.i;
			  (yyval.rec).typ = (yyvsp[0].rec).typ;
			  (yyval.rec).hasval = (yyvsp[0].rec).hasval;
			;}
    break;

  case 213:
#line 1422 "soapcpp2_yacc.y"
    { if ((yyvsp[0].rec).hasval)
				(yyval.rec).val.i = ~(yyvsp[0].rec).val.i;
			  (yyval.rec).typ = (yyvsp[0].rec).typ;
			  (yyval.rec).hasval = (yyvsp[0].rec).hasval;
			;}
    break;

  case 214:
#line 1427 "soapcpp2_yacc.y"
    { if ((yyvsp[0].rec).hasval) {
				if (integer((yyvsp[0].rec).typ))
					(yyval.rec).val.i = -(yyvsp[0].rec).val.i;
				else if (real((yyvsp[0].rec).typ))
					(yyval.rec).val.r = -(yyvsp[0].rec).val.r;
				else	typerror("string?");
			  }
			  (yyval.rec).typ = (yyvsp[0].rec).typ;
			  (yyval.rec).hasval = (yyvsp[0].rec).hasval;
			;}
    break;

  case 215:
#line 1437 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 216:
#line 1438 "soapcpp2_yacc.y"
    { if ((yyvsp[0].rec).typ->type == Tpointer) {
			  	(yyval.rec).typ = (Tnode*) (yyvsp[0].rec).typ->ref;
			  } else
			  	typerror("dereference of non-pointer type");
			  (yyval.rec).sto = Snone;
			  (yyval.rec).hasval = False;
			;}
    break;

  case 217:
#line 1445 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkpointer((yyvsp[0].rec).typ);
			  (yyval.rec).sto = Snone;
			  (yyval.rec).hasval = False;
			;}
    break;

  case 218:
#line 1450 "soapcpp2_yacc.y"
    { (yyval.rec).hasval = True;
			  (yyval.rec).typ = mkint();
			  (yyval.rec).val.i = (yyvsp[-1].rec).typ->width;
			;}
    break;

  case 219:
#line 1454 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[0].rec); ;}
    break;

  case 220:
#line 1457 "soapcpp2_yacc.y"
    { (yyval.rec) = (yyvsp[-1].rec); ;}
    break;

  case 221:
#line 1458 "soapcpp2_yacc.y"
    { if ((p = enumentry((yyvsp[0].sym))) == (Entry*) 0)
				p = undefined((yyvsp[0].sym));
			  else
			  	(yyval.rec).hasval = True;
			  (yyval.rec).typ = p->info.typ;
			  (yyval.rec).val = p->info.val;
			;}
    break;

  case 222:
#line 1465 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkint();
			  (yyval.rec).hasval = True;
			  (yyval.rec).val.i = (yyvsp[0].i);
			;}
    break;

  case 223:
#line 1469 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkint();
			  (yyval.rec).hasval = True;
			  (yyval.rec).val.i = 0;
			;}
    break;

  case 224:
#line 1473 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkfloat();
			  (yyval.rec).hasval = True;
			  (yyval.rec).val.r = (yyvsp[0].r);
			;}
    break;

  case 225:
#line 1477 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkchar();
			  (yyval.rec).hasval = True;
			  (yyval.rec).val.i = (yyvsp[0].c);
			;}
    break;

  case 226:
#line 1481 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkstring();
			  (yyval.rec).hasval = True;
			  (yyval.rec).val.s = (yyvsp[0].s);
			;}
    break;

  case 227:
#line 1485 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkbool();
			  (yyval.rec).hasval = True;
			  (yyval.rec).val.i = 0;
			;}
    break;

  case 228:
#line 1489 "soapcpp2_yacc.y"
    { (yyval.rec).typ = mkbool();
			  (yyval.rec).hasval = True;
			  (yyval.rec).val.i = 1;
			;}
    break;


      default: break;
    }

/* Line 1126 of yacc.c.  */
#line 3907 "soapcpp2_yacc.tab.c"

  yyvsp -= yylen;
  yyssp -= yylen;


  YY_STACK_PRINT (yyss, yyssp);

  *++yyvsp = yyval;


  /* Now `shift' the result of the reduction.  Determine what state
     that goes to, based on the state we popped back to and the rule
     number reduced by.  */

  yyn = yyr1[yyn];

  yystate = yypgoto[yyn - YYNTOKENS] + *yyssp;
  if (0 <= yystate && yystate <= YYLAST && yycheck[yystate] == *yyssp)
    yystate = yytable[yystate];
  else
    yystate = yydefgoto[yyn - YYNTOKENS];

  goto yynewstate;


/*------------------------------------.
| yyerrlab -- here on detecting error |
`------------------------------------*/
yyerrlab:
  /* If not already recovering from an error, report this error.  */
  if (!yyerrstatus)
    {
      ++yynerrs;
#if YYERROR_VERBOSE
      yyn = yypact[yystate];

      if (YYPACT_NINF < yyn && yyn < YYLAST)
	{
	  int yytype = YYTRANSLATE (yychar);
	  YYSIZE_T yysize0 = yytnamerr (0, yytname[yytype]);
	  YYSIZE_T yysize = yysize0;
	  YYSIZE_T yysize1;
	  int yysize_overflow = 0;
	  char *yymsg = 0;
#	  define YYERROR_VERBOSE_ARGS_MAXIMUM 5
	  char const *yyarg[YYERROR_VERBOSE_ARGS_MAXIMUM];
	  int yyx;

#if 0
	  /* This is so xgettext sees the translatable formats that are
	     constructed on the fly.  */
	  YY_("syntax error, unexpected %s");
	  YY_("syntax error, unexpected %s, expecting %s");
	  YY_("syntax error, unexpected %s, expecting %s or %s");
	  YY_("syntax error, unexpected %s, expecting %s or %s or %s");
	  YY_("syntax error, unexpected %s, expecting %s or %s or %s or %s");
#endif
	  char *yyfmt;
	  char const *yyf;
	  static char const yyunexpected[] = "syntax error, unexpected %s";
	  static char const yyexpecting[] = ", expecting %s";
	  static char const yyor[] = " or %s";
	  char yyformat[sizeof yyunexpected
			+ sizeof yyexpecting - 1
			+ ((YYERROR_VERBOSE_ARGS_MAXIMUM - 2)
			   * (sizeof yyor - 1))];
	  char const *yyprefix = yyexpecting;

	  /* Start YYX at -YYN if negative to avoid negative indexes in
	     YYCHECK.  */
	  int yyxbegin = yyn < 0 ? -yyn : 0;

	  /* Stay within bounds of both yycheck and yytname.  */
	  int yychecklim = YYLAST - yyn;
	  int yyxend = yychecklim < YYNTOKENS ? yychecklim : YYNTOKENS;
	  int yycount = 1;

	  yyarg[0] = yytname[yytype];
	  yyfmt = yystpcpy (yyformat, yyunexpected);

	  for (yyx = yyxbegin; yyx < yyxend; ++yyx)
	    if (yycheck[yyx + yyn] == yyx && yyx != YYTERROR)
	      {
		if (yycount == YYERROR_VERBOSE_ARGS_MAXIMUM)
		  {
		    yycount = 1;
		    yysize = yysize0;
		    yyformat[sizeof yyunexpected - 1] = '\0';
		    break;
		  }
		yyarg[yycount++] = yytname[yyx];
		yysize1 = yysize + yytnamerr (0, yytname[yyx]);
		yysize_overflow |= yysize1 < yysize;
		yysize = yysize1;
		yyfmt = yystpcpy (yyfmt, yyprefix);
		yyprefix = yyor;
	      }

	  yyf = YY_(yyformat);
	  yysize1 = yysize + yystrlen (yyf);
	  yysize_overflow |= yysize1 < yysize;
	  yysize = yysize1;

	  if (!yysize_overflow && yysize <= YYSTACK_ALLOC_MAXIMUM)
	    yymsg = (char *) YYSTACK_ALLOC (yysize);
	  if (yymsg)
	    {
	      /* Avoid sprintf, as that infringes on the user's name space.
		 Don't have undefined behavior even if the translation
		 produced a string with the wrong number of "%s"s.  */
	      char *yyp = yymsg;
	      int yyi = 0;
	      while ((*yyp = *yyf))
		{
		  if (*yyp == '%' && yyf[1] == 's' && yyi < yycount)
		    {
		      yyp += yytnamerr (yyp, yyarg[yyi++]);
		      yyf += 2;
		    }
		  else
		    {
		      yyp++;
		      yyf++;
		    }
		}
	      yyerror (yymsg);
	      YYSTACK_FREE (yymsg);
	    }
	  else
	    {
	      yyerror (YY_("syntax error"));
	      goto yyexhaustedlab;
	    }
	}
      else
#endif /* YYERROR_VERBOSE */
	yyerror (YY_("syntax error"));
    }



  if (yyerrstatus == 3)
    {
      /* If just tried and failed to reuse look-ahead token after an
	 error, discard it.  */

      if (yychar <= YYEOF)
        {
	  /* Return failure if at end of input.  */
	  if (yychar == YYEOF)
	    YYABORT;
        }
      else
	{
	  yydestruct ("Error: discarding", yytoken, &yylval);
	  yychar = YYEMPTY;
	}
    }

  /* Else will try to reuse look-ahead token after shifting the error
     token.  */
  goto yyerrlab1;


/*---------------------------------------------------.
| yyerrorlab -- error raised explicitly by YYERROR.  |
`---------------------------------------------------*/
yyerrorlab:

  /* Pacify compilers like GCC when the user code never invokes
     YYERROR and the label yyerrorlab therefore never appears in user
     code.  */
  if (0)
     goto yyerrorlab;

yyvsp -= yylen;
  yyssp -= yylen;
  yystate = *yyssp;
  goto yyerrlab1;


/*-------------------------------------------------------------.
| yyerrlab1 -- common code for both syntax error and YYERROR.  |
`-------------------------------------------------------------*/
yyerrlab1:
  yyerrstatus = 3;	/* Each real token shifted decrements this.  */

  for (;;)
    {
      yyn = yypact[yystate];
      if (yyn != YYPACT_NINF)
	{
	  yyn += YYTERROR;
	  if (0 <= yyn && yyn <= YYLAST && yycheck[yyn] == YYTERROR)
	    {
	      yyn = yytable[yyn];
	      if (0 < yyn)
		break;
	    }
	}

      /* Pop the current state because it cannot handle the error token.  */
      if (yyssp == yyss)
	YYABORT;


      yydestruct ("Error: popping", yystos[yystate], yyvsp);
      YYPOPSTACK;
      yystate = *yyssp;
      YY_STACK_PRINT (yyss, yyssp);
    }

  if (yyn == YYFINAL)
    YYACCEPT;

  *++yyvsp = yylval;


  /* Shift the error token. */
  YY_SYMBOL_PRINT ("Shifting", yystos[yyn], yyvsp, yylsp);

  yystate = yyn;
  goto yynewstate;


/*-------------------------------------.
| yyacceptlab -- YYACCEPT comes here.  |
`-------------------------------------*/
yyacceptlab:
  yyresult = 0;
  goto yyreturn;

/*-----------------------------------.
| yyabortlab -- YYABORT comes here.  |
`-----------------------------------*/
yyabortlab:
  yyresult = 1;
  goto yyreturn;

#ifndef yyoverflow
/*-------------------------------------------------.
| yyexhaustedlab -- memory exhaustion comes here.  |
`-------------------------------------------------*/
yyexhaustedlab:
  yyerror (YY_("memory exhausted"));
  yyresult = 2;
  /* Fall through.  */
#endif

yyreturn:
  if (yychar != YYEOF && yychar != YYEMPTY)
     yydestruct ("Cleanup: discarding lookahead",
		 yytoken, &yylval);
  while (yyssp != yyss)
    {
      yydestruct ("Cleanup: popping",
		  yystos[*yyssp], yyvsp);
      YYPOPSTACK;
    }
#ifndef yyoverflow
  if (yyss != yyssa)
    YYSTACK_FREE (yyss);
#endif
  return yyresult;
}


#line 1495 "soapcpp2_yacc.y"


/*
 * ???
 */
int
yywrap()
{	return 1;
}

/******************************************************************************\

	Support routines

\******************************************************************************/

static Node
op(const char *op, Node p, Node q)
{	Node	r;
	Tnode	*typ;
	r.typ = p.typ;
	r.sto = Snone;
	if (p.hasval && q.hasval) {
		if (integer(p.typ) && integer(q.typ))
			switch (op[0]) {
			case '|':	r.val.i = p.val.i |  q.val.i; break;
			case '^':	r.val.i = p.val.i ^  q.val.i; break;
			case '&':	r.val.i = p.val.i &  q.val.i; break;
			case '<':	r.val.i = p.val.i << q.val.i; break;
			case '>':	r.val.i = p.val.i >> q.val.i; break;
			case '+':	r.val.i = p.val.i +  q.val.i; break;
			case '-':	r.val.i = p.val.i -  q.val.i; break;
			case '*':	r.val.i = p.val.i *  q.val.i; break;
			case '/':	r.val.i = p.val.i /  q.val.i; break;
			case '%':	r.val.i = p.val.i %  q.val.i; break;
			default:	typerror(op);
			}
		else if (real(p.typ) && real(q.typ))
			switch (op[0]) {
			case '+':	r.val.r = p.val.r + q.val.r; break;
			case '-':	r.val.r = p.val.r - q.val.r; break;
			case '*':	r.val.r = p.val.r * q.val.r; break;
			case '/':	r.val.r = p.val.r / q.val.r; break;
			default:	typerror(op);
			}
		else	semerror("illegal constant operation");
		r.hasval = True;
	} else {
		typ = mgtype(p.typ, q.typ);
		r.hasval = False;
	}
	return r;
}

static Node
iop(const char *iop, Node p, Node q)
{	if (integer(p.typ) && integer(q.typ))
		return op(iop, p, q);
	typerror("integer operands only");
	return p;
}

static Node
relop(const char *op, Node p, Node q)
{	Node	r;
	Tnode	*typ;
	r.typ = mkint();
	r.sto = Snone;
	r.hasval = False;
	if (p.typ->type != Tpointer || p.typ != q.typ)
		typ = mgtype(p.typ, q.typ);
	return r;
}

/******************************************************************************\

	Scope management

\******************************************************************************/

/*
mkscope - initialize scope stack with a new table and offset
*/
static void
mkscope(Table *table, int offset)
{	sp = stack-1;
	enterscope(table, offset);
}

/*
enterscope - enter a new scope by pushing a new table and offset on the stack
*/
static void
enterscope(Table *table, int offset)
{	if (++sp == stack+MAXNEST)
		execerror("maximum scope depth exceeded");
	sp->table = table;
	sp->val = 0;
	sp->offset = offset;
	sp->grow = True;	/* by default, offset grows */
	sp->mask = False;
}

/*
exitscope - exit a scope by popping the table and offset from the stack
*/
static void
exitscope()
{	check(sp-- != stack, "exitscope() has no matching enterscope()");
}

/******************************************************************************\

	Undefined symbol

\******************************************************************************/

static Entry*
undefined(Symbol *sym)
{	Entry	*p;
	sprintf(errbuf, "undefined identifier '%s'", sym->name);
	semwarn(errbuf);
	p = enter(sp->table, sym);
	p->level = GLOBAL;
	p->info.typ = mkint();
	p->info.sto = Sextern;
	p->info.hasval = False;
	return p;
}

/*
mgtype - return most general type among two numerical types
*/
Tnode*
mgtype(Tnode *typ1, Tnode *typ2)
{	if (numeric(typ1) && numeric(typ2)) {
		if (typ1->type < typ2->type)
			return typ2;
	} else	typerror("non-numeric type");
	return typ1;
}

/******************************************************************************\

	Type checks

\******************************************************************************/

static int
integer(Tnode *typ)
{	switch (typ->type) {
	case Tchar:
	case Tshort:
	case Tint:
	case Tlong:	return True;
	default:	break;
	}
	return False;
}

static int
real(Tnode *typ)
{	switch (typ->type) {
	case Tfloat:
	case Tdouble:
	case Tldouble:	return True;
	default:	break;
	}
	return False;
}

static int
numeric(Tnode *typ)
{	return integer(typ) || real(typ);
}

static void
add_fault(Table *gt)
{ Table *t;
  Entry *p1, *p2, *p3, *p4;
  Symbol *s1, *s2, *s3, *s4;
  imported = NULL;
  s1 = lookup("SOAP_ENV__Code");
  p1 = entry(classtable, s1);
  if (!p1 || !p1->info.typ->ref)
  { t = mktable((Table*)0);
    if (!p1)
    { p1 = enter(classtable, s1);
      p1->info.typ = mkstruct(t, 3*4);
      p1->info.typ->id = s1;
    }
    else
      p1->info.typ->ref = t;
    p2 = enter(t, lookup("SOAP_ENV__Value"));
    p2->info.typ = qname;
    p2->info.minOccurs = 0;
    p2 = enter(t, lookup("SOAP_ENV__Subcode"));
    p2->info.typ = mkpointer(p1->info.typ);
    p2->info.minOccurs = 0;
  }
  s2 = lookup("SOAP_ENV__Detail");
  p2 = entry(classtable, s2);
  if (!p2 || !p2->info.typ->ref)
  { t = mktable((Table*)0);
    if (!p2)
    { p2 = enter(classtable, s2);
      p2->info.typ = mkstruct(t, 3*4);
      p2->info.typ->id = s2;
    }
    else
      p2->info.typ->ref = t;
    p3 = enter(t, lookup("__type"));
    p3->info.typ = mkint();
    p3->info.minOccurs = 0;
    p3 = enter(t, lookup("fault"));
    p3->info.typ = mkpointer(mkvoid());
    p3->info.minOccurs = 0;
    p3 = enter(t, lookup("__any"));
    p3->info.typ = xml;
    p3->info.minOccurs = 0;
    custom_fault = 0;
  }
  s4 = lookup("SOAP_ENV__Reason");
  p4 = entry(classtable, s4);
  if (!p4 || !p4->info.typ->ref)
  { t = mktable((Table*)0);
    if (!p4)
    { p4 = enter(classtable, s4);
      p4->info.typ = mkstruct(t, 4);
      p4->info.typ->id = s4;
    }
    else
      p4->info.typ->ref = t;
    p3 = enter(t, lookup("SOAP_ENV__Text"));
    p3->info.typ = mkstring();
    p3->info.minOccurs = 0;
  }
  s3 = lookup("SOAP_ENV__Fault");
  p3 = entry(classtable, s3);
  if (!p3 || !p3->info.typ->ref)
  { t = mktable(NULL);
    if (!p3)
      p3 = enter(classtable, s3);
    p3->info.typ = mkstruct(t, 9*4);
    p3->info.typ->id = s3;
    p3 = enter(t, lookup("faultcode"));
    p3->info.typ = qname;
    p3->info.minOccurs = 0;
    p3 = enter(t, lookup("faultstring"));
    p3->info.typ = mkstring();
    p3->info.minOccurs = 0;
    p3 = enter(t, lookup("faultactor"));
    p3->info.typ = mkstring();
    p3->info.minOccurs = 0;
    p3 = enter(t, lookup("detail"));
    p3->info.typ = mkpointer(p2->info.typ);
    p3->info.minOccurs = 0;
    p3 = enter(t, s1);
    p3->info.typ = mkpointer(p1->info.typ);
    p3->info.minOccurs = 0;
    p3 = enter(t, s4);
    p3->info.typ = mkpointer(p4->info.typ);
    p3->info.minOccurs = 0;
    p3 = enter(t, lookup("SOAP_ENV__Node"));
    p3->info.typ = mkstring();
    p3->info.minOccurs = 0;
    p3 = enter(t, lookup("SOAP_ENV__Role"));
    p3->info.typ = mkstring();
    p3->info.minOccurs = 0;
    p3 = enter(t, lookup("SOAP_ENV__Detail"));
    p3->info.typ = mkpointer(p2->info.typ);
    p3->info.minOccurs = 0;
  }
}

static void
add_XML()
{ Symbol *s = lookup("_XML");
  p = enter(typetable, s);
  xml = p->info.typ = mksymtype(mkstring(), s);
  p->info.sto = Stypedef;
}

static void
add_qname()
{ Symbol *s = lookup("_QName");
  p = enter(typetable, s);
  qname = p->info.typ = mksymtype(mkstring(), s);
  p->info.sto = Stypedef;
}

static void
add_header(Table *gt)
{ Table *t;
  Entry *p;
  Symbol *s = lookup("SOAP_ENV__Header");
  imported = NULL;
  p = entry(classtable, s);
  if (!p || !p->info.typ->ref)
  { t = mktable((Table*)0);
    if (!p)
      p = enter(classtable, s);
    p->info.typ = mkstruct(t, 0);
    p->info.typ->id = s;
    custom_header = 0;
  }
}

static void
add_response(Entry *fun, Entry *ret)
{ Table *t;
  Entry *p, *q;
  Symbol *s;
  size_t n = strlen(fun->sym->name);
  char *r = (char*)emalloc(n+9);
  strcpy(r, fun->sym->name);
  strcat(r, "Response");
  if (!(s = lookup(r)))
    s = install(r, ID);
  free(r);
  t = mktable((Table*)0);
  q = enter(t, ret->sym);
  q->info = ret->info;
  if (q->info.typ->type == Treference)
    q->info.typ = (Tnode*)q->info.typ->ref;
  p = enter(classtable, s);
  p->info.typ = mkstruct(t, 4);
  p->info.typ->id = s;
  fun->info.typ->response = p;
}

static void
add_result(Tnode *typ)
{ Entry *p;
  if (!typ->ref || !((Tnode*)typ->ref)->ref)
  { semwarn("response struct/class must be declared before used in function prototype");
    return;
  }
  for (p = ((Table*)((Tnode*)typ->ref)->ref)->list; p; p = p->next)
    if (p->info.sto & Sreturn)
      return;
  for (p = ((Table*)((Tnode*)typ->ref)->ref)->list; p; p = p->next)
  { if (p->info.typ->type != Tfun && !(p->info.sto & Sattribute) && !is_transient(p->info.typ) && !(p->info.sto & (Sprivate|Sprotected)))
      p->info.sto = (Storage)((int)p->info.sto | (int)Sreturn);
      return;
  }
}

