#ifndef _ERRNO_H_
#define _ERRNO_H_
/*============================================================================
=  Novell Software Development Kit--NetWare Loadable Module (NLM) Source Code
=
=  Copyright (C) 1999 Novell, Inc. All Rights Reserved.
=
=  This work is subject to U.S. and international copyright laws and treaties.
=  Use and redistribution of this work is subject  to  the  license  agreement
=  accompanying  the  software  development kit (SDK) that contains this work.
=  However, no part of this work may be revised and/or  modified  without  the
=  prior  written consent of Novell, Inc. Any use or exploitation of this work
=  without authorization could subject the perpetrator to criminal  and  civil
=  liability. 
=
=  errno.h
==============================================================================
*/

/* -------------------------- Base POSIX-mandated constants --------------- */
#define ENOENT          1  /* no such file or directory                     */
#define E2BIG           2  /* arg list too big                              */
#define ENOEXEC         3  /* exec format error                             */
#define EBADF           4  /* bad file number                               */
#define ENOMEM          5  /* not enough memory                             */
#define EACCES          6  /* permission denied                             */
#define EEXIST          7  /* file exists                                   */
#define EXDEV           8  /* cross-device link                             */
#define EINVAL          9  /* invalid argument                              */
#define ENFILE          10 /* file table overflow                           */
#define EMFILE          11 /* too many open files                           */
#define ENOSPC          12 /* no space left on device                       */

#define EDOM            13 /* argument too large                            */
#define ERANGE          14 /* result too large                              */

#define EDEADLK         15 /* resource deadlock would occur                 */

/* -------------------------- Miscellaneous NLM Library constants --------- */
#define EINUSE          16 /* resource(s) in use                            */
#define ESERVER         17 /* server error (memory out, I/O error, etc.)    */
#define ENOSERVR        18 /* no server (queue server, file server, etc.)   */
#define EWRNGKND        19 /* wrong kind--an operation is being...          */
                           /*  ...attempted on the wrong kind of object     */
#define ETRNREST        20 /* transaction restarted                         */
#define ERESOURCE       21 /* resources unavailable (maybe permanently)     */
#define EBADHNDL        22 /* bad non-file handle (screen, semaphore, etc)  */
#define ENO_SCRNS       23 /* screen I/O attempted when no screens          */

/* -------------------------- Additional POSIX / traditional UNIX constants */
#define EAGAIN          24 /* resource temporarily unavailable              */
#define ENXIO           25 /* no such device or address                     */
#define EBADMSG         26 /* not a data message                            */
#define EFAULT          27 /* bad address                                   */
#define EIO             28 /* physical I/O error                            */
#define ENODATA         29 /* no data                                       */
#define ENOSTRMS        30 /* streams not available                         */

                           /* Berkeley sockets constants ------------------ */
#define EPROTO          31 /* fatal protocol error                          */
#define EPIPE           32 /* broken pipe                                   */
#define ESPIPE          33 /* illegal seek                                  */

                           /* Non-blocking and interrupt I/O constants ---- */
#define ETIME           34 /* ioctl acknowledge timeout                     */
#define EWOULDBLOCK     35 /* operation would block                         */
#define EINPROGRESS     36 /* operation now in progress                     */
#define EALREADY        37 /* operation already in progress                 */

                           /* IPC network argument constants -------------- */
#define ENOTSOCK        38 /* socket operation on non-socket                */
#define EDESTADDRREQ    39 /* destination address required                  */
#define EMSGSIZE        40 /* message too long                              */
#define EPROTOTYPE      41 /* protocol wrong type for socket                */
#define ENOPROTOOPT     42 /* protocol not available                        */
#define EPROTONOSUPPORT 43 /* protocol not supported                        */
#define ESOCKTNOSUPPORT 44 /* socket type not supported                     */
#define EOPNOTSUPP      45 /* operation not supported on socket             */
#define EPFNOSUPPORT    46 /* protocol family not supported                 */
#define EAFNOSUPPORT    47 /* address family unsupported by protocol family */
#define EADDRINUSE      48 /* address already in use                        */
#define EADDRNOTAVAIL   49 /* can't assign requested address                */

                           /* Operational constants ----------------------- */
#define ENETDOWN        50 /* network is down                               */
#define ENETUNREACH     51 /* network is unreachable                        */
#define ENETRESET       52 /* network dropped connection on reset           */
#define ECONNABORTED    53 /* software caused connection abort              */
#define ECONNRESET      54 /* connection reset by peer                      */
#define ENOBUFS         55 /* no buffer space available                     */
#define EISCONN         56 /* socket is already connected                   */
#define ENOTCONN        57 /* socket is not connected                       */
#define ESHUTDOWN       58 /* can't send after socket shutdown              */
#define ETOOMANYREFS    59 /* too many references: can't splice             */
#define ETIMEDOUT       60 /* connection timed out                          */
#define ECONNREFUSED    61 /* connection refused                            */

/* -------------------------- Additional POSIX-mandated constants --------- */
#define EBUSY           62 /* resource busy                                 */
#define EINTR           63 /* interrupted function call                     */
#define EISDIR          64 /* is a directory                                */
#define ENAMETOOLONG    65 /* filename too long                             */
#define ENOSYS          66 /* function not implemented                      */
#define ENOTDIR         67 /* not a directory                               */
#define ENOTEMPTY       68 /* directory not empty                           */
#define EPERM           69 /* operation not permitted                       */

#define ECHILD          70 /* no child process                              */
#define EFBIG           71 /* file too large                                */
#define EMLINK          72 /* too many links                                */
#define ENODEV          73 /* no such device                                */
#define ENOLCK          74 /* no locks available                            */
#define ENOTTY          75 /* inappropriate I/O control operation           */
#define EFTYPE          ENOTTY /* inappropriate operation for file type     */
#define EROFS           76 /* read-only file system                         */
#define ESRCH           77 /* no such process                               */
#define ECANCELED       78 /* operation was cancelled                       */
#define ENOTSUP         79 /* this optional functionality not supported     */

/* -------------------------- CLib-implementation-specific constants ------ */
#define ECANCELLED      ECANCELED
#define ENLMDATA        100/* anomaly in NLM data structure                 */
#define EILSEQ          101/* illegal character sequence in multibyte       */
#define EINCONSIS       102/* internal library inconsistency                */
#define EDOSTEXTEOL     103/* DOS-text file inconsistency--no newline...    */
                           /* ...after carriage return                      */
#define ENONEXTANT      104/* object doesn't exist                          */
#define ENOCONTEXT      105/* no thread library context present             */

#define ELASTERR        ENOCONTEXT

#ifdef __cplusplus
extern "C"
{
#endif

int  *__get_errno_ptr( void );

#ifdef __cplusplus
}
#endif

#define errno  *__get_errno_ptr()

#endif
