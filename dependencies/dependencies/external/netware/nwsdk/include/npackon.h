/* this header sets packing to 1 for different compilers */

#if defined (_MSC_VER) && !defined(__BORLANDC__)
# if (_MSC_VER > 600)
#  pragma warning(disable:4103)
# endif
#elif defined (__BORLANDC__)
# if (__BORLANDC__ >= 0x500)
#  pragma warn -pck
# endif
#endif

#if defined (__BORLANDC__)
# if (__BORLANDC__ >= 0x500)
#  pragma pack(push)
# endif
#elif defined (__WATCOMC__)
# if (__WATCOMC__ >= 1050)
#  pragma pack(push)
# endif
#elif defined (__MWERKS__)
# if (__MWERKS__ >= 0x2100)
#  pragma pack(push)
# endif
#elif defined(__ECC__) || defined(__ECPP__)
# pragma pack(push)
#elif defined (_MSC_VER) 
# if (_MSC_VER >= 900)
#  pragma pack(push)
# endif
#endif

#if defined(N_PLAT_DOS)\
	|| (defined(N_PLAT_MSW) && defined(N_ARCH_16) && !defined(N_PLAT_WNT))\
	|| defined(N_PLAT_NLM)\
	|| defined(N_PLAT_OS2)\
	|| defined(N_PLAT_UNIX)\
	|| defined(N_PACK_1)\
	|| defined(N_FORCE_INT_16)

# if defined(__BORLANDC__) 
#   if (__BORLANDC__ < 0x500)
#     pragma option -a-
#   else
#     pragma pack(1)
#   endif
# else
#   pragma pack(1)
# endif

#else

# pragma pack(4)

#endif
