/* this header sets packing back to default for different compilers */

#if defined (__BORLANDC__)
# if (__BORLANDC__ >= 0x500)
#  pragma pack(pop)
# else
#  pragma option -a.
# endif
#elif defined (__WATCOMC__)
# if (__WATCOMC__ >= 1050)
#  pragma pack(pop)
# else
#  pragma pack()
# endif
#elif defined (__MWERKS__)
# if (__MWERKS__ >= 0x2100)
#  pragma pack(pop)
# else
#  pragma pack()
# endif
#elif defined(__ECC__) || defined(__ECPP__)
# pragma pack(pop)
#elif defined (_MSC_VER)
# if (_MSC_VER >= 900)
#  pragma pack(pop)
# else
#  pragma pack()
# endif
#else
# pragma pack()
#endif


#ifdef N_PACK_1
#undef N_PACK_1
#endif

