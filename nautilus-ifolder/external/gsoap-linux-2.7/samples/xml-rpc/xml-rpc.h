/*

XML-RPC binding

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

/// Scalar <boolean> element

typedef char		_boolean;	///< boolean values are 0 (false) or 1 (true)

/// Scalar <double> element

typedef double		_double;	///< double floating point value

/// Scalar <i4> element

typedef int		_i4;		///< 32bit int value

/// Scalar <int> element

typedef int		_int;		///< 32bit int value

/// Scalar <string> element

typedef char*		_string;	///< string value

/// Scalar <dateTime.iso8601> element

typedef char*		_dateTime_DOTiso8601;	///< ISO8601 date and time formatted string

/// <base64> binary data element

struct _base64
{ unsigned char*	__ptr;		///< pointer to raw binary data block
  int			__size;		///< size of raw binary data block
};

/// <struct> element

struct _struct
{ int			__size;		///< number of members
  struct member*	member;		///< pointer to member array
};

/// <array> element

struct _array
{ int			__size;		///< number of array elements
  struct value*		value;		///< pointer to array elements
};

/// <value>

struct value
{ int			__type;		///< SOAP_TYPE_X
  void*			ref;		///< ref to data
  char*			__any;		///< <value> string content
};

/// <member>

struct member
{ char*			name;
  struct value		value;
};

/// <params>

struct params
{ int			__size;		///< number of parameters
  struct param*		param;		///< pointer to array of parameters
};

/// <param>

struct param
{ struct value		value;		///< parameter value
};

/// <methodCall>

struct methodCall
{ char*			methodName;	///< name of the method
  struct params		params;		///< method parameters
};

/// <methodResponse>
 
struct methodResponse
{ struct params*	params;		///< response parameters
  struct fault*		fault;		///< response fault
};
  
/// <fault>

struct fault
{ struct value		value;		///< value of the fault
};
