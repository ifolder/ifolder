#ifndef __ENABLE_H__
#define __ENABLE_H__
/*****************************************************************************
 *
 *	(C) Copyright 1988-1994 Novell, Inc.
 *	All Rights Reserved.
 *
 *	This program is an unpublished copyrighted work which is proprietary
 *	to Novell, Inc. and contains confidential information that is not
 *	to be reproduced or disclosed to any other person or entity without
 *	prior written consent from Novell, Inc. in each and every instance.
 *
 *	WARNING:  Unauthorized reproduction of this program as well as
 *	unauthorized preparation of derivative works based upon the
 *	program or distribution of copies by sale, rental, lease or
 *	lending are violations of federal copyright laws and state trade
 *	secret laws, punishable by civil and criminal penalties.
 *
 *  $Workfile:   enable.h  $
 *  $Modtime:   Jul 20 2001 15:56:42  $
 *  $Revision$
 *  
 ****************************************************************************/

/*
	NetWare/386 - Header file for language enabling
	
	Written by: Lloyd Honomichl
	Date:		January 16, 1991
*/

/****************************************************************************/

/*
	number of defined languages as well as their ID numbers
*/

#define CanadianFrenchLanguageID	0
#define ChineseLanguageID			1	/* simplified */
#define DanishLanguageID			2
#define DutchLanguageID				3
#define EnglishLanguageID			4
#define FinnishLanguageID			5
#define FrenchLanguageID			6
#define GermanLanguageID			7
#define	ItalianLanguageID			8
#define	JapaneseLanguageID			9
#define KoreanLanguageID			10
#define NorwegianLanguageID			11
#define PortugueseLanguageID		12	/* Brazil */
#define	RussianLanguageID			13
#define SpanishLanguageID			14	/* Latin America */
#define SwedishLanguageID			15
#define ChineseTradLanguageID		16	/* traditional */
#define PolishLanguageID			17
#define PortuguesePortLanguageID	18	/* Portugal */
#define SpanishSpainLanguageID		19	/* Spain */
#define HungarianLanguageID			20	/* HUNGARIAN */
#define CzechLanguageID				21	/* Czech */
#define ArabicLanguageID			22	/* Arabic */
#define HebrewLanguageID			23	/* Hebrew */
#define ThaiLanguageID				24	/* Thai */
#define TurkeyLanguageID			25	/* Turkey */
#define GreekLanguageID				26	/* Greek */

#define NumberOfPreDefinedLanguages	27

/* extra languague number range, not inclusive (valid is 100-999) */
#define	MIN_LANGUAGE_ID				99
#define	MAX_LANGUAGE_ID				1000

/*
	The os language table (indexed by language ID)
*/

extern BYTE *OSLanguageName[NumberOfPreDefinedLanguages];

/*
	Extra Language list structure
*/
struct ExtraLanguageStructure
{
	struct ExtraLanguageStructure	*next;
	struct ExtraLanguageStructure	*prev;
	int								LanguageID;
	BYTE							*LanguageName;
};

extern struct ExtraLanguageStructure *ExtraLanguageListHead;

/*
	DOS Country info structure
*/
struct DOSCountryInfoStructure
{
	WORD dateFmt;			/* Date format								*/
	BYTE currencySym[5];	/* Currency symbol							*/
	BYTE thousandSep[2];	/* Thousands separator						*/
	BYTE decimalSep[2];		/* Decimal separator						*/	
	BYTE dateSep[2];		/* Date separator							*/
	BYTE timeSep[2];		/* Time separator							*/
	BYTE currencyFmt;		/* Currency format							*/
	BYTE currencyDig;		/* Significant digits in currency			*/
	BYTE timeFmt;			/* Time format								*/
	LONG caseMapRoutine;	/* Routine to call for case mapping			*/
	BYTE dataListSep[2];	/* Data list separator						*/
	BYTE reserved[10];		/* Reserved									*/
};

struct NWEXTENDED_COUNTRY_INFO
{
	BYTE	infoID;			/* ??										*/
	WORD	size;			/* ??										*/
	WORD  	countryID;
	WORD  	codePage;
	WORD  	dateFormat;		
	BYTE    currencySymbol[5];
	BYTE    thousandSeparator[2];
	BYTE    decimalSeparator[2];
	BYTE    dateSeparator[2];
	BYTE    timeSeparator[2];
	BYTE    currencyFormatFlags;
	BYTE    digitsInCurrency;
	BYTE    timeFormat;
    BYTE	junk[4];		/* Would have pointed to upper case function*/
	BYTE    dataListSeparator[2];
	BYTE    PAD[10];
};


extern struct DOSCountryInfoStructure DOSCountryInfo;

extern WORD DOSCountryID;
extern WORD DOSCodePage;

extern BYTE *weekDayNames[];	/* Names of weekdays						*/
extern BYTE *monthNames[];		/* Names of months							*/
extern BYTE *monthAbbrevs[];
extern BYTE *ENGLISHmonthNames[];
extern BYTE *ENGLISHweekDayNames[];
extern BYTE *ENGLISHmonthAbbrevs[];


extern LONG OSDoubleByteSpace;	/* value for space character				*/

/*
	Flags passed to date and time formatting routines
*/
#define EN_INCLUDE_SECONDS	0x01	/* Include seconds in time				*/
#define EN_INCLUDE_WEEKDAY	0x02	/* Include the day of the week in date	*/
#define EN_USE_ALPHA_MONTH	0x04	/* Use month name, not number			*/
#define EN_USE_4_DIGIT_YEAR 0x08	/* Print four digit year				*/
#define EN_USE_ABBREV_MONTH	0x10	/* Use month abbreviation, not number	*/

/*
	Buffer lengths required for formatting dates and times.  The numeric
	format lengths are known, but those with text for the day of the week
	and the month are padded, since we don't know how long they may be
	after translation
*/
#define EN_TIME_LEN		   11	/* HH:MM:SSam								*/
#define EN_DATE_LEN		   11	/* MM/DD/YYYY								*/
#define EN_DATE_TIME_LEN   23	/* MM/DD/YYYY  HH:MM:SSam					*/
#define EN_TEXT_DATE_LEN   80	/* Wednesday  September 31, 1990  HH:MM:SSam*/

/****************************************************************************/
/*
	Line draw character macros
*/

/* Ä */
#define LDC_H1		OSLineDrawCharTable[0]
/* Í */
#define LDC_H2		OSLineDrawCharTable[1]
/* ³ */
#define LDC_V1		OSLineDrawCharTable[2]
/* º */
#define LDC_V2		OSLineDrawCharTable[3]
/* Ú */
#define LDC_UL1		OSLineDrawCharTable[4]
/* ¿ */
#define LDC_UR1		OSLineDrawCharTable[5]
/* À */
#define LDC_LL1		OSLineDrawCharTable[6]
/* Ù */
#define LDC_LR1		OSLineDrawCharTable[7]
/* É */
#define LDC_UL2		OSLineDrawCharTable[8]
/* » */
#define LDC_UR2		OSLineDrawCharTable[9]
/* È */
#define LDC_LL2		OSLineDrawCharTable[10]
/* ¼ */
#define LDC_LR2		OSLineDrawCharTable[11]
/* Á */
#define LDC_UT1		OSLineDrawCharTable[12]
/* Â */
#define LDC_DT1		OSLineDrawCharTable[13]
/* ´ */
#define LDC_LT1		OSLineDrawCharTable[14]
/* Ã */
#define LDC_RT1		OSLineDrawCharTable[15]
/* Ð */
#define LDC_UT12	OSLineDrawCharTable[16]
/* Ò */
#define LDC_DT12	OSLineDrawCharTable[17]
/* µ */
#define LDC_LT12	OSLineDrawCharTable[18]
/* Æ */
#define LDC_RT12	OSLineDrawCharTable[19]
/* Ï */
#define LDC_UT21	OSLineDrawCharTable[20]
/* Ñ */
#define LDC_DT21	OSLineDrawCharTable[21]
/* ¶ */
#define LDC_LT21	OSLineDrawCharTable[22]
/* Ç */
#define LDC_RT21	OSLineDrawCharTable[23]
/* Ê */
#define LDC_UT2		OSLineDrawCharTable[24]
/* Ë */
#define LDC_DT2		OSLineDrawCharTable[25]
/* ¹ */
#define LDC_LT2		OSLineDrawCharTable[26]
/* Ì */
#define LDC_RT2		OSLineDrawCharTable[27]
/* Ö */
#define LDC_UL12	OSLineDrawCharTable[28]
/* · */
#define LDC_UR12	OSLineDrawCharTable[29]
/* Ó */
#define LDC_LL12	OSLineDrawCharTable[30]
/* ½ */
#define LDC_LR12	OSLineDrawCharTable[31]
/* Õ */
#define LDC_UL21	OSLineDrawCharTable[31]
/* ¸ */
#define LDC_UR21	OSLineDrawCharTable[33]
/* Ô */
#define LDC_LL21	OSLineDrawCharTable[34]
/* ¾ */
#define LDC_LR21	OSLineDrawCharTable[35]
/* Å */
#define LDC_X1		OSLineDrawCharTable[36]
/* × */
#define LDC_X12		OSLineDrawCharTable[37]
/* Ø */
#define LDC_X21		OSLineDrawCharTable[38]
/* Î */
#define LDC_X2		OSLineDrawCharTable[39]
/*  */
#define LDC_UP		OSLineDrawCharTable[40]
/*  */
#define LDC_DOWN	OSLineDrawCharTable[41]
/*  */
#define LDC_LEFT	OSLineDrawCharTable[42]
/*  */
#define LDC_RIGHT 	OSLineDrawCharTable[43]
/* ° */
#define LDC_BG1		OSLineDrawCharTable[44]
/* ± */
#define LDC_BG2		OSLineDrawCharTable[45]
/* ² */
#define LDC_BG3		OSLineDrawCharTable[46]
/* Û */
#define LDC_BG4		OSLineDrawCharTable[47]

/****************************************************************************/
/****************************************************************************/

/* ProcessLocaleConfigFile
 *
 * This procedure is designed to be used by the INSTALL NLM.  It should be
 * called before the name space NLMs are loaded and initialized.  It should
 * be called before the volumes are mounted.  It should be called before the
 * screen driver is loaded.  It should be called before vrepair is loaded.
 * It should be called before anything else that takes a snapshot of the
 * locale tables.
 */

extern LONG ProcessLocaleConfigFile(
				struct ScreenStruct *screenID,
				BYTE *fileName);

extern LONG ResetFileServerName(
		BYTE *newFileServerName);

#endif /* __ENABLE_H__ */
