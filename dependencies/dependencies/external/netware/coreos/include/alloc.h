#ifndef __ALLOC_H__
#define __ALLOC_H__
/*****************************************************************************
 *
 *	(C) Copyright 1997-1998 Novell, Inc.
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
 *  $Workfile:   alloc.h  $
 *  $Modtime:   Sep 07 1999 17:14:12  $
 *  $Revision$
 *
 ****************************************************************************/

/*--------------------------------------------------------------------
*	File Name:	alloc.h
*	Written by:	Rick Johnson
*	Date:		September 16, 1997
*-------------------------------------------------------------------*/

/*--------------------------------------------------------------------
*	Includes
*-------------------------------------------------------------------*/

/*--------------------------------------------------------------------
*	Defines
*-------------------------------------------------------------------*/
#define LOGICAL_NEQ_PHYSICAL			0x00000000
#define LOGICAL_EQ_PHYSICAL				0x00000001
#define NO_SLEEP_ALLOC					0x00000000
#define SLEEP_OK_ALLOC					0x00000002

#define OBJ_SUCCESS						0
#define OBJ_OUT_OF_MEMORY				1
#define OBJ_PARAMETER_ERROR				2
#define OBJ_TOO_LARGE					3
#define OBJ_RETURN_RTAG_ERROR			4
#define OBJ_RESERVED_5					5
#define OBJ_RESERVED_6					6
#define OBJ_RESERVED_7					7
#define OBJ_RESERVED_8					8
#define OBJ_RESERVED_9					9

#define GET_SUCCESS						0
#define GET_INVALID_MODULE				1
#define GET_BUFFER_TOO_SMALL			2
#define GET_INTERNAL_ERROR				3
#define GET_PTR_INVALID					4
#define GET_PTR_ALREADY_FREE			5
#define GET_CORRUPT_PRECEDING_REDZONE	6
#define GET_CORRUPT_TRAILING_REDZONE	7
#define GET_INVALID_POOL_TYPE			8
#define GET_INVALID_NODE_SIZE			9

#define SET_SUCCESS						0
#define SET_INVALID_MODULE				1
#define SET_INVALID_MODE				2
#define SET_FAILURE						3

#define SUCCESS							0
#define INVALID_MODULE					1

#define ALLOC_NORMAL_MODE				0
#define ALLOC_DEBUG_MODE				1	/* strict requested size check */
#define ALLOC_RELAXED_DEBUG_MODE		2	/* looser granted size check */

#define MAX_OBJECT_CACHE_NAME			64

/*--------------------------------------------------------------------
*	Typedefs
*-------------------------------------------------------------------*/
struct NLMAllocGeometry
{
	UINT mode;
	UINT logNeqPhysPoolSizeCount;
    UINT logNeqPhysObjectCacheCount;
    UINT logEqPhysPoolSizeCount;
    UINT logEqPhysObjectCacheCount;
};

struct SizeSummary
{
    UINT size;
    UINT type;
    UINT totalBytes;
    UINT inUseCount;
    UINT freeCount;
    UINT corruptionCount;
};

struct AllocSummary
{
    UINT type;
    UINT smallestSize;
    UINT largestSize;
    UINT inUseBytes;
    UINT freeBytes;
    UINT totalBytes;
    UINT inUseCount;
    UINT freeCount;
    UINT corruptionCount;
};

struct ObjectCacheSummary
{
    STR name[MAX_OBJECT_CACHE_NAME];
    UINT size;
    UINT type;
    UINT totalBytes;
    UINT inUseCount;
    UINT freeCount;
    UINT corruptionCount;
    UINT constructedCount;
    UINT unConstructedCount;
};

/*--------------------------------------------------------------------
*	Global Variables
*-------------------------------------------------------------------*/

/*--------------------------------------------------------------------
*	Prototypes
*-------------------------------------------------------------------*/

/*=============================================================================
*
*	AllocSleepOK
*
*	Used to allocate dynamic memory.  Where possible the AllocSleepOK
*	routine should be used instead of the Alloc routine.  In most
*	cases the call should be made using a resource tag with the
*	AllocSignature.  This maps the memory in a logical != physical
*	manner.  Non-adjacent physical pages are spliced together to make
*	a logically contiguous piece of memory.  This should eliminate
*	file cache fragmentation.
*
*	Backwards compatibility for older device drivers is provided by
*	using a resource tag with the AllocIOSignature.  This maps the
*	memory in a logical == physical manner.  Adjacent physical pages
*	are mapped to their same logical addresses to form a logically
*	and physically contiguous piece of memory.
*
*	BLOCKING routine
*
*	Parameters:
*		sizeInBytes	in		Amount of memory requested
*		rtag		in		Resource tag for tracking amount of
*							allocated memory.  AllocSignature gets
*							logical != physical memory.
*							AllocIOSignature gets logical == physical
*							memory.
*		sleptFlag	out		0 means did not block while allocating
*							memory.  Non-0 means did block while
*							allocating memory.
*
*	Return Values:
*		NULL				Allocation failure
*		pointer				Pointer to allocated memory
*
*============================================================================*/
extern void *AllocSleepOK (
		LONG sizeInBytes,
		struct ResourceTagStructure *rtag,
		LONG *sleptFlag);

/*=============================================================================
*
*	Alloc
*
*	Used to allocate dynamic memory.  Where possible the AllocSleepOK
*	routine should be used instead of the Alloc routine.  In most
*	cases the call should be made using a resource tag with the
*	AllocSignature.  This maps the memory in a logical != physical
*	manner.  Non-adjacent physical pages are spliced together to make
*	a logically contiguous piece of memory.  This should eliminate
*	file cache fragmentation.
*
*	Backwards compatibility for older device drivers is provided by
*	using a resource tag with the AllocIOSignature.  This maps the
*	memory in a logical == physical manner.  Adjacent physical pages
*	are mapped to their same logical addresses to form a logically
*	and physically contiguous piece of memory.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		sizeInBytes	in		Amount of memory requested
*		rtag		in		Resource tag for tracking amount of
*							allocated memory.  AllocSignature gets
*							logical != physical memory.
*							AllocIOSignature gets logical == physical
*							memory.
*
*	Return Values:
*		NULL				Allocation failure
*		pointer				Pointer to allocated memory
*
*============================================================================*/
extern void *Alloc (
		LONG sizeInBytes,
		struct ResourceTagStructure *rtag);

/*=============================================================================
*
*	Free
*	
*	Used to return memory to an NLM's memory pool that was allocated
*	using the AllocSleepOK or Alloc routines.
*	
*	NON-BLOCKING routine
*
*	Parameters:
*		address		in		Pointer to the allocated memory being
*							returned
*
*============================================================================*/
extern void Free (
		void *address);

/*=============================================================================
*
*	FreeAll
*	
*	Used to return all outstanding memory that an NLM has allocated
*	using the AllocSleepOK and Alloc functions.  This function is 
*	intended for use at NLM unload time.  Calling this function before
*	all threads associated with an NLM have terminated may result in
*	inaccurate allocation counts being returned to the caller.
*	
*	BLOCKING routine
*
*	Parameters:
*		moduleHandle			in		NLM that is to have its 
*										allocated memory freed
*		totalBytesFreed			out		Number of bytes that were 
*										freed.
*		totalAllocationsFreed	out		Number of outstanding 
*										allocations that were freed.
*
*	Return Values:
*		SUCCESS					All outstanding allocations
*										were freed.
*		FREE_INVALID_MODULE				Invalid moduleHandle parameter
*
*============================================================================*/
extern UINT FreeAll (
		UINT32 moduleHandle,
		UINT *totalBytesFreed,
		UINT *totalAllocationsFreed);

/*=============================================================================
*
*	SizeOfAllocBlock
*
*	Used to determine how many bytes make up a piece of memory
*	allocated with the AllocSleepOK or Alloc routines.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		address		in		Allocated memory pointer
*
*	Return Values:
*		0					Detected something wrong with the
*							allocated memory pointer
*		non 0				Allocated memory size in bytes
*
*============================================================================*/
extern LONG SizeOfAllocBlock (
		void *address);

/*=============================================================================
*
*	AllocGarbageCollect
*
*	Old API - provided for backwards compatibility.
*
*	Used to return unused/freed allocated memory associated with an
*	NLM to the system.  The new system garbage collector chooses when
*	to garbage collect more judiciously, is less obtrusive, and is
*	more efficient in handling alloc pool memory reclamation than the
*	previous garbage collector.
*
*	BLOCKING routine
*
*	Parameters:
*		nlm				in		Module to be garbage collected
*
*============================================================================*/
extern void AllocGarbageCollect (
		struct LoadDefinitionStructure *nlm);

/*=============================================================================
*
*	GetNLMAllocMemoryCounts
*
*	Used to get information about how much memory is associated with
*	an NLM's alloc pools.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		moduleHandle	in		Look up info about this module
*		freeBytes		out		Number of available bytes
*		freeNodes		out		Number of available nodes
*		allocatedBytes	out		Number of in use bytes
*		allocatedNodes	out		Number of in use nodes
*		totalMemory		out		Total number of available bytes +
*								  in use bytes + overhead bytes
*
*	Return Values:
*		GET_SUCCESS
*		GET_INVALID_MODULE
*
*============================================================================*/
extern UINT GetNLMAllocMemoryCounts (
		UINT32 moduleHandle,
		UINT32 *freeBytes,
		UINT32 *freeNodes,
		UINT32 *allocatedBytes,
		UINT32 *allocatedNodes,
		UINT32 *totalMemory);

/*=============================================================================
*
*	GetNLMAllocMemoryPtrs
*
*	Used to fill a caller supplied buffer with the in use pointers 
*	to an NLM's allocated memory.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		moduleHandle		in		Look up info about this module
*		buffer				in		Buffer that receives in use info
*		maxBufferEntries	in		Number of entries in buffer
*		usedBufferEntries	out		Number of buffer entries reported
*
*	Return Values:
*		GET_SUCCESS
*		GET_INVALID_MODULE
*		GET_BUFFER_TOO_SMALL
*	
*============================================================================*/
extern UINT GetNLMAllocMemoryPtrs (
		UINT32 moduleHandle,
		void *buffer[],
		UINT32 maxBufferEntries,
		UINT32 *usedBufferEntries);

/*=============================================================================
*
*	GetNLMAllocMemoryPtrInfo
*
*	Used to return information about an allocated memory pointer.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		address				in		Look up info about this pointer
*		size				out		Bytes allocated to the pointer
*		rtag				out		Resource tag used to allocate
*									the pointer
*	Return Values:
*		GET_SUCCESS
*		GET_PTR_INVALID
*		GET_PTR_ALREADY_FREE
*		GET_CORRUPT_PRECEDING_REDZONE
*		GET_CORRUPT_TRAILING_REDZONE
*
*============================================================================*/
extern UINT GetNLMAllocMemoryPtrInfo (
		void *address,
		UINT *size,
		struct ResourceTagStructure **rtag);

/*=============================================================================
*
*	GetAllocMemoryOverhead
*
*	Used to determine how much overhead is associated with a memory
*	allocation that is a page or page multiple in size.  This allows
*	the allocation request size to be reduced slightly thereby
*	reducing memory waste of nearly a page each for such allocations.
*	This can be useful for optimal sizing of thread stacks and other
*	structures that are close in size to a page or page multiple.
*	
*	NON-BLOCKING routine
*
*	Return Value:
*		bytes of overhead
*
*============================================================================*/
extern UINT GetAllocMemoryOverhead (void);

/*=============================================================================
*
*	CreateObjectCache
*
*	Initializes an object cache.  An object cache is a set of same
*	sized memory buffers with a constructor and a destructor function
*	associated with it.
*
*	The constructor is applied to each object at some time before it
*	is used, but only once.
*
*	The destructor is not applied when the object is returned to the
*	cache.  Rather, it is called when the garbage collector is able
*	to reap a portion of the object cache or when the object cache is
*	destroyed.
*
*	For performance, objects are cache line aligned and colored.
*
*	BLOCKING routine
*
*	CreateObjectCache Parameters:
*		nlmHandle			in		Object cache is tied to an nlm
*		name				in		Name of the object cache
*		objectSizeInBytes	in		Size of each object in the cache
*		flags				in		LOGICAL_NEQ_PHYSICAL
*		parameter			in		Parameter is passed to the
*									constructor and destructor
*		constructor			in		Object initializing function
*		destructor			in		Object clean up function
*		objectCacheID		out		Pointer to where to store the
*									object cache handle.  This is
*									really a void **, but void *
*									saves unnecessary casting.
*
*	CreateObjectCache Return Values:
*		OBJ_SUCCESS
*		OBJ_OUT_OF_MEMORY
*
*	constructor Parameters:
*		object				in		Object to initialize
*		parameter			in		Info relating to this cache
*		options				in		Options passed through when
*									AllocateObject is called, i.e.
*									SLEEP_OK_ALLOC, NO_SLEEP_ALLOC
*
*	constructor Return Values:
*		OBJ_SUCCESS					Developer must return this if
*									constructor is successful
*		developer defined error		A value that is not in the range
*									OBJ_OUT_OF_MEMORY ... OBJ_RESERVED_9
*
*	destructor Parameters:
*		object				in		Object to uninitialize
*		parameter			in		Info relating to this cache
*
*============================================================================*/
extern UINT CreateObjectCache (
		void *nlmHandle,
		STR *name,
		UINT objectSizeInBytes,
		UINT flags,
		void *parameter,
		UINT (*constructor) (	
				void *object,
				void *parameter,
				UINT options),
		void (*destructor) (
				void *object,
				void *parameter),
		void *objectCacheID);

/*=============================================================================
*
*	DestroyObjectCache
*
*	Tears down an object cache initialized by the CreateObjectCache
*	function.  The destructor is applied to all constructed objects
*	and all associated memory is returned to the system.
*
*	To avoid potential deadlock, this function should not be called
*	from an object cache destructor function.
*
*	BLOCKING routine
*
*	Parameters:
*		objectCacheID	in		Cache handle from CreateObjectCache
*
*	Return Values:
*		OBJ_SUCCESS
*		OBJ_RETURN_RTAG_ERROR
*
*============================================================================*/
extern UINT DestroyObjectCache (
		void *objectCacheID);

/*=============================================================================
*
*	AllocateObject
*
*	Gets an object from an already initialized cache.  An object
*	cache is initialized by the CreateObjectCache function.  The new
*	object has had the constructor applied, but only once.
*
*	BLOCKING or NON-BLOCKING routine depending on options parameter.
*	Where possible, this function should block.  The object cache
*	allocator has a better chance of getting needed memory with less
*	impact to the system if allowed to block.
*
*	Parameters:
*		objectCacheID	in   	Cache handle from CreateObjectCache
*		options			in   	SLEEP_OK_ALLOC, NO_SLEEP_ALLOC
*								Also passed to the constructor.
*		object			out		Pointer to where to store the object 
*						     	pointer.  This is really a void **, 
*							 	but void * saves the developer
*								unneeded casting.
*
*	Return Values:
*		OBJ_SUCCESS
*		OBJ_OUT_OF_MEMORY
*		OBJ_RESERVED_x
*		developer defined constructor error 
*
*============================================================================*/
extern UINT AllocateObject (
		void *objectCacheID,
		UINT options,
		void *object);

/*=============================================================================
*
*	FreeObject
*
*	Returns an object that has been allocated with the AllocateObject
*	function to its cache.  The object is left in the cache in a
*	constructed state and the destructor is not applied at this time.
*
*	NON_BLOCKING routine
*
*	Parameters:
*		object			in		Pointer to the object to be returned
*
*============================================================================*/
extern void FreeObject (
		void *object);

/*=============================================================================
*
*	SizeOfObject
*
*	Gets the size of an object that has been allocated with the
*	AllocateObject function.  This is the same as the parameter
*	objectSizeInBytes that is passed to CreateObjectCache.
*
*	NON_BLOCKING routine
*
*	Parameters:
*		object			in		Pointer to the object
*
*	Return Values:
*		0						Detected something wrong with the
*								object pointer
*		non 0					The object size in bytes
*
*============================================================================*/
extern UINT SizeOfObject (
		void *object);

/*=============================================================================
*
*	SetNLMAllocMemoryMode
*
*	Used to set the mode of the allocation pool associated with
*	an NLM.  If there is any memory in the alloc pool, whether in
*	use or not, the function call will fail.  An ideal time to set
*	the mode is during the EVENT_MODULE_LOAD_PRE_INIT event.  This
*	event is generated after the module is loaded but before the
*	module initialization code is executed.
*
*	NON_BLOCKING routine
*
*	Parameters:
*		moduleHandle	in		Set the mode for this module
*		mode			in		ALLOC_NORMAL_MODE, ALLOC_DEBUG_MODE,
*								ALLOC_RELAXED_DEBUG_MODE
*
*	Return Values:
*		SET_SUCCESS				Mode successfully set
*		SET_INVALID_MODULE		Bad module handle passed to function
*		SET_INVALID_MODE		Unrecognized mode passed to function
*		SET_FAILURE				There's already memory in the pool
*
*============================================================================*/
UINT SetNLMAllocMemoryMode (
		UINT32 moduleHandle,
		UINT mode);

/*=============================================================================
*
*	SuppressNLMUnloadAllocMessage
*
*	Used to disable the warning about unfreed allocated memory when
*	an NLM unloads.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		moduleHandle	in		Disable the message for this module
*
*	Return Values:
*		SUCCESS					Message won't be displayed
*		INVALID_MODULE			Bad moduleHandle parameter
*
*============================================================================*/
UINT SuppressNLMUnloadAllocMessage (
		UINT32 moduleHandle);

/*=============================================================================
*
*	GetNLMAllocGeometry
*
*	Used to get memory allocation information that applies to a single
*	nlm.
*
*	BLOCKING routine
*
*	Parameters:
*		moduleHandle	in		Look up memory alloc info for this module
*		geometry		out		Pointer to structure to be filled out
*
*	Return Values:
*		SUCCESS					
*
*============================================================================*/
UINT GetNLMAllocGeometry (
        UINT32 moduleHandle,
		struct NLMAllocGeometry *geometry);

/*=============================================================================
*
*	GetNLMPoolAllocSummary
*
*	Used to summarize memory allocation information for a memory
*	allocation pool belonging to a single nlm.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		moduleHandle	in		Look up memory alloc info for this module
*		poolType		in		LOGICAL_NEQ_PHYSICAL, LOGICAL_EQ_PHYSICAL
*		entriesInTable	in		Number of slots in the table
*		table			out		Pass in a pointer, function fills out the 
*								memory with information about memory pool's
*								allocations.
*		entriesUsed		out		Number of table entries that were filled
*								out by this function.
*		summary			out		Pass in a pointer, function fills out the 
*								memory with a summary about the memory pool's
*								allocations.
*
*	Return Values:
*		SUCCESS
*		GET_INVALID_POOL_TYPE
*		GET_BUFFER_TOO_SMALL
*
*============================================================================*/
UINT GetNLMPoolAllocSummary (
        UINT32 moduleHandle,
        UINT poolType,
        UINT entriesInTable,
        struct SizeSummary *sizeTable,
        UINT *entriesUsed,
		struct AllocSummary *summary);

/*=============================================================================
*
*	GetNLMPoolAllocCorruptionList
*
*	Used to retrieve a list of corrupted memory allocations of a 
*	particular size and type belonging to a single nlm.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		moduleHandle	in		Look up memory alloc info for this module
*		poolType		in		LOGICAL_NEQ_PHYSICAL, LOGICAL_EQ_PHYSICAL
*		size			in		Size of the allocated piece of memory
*		entriesInTable	in		Number of slots in the table
*		table			out		Pass in a pointer, function fills out the 
*								memory with a set of corrupted memory 
*								allocation pointers.
*		entriesUsed		out		Number of table slots used
*
*	Return Values:
*		SUCCESS
*		GET_INVALID_POOL_TYPE
*		GET_INVALID_NODE_SIZE
*		GET_BUFFER_TOO_SMALL
*
*============================================================================*/
UINT GetNLMPoolAllocCorruptionList (
        UINT32 moduleHandle,
		UINT poolType,
		UINT size,
        UINT entriesInTable,
        void *corruptionTable[],
        UINT *entriesUsed);

/*====================================================================
*
*	GetNLMPoolAllocInUseList
*
*	Used to retrieve a list of in use memory allocation pointers of
*	a particular size and type belonging to a single nlm.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		moduleHandle	in		Look up memory alloc info for this module
*		poolType		in		LOGICAL_NEQ_PHYSICAL, LOGICAL_EQ_PHYSICAL
*		size			in		Size of the allocated piece of memory
*		entriesInTable	in		Number of slots in the table
*		table			out		Pass in a pointer, function fills out the 
*								memory with a set of in use memory 
*								allocation pointers.
*		entriesUsed		out		Number of table slots used
*
*	Return Values:
*		SUCCESS
*		GET_BUFFER_TOO_SMALL	(successful, but table is full)
*		GET_INVALID_NODE_SIZE
*		GET_INVALID_POOL_TYPE
*
*===================================================================*/
UINT GetNLMPoolAllocInUseList (
        UINT32 moduleHandle,
        UINT poolType,
		UINT nodeSize,
        UINT entriesInTable,
        void *inUseTable[],
        UINT *entriesUsed);

/*=============================================================================
*
*	GetNLMObjectCacheList
*
*	Used to retrieve a list of object cache ids of a particular type 
*	that belong to a single nlm.
*
*	BLOCKING routine
*
*	Parameters:
*		moduleHandle		in		Look up object cache info for this module
*		cacheType			in		LOGICAL_NEQ_PHYSICAL, LOGICAL_EQ_PHYSICAL
*		entriesInTable		in		Number of slots in the table
*		objectCacheTable	out		Pass in a pointer, function fills out the 
*									memory with table of object cache ids.
*		entriesUsed			out		Number of table slots used
*
*	Return Values:
*		SUCCESS
*		GET_INVALID_POOL_TYPE
*		GET_BUFFER_TOO_SMALL
*
*============================================================================*/
UINT GetNLMObjectCacheList (
        UINT32 moduleHandle,
		UINT cacheType,
        UINT entriesInTable,
        void *objectCacheTable[],
        UINT *entriesUsed);

/*=============================================================================
*
*	GetObjectCacheSummary
*
*	Used to summarize memory allocation information for an object cache.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		objectCacheID	in		Look up info for this object cache
*		summary			out		Pass in pointer, summary info filled out
*
*	Return Values:
*		SUCCESS					
*
*============================================================================*/
UINT GetObjectCacheSummary (
        void *objectCacheID,
        struct ObjectCacheSummary *summary);

/*=============================================================================
*
*	GetObjectCacheCorruptionList
*
*	Used to retrieve a list of corrupted object allocations that belong to an
*	object cache that belongs to an nlm.
*
*	NON-BLOCKING routine
*
*	Parameters:
*		objectCacheID	in		Look up object corruptions in this object cache
*		entriesInTable	in		Number of slots in the table
*		table			out		Pass in a pointer, function fills out the 
*								memory with a table of corrupted object
*								pointers.
*		entriesUsed		out		Number of table slots used
*
*	Return Values:
*		SUCCESS
*		GET_BUFFER_TOO_SMALL
*
*============================================================================*/
UINT GetObjectCacheCorruptionList (
        void *objectCacheID,
        UINT entriesInTable,
        void *corruptionTable[],
        UINT *entriesUsed);

/****************************************************************************/
/****************************************************************************/
#endif /* __ALLOC_H__ */
