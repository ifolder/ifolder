/* this header sets packing back to default for different compilers */

#if (_MSC_VER >= 900)\
	|| (__BORLANDC__ >= 0x500)\
	|| (__WATCOMC__ >= 1050)\
	|| (__MWERKS__ >= 0x2100)\
	|| defined(__ECC__) || defined(__ECPP__)

# pragma pack(pop)

#elif defined(__BORLANDC__)

# pragma option -a.

#else

# pragma pack()

#endif

#undef N_PACK_1
