/* this header sets packing to 1 for different compilers */

#if (_MSC_VER > 600)
# pragma warning(disable:4103)
#elif (__BORLANDC__ >= 0x500)
# pragma warn -pck
#endif

#if (_MSC_VER >= 900)\
	|| (__BORLANDC__ >= 0x500)\
	|| (__WATCOMC__ >= 1050)\
	|| (__MWERKS__ >= 0x2100)\
	|| defined(__ECC__) || defined(__ECPP__)
# pragma pack(push)
#endif

#if defined(N_PLAT_DOS)\
	|| (defined(N_PLAT_MSW) && defined(N_ARCH_16) && !defined(N_PLAT_WNT))\
	|| defined(N_PLAT_NLM)\
	|| defined(N_PLAT_OS2)\
	|| defined(N_PLAT_UNIX)\
	|| defined(N_PACK_1)\
	|| defined(N_FORCE_INT_16)

# if defined(__BORLANDC__) && (__BORLANDC__ < 0x500)
#  pragma option -a-
# else
#  pragma pack(1)
# endif

#else

# pragma pack(4)

#endif
