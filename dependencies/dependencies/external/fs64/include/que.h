/****************************************************************************
 |
 |	(C) Copyright 1985, 1991, 1993, 1996 Novell, Inc.
 |	All Rights Reserved.
 |
 |	This program is an unpublished copyrighted work which is proprietary
 |	to Novell, Inc. and contains confidential information that is not
 |	to be reproduced or disclosed to any other person or entity without
 |	prior written consent from Novell, Inc. in each and every instance.
 |
 |	WARNING:  Unauthorized reproduction of this program as well as
 |	unauthorized preparation of derivative works based upon the
 |	program or distribution of copies by sale, rental, lease or
 |	lending are violations of federal copyright laws and state trade
 |	secret laws, punishable by civil and criminal penalties.
 |
 |***************************************************************************
 |
 |	 NetWare Advance File Services (NSS) module
 |
 |---------------------------------------------------------------------------
 |
 | $Author$
 | $Modtime:   19 Jun 2001 16:09:06  $
 |
 | $Workfile:   que.h  $
 | $Revision$
 |
 |---------------------------------------------------------------------------
 |	This module is used to:
 |		Define all of the QUEING operations
 |
 | WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING! WARNING!
 |
 | This header file should ONLY be used for NSS internal development.
 | This includes Semantic Agents (SA) and Loadable Storage Services (LSS).
 | Any other use may cause conflicts which NSS will NOT fix.
 +-------------------------------------------------------------------------*/
#ifndef _QUE_H_
#define _QUE_H_

#ifndef _STDDEF_H_
#	include <stddef.h>
#endif
#ifndef _OMNI_H_
#	include <omni.h>
#endif

#ifdef __cplusplus
extern "C" {
#endif

/*----------------------------------------------------------------------------
 | Que.h defines a set of macros for manipulating various types of
 | linked lists.  When a linked list needs to be used, these macros
 | and functions will be used.  A structure that wants to use one or
 | more of these linked lists, must include in their structure
 | definition a link field of a type appropriate to the type of linked
 | list structure being used.  The link field can be any place in the
 | structure though being placed at the front of the structure generates
 | slightly better code.
 | 
 | We support four types of linked list.
 | 
 | 	1. A stack (STK)
 | 	2. A singly linked list (SQ)
 | 	3. A singly linked circular list (CIR)
 | 	4. A doubly linked circular list (DQ)
 | 
 | The following sections describe each of these link types in
 | detail and how to use the various operations and under what
 | circumstances they should be used.
 | 
 | You have several compile time options to ENABLE or DISABLE to
 | select how these routines are implemented in your system.
 | 
 | 	MCCABE		ENABLE	Sets rest of options to minimize complexity
 | 						measures from the MCCABE tools.
 | 				DISABLE	Use options set by developer.
 | 
 | 	QUE_NULL	ENABLE	NULL out next pointer after dequeuing
 | 				DISABLE	Next pointer is not nulled, thus QMEMBER
 | 						and QUE_CHECK are not available.
 | 
 | 	QUE_CHECK	ENABLE	Make sure 'next' is NULL when putting an
 | 						element into a linked list.  This should
 | 						be ENABLED during development but should
 | 						be DISABLED for maximum performance.
 | 				DISABLE	No checking done.
 | 
 | 	QUE_MACRO	ENABLE	Use the macro (in-line) versions.  Because
 | 						of locality in caches, it may be more
 | 						efficient to use the function version of
 | 						these routines rather than the macro version.
 | 				DISABLE	Use the function versions.  Makes it
 | 						easier to step over queuing functions
 | 						when debugging.
 | 
 | The generic macros work across all linked list types.  The following
 | parameters are the same for all the macros.
 | 
 | 	type
 | 		INPUT: The type of the structure pointed to by 'item'.
 | 	linkField
 | 		INPUT: The field allocated in the structure to be
 | 		used as the link field for the linked list.  It should
 | 		be declared to be of appropriate for the linked list.
 | 
 | 	Definitions used in USAGE sections:
 | 
 | 		typedef struct Xyz_s
 | 		{
 | 			int			field_a;
 | 			DQlink_t	link;
 | 			int			field_b;
 | 		} Xyz_s;
 | 
 | 		DQhead_t	Head;
 | 		Zyz_s		Xyzzy;
 | 		Zyz_s		*xyz;
 | 
 | 	FRONTADDR(item, type, linkField)
 | 			Because the link field can be any place in the structure,
 | 			sometimes you have a pointer to the link field and want
 | 			a pointer to the beginning of the structure.  FRONTADDR
 | 			subtracts the offset of the 'linkField' from the pointer
 | 			to adjust 'item' to point to the front of the structure.
 | 		item
 | 			UPDATE: points to the link field and is modified to point
 | 			to the front of the structure.
 | 		USAGE:
 | 			{
 | 				Xyz_s	*abc = (Xyz_s *)&Xyzzy.link;
 | 
 | 				FRONTADDR(abc, Xyz_s, link);
 | 			}
 | 
 | 	STRUCT(item, type, linkField)
 | 			Same as FRONTADDR but rather than updating 'item', lets
 | 			the new pointer be assigned to a variable of your choice.
 | 		item
 | 			INPUT: points to the link field in the structure.
 | 		USAGE:
 | 			{
 | 				DQlink_t	*abc = &Xyzzy.link;
 | 				Xyz_s		*efg;
 | 
 | 				efg = STRUCT(abc, Xyz_s, link);
 | 			}
 | 
 | 	NEXT(item)
 | 			Gets the next item in a linked list.  In this case,
 | 			item points to the link field of the structure.
 | 		item
 | 			INPUT: points to the link field of the structure.
 | 		USAGE:
 | 			{
 | 				DQlink_t	*abc = &Xyzzy.link;
 | 				DQlink_t	*next;
 | 				Xyz_s		*efg;
 | 
 | 				next = NEXT(abc);
 | 				efg = STRUCT(next, Xyz_s, link);
 | 			}
 | 
 | 	ONEXT(item, type, linkField)
 | 			Gets the next item in a linked list.  In this case,
 | 			assumes item is pointing to the front of the structure
 | 			and returns the pointer to the front of the next
 | 			element in the structure.
 | 		item
 | 			INPUT: points to the front of the structure.
 | 		USAGE:
 | 			{
 | 				Xyz_s	*abc = &Xyzzy;
 | 				Xyz_s	*next;
 | 
 | 				next = NEXT(abc, Xyz_s, link);
 | 			}
 | 
 | 	PREV and OPREV only apply to doubly linked lists which have
 | 	a previous element pointer but are included here because they
 | 	complement NEXT and ONEXT.
 | 
 | 	PREV(item)
 | 			Gets the previous item in a doubly linked list.  In
 | 			this case, item points to the link field of the structure.
 | 			Useful for backing up when in a loop before removing
 | 			the current element being processed.
 | 		item
 | 			INPUT: points to the link field of the structure.
 | 		USAGE:
 | 			{
 | 				DQlink_t	*abc = &Xyzzy.link;
 | 				DQlink_t	*prev;
 | 				Xyz_s		*efg;
 | 
 | 				prev = PREV(abc);
 | 				efg = STRUCT(prev, Xyz_s, link);
 | 			}
 | 
 | 	OPREV(item, type, linkField)
 | 			Gets the previous item in a doubly linked list.  In
 | 			this case, it assumes item is pointing to the front
 | 			of the structure and returns the pointer to the front
 | 			of the previous element in the structure.
 | 		item
 | 			INPUT: points to the front of the structure.
 | 		USAGE:
 | 			{
 | 				Xyz_s	*abc = &Xyzzy;
 | 				Xyz_s	*prev;
 | 
 | 				prev = PREV(abc, Xyz_s, link);
 | 			}
 | 
 | 	NULLIFY(link)
 | 			Used internally by link list routines to set the
 | 			'next' field to NULL if QUE_NULL is ENABLED.  Can
 | 			be used by code to initialize the link field of
 | 			a structure if the structure is not zeroed.
 | 		link
 | 			INPUT: pointer to the link field of the structure.
 | 		USAGE:
 | 			{
 | 				NULLIFY( &Xyzzy.link);
 | 			}
 | 
 | 	QMEMBER(link)
 | 			Used to test if linkField is in a linked list.  It does
 | 			this by comparing the linkField to NULL.  Only works if
 | 			QUE_NULL is ENABLED.
 | 		item
 | 			INPUT: pointer to the link field of the structure.
 | 		USAGE:
 | 			{
 | 				if (QMEMBER( &Xyzzy.link))
 | 				{
 | 					DQ_RMV( &Xyzzy, link);
 | 			}
 | 
 | Stack: STK
 | 	A stack is a singly linked list that supports Last-In-First-Out
 | 	(LIFO) order access to its members.  The last element on the
 | 	list points to NULL.
 | 
 | 	Empty Head
 | 	+-------+
 | 	| NULL  |
 | 	+-------+
 | 	
 | 	+-------+
 | 	|  Top  +-------+
 | 	+-------+       |
 | 				    |
 | 				    v
 | 				+-------+
 | 				|   A   | first item on list
 | 				+---+---+
 | 				    |
 | 				    v
 | 				+-------+
 | 				|   B   |
 | 				+---+---+
 | 				    |
 | 				    v
 | 				+-------+
 | 				|   C   | last item on list
 | 				+---+---+
 | 				    |
 | 	              --+--
 | 				   ---
 | 					-
 | 
 | 	typedef struct Xyz_s
 | 	{
 | 		int			field_a;
 | 		STKlink_t	stkLink;
 | 		int			field_b;
 | 	} Xyz_s;
 | 	
 | 	zyzzy()
 | 	{
 | 		STKtop_t	Top;
 | 		Xyz_s		A, B, C;
 | 		Xyz_s		*a, *b, *c;
 | 		
 | 		STK_INIT(Top);
 | 	
 | 		STK_PUSH(Top, &C, stkLink);
 | 		STK_PUSH(Top, &B, stkLink);
 | 		STK_PUSH(Top, &A, stkLink);
 | 	
 | 		STK_POP(Top, a, Xyz_s, stkLink);
 | 		STK_POP(Top, b, Xyz_s, stkLink);
 | 		STK_POP(Top, c, Xyz_s, stkLink);
 | 	}
 | 
 | Stacks are useful for handling free lists of resources.  The link fields
 | have been designed so that a structure that normally resides on some
 | other type of linked data structure, can be stored on a free list managed
 | by the stack macros.
 | 
 | Typedefs:
 | 
 | 	STKlink_t	Use the STKlink_t typedef to define the link field
 | 				in the structures to be managed as a stack.  This
 | 				field must be initialized to zero either by zeroing
 | 				the whole structure or calling NULLIFY with the field.
 | 
 | 	STKtop_t	Use the STKtop_t typedef to define the top of the
 | 				stack or LIFO.  The top must be initialized by either
 | 				using STK_INIT or STK_INIT_ELEMENTS.
 | 
 | Macros:
 | 
 | 	STK_INIT(top)
 | 			Initialize the top of the stack.
 | 		top
 | 			OUTPUT: A variable of type STKtop_t used for the
 | 			top of the stack.
 | 
 | 	STK_STATIC_INIT()
 | 			Uses compile time initialization to initialize the head.
 | 		USAGE:
 | 			STKtop_t Top = STK_STATIC_INIT();
 | 
 | 	STK_INIT_ELEMENTS(top, data, numElements, typeElement, linkField)
 | 			Used to take an array of items and initialize all of them
 | 			and push them onto the stack.  A nice way to build a free
 | 			list during initialization.
 | 		top
 | 			OUTPUT: A variable of type STKtop_t used for the
 | 			top of the stack.  Assumed to be uninitialized.
 | 		data
 | 			INPUT: Pointer to the first item in the array. Usually
 | 			the array name.
 | 		numElements
 | 			INPUT: Number of elements in the array.
 | 		typeElement
 | 			INPUT: The type of structure of the elements in the array.
 | 		linkField:
 | 			INPUT: The name of the field to be used for stack link (does
 | 			not have to be of type STKlink_t).
 | 		USAGE:
 | 			{
 | 				Xyz_s		Xyz[10];
 | 				STKtop_t	Top;
 | 
 | 				STK_INIT_ELEMENTS(Top, Xyz, 10, Xyz_s, link);
 | 			}
 | 
 | 	STK_EMPTY(top)
 | 			Is True, if top of stack is empty, otherwise is False.
 | 			Used to test if there are any items left on the stack.
 | 		top
 | 			INPUT: The top of the stack.
 | 		USAGE:
 | 			if (STK_EMPTY(Top)) { ... }
 | 
 | 	STK_NOT_EMPTY(top)
 | 			Is False, if top of stack is empty, otherwise is True.
 | 			Easier to understand then "if (!STK_EMPTY(top))...".
 | 		top
 | 			INPUT: The top of the stack.
 | 		USAGE:
 | 			if (STK_NOT_EMPTY(Top)) { ... }
 | 
 | 	STK_PUSH(top, item, linkField)
 | 			Push an item on to the stack.  If the stack is immediately
 | 			popped, this item will come off first.
 | 		top
 | 			UPDATE: The top of the stack.  Updated to point to 'item'.
 | 		item
 | 			INPUT: Pointer to a structure to be put in the stack.
 | 			The 'linkField' of 'item' is set to the current structure
 | 			pointer in 'top' and 'top' points to 'item'.
 | 
 | 	STK_POP(top, item, type, linkField)
 | 			Pop the top item off the stack.  If the stack is empty,
 | 			item is set to NULL, otherwise it has the top structure
 | 			on the stack and the top now points to the next item on
 | 			the stack.
 | 		top
 | 			UPDATE: The top of the stack.  Updated to point to
 | 			the next item on the stack.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to NULL if the stack is empty, otherwise set to a
 | 			pointer to the structure pointed to by top.
 | 
 | 	STK_POP_NO_CHECK(top, item, type, linkField)
 | 			STK_POP_NO_CHECK is just like STK_POP except we know
 | 			something is on the stack having done a STK_NOT_EMPTY
 | 			or a STK_PEEK earlier.
 | 		top
 | 			UPDATE: The top of the stack.  Updated to point to
 | 			the next item on the stack.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to NULL if the stack is empty, otherwise set to a
 | 			pointer to the structure pointed to by top.
 | 
 | 	STK_PEEK(top, item, type, linkField)
 | 			Sets 'item' to the first element at the top of the stack
 | 			but does not remove it from the stack.  Good for checking
 | 			resources before committing them.  Usually used with
 | 			STK_DROP.
 | 		top
 | 			INPUT: The top of the stack.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to NULL if the stack is empty, otherwise set to a
 | 			pointer to the structure pointed to by top.
 | 
 | 	STK_DROP(top, item, type, linkField)
 | 			Used to drop or remove 'item' from the top of the stack.
 | 			Normally, 'item' is obtained by using STK_PEEK.
 | 		top
 | 			UPDATE: The top of the stack.  Updated to point to
 | 			the next item on the stack.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to NULL if the stack is empty, otherwise set to a
 | 			pointer to the structure pointed to by top.
 | 
 | 	STK_RMV(top, item, linkField)
 | 			Removes the designated 'item' from the stack by starting
 | 			at the head of the stack and scanning the stack linearly
 | 			until it finds the item and removes it from the stack.
 | 		top
 | 			UPDATE: The top of the stack.  Updated to point to
 | 			the next item on the stack.
 | 		item
 | 			INPUT: Pointer to a structure of type 'type'.  This is the
 | 			item to be removed from the stack.
 | 		RETURNS:
 | 			TRUE: found the item in the stack and removed it.
 | 			FALSE: did not find the item in the stack and did nothing.
 | 
 | 	STK_FOREACH(head, item, type, linkField)
 | 			Used to scan through the stack to process it or search for
 | 			a particular element in the stack.  It is really a 'for' loop
 | 			with all the pieces setup for scanning the stack.
 | 		top
 | 			INPUT: A pointer to the top of the stack.
 | 		item
 | 			OUTPUT: A pointer to the current item in the stack.
 | 		USAGE:
 | 			{
 | 				extern STKtop_t	Top;
 | 				Xyz_s			*x;
 | 
 | 				STK_FOREACH(Top, x, Xyz_s, stkLink)
 | 				{
 | 					if (x->field_a == 42) return x;
 | 				}
 | 			}
 | Singly Linked Queue: SQ
 | 	A singly linked queue is a linked lists that supports First-In-First-Out
 | 	(FIFO) order access to its members.  The head has a pointer to the
 | 	beginning and end of the linked list.  This is the most efficient
 | 	for enqueuing and dequeuing operations of the FIFO data structures
 | 	we support.  The head does take more space than the circular linked
 | 	lists (CIR).  Elements can be inserted at either the head or the tail
 | 	of the queue but can only be take from the head.
 | 
 | 	Empty Head
 | 	+-------+
 | 	| next  |<------+
 | 	+-------+       |
 | 	| last  |       |
 | 	+---+---+       |
 | 		|           |
 | 		+-----------+
 | 	
 | 	  Head
 | 	+-------+
 | 	| next  +-------+
 | 	+-------+       |
 | 	| last  |       |
 | 	+-------+       |
 | 		|		    |
 | 		|		    v
 | 		|		+-------+
 | 		|		|   A   | first item on list
 | 		|		+---+---+
 | 		|		    |
 | 		|		    v
 | 		|		+-------+
 | 		|		|   B   |
 | 		|		+---+---+
 | 		|		    |
 | 		|		    v
 | 		|		+-------+
 | 		+------>|   C   | last item on list
 | 				+-------+
 | 
 | 	typedef struct Xyz_s
 | 	{
 | 		int			field_a;
 | 		SQlink_t	sqLink;
 | 		int			field_b;
 | 	} Xyz_s;
 | 	
 | 	zyzzy()
 | 	{
 | 		SQhead_t	Head;
 | 		Xyz_s		A, B, C;
 | 		Xyz_s		*a, *b, *c;
 | 		
 | 		SQ_INIT( &Head);
 | 	
 | 		SQ_ENQ( &Head, &A, sqLink);
 | 		SQ_ENQ( &Head, &B, sqLink);
 | 		SQ_ENQ( &Head, &C, sqLink);
 | 	
 | 		SQ_DEQ( &Head, a, Xyz_s, sqLink);
 | 		SQ_DEQ( &Head, b, Xyz_s, sqLink);
 | 		SQ_DEQ( &Head, c, Xyz_s, sqLink);
 | 	}
 | 
 | Singly linked queues can quickly process queues that are normally
 | accessed in FIFO order and elements are rarely or never removed
 | from the middle of the queue.  To remove an element from the middle
 | of the queue requires starting a scan at the head and following the
 | links until the desired element is found.
 | 
 | Typedefs:
 | 
 | 	SQlink_t	Use the SQlink_t typedef to define the link field
 | 				in the structures to be managed as a singly linked
 | 				queue.  This field must be initialized to zero either
 | 				by zeroing the whole structure or calling NULLIFY
 | 				with the field.
 | 
 | 	SQhead_t	Use the SQhead_t typedef to define the head of the
 | 				singly linked queue.  The head  must be initialized by
 | 				either using SQ_INIT or SQ_INIT_ELEMENTS.
 | 
 | Macros:
 | 
 | 	SQ_INIT(head)
 | 			Initialize the head of the queue.
 | 		head
 | 			OUTPUT: A pointer to a variable of type SQhead_t.
 | 
 | 	SQ_INIT_ELEMENTS(head, data, numElements, typeElement, linkField)
 | 			Used to take an array of items and initialize all of them
 | 			and enqueue them in the queue. 
 | 		head
 | 			OUTPUT: A variable of type SQhead_t used for the
 | 			head of the queue.  Assumed to be uninitialized.
 | 		data
 | 			INPUT: Pointer to the first item in the array. Usually
 | 			the array name.
 | 		numElements
 | 			INPUT: Number of elements in the array.
 | 		typeElement
 | 			INPUT: The type of structure of the elements in the array.
 | 		linkField:
 | 			INPUT: The name of the field to be used for queue link.
 | 		USAGE:
 | 			{
 | 				Xyz_s		Xyz[10];
 | 				SQhead_t	Head;
 | 
 | 				SQ_INIT_ELEMENTS(Head, Xyz, 10, Xyz_s, link);
 | 			}
 | 
 | 	SQ_EMPTY(head)
 | 			Is True, if queue is empty, otherwise is False.
 | 			Used to test if there are any items left in the queue.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		USAGE:
 | 			if (SQ_EMPTY(Head)) { ... }
 | 
 | 	SQ_NOT_EMPTY(head)
 | 			Is False, if queue is empty, otherwise is True.
 | 			Easier to understand then "if (!SQ_EMPTY(head))...".
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		USAGE:
 | 			if (SQ_NOT_EMPTY(Head)) { ... }
 | 
 | 	SQ_ENQ(head, item, linkField)
 | 			Enqueue an item on to the tail of the queue.
 | 		head
 | 			UPDATE: Pointer to the head of the queue.  The tail
 | 			pointer in the head will point to 'item'.
 | 		item
 | 			INPUT: Pointer to the element to be inserted at the
 | 			tail of the queue.
 | 
 | 	SQ_PUSH(head, item, linkField)
 | 			Push an item on to the front of the queue.  If the queue
 | 			is immediately dequeued, this item will come off.  Useful
 | 			preempting others or forcing something to be reused quickly.
 | 		head
 | 			UPDATE: Pointer to the head of the queue.  The head
 | 			pointer in the head will point to 'item'.
 | 		item
 | 			UPDATE: Pointer to the element to be inserted at the head
 | 			of the queue.  The link field of 'item' points to what
 | 			the 'head' did.
 | 
 | 	SQ_DEQ(head, item, type, linkField)
 | 			Dequeue the first item off the queue.  If the queue is
 | 			empty, item is set to NULL, otherwise it has the oldest
 | 			structure in the queue and the head now points to the
 | 			next item in the queue.
 | 		head
 | 			UPDATE: A pointer to the head of the queue.  Updated to
 | 			point to the next item on the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to NULL if the queue is empty, otherwise set to a
 | 			pointer to the structure pointed to by head.
 | 
 | 	SQ_DEQ_NO_CHECK(head, item, type, linkField)
 | 			SQ_DEQ_NO_CHECK is just like SQ_DEQ except we know
 | 			something is on the queue having done a SQ_NOT_EMPTY
 | 			or a SQ_PEEK earlier.
 | 		head
 | 			UPDATE: A pointer to the head of the queue.  Updated to
 | 			point to the next item on the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to a pointer to the structure pointed to by head.
 | 
 | 	SQ_PEEK(head, item, type, linkField)
 | 			Sets 'item' to the first element at the head of the queue
 | 			but does not remove it from the queue.  Good for checking
 | 			resources before committing them.  Usually used with
 | 			SQ_DROP.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to a pointer to the structure pointed to by head.  If
 | 			queue is empty, it is set to NULL.
 | 
 | 	SQ_PEEK_LAST(head, item, type, linkField)
 | 			Sets 'item' to the element at the tail of the queue
 | 			but does not remove it from the queue.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to a pointer to the structure pointed to by head.  If
 | 			queue is empty, it is set to NULL.
 | 
 | 	SQ_DROP(head, item, linkField)
 | 			Used to drop or remove 'item' from the head of the queue.
 | 			Normally, 'item' is obtained by using SQ_PEEK.
 | 		head
 | 			UPDATE: A pointer to the head of the queue.  Updated to
 | 			point to the next item on the queue.
 | 		item
 | 			UPDATE: Pointer to a structure of type 'type'.  'item'
 | 			was obtained by doing a SQ_PEEK.
 | 
 | 	SQ_RMV(head, item, linkField)
 | 			Removes the designated 'item' from the queue by starting
 | 			at the head of the queue and scanning the queue linearly
 | 			until it finds the item and removes it from the queue.
 | 		head
 | 			UPDATE: A pointer to the head of the queue.  If the 'item'
 | 			is pointed to by the head or tail, the head structure is
 | 			updated appropriately.
 | 		item
 | 			INPUT: Pointer to a structure of type 'type'.  LinkField
 | 			will be set to NULL, if QUE_NULL is enabled.
 | 
 | 	SQ_APPEND(head, appendee)
 | 			Used to append one queue to another queue.  All the
 | 			elements in the appendee queue are placed at the tail
 | 			of the head queue.
 | 		head
 | 			UPDATE: A pointer to the head of the queue that the other
 | 			queue will be appended.  Its tail pointer is set to the
 | 			tail of the 'appendee' queue.
 | 		appendee
 | 			UPDATE: A pointer to the head of the queue that will be
 | 			attached to the end of the head queue.  'appendee' is
 | 			made an empty queue.
 | 
 | 	SQ_PREPEND(head, prependee)
 | 			Used to prepend one queue to another queue.  All the
 | 			elements in the prepend queue are placed at the front
 | 			of the head queue.
 | 		head
 | 			UPDATE: A pointer to the head of the queue that the other
 | 			queue will be prepended.  Its head pointer is set to the
 | 			head of the 'prependee' queue.
 | 		prependee
 | 			UPDATE: A pointer to the head of the queue that will be
 | 			attached to the beginning of the head queue.  The 'prependee'
 | 			is made an empty queue.
 | 
 | 	SQ_FIND(head, item, linkField)
 | 			Returns TRUE if it finds 'item' in the queue, otherwise
 | 			returns FALSE.  Does a linear search of the queue to find
 | 			the 'item'.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 
 | 	SQ_CNT(head)
 | 			Returns the number of elements in the queue.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | Singly Linked Circular Queue: CIR
 | 	A singly linked circular queue is a linked list where the head of
 | 	the queue actually points to the tail element which points to the
 | 	head element.  It is a little more costly to manipulate than the
 | 	singly linked queue described above but it only uses one pointer
 | 	for the head so can be used when many instances of queues are needed.
 | 
 | 	Empty Head
 | 	+-------+
 | 	|   0   |
 | 	+-------+
 | 	
 | 	+-------+
 | 	| Head  +-------+
 | 	+-------+       |
 | 				    |
 | 				    v
 | 				+-------+
 | 		+------>|   C   | last item on list
 | 		|		+---+---+
 | 		|		    |
 | 		|		    v
 | 		|		+-------+
 | 		|		|   A   | first item on list (note different order).
 | 		|		+---+---+
 | 		|		    |
 | 		|		    v
 | 		|		+-------+
 | 		+-------+   B   |
 | 				+-------+
 | 
 | 	typedef struct Xyz_s
 | 	{
 | 		int			field_a;
 | 		CIRlink_t	cirLink;
 | 		int			field_b;
 | 	} Xyz_s;
 | 	
 | 	zyzzy()
 | 	{
 | 		CIRhead_t	Head;
 | 		Xyz_s		A, B, C;
 | 		Xyz_s		*a, *b, *c;
 | 		
 | 		CIR_INIT(Head);
 | 	
 | 		CIR_ENQ(Head, &A, cirLink);
 | 		CIR_ENQ(Head, &B, cirLink);
 | 		CIR_ENQ(Head, &C, cirLink);
 | 	
 | 		CIR_DEQ(Head, a, Xyz_s, cirLink);
 | 		CIR_DEQ(Head, b, Xyz_s, cirLink);
 | 		CIR_DEQ(Head, c, Xyz_s, cirLink);
 | 	}
 |
 | Typedefs:
 | 
 | 	CIRlink_t	Use the CIRlink_t typedef to define the link field
 | 				in the structures to be managed as a singly linked
 | 				circular queue.  This field must be initialized to
 | 				zero either by zeroing the whole structure or calling
 | 				NULLIFY with the field.
 | 
 | 	CIRhead_t	Use the CIRhead_t typedef to define the head of the
 | 				singly linked circular queue.  The head  must be
 | 				initialized by either using CIR_INIT or CIR_INIT_ELEMENTS.
 | 
 | Macros:
 | 
 | 	CIR_INIT(head)
 | 			Initialize the head of the queue.
 | 		head
 | 			OUTPUT: A variable of type CIRhead_t initialized to NULL.
 | 
 | 	CIR_INIT_ELEMENTS(head, data, numElements, typeElement, linkField)
 | 			Used to take an array of items and initialize all of them
 | 			and enqueue them in the queue. 
 | 		head
 | 			OUTPUT: A variable of type CIRhead_t used for the
 | 			head of the queue.  Assumed to be uninitialized.
 | 		data
 | 			INPUT: Pointer to the first item in the array. Usually
 | 			the array name.
 | 		numElements
 | 			INPUT: Number of elements in the array.
 | 		typeElement
 | 			INPUT: The type of structure of the elements in the array.
 | 		linkField:
 | 			INPUT: The name of the field to be used for queue link.
 | 		USAGE:
 | 			{
 | 				Xyz_s		Xyz[10];
 | 				CIRhead_t	Head;
 | 
 | 				CIR_INIT_ELEMENTS(Head, Xyz, 10, Xyz_s, link);
 | 			}
 | 
 | 	CIR_EMPTY(head)
 | 			Is True, if queue is empty, otherwise is False.
 | 			Used to test if there are any items left in the queue.
 | 		head
 | 			INPUT: The head of the queue.
 | 		USAGE:
 | 			if (CIR_EMPTY(Head)) { ... }
 | 
 | 	CIR_NOT_EMPTY(head)
 | 			Is False, if queue is empty, otherwise is True.
 | 			Easier to understand then "if (!CIR_EMPTY(head))...".
 | 		head
 | 			INPUT: The head of the queue.
 | 		USAGE:
 | 			if (CIR_NOT_EMPTY(Head)) { ... }
 | 
 | 	CIR_ENQ(head, item, linkField)
 | 			Enqueue an item on to the tail of the queue.
 | 		head
 | 			UPDATE: The head of the queue.  The head will now point
 | 			to 'item' and 'item' will point to what 'head' was pointing
 | 			and the old tail's link field will point to 'item'.
 | 		item
 | 			UPDATE: Pointer to element to be inserted at tail of queue.
 | 			Its link field is updated to point to the head element.
 | 
 | 	CIR_PUSH(head, item, linkField)
 | 			Push an item on to the front of the queue.  If the queue
 | 			is immediately dequeued, this item will come off.  Useful
 | 			preempting others or forcing something to be reused quickly.
 | 		head
 | 			INPUT: The head of the queue.  The tail element that head
 | 			points to will have its link field updated to point to
 | 			'item' and 'item' will point to what was the head element
 | 			of the queue.
 | 		item
 | 			UPDATE: Pointer to element to be inserted at head of queue.
 | 			Its link field is updated to point to the old head element.
 | 
 | 	CIR_DEQ(head, item, type, linkField)
 | 			Dequeue the first item off the queue.  If the queue is
 | 			empty, item is set to NULL, otherwise it has the oldest
 | 			structure in the queue and the head now points to the
 | 			next item in the queue.
 | 		head
 | 			UPDATED: The head of the queue.  The tail element is updated
 | 			to point to the next element of the queue.  If this is the
 | 			last element in the queue, 'head' is set to NULL.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to NULL if the queue is empty, otherwise set to a
 | 			pointer to the structure pointed to by the tail element
 | 			pointed to by 'head'.
 | 
 | 	CIR_DEQ_NO_CHECK(head, item, type, linkField)
 | 			CIR_DEQ_NO_CHECK is just like CIR_DEQ except we know
 | 			something is on the queue having done a CIR_NOT_EMPTY
 | 			or a CIR_PEEK earlier.
 | 		head
 | 			UPDATED: The head of the queue.  The tail element is updated
 | 			to point to the next element of the queue.  If this is the
 | 			last element in the queue, 'head' is set to NULL.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to NULL if the queue is empty, otherwise set to a
 | 			pointer to the structure pointed to by the tail element
 | 			pointed to by 'head'.
 | 
 | 	CIR_PEEK(head, item, type, linkField)
 | 			Sets 'item' to the first element at the head of the queue
 | 			but does not remove it from the queue.  Good for checking
 | 			resources before committing them.  Usually used with
 | 			CIR_DROP.
 | 		head
 | 			INPUT: The head of the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to the first element of the queue which is pointed
 | 			to by tail element which is pointed to by head.  If
 | 			queue is empty, it is set to NULL.
 | 
 | 	CIR_DROP(head, item, linkField)
 | 			Used to drop or remove 'item' from the head of the queue.
 | 			Normally, 'item' is obtained by using CIR_PEEK.
 | 		head
 | 			UPDATE: The head of the queue.  The tail element is
 | 			set pointing to the next element unless the queue
 | 			is now empty in which case the 'head' is set to NULL.
 | 		item
 | 			UPDATE: Pointer to a structure of type 'type'.  'item'
 | 			was obtained by doing a CIR_PEEK.
 | 
 | 	CIR_RMV(head, item, linkField)
 | 			Removes the designated 'item' from the queue by starting
 | 			at the head of the queue and scanning the queue linearly
 | 			until it finds the item and removes it from the queue.
 | 		head
 | 			UPDATE: The head of the queue.  If the 'item' is pointed
 | 			to by the 'head', the head is updated appropriately.
 | 		item
 | 			INPUT: Pointer to a structure of type 'type'.  LinkField
 | 			will be set to NULL, if QUE_NULL is enabled.
 | 
 | 	CIR_APPEND(head, appendee)
 | 			Used to append one queue to another queue.  All the
 | 			elements in the appendee queue are placed at the tail
 | 			of the head queue.
 | 		head
 | 			UPDATE: The head of the queue that the other queue will
 | 			be appended.
 | 		appendee
 | 			UPDATE: The head of the queue that will be attached to
 | 			the end of the head queue.  'appendee' is made an empty
 | 			queue.
 | 
 | 	CIR_PREPEND(head, prependee)
 | 			Used to prepend one queue to another queue.  All the
 | 			elements in the prepend queue are placed at the front
 | 			of the head queue.
 | 		head
 | 			UPDATE: The head of the queue that the other queue will
 | 			be prepended.  Its head pointer is set to the head of
 | 			the 'prependee' queue.
 | 		prependee
 | 			UPDATE: The head of the queue that will be attached to
 | 			the beginning of the head queue.  The 'prependee' is made
 | 			an empty queue.
 | 
 | 	CIR_FIND(head, item, linkField)
 | 			Returns TRUE if it finds 'item' in the queue, otherwise
 | 			returns FALSE.  Does a linear search of the queue to find
 | 			the 'item'.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 
 | 	CIR_CNT(head)
 | 			Returns the number of elements in the queue.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | Doubly Linked Circular Queue: DQ
 | A doubly linked circular queue is the most general of these link list
 | routines and of course it takes the most resources to manipulate and
 | store.  The head of a doubly linked queue appears as a member of the
 | queue.  It has a 'next' and 'previous' pointer and can be traversed
 | in either direction.  The queue is empty when the head points to itself.
 | 
 | 			Empty Head
 | 			+-------+
 | 		+-->| next  +---+
 | 		|	+-------+   |
 | 		+---+ prev  |<--+
 | 			+-------+
 | 			
 | 			  Head
 | 		+-->+-------+<---------------+
 | 		|	| next  +-------+        |
 | 		|	+-------+       |        |
 | 		|	| prev  |       |        |
 | 		|	+---+---+       |        |
 | 		|		|			|        |
 | 		|		|			v        |
 | 		|		|	+-->+-------+    |
 | 		|		|	|	|   A   |    | first item on list
 | 		|		|	|	+-------+    |
 | 		|		|	|	|       +----+
 | 		|		|	|	+---+---+
 | 		|		|	|	    |
 | 		|		|	|	    v
 | 		|		|	|	+-------+<---+
 | 		|		|	|	|   B   |    |
 | 		|		|	|	+-------+    |
 | 		|		|	+---+       |    |
 | 		|		|		+---+---+    |
 | 		|		|		    |        |
 | 		|		|		    v        |
 | 		|		+------>+-------+    |
 | 		+---------------+   C   |    | last item on list
 | 						+-------+    |
 | 						|       +----+
 | 						+-------+
 | 
 | 	typedef struct Xyz_s
 | 	{
 | 		int			field_a;
 | 		DQlink_t	dqLink;
 | 		int			field_b;
 | 	} Xyz_s;
 | 	
 | 	zyzzy()
 | 	{
 | 		DQhead_t	Head;
 | 		Xyz_s		A, B, C;
 | 		Xyz_s		*a, *b, *c;
 | 		
 | 		DQ_INIT( &Head);
 | 	
 | 		DQ_ENQ( &Head, &A, dqLink);
 | 		DQ_ENQ( &Head, &B, dqLink);
 | 		DQ_ENQ( &Head, &C, dqLink);
 | 	
 | 		DQ_DEQ( &Head, a, Xyz_s, dqLink);
 | 		DQ_DEQ( &Head, b, Xyz_s, dqLink);
 | 		DQ_DEQ( &Head, c, Xyz_s, dqLink);
 | 	}
 | 
 | Their biggest advantages are easy removal of an item from the middle
 | of a list (you don't even have to know the head) and simple routines
 | for scanning the list.  For data structures that need to use multiple
 | linked lists, this is the queue of choice because once you have found
 | the element on one list, you can easily remove it from other lists.
 | 
 | Typedefs:
 | 
 | 	DQlink_t	Use the DQlink_t typedef to define the link field
 | 				in the structures to be managed as doubly linked
 | 				queue.  This field must be initialized to zero either
 | 				by zeroing the whole structure or calling NULLIFY
 | 				with the field.
 | 
 | 	DQhead_t	Use the DQhead_t typedef to define the head of the
 | 				doubly linked queue.  The head  must be initialized by
 | 				either using DQ_INIT or DQ_INIT_ELEMENTS.
 | 
 | Macros:
 | 
 | 	DQ_INIT(head)
 | 			Initialize the head of the queue.
 | 		head
 | 			OUTPUT: A pointer to a variable of type DQhead_t.
 | 		USAGE:
 | 			DQ_INIT( &Head);
 | 
 | 	DQ_STATIC_INIT(head)
 | 			Uses compile time initialization to initialize the head.
 | 		head
 | 			OUTPUT: A variable of type DQhead_t.
 | 		USAGE:
 | 			DQhead_t	Head = DQ_STATIC_INIT(Head);
 | 
 | 	DQ_INIT_ELEMENTS(head, data, numElements, typeElement, linkField)
 | 			Used to take an array of items and initialize all of them
 | 			and enqueue them in the queue. 
 | 		head
 | 			OUTPUT: A variable of type DQhead_t used for the
 | 			head of the queue.  Assumed to be uninitialized.
 | 		data
 | 			INPUT: Pointer to the first item in the array. Usually
 | 			the array name.
 | 		numElements
 | 			INPUT: Number of elements in the array.
 | 		typeElement
 | 			INPUT: The type of structure of the elements in the array.
 | 		linkField:
 | 			INPUT: The name of the field to be used for queue link.
 | 		USAGE:
 | 			{
 | 				Xyz_s		Xyz[10];
 | 				DQhead_t	Head;
 | 
 | 				DQ_INIT_ELEMENTS(Head, Xyz, 10, Xyz_s, link);
 | 			}
 | 
 | 	DQ_EMPTY(head)
 | 			Is True, if queue is empty, otherwise is False.
 | 			Used to test if there are any items left in the queue.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		USAGE:
 | 			if (DQ_EMPTY(Head)) { ... }
 | 
 | 	DQ_NOT_EMPTY(head)
 | 			Is False, if queue is empty, otherwise is True.
 | 			Easier to understand then "if (!DQ_EMPTY(head))...".
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		USAGE:
 | 			if (DQ_NOT_EMPTY(Head)) { ... }
 | 
 | 	DQ_IS_MORE_THAN_ONE(head)
 | 			Is True, if more than one item on queue, otherwise is False.
 | 			It is easy to test if there is only 0 or 1 items on a DQ.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		USAGE:
 | 			if (DQ_MORE_THAN_ONE(Head)) { ... }
 |
 | 	DQ_ENQ(head, item, linkField)
 | 			Enqueue an item on to the tail of the queue.
 | 		head
 | 			UPDATE: Pointer to the head of the queue.  The 'previous'
 | 			pointer of the head will point to 'item'.  If the list was
 | 			empty, the 'next' field of 'head' will point to 'item'.
 | 		item
 | 			INPUT: Pointer to the element to be inserted at the
 | 			tail of the queue.  Its 'next' pointer will point to
 | 			the 'head' and its 'previous' pointer will point to
 | 			the old tail.
 | 
 | 	DQ_PUSH(head, item, linkField)
 | 			Push an item on to the front of the queue.  If the queue
 | 			is immediately dequeued, this item will come off.  Useful
 | 			preempting others or forcing something to be reused quickly.
 | 		head
 | 			UPDATE: Pointer to the head of the queue.  The 'next' field
 | 			of 'head' will point to 'item'.
 | 		item
 | 			UPDATE: Pointer to the element to be inserted at the head
 | 			of the queue.  The 'previous' field of 'item' will point
 | 			to the 'head' and the 'next' field will point to the old
 | 			head element of the queue.
 | 
 | 	DQ_DEQ(head, item, type, linkField)
 | 			Dequeue the first item off the queue.  If the queue is
 | 			empty, item is set to NULL, otherwise it has the oldest
 | 			structure in the queue and the head now points to the
 | 			next item in the queue.
 | 		head
 | 			UPDATE: A pointer to the head of the queue.  Updated to
 | 			point to the next item on the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to NULL if the queue is empty, otherwise set to a
 | 			pointer to the structure pointed to by head.
 | 
 | 	DQ_DEQ_NO_CHECK(head, item, type, linkField)
 | 			DQ_DEQ_NO_CHECK is just like DQ_DEQ except we know
 | 			something is on the queue having done a DQ_NOT_EMPTY
 | 			or a DQ_PEEK earlier.
 | 		head
 | 			UPDATE: A pointer to the head of the queue.  Updated to
 | 			point to the next item on the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to a pointer to the structure pointed to by head.
 | 
 | 	DQ_TAKE(head, item, type, linkField)
 | 			Dequeue the last item off the queue (which is normally the
 |			the last item inserted).  If the queue is empty, item is set
 |			to NULL, otherwise it has the newest structure in the queue
 |			and the head's back link now points to the next newest item
 |			in the queue.
 | 		head
 | 			UPDATE: A pointer to the head of the queue.  Its back link
 |			is updated to point to the next previous item on the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to NULL if the queue is empty, otherwise set to a
 | 			pointer to the structure pointed to by head.
 |
 | 	DQ_PEEK(head, item, type, linkField)
 | 			Sets 'item' to the first element at the head of the queue
 | 			but does not remove it from the queue.  Good for checking
 | 			resources before committing them.  Usually used with
 | 			DQ_DROP.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to a pointer to the structure pointed to by head.  If
 | 			queue is empty, it is set to NULL.
 | 
 | 	DQ_PEEK_END(head, item, type, linkField)
 | 			Sets 'item' to the LAST element at the head of the queue
 | 			but does not remove it from the queue.  Good for checking
 | 			resources before committing them.  
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		item
 | 			OUTPUT: Pointer to a structure of type 'type'.  Set
 | 			to a pointer to the structure pointed to by head.  If
 | 			queue is empty, it is set to NULL.
 |
 | 	DQ_DROP(head, item, linkField)
 | 			Used to drop or remove 'item' from the head of the queue.
 | 			Normally, 'item' is obtained by using DQ_PEEK.
 | 		head
 | 			UPDATE: A pointer to the head of the queue.  Updated to
 | 			point to the next item on the queue.
 | 		item
 | 			UPDATE: Pointer to a structure of type 'type'.  'item'
 | 			was obtained by doing a DQ_PEEK.
 | 
 | 	DQ_RMV(item, linkField)
 | 			Removes the designated 'item' from the queue.  Because
 | 			we have a 'next' and 'previous' pointers, this operation
 | 			is done very quickly.  Unlike SQ_RMV and CIR_RMV, we don't
 | 			even have to know the head of the queue.
 | 		item
 | 			INPUT: Pointer to a structure of type 'type'.  'next' field
 | 			will be set to NULL, if QUE_NULL is enabled.  Its successor
 | 			and predecessor elements in the queue will now point
 | 			to each others
 | 
 | 	DQ_APPEND(head, appendee)
 | 			Used to append one queue to another queue.  All the
 | 			elements in the appendee queue are placed at the tail
 | 			of the head queue.
 | 		head
 | 			UPDATE: A pointer to the head of the queue that the other
 | 			queue will be appended.  Its 'previous' pointer is set to the
 | 			tail of the 'appendee' queue.
 | 		appendee
 | 			UPDATE: A pointer to the head of the queue that will be
 | 			attached to the end of the head queue.  'appendee' is
 | 			made an empty queue.
 | 
 | 	DQ_PREPEND(head, prependee)
 | 			Used to prepend one queue to another queue.  All the
 | 			elements in the prepend queue are placed at the front
 | 			of the head queue.
 | 		head
 | 			UPDATE: A pointer to the head of the queue that the other
 | 			queue will be prepended.  Its head pointer is set to the
 | 			head of the 'prependee' queue.
 | 		prependee
 | 			UPDATE: A pointer to the head of the queue that will be
 | 			attached to the beginning of the head queue.  The 'prependee'
 | 			is made an empty queue.
 | 
 | 	DQ_FOREACH(head, item, type, linkField)
 | 			Used to scan through the queue to process it or search for
 | 			a particular element in the queue.  It is really a 'for' loop
 | 			with all the pieces setup for scanning the queue.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		item
 | 			OUTPUT: A pointer to the current item in the queue.
 | 		USAGE:
 | 			{
 | 				extern DQhead_t	Head;
 | 				Xyz_s			*x;
 | 
 | 				DQ_FOREACH( &Head, x, Xyz_s, dqLink)
 | 				{
 | 					if (x->field_a == 42) return x;
 | 				}
 | 			}
 | 
 | 	DQ_ISHEADNEXT(head, item, type, linkField)
 |			Can be used to check if you are at the last item in the
 |			list.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		item
 | 			OUTPUT: A pointer to the current item in the queue.
 | 		USAGE:
 | 			{
 | 				extern DQhead_t	Head;
 | 				Xyz_s			*x;
 | 
 | 				DQ_FOREACH( &Head, x, Xyz_s, dqLink)
 | 				{
 | 					if (x->field_a == 42) return x;
 |
 |					if (DQ_ISHEADNEXT( &head, x, Xyz_s, dqLink))
 |					{
 |						processLastElement(x);
 |					}
 | 				}
 | 			}
 | 
 | 	DQ_NEXT(head, item, type, linkField)
 | 			Get the next 'item' in the queue.  If you reach the head,
 | 			'item' is set to NULL.  Useful for more generic scanning.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		item
 | 			INPUT: A pointer to the current item in the queue.
 | 		USAGE:
 | 			{
 | 				extern DQhead_t	Head;
 | 				Xyz_s			*x;
 | 
 | 				x = STRUCT( &Head, Xyz_s, dqLink);
 | 				x = DQ_NEXT( &Head, x, Xyz_s, dqLink);
 | 				while (x != NULL)
 | 				{
 | 					if (x->field_a == 42) return x;
 | 					x = DQ_NEXT( &Head, x, Xyz_s, dqLink);
 | 				}
 | 				return NULL;
 | 			}
 | 
 | 	DQ_PREV(head, item, type, linkField)
 | 			Get the previous 'item' in the queue.  If you reach the head,
 | 			'item' is set to NULL.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 		item
 | 			INPUT: A pointer to the current item in the queue.
 | 		USAGE:
 | 			{
 | 				extern DQhead_t	Head;
 | 				Xyz_s			*x;
 | 
 | 				x = STRUCT( &Head, Xyz_s, dqLink);
 | 				x = DQ_PREV( &Head, x, Xyz_s, dqLink);
 | 				for(;;)
 | 				{
 | 					if (x->field_a == 42) return x;
 | 					x = DQ_PREV( &Head, x, Xyz_s, dqLink);
 | 				}
 | 			}
 | 
 | 	DQ_FIND(head, item, linkField)
 | 			Returns TRUE if it finds 'item' in the queue, otherwise
 | 			returns FALSE.  Does a linear search of the queue to find
 | 			the 'item'.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 | 
 | 	DQ_CNT(head)
 | 			Returns the number of elements in the queue.
 | 		head
 | 			INPUT: A pointer to the head of the queue.
 +--------------------------------------------------------------------------*/

#define QUE_NULL	ENABLE	/* NULL out next pointer after dequeuing
							 * This will ALWAYS be enabled for PSS so that
							 * we can easily tell if something is a member
							 * of linked list.
							 */




/*
 * Generic link definition. Should only be used
 * by generic functions.  Never used by itself.
 */
struct Link_s
{
	struct Link_s		*next;
};

typedef struct Link_s	*Link_t;

/*
 * Stack link and head
 */
struct STKlink_s
{
	struct STKlink_s	*next;
};

typedef struct STKlink_s	*STKlink_t;
typedef struct STKlink_s	*STKtop_t;

/*
 * Singly Linked Queue link and header
 */
struct SQlink_s
{
	struct SQlink_s		*next;
};

typedef struct SQlink_s	*SQlink_t;

typedef struct SQhead_t
{
	SQlink_t	next;
	SQlink_t	last;
} SQhead_t;

/*
 * Circular Linked Queue link and header
 */
struct CIRlink_s
{
	struct CIRlink_s	*next;
};

typedef struct CIRlink_s	*CIRlink_t;
typedef struct CIRlink_s	*CIRhead_t;

/*
 * Doubly Linked Queue link and header
 */
typedef struct DQlink_t
{
	struct DQlink_t		*next;
	struct DQlink_t		*prev;
} DQlink_t;

typedef struct DQlink_t	DQhead_t;

/*
 * Ordered set link and header
 */
typedef struct SETlink_t
{
	struct SETlink_t	*next;
	struct SETlink_t	*prev;
	SQUAD				setNum;
} SETlink_t;

typedef struct SETlink_t	SEThead_t;

/*
 * Generic macros:
 */
#define FRONTADDR(item, type, linkField)	\
	((item) = ((type *)(((ADDR)(item)) - offsetof(type, linkField))))

#define STRUCT(item, type, linkField)	\
	((type *)(((ADDR)(item)) - offsetof(type, linkField)))

#define NEXT(item)	(((Link_t)(item))->next)

#define ONEXT(item, type, linkField)							\
	((type *)((ADDR)(((Link_t)&((item)->linkField))->next)		\
	- offsetof(type, linkField)))

#define PREV(item)	((item)->prev)

#define OPREV(item, type, linkField)							\
	((type *)((ADDR)((item)->linkField.prev)					\
	- offsetof(type, linkField)))

/*
 * To allow some sanity checks on queue and stack operations, we null out
 * the link field.  This lets us check if an item is already on a list
 * before putting into a list.  NULLIFY is used to set the link field to
 * NULL before is used the first time.
 */

#if QUE_NULL IS_ENABLED

#	define NULLIFY(linkField)	(NEXT(linkField) = NULL)
#	define QMEMBER(linkField)	(NEXT(linkField) != NULL)

#	if QUE_CHECK IS_ENABLED

		extern int LBQ_QAssertError(char *, Link_t);

#		define IS_NULL(linkField)				\
			((NEXT(linkField) == NULL) ||		\
			LBQ_QAssertError(WHERE, (Link_t)linkField))
#	else
#		define IS_NULL(linkField)	(TRUE)
#	endif

#else
	/*
	 * NOTE: QMEMBER is not defined in this case!
	 */
#	define NULLIFY(linkField)	((void)0)
#	define IS_NULL(linkField)	(TRUE)

#endif

/****************************************************************************/
/* function prototypes for all STK functions */
extern void LBQ_STKpush(STKtop_t *, STKlink_t);
extern ADDR LBQ_STKpop(STKtop_t *top, NINT offset);
extern ADDR LBQ_STKpopNoCheck(STKtop_t *top, NINT offset);
extern void LBQ_STKdrop(STKtop_t *);
extern int	LBQ_STKrmv(STKtop_t *, STKlink_t);
extern void LBQ_STKinitElements(STKtop_t *stacktop, ADDR start,NINT num, NINT size);
							

#define STK_INIT(top)		((top) = NULL)

#define STK_STATIC_INIT()	{ NULL }

#define STK_EMPTY(top)		((top) == NULL)

#define STK_NOT_EMPTY(top)	((top) != NULL)

#if QUE_MACRO IS_ENABLED
#define STK_PUSH(top, item, linkField)										\
{																			\
	STKlink_t	_item = (STKlink_t)&((item)->linkField);					\
																			\
	if (IS_NULL(_item))														\
	{																		\
		_item->next = (top);												\
		(top) = _item;														\
	}																		\
}
#else

#define STK_PUSH(top, item, linkField)										\
{																			\
	LBQ_STKpush( &top, (STKlink_t)&((item)->linkField));						\
}
#endif

#if QUE_MACRO IS_ENABLED
#define STK_POP(top, item, type, linkField)									\
{																			\
	STKlink_t	_next;														\
																			\
	_next = top;															\
	if (_next != NULL)														\
	{																		\
		top = _next->next;													\
		NULLIFY(_next);														\
		item = (type *)(((ADDR)_next) - offsetof(type, linkField));			\
	}																		\
	else																	\
	{																		\
		item = NULL;														\
	}																		\
}
#else

#define STK_POP(top, item, type, linkField)									\
{																			\
	item = (type *)LBQ_STKpop( &top, offsetof(type, linkField));			\
}
#endif

#if QUE_MACRO IS_ENABLED
#define STK_POP_NO_CHECK(top, item, type, linkField)						\
{																			\
	STKlink_t	_next;														\
																			\
	_next = top;															\
	top = _next->next;														\
	NULLIFY(_next);															\
	item = (type *)(((ADDR)_next) - offsetof(type, linkField));				\
}
#else

#define STK_POP_NO_CHECK(top, item, type, linkField)						\
{																			\
	item = (type *)LBQ_STKpopNoCheck( &top, offsetof(type, linkField));		\
}
#endif

#define STK_PEEK(top, item, type, linkField)								\
{																			\
	if (STK_EMPTY(top))														\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		item = (type *)(((ADDR)top) - offsetof(type, linkField));			\
	}																		\
}

#if QUE_MACRO IS_ENABLED
#define STK_DROP(top, item, linkField)										\
{																			\
	STKlink_t	_next = (STKlink_t)&((item)->linkField);					\
																			\
	ASSERT((top)->next == _next);											\
	top = _next->next;														\
	NULLIFY(_next);															\
}
#else

#define STK_DROP(top, item, linkField)										\
{																			\
	LBQ_STKdrop( &top);														\
}
#endif

#define STK_RMV(top, item, linkField)										\
	(LBQ_STKrmv( &(top), (STKlink_t)&((item)->linkField)))

#define STK_FOREACH(top, item, type, linkField)								\
	for (item =  (type *)(((ADDR)(top)) - offsetof(type, linkField));		\
		 (ADDR)item != (0 - offsetof(type, linkField));						\
		 item =  ONEXT(item, type, linkField)								\
	)

#define STK_INIT_ELEMENTS(top, data, numElements, typeElement, linkField)	\
{																			\
	LBQ_STKinitElements( &top, ((ADDR)data) + offsetof(typeElement, linkField),	\
							numElements, sizeof(typeElement));				\
}

/****************************************************************************/
/* function prototypes for all SQ functions */
extern void LBQ_SQenq(SQhead_t *, SQlink_t);
extern void LBQ_SQpush(SQhead_t *, SQlink_t);
extern ADDR LBQ_SQdeq(SQhead_t *sqhead, NINT offset);
extern ADDR LBQ_SQdeqNoCheck(SQhead_t *sqhead, NINT offset);
extern void LBQ_SQdrop(SQhead_t *);
extern int	LBQ_SQrmv(SQhead_t *, SQlink_t);
extern void LBQ_SQappend(SQhead_t *, SQhead_t *);
extern void LBQ_SQprepend(SQhead_t *, SQhead_t *);
extern void LBQ_SQinitElements(SQhead_t *sqhead, ADDR start, NINT num, NINT size);
extern int LBQ_SQfind(SQhead_t *head, SQlink_t item);
extern int LBQ_SQcnt(SQhead_t *head);


#define SQ_CNT(head)		LBQ_SQcnt(head)

#define SQ_INIT(head)		((head)->last = ((SQlink_t)head))

#define SQ_EMPTY(head)		((head)->last == ((SQlink_t)head))

#define SQ_NOT_EMPTY(head)	((head)->last != ((SQlink_t)head))

#if QUE_MACRO IS_ENABLED
#define SQ_ENQ(head, item, linkField)										\
{																			\
	SQhead_t	*_head = head;												\
	SQlink_t	_item = (SQlink_t)&((item)->linkField);						\
																			\
	if (IS_NULL(_item))														\
	{																		\
		_head->last->next = _item;											\
		_head->last       = _item;											\
	}																		\
}
#else

#define SQ_ENQ(head, item, linkField)										\
{																			\
	LBQ_SQenq(head, (SQlink_t)&((item)->linkField));							\
}
#endif

#if QUE_MACRO IS_ENABLED
#define SQ_PUSH(head, item, linkField)										\
{																			\
	SQhead_t	*_head = head;												\
	SQlink_t	_item = (SQlink_t)&((item)->linkField);						\
																			\
	if (IS_NULL(_item))														\
	{																		\
		if (SQ_EMPTY(_head))												\
		{																	\
			_head->last = _item;											\
		}																	\
		else																\
		{																	\
			_item->next = _head->next;										\
		}																	\
		_head->next = _item;												\
	}																		\
}
#else

#define SQ_PUSH(head, item, linkField)										\
{																			\
	LBQ_SQpush(head, (SQlink_t)&((item)->linkField));						\
}
#endif

#if QUE_MACRO IS_ENABLED
#define SQ_DEQ(head, item, type, linkField)									\
{																			\
	SQhead_t	*_head = head;												\
	SQlink_t	_item;														\
	SQlink_t	_last;														\
																			\
	_last = _head->last;													\
	if (_last == (SQlink_t)_head)											\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = _head->next;												\
		if (_last == _item)													\
		{																	\
			_head->last = (SQlink_t)_head;									\
		}																	\
		else																\
		{																	\
			_head->next = _item->next;										\
		}																	\
		NULLIFY(_item);														\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}
#else

#define SQ_DEQ(head, item, type, linkField)									\
{																			\
	item = (type *)LBQ_SQdeq(head, offsetof(type, linkField));				\
}
#endif

#if QUE_MACRO IS_ENABLED
#define SQ_DEQ_NO_CHECK(head, item, type, linkField)						\
{																			\
	SQhead_t	*_head = head;												\
	SQlink_t		_item;													\
	SQlink_t		_last;													\
																			\
	_last = _head->last;													\
	_item = _head->next;													\
	if (_last == _item)														\
	{																		\
		_head->last = (SQlink_t)_head;										\
	}																		\
	else																	\
	{																		\
		_head->next = _item->next;											\
	}																		\
	NULLIFY(_item);															\
	item = (type *)(((ADDR)_item) - offsetof(type, linkField));				\
}
#else

#define SQ_DEQ_NO_CHECK(head, item, type, linkField)						\
{																			\
	item = (type *)LBQ_SQdeqNoCheck(head, offsetof(type, linkField));		\
}
#endif

#define SQ_PEEK(head, item, type, linkField)								\
{																			\
	if (SQ_EMPTY(head))														\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		item = (type *)(((ADDR)((head)->next)) - offsetof(type, linkField));\
	}																		\
}

#define SQ_PEEK_LAST(head, item, type, linkField)							\
{																			\
	if (SQ_EMPTY(head))														\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		item = (type *)(((ADDR)((head)->last)) - offsetof(type, linkField));\
	}																		\
}

#if QUE_MACRO IS_ENABLED
#define SQ_DROP(head, item, linkField)										\
{																			\
	SQhead_t	*_head = (SQhead_t *)head;									\
	SQlink_t	_item = (SQlink_t)&((item)->linkField);						\
																			\
	ASSERT(_head->next == _item);											\
	if (_head->last == _item)												\
	{																		\
		_head->last = (SQlink_t)_head;										\
	}																		\
	else																	\
	{																		\
		_head->next = _item->next;											\
	}																		\
	NULLIFY(_item);															\
}
#else

#define SQ_DROP(head, item, linkField)										\
{																			\
	LBQ_SQdrop(head);														\
}
#endif

#define SQ_RMV(head, item, linkField)										\
	(LBQ_SQrmv(head, (SQlink_t)&((item)->linkField)))

#if QUE_MACRO IS_ENABLED
#define SQ_APPEND(head, appendee)											\
{																			\
	SQhead_t	*_head = head;												\
	SQhead_t	*_appendee = appendee;										\
																			\
	if (SQ_NOT_EMPTY(_appendee))											\
	{																		\
		_head->last->next = _appendee->next;								\
		_head->last       = _appendee->last;								\
		SQ_INIT(_appendee);													\
	}																		\
}
#else

#define SQ_APPEND(head, appendee)											\
{																			\
	LBQ_SQappend(head, appendee);											\
}
#endif

#if QUE_MACRO IS_ENABLED
#define SQ_PREPEND(head, prependee)											\
{																			\
	SQhead_t	*_head = head;												\
	SQhead_t	*_prependee = prependee;									\
																			\
	if (SQ_NOT_EMPTY(_prependee))											\
	{																		\
		if (SQ_EMPTY(_head))												\
		{																	\
			_head->last = _prependee->last;									\
		}																	\
		else																\
		{																	\
			_prependee->last->next = _head->next;							\
		}																	\
		_head->next = _prependee->next;										\
		SQ_INIT(_prependee);												\
	}																		\
}
#else

#define SQ_PREPEND(head, prependee)											\
{																			\
	LBQ_SQprepend(head, prependee);											\
}
#endif

#define SQ_INIT_ELEMENTS(head, data, numElements, typeElement, linkField)	\
{																			\
	LBQ_SQinitElements( &head, ((ADDR)data) + offsetof(typeElement, linkField),	\
							numElements, sizeof(typeElement));				\
}

#define SQ_FIND(head, item, linkField)										\
				LBQ_SQfind(head, (SQlink_t)&((item)->linkField))


/****************************************************************************/
/* function prototypes for all CIR functions */
extern void LBQ_CIRenq(CIRhead_t *, CIRlink_t);
extern void LBQ_CIRpush(CIRhead_t *, CIRlink_t);
extern ADDR LBQ_CIRdeq(CIRhead_t *cirhead, NINT offset);
extern ADDR LBQ_CIRdeqNoCheck(CIRhead_t *cirhead, NINT offset);
extern void LBQ_CIRdrop(CIRhead_t *);
extern int LBQ_CIRrmv(CIRhead_t *, CIRlink_t);
extern void LBQ_CIRappend(CIRhead_t *, CIRhead_t *);
extern void LBQ_CIRprepend(CIRhead_t *, CIRhead_t *);
extern void LBQ_CIRinitElements(CIRhead_t *cirhead, ADDR start,NINT num, NINT size);
extern int LBQ_CIRfind(CIRhead_t head, CIRlink_t item);
extern int LBQ_CIRcnt(CIRhead_t head);


#define CIR_CNT(head)		LBQ_CIRcnt(head)

#define CIR_INIT(head)		((head) = NULL)

#define CIR_EMPTY(head)		((head) == NULL)

#define CIR_NOT_EMPTY(head)	((head) != NULL)

#if QUE_MACRO IS_ENABLED
#define CIR_ENQ(head, item, linkField)										\
{																			\
	CIRlink_t	_item = (CIRlink_t)&((item)->linkField);					\
																			\
	if (IS_NULL(_item))														\
	{																		\
		if (CIR_EMPTY(head))												\
		{																	\
			_item->next = _item;											\
		}																	\
		else																\
		{																	\
			_item->next = (head)->next;										\
			(head)->next = _item;											\
		}																	\
		(head) = (CIRlink_t)_item;											\
	}																		\
}
#else

#define CIR_ENQ(head, item, linkField)										\
{																			\
	LBQ_CIRenq( &head, (CIRlink_t)&((item)->linkField));						\
}
#endif

#if QUE_MACRO IS_ENABLED
#define CIR_PUSH(head, item, linkField)										\
{																			\
	CIRlink_t	_item = (CIRlink_t)&((item)->linkField);					\
																			\
	if (IS_NULL(_item))														\
	{																		\
		if (CIR_EMPTY(head))												\
		{																	\
			_item->next = _item;											\
			(head) = (CIRlink_t)item;										\
		}																	\
		else																\
		{																	\
			_item->next = (head)->next;										\
			(head)->next = _item;											\
		}																	\
	}																		\
}
#else

#define CIR_PUSH(head, item, linkField)										\
{																			\
	LBQ_CIRpush( &head, (CIRlink_t)&((item)->linkField));					\
}
#endif

#if QUE_MACRO IS_ENABLED
#define CIR_DEQ(head, item, type, linkField)								\
{																			\
	CIRlink_t	_item;														\
																			\
	if (CIR_EMPTY(head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = (head)->next;												\
		if (_item == (head))												\
		{																	\
			(head) = NULL;													\
		}																	\
		else																\
		{																	\
			(head)->next = _item->next;										\
		}																	\
		NULLIFY(_item);														\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}
#else

#define CIR_DEQ(head, item, type, linkField)								\
{																			\
	item = (type *)LBQ_CIRdeq( &head, offsetof(type, linkField));			\
}
#endif

#if QUE_MACRO IS_ENABLED
#define CIR_DEQ_NO_CHECK(head, item, type, linkField)						\
{																			\
	CIRlink_t	_item;														\
																			\
	_item = (head)->next;													\
	if (_item == (head))													\
	{																		\
		(head) = NULL;														\
	}																		\
	else																	\
	{																		\
		(head)->next = _item->next;											\
	}																		\
	NULLIFY(_item);															\
	item = (type *)(((ADDR)_item) - offsetof(type, linkField));				\
}
#else

#define CIR_DEQ_NO_CHECK(head, item, type, linkField)						\
{																			\
	item = (type *)LBQ_CIRdeqNoCheck( &head, offsetof(type, linkField));		\
}
#endif

#define CIR_PEEK(head, item, type, linkField)								\
{																			\
	if (CIR_EMPTY(head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		item = (type *)(((ADDR)((head)->next)) - offsetof(type, linkField));\
	}																		\
}

#if QUE_MACRO IS_ENABLED
#define CIR_DROP(head, item, linkField)										\
{																			\
	CIRlink_t	_item = (CIRlink_t)&((item)->linkField);					\
																			\
	ASSERT((head)->next == _item);											\
	if (_item == (head))													\
	{																		\
		(head) = NULL;														\
	}																		\
	else																	\
	{																		\
		(head)->next = _item->next;											\
	}																		\
	NULLIFY(_item);															\
}
#else

#define CIR_DROP(head, item, linkField)										\
{																			\
	LBQ_CIRdrop( &head);														\
}
#endif

#define CIR_RMV(head, item, linkField)										\
	(LBQ_CIRrmv( &(head), (CIRlink_t)&((item)->linkField)))

#if QUE_MACRO IS_ENABLED
#define CIR_APPEND(head, appendee)											\
{																			\
	CIRlink_t	temp;														\
																			\
	if (CIR_NOT_EMPTY(appendee))											\
	{																		\
		if (CIR_NOT_EMPTY(head))											\
		{																	\
			temp = (appendee)->next;										\
			(appendee)->next = (head)->next;								\
			(head)->next = temp;											\
		}																	\
		(head) = (appendee);												\
		CIR_INIT(appendee);													\
	}																		\
}
#else

#define CIR_APPEND(head, appendee)											\
{																			\
	LBQ_CIRappend( &head, &appendee);										\
}
#endif

#if QUE_MACRO IS_ENABLED
#define CIR_PREPEND(head, prependee)										\
{																			\
	CIRlink_t	temp;														\
																			\
	if (CIR_NOT_EMPTY(prependee))											\
	{																		\
		if (CIR_NOT_EMPTY(head))											\
		{																	\
			temp = (prependee)->next;										\
			(prependee)->next = (head)->next;								\
			(head)->next = temp;											\
		}																	\
		else																\
		{																	\
			(head) = (prependee);											\
		}																	\
		CIR_INIT(prependee);												\
	}																		\
}
#else

#define CIR_PREPEND(head, prependee)										\
{																			\
	LBQ_CIRprepend( &head, &prependee);										\
}
#endif

#define CIR_INIT_ELEMENTS(head, data, numElements, typeElement, linkField)	\
{																			\
	LBQ_CIRinitElements( &head, ((ADDR)data) + offsetof(typeElement, linkField),\
							numElements, sizeof(typeElement));				\
}

#define CIR_FIND(head, item, linkField)										\
				LBQ_CIRfind(head, (CIRlink_t)&((item)->linkField))



/****************************************************************************/
/* function prototypes for all DQ functions */
extern BOOL	DoDQAudit;

extern void LBQ_DQenq(DQhead_t *, DQlink_t *);
extern void LBQ_DQpush(DQhead_t *, DQlink_t *);
extern ADDR LBQ_DQdeq(DQhead_t *dqhead, NINT offset);
extern ADDR LBQ_DQdeqNoCheck(DQhead_t *dqhead, NINT offset);
extern ADDR LBQ_DQtake(DQhead_t *dqhead, NINT offset);
extern void LBQ_DQdrop(DQhead_t *);
extern void LBQ_DQrmv(DQlink_t *);
extern void LBQ_DQappend(DQhead_t *, DQhead_t *);
extern void LBQ_DQprepend(DQhead_t *, DQhead_t *);
extern void LBQ_DQinitElements(DQhead_t *dqhead, ADDR start,NINT num, NINT size);
extern int LBQ_DQfind(DQhead_t *head, DQlink_t *item);
extern int LBQ_DQcnt(DQhead_t *head);
extern int LBQ_DQaudit(DQhead_t *head);


#define DQ_CNT(head)			LBQ_DQcnt(head)

#define DQ_INIT(head)			((head)->next = (head)->prev = (head))

#define DQ_STATIC_INIT(head)	{ &(head), &(head) }

#define DQ_EMPTY(head)			((head)->next == head)

#define DQ_NOT_EMPTY(head)		((head)->next != head)

#define DQ_IS_MORE_THAN_ONE(head)	((head)->next != (head)->prev)

#if QUE_MACRO IS_ENABLED
#define DQ_ENQ(head, item, linkField)										\
{																			\
	DQhead_t	*_head = head;												\
	DQlink_t	*_item = &((item)->linkField);								\
																			\
	if (IS_NULL(_item))														\
	{																		\
		_item->prev = _head->prev;											\
		_item->next = _head;												\
		_head->prev->next = _item;											\
		_head->prev = _item;												\
	}																		\
}
#else

#define DQ_ENQ(head, item, linkField)										\
{																			\
	LBQ_DQenq(head, &((item)->linkField));									\
}
#endif

#if QUE_MACRO IS_ENABLED
#define DQ_PUSH(head, item, linkField)										\
{																			\
	DQhead_t	*_head = head;												\
	DQlink_t	*_item = &((item)->linkField);								\
																			\
	if (IS_NULL(_item))														\
	{																		\
		_item->next = _head->next;											\
		_item->prev = _head;												\
		_head->next->prev = _item;											\
		_head->next = _item;												\
	}																		\
}
#else

#define DQ_PUSH(head, item, linkField)										\
{																			\
	LBQ_DQpush(head, &((item)->linkField));									\
}
#endif

#if QUE_MACRO IS_ENABLED
#define DQ_DEQ(head, item, type, linkField)									\
{																			\
	DQhead_t	*_head = head;												\
	DQlink_t	*_item;														\
																			\
	if (DQ_EMPTY(_head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = _head->next;												\
		_head->next = _item->next;											\
		_head->next->prev = _head;											\
		NULLIFY(_item);														\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}
#else

#define DQ_DEQ(head, item, type, linkField)									\
{																			\
	item = (type *)LBQ_DQdeq(head, offsetof(type, linkField));				\
}
#endif

#if QUE_MACRO IS_ENABLED
#define DQ_DEQ_NO_CHECK(head, item, type, linkField)						\
{																			\
	DQhead_t	*_head = head;												\
	DQlink_t	*_item;														\
																			\
	_item = _head->next;													\
	_head->next = _item->next;												\
	_head->next->prev = _head;												\
	NULLIFY(_item);															\
	item = (type *)(((ADDR)_item) - offsetof(type, linkField));				\
}
#else

#define DQ_DEQ_NO_CHECK(head, item, type, linkField)						\
{																			\
	item = (type *)LBQ_DQdeqNoCheck(head, offsetof(type, linkField));		\
}
#endif

#if QUE_MACRO IS_ENABLED
#define DQ_TAKE(head, item, type, linkField)								\
{																			\
	DQhead_t	*_head = head;												\
	DQlink_t	*_item;														\
																			\
	if (DQ_EMPTY(_head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = _head->prev;												\
		_head->prev = _item->prev;											\
		_head->prev->next = _head;											\
		NULLIFY(_item);														\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}
#else

#define DQ_TAKE(head, item, type, linkField)								\
{																			\
	item = (type *)LBQ_DQtake(head, offsetof(type, linkField));				\
}
#endif

#define DQ_PEEK(head, item, type, linkField)								\
{																			\
	DQhead_t	*_head = head;												\
	DQlink_t	*_item;														\
																			\
	if (DQ_EMPTY(_head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = _head->next;												\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}

#define DQ_PEEK_END(head, item, type, linkField)							\
{																			\
	DQhead_t	*_head = head;												\
	DQlink_t	*_item;														\
																			\
	if (DQ_EMPTY(_head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = _head->prev;												\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}

#if QUE_MACRO IS_ENABLED
#define DQ_DROP(head, item, linkField)										\
{																			\
	DQhead_t	*_head = head;												\
	DQlink_t	*_item = &((item)->linkField);								\
																			\
	_head->next = _item->next;												\
	_head->next->prev = _head;												\
	NULLIFY(_item);															\
}
#else

#define DQ_DROP(head, item, linkField)										\
{																			\
	LBQ_DQdrop(head);														\
}
#endif

#if QUE_MACRO IS_ENABLED
#define DQ_RMV(item, linkField)												\
{																			\
	DQlink_t		*_item = &((item)->linkField);							\
																			\
	_item->next->prev = _item->prev;										\
	_item->prev->next = _item->next;										\
	NULLIFY(_item);															\
}
#else

#define DQ_RMV(item, linkField)												\
{																			\
	LBQ_DQrmv(&((item)->linkField));										\
}
#endif

#if QUE_MACRO IS_ENABLED
#define DQ_APPEND(head, appendee)											\
{																			\
	DQhead_t	*_head = (DQhead_t *)head;									\
	DQhead_t	*_appendee = (DQhead_t *)appendee;							\
																			\
	if (DQ_NOT_EMPTY(_appendee))											\
	{																		\
		_appendee->prev->next = _head;										\
		_appendee->next->prev = _head->prev;								\
		_head->prev->next = _appendee->next;								\
		_head->prev = _appendee->prev;										\
		DQ_INIT(_appendee);													\
	}																		\
}
#else

#define DQ_APPEND(head, appendee)											\
{																			\
	LBQ_DQappend(head, appendee);											\
}
#endif

#if QUE_MACRO IS_ENABLED
#define DQ_PREPEND(head, prependee)											\
{																			\
	DQhead_t	*_head = (DQhead_t *)head;									\
	DQhead_t	*_prependee = (DQhead_t *)prependee;						\
																			\
	if (DQ_NOT_EMPTY(_prependee))											\
	{																		\
		_prependee->prev->next = _head->next;								\
		_head->next->prev = _prependee->prev;								\
		_prependee->next->prev = _head;										\
		_head->next = _prependee->next;										\
		DQ_INIT(_prependee);												\
	}																		\
}
#else

#define DQ_PREPEND(head, prependee)											\
{																			\
	LBQ_DQprepend(head, prependee);											\
}
#endif

#define DQ_FOREACH(head, item, type, linkField)								\
	for (item =  (type *)(((ADDR)((head)->next)) - offsetof(type, linkField));\
		 item != (type *)(((ADDR)head) - offsetof(type, linkField));		\
		 item =  ONEXT(item, type, linkField)								\
	)

#define DQ_ISHEADNEXT(head, item, type, linkField)							\
	((type *)((((item)->linkField.next) == (DQlink_t *)(head))))

#define DQ_NEXT(head, item, type, linkField)								\
	((type *)((((item)->linkField.next) == (DQlink_t *)(head))				\
		? NULL																\
		: ((ADDR)((item)->linkField.next) - offsetof(type, linkField))))

#define DQ_PREV(head, item, type, linkField)								\
	((type *)((((item)->linkField.prev) == (DQlink_t *)(head))				\
		? NULL																\
		: ((ADDR)((item)->linkField.prev) - offsetof(type, linkField))))


#define DQ_INIT_ELEMENTS(head, data, numElements, typeElement, linkField)	\
{																			\
	LBQ_DQinitElements( &head, ((ADDR)data) + offsetof(typeElement, linkField),	\
							numElements, sizeof(typeElement));				\
}


#define DQ_FIND(head, item, linkField)										\
				LBQ_DQfind(head, &((item)->linkField))

#if QUE_MACRO IS_ENABLED
#define DQ_AUDIT(head)	((void)0);
#else
#define DQ_AUDIT(head)	((void)(DoDQAudit && LBQ_DQaudit(head)))
#endif

/****************************************************************************/
/* function prototypes for all SET functions */



/*
|	SET_FOREACHBLOCKING() & SET_FOREACHBLOCKINGEND()
|		Use in places where the link list may have a item removed because
|	your code blocks (another thread does removal) or itself removes items
|	from the list link.
|		Same as SET_FOREACH but assumes that body of the FOR statement
|	has blocked (SET_FOREACH assumes that body does not block). In addition,
|	SET_FOREACHBLOCKING goes through the list using the NEXT link (oldest
|	to newest).  You MUST end the for loop with the macro SET_FOREACHBLOCKINGEND
|	(this macro resets the current item to the HEAD just in case the list
|	changed on us).  YOU MUST NOT USE CONTINUE statements in the body
|	of your for loop.  Instead do a goto to the line above your
|	SET_FOREACHBLOCKINGEND macro.
|		SET_FOREACHBLOCKING is much slower than SET_FOREACH because the
|	link list must be scanned from the HEAD each time.  Generally, items
|	on the list link need a 'use count' on them so that the routine
|	that removes/frees the item is aware that an item is in use.
|
|	Use SETs when you have link lists with items that may be removed on
|	you while a thread is blocked.  Another solution is a latch that
|	protects the link list.  The viability of this depends on how long
|	you block and deadlock concerns.
|
|	See Also
|		SET_APPLY - Neat way to 'apply' a single function to a SET.
|		SET_FOREACH - For loop for code that does not block.
|
*/
#define SET_FOREACHBLOCKING(head, item, type, linkField)					\
	{																		\
		SETlink_t		_dummySet;											\
		_dummySet.setNum = (head)->next->setNum - 1; 						\
		SET_FOREACH(head, item, type, linkField)							\
		{																	\
			if ( SET_LE( &(item)->linkField, &_dummySet ) )					\
			{																\
				continue;													\
			}																\
			else															\
			{		/* Done up front just in case item is freed. */			\
				_dummySet.setNum = (item)->linkField.setNum;				\
			}


#define SET_FOREACHBLOCKINGEND(head, item, type, linkField)					\
			item = (type*)(((ADDR)((head)))-offsetof(type,linkField));		\
		}																	\
	}


#define SET_GT(_x, _y)	(((_x)->setNum - (_y)->setNum) > 0)
#define SET_LT(_x, _y)	(((_x)->setNum - (_y)->setNum) < 0)
#define SET_GE(_x, _y)	(((_x)->setNum - (_y)->setNum) >= 0)
#define SET_LE(_x, _y)	(((_x)->setNum - (_y)->setNum) <= 0)
#define SET_EQ(_x, _y)	((_x)->setNum == (_y)->setNum)
#define SET_NE(_x, _y)	((_x)->setNum != (_y)->setNum)

extern void LBQ_SETenq(SEThead_t *, SETlink_t *);
extern void LBQ_SETpush(SEThead_t *, SETlink_t *);
extern ADDR LBQ_SETdeq(SEThead_t *sethead, NINT offset);
extern ADDR LBQ_SETdeqNoCheck(SEThead_t *sethead, NINT offset);
extern ADDR LBQ_SETtake(SEThead_t *sethead, NINT offset);
extern void LBQ_SETdrop(SEThead_t *);
extern void LBQ_SETrmv(SETlink_t *);
extern void LBQ_SETinitElements(SEThead_t *sethead, ADDR start,NINT num, NINT size);
extern int LBQ_SETfind(SEThead_t *head, SETlink_t *item);
extern int LBQ_SETcnt(SEThead_t *head);
extern STATUS LBQ_SETapply(SEThead_t *head, statusfunc_t userFunction, void *userArgs);


#define SET_CNT(head)			LBQ_SETcnt(head)

#define SET_INIT(head)			((head)->setNum = 0,						\
								(head)->next = (head)->prev = (head))

#define SET_STATIC_INIT(head)	{ &(head), &(head), 0 }

#define SET_EMPTY(head)			((head)->next == head)

#define SET_NOT_EMPTY(head)		((head)->next != head)

#if QUE_MACRO IS_ENABLED
#define SET_ENQ(head, item, linkField)										\
{																			\
	SEThead_t	*_head = head;												\
	SETlink_t	*_item = &((item)->linkField);								\
																			\
	if (IS_NULL(_item))														\
	{																		\
		_item->setNum = _head->setNum++;									\
		_item->prev = _head->prev;											\
		_item->next = _head;												\
		_head->prev->next = _item;											\
		_head->prev = _item;												\
	}																		\
}
#else

#define SET_ENQ(head, item, linkField)										\
{																			\
	LBQ_SETenq(head, &((item)->linkField));									\
}
#endif

#if QUE_MACRO IS_ENABLED
#define SET_PUSH(head, item, linkField)										\
{																			\
	SEThead_t	*_head = head;												\
	SETlink_t	*_item = &((item)->linkField);								\
																			\
	if (IS_NULL(_item))														\
	{																		\
		_item->setNum = _head->next->setNum - 1;							\
		_item->next = _head->next;											\
		_item->prev = _head;												\
		_head->next->prev = _item;											\
		_head->next = _item;												\
	}																		\
}
#else

#define SET_PUSH(head, item, linkField)										\
{																			\
	LBQ_SETpush(head, &((item)->linkField));								\
}
#endif

#if QUE_MACRO IS_ENABLED
#define SET_DEQ(head, item, type, linkField)								\
{																			\
	SEThead_t	*_head = head;												\
	SETlink_t	*_item;														\
																			\
	if (SET_EMPTY(_head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = _head->next;												\
		_head->next = _item->next;											\
		_head->next->prev = _head;											\
		NULLIFY(_item);														\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}
#else

#define SET_DEQ(head, item, type, linkField)								\
{																			\
	item = (type *)LBQ_SETdeq(head, offsetof(type, linkField));				\
}
#endif

#if QUE_MACRO IS_ENABLED
#define SET_DEQ_NO_CHECK(head, item, type, linkField)						\
{																			\
	SEThead_t	*_head = head;												\
	SETlink_t	*_item;														\
																			\
	_item = _head->next;													\
	_head->next = _item->next;												\
	_head->next->prev = _head;												\
	NULLIFY(_item);															\
	item = (type *)(((ADDR)_item) - offsetof(type, linkField));				\
}
#else

#define SET_DEQ_NO_CHECK(head, item, type, linkField)						\
{																			\
	item = (type *)LBQ_SETdeqNoCheck(head, offsetof(type, linkField));		\
}
#endif

#if QUE_MACRO IS_ENABLED
#define SET_TAKE(head, item, type, linkField)								\
{																			\
	SEThead_t	*_head = head;												\
	SETlink_t	*_item;														\
																			\
	if (SET_EMPTY(_head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = _head->prev;												\
		_head->prev = _item->prev;											\
		_head->prev->next = _head;											\
		NULLIFY(_item);														\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}
#else

#define SET_TAKE(head, item, type, linkField)								\
{																			\
	item = (type *)LBQ_SETtake(head, offsetof(type, linkField));			\
}
#endif

#define SET_PEEK(head, item, type, linkField)								\
{																			\
	SEThead_t	*_head = head;												\
	SETlink_t	*_item;														\
																			\
	if (SET_EMPTY(_head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = _head->next;												\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}

#define SET_PEEK_END(head, item, type, linkField)							\
{																			\
	SEThead_t	*_head = head;												\
	SETlink_t	*_item;														\
																			\
	if (SET_EMPTY(_head))													\
	{																		\
		item = NULL;														\
	}																		\
	else																	\
	{																		\
		_item = _head->prev;												\
		item = (type *)(((ADDR)_item) - offsetof(type, linkField));			\
	}																		\
}

#if QUE_MACRO IS_ENABLED
#define SET_DROP(head, item, linkField)										\
{																			\
	SEThead_t	*_head = head;												\
	SETlink_t	*_item = &((item)->linkField);								\
																			\
	_head->next = _item->next;												\
	_head->next->prev = _head;												\
	NULLIFY(_item);															\
}
#else

#define SET_DROP(head, item, linkField)										\
{																			\
	LBQ_SETdrop(head);														\
}
#endif

#if QUE_MACRO IS_ENABLED
#define SET_RMV(item, linkField)											\
{																			\
	SETlink_t		*_item = &((item)->linkField);							\
																			\
	_item->next->prev = _item->prev;										\
	_item->prev->next = _item->next;										\
	NULLIFY(_item);															\
}
#else

#define SET_RMV(item, linkField)											\
{																			\
	LBQ_SETrmv(&((item)->linkField));										\
}
#endif

#define SET_FOREACH(head, item, type, linkField)							\
	for (item =  (type *)(((ADDR)((head)->next)) - offsetof(type, linkField));\
		 item != (type *)(((ADDR)head) - offsetof(type, linkField));		\
		 item =  ONEXT(item, type, linkField)								\
	)

#define SET_ISHEADNEXT(head, item, type, linkField)							\
	((type *)((((item)->linkField.next) == (SETlink_t *)(head))))

#define SET_NEXT(head, item, type, linkField)								\
	((type *)((((item)->linkField.next) == (SETlink_t *)(head))				\
		? NULL																\
		: ((ADDR)((item)->linkField.next) - offsetof(type, linkField))))

#define SET_PREV(head, item, type, linkField)								\
	((type *)((((item)->linkField.prev) == (SETlink_t *)(head))				\
		? NULL																\
		: ((ADDR)((item)->linkField.prev) - offsetof(type, linkField))))


#define SET_INIT_ELEMENTS(head, data, numElements, typeElement, linkField)	\
{																			\
	LBQ_SETinitElements( &head, ((ADDR)data) + offsetof(typeElement, linkField),\
							numElements, sizeof(typeElement));				\
}

#define SET_APPLY(head, userFunction, userArgs)								\
				LBQ_SETapply(head, userFunction, userArgs)

#define SET_FIND(head, item, linkField)										\
				LBQ_SETfind(head, &((item)->linkField))


#ifdef __cplusplus
}
#endif

#endif /* _QUE_H_ */
