/*++

Copyright (c) 1997  Microsoft Corporation

Module Name:

    qos.h - QoS definitions for NDIS components.

Abstract:

    This module defines the Quality of Service structures and types used
    by Winsock applications.

Revision History:

--*/

#ifndef __QOS_H_
#define __QOS_H_


/*
 *  Definitions for valued-based Service Type for each direction of data flow.
 */
typedef ULONG   SERVICETYPE;
#define SERVICETYPE_NOTRAFFIC               0x00000000  /* No data in this
                                                         * direction */
#define SERVICETYPE_BESTEFFORT              0x00000001  /* Best Effort */
#define SERVICETYPE_CONTROLLEDLOAD          0x00000002  /* Controlled Load */
#define SERVICETYPE_GUARANTEED              0x00000003  /* Guaranteed */
#define SERVICETYPE_NETWORK_UNAVAILABLE     0x00000004  /* Used to notify 
                                                         * change to user */
#define SERVICETYPE_GENERAL_INFORMATION     0x00000005  /* corresponds to 
                                                         * "General Parameters"
                                                         * defined by IntServ */
#define SERVICETYPE_NOCHANGE                0x00000006  /* used to indicate
                                                         * that the flow spec
                                                         * contains no change
                                                         * from any previous
                                                         * one */

#define SERVICETYPE_NONCONFORMING           0x00000009  /* Non-Conforming Traffic */
#define SERVICETYPE_CUSTOM1                 0x0000000A  /* Custom ServiceType 1 */
#define SERVICETYPE_CUSTOM2                 0x0000000B  /* Custom ServiceType 2 */ 
#define SERVICETYPE_CUSTOM3                 0x0000000C  /* Custom ServiceType 3 */ 
#define SERVICETYPE_CUSTOM4                 0x0000000D  /* Custom ServiceType 4 */ 



/*
 *  Definitions for bitmap-based Service Type for each direction of data flow.
 */

#define SERVICE_BESTEFFORT                  0x80020000
#define SERVICE_CONTROLLEDLOAD              0x80040000
#define SERVICE_GUARANTEED                  0x80080000
#define SERVICE_CUSTOM1                     0x80100000
#define SERVICE_CUSTOM2                     0x80200000
#define SERVICE_CUSTOM3                     0x80400000
#define SERVICE_CUSTOM4                     0x80800000


/*
 *  Number of available Service Types.
 */
#define NUM_SERVICETYPES                    8


/*
 * to turn on immediate traffic control, OR ( | ) this flag with the 
 * ServiceType field in the FLOWSPEC
 */
// #define SERVICE_IMMEDIATE_TRAFFIC_CONTROL   0x80000000   // obsolete

#define SERVICE_NO_TRAFFIC_CONTROL   0x81000000

/*
 * this flag can be used with the immediate traffic control flag above to
 * prevent any rsvp signaling messages from being sent. Local traffic 
 * control will be invoked, but no RSVP Path messages will be sent.This flag
 * can also be used in conjunction with a receiving flowspec to suppress 
 * the automatic generation of a Reserve message.  The application would 
 * receive notification that a Path  message had arrived and would then need
 * to alter the QOS by issuing WSAIoctl( SIO_SET_QOS ), to unset this flag 
 * and thereby cause Reserve messages to go out.
 */

#define SERVICE_NO_QOS_SIGNALING   0x40000000

#define STATUS_QOS_RELEASED                 0x10101010  /* rsvp status code */

/*
 *  Flow Specifications for each direction of data flow.
 */
typedef struct _flowspec
{
    ULONG       TokenRate;              /* In Bytes/sec */
    ULONG       TokenBucketSize;        /* In Bytes */
    ULONG       PeakBandwidth;          /* In Bytes/sec */
    ULONG       Latency;                /* In microseconds */
    ULONG       DelayVariation;         /* In microseconds */
    SERVICETYPE ServiceType;
    ULONG       MaxSduSize;             /* In Bytes */
    ULONG       MinimumPolicedSize;     /* In Bytes */

} FLOWSPEC, *PFLOWSPEC, * LPFLOWSPEC;

/*
 * this value can be used in the FLOWSPEC structure to instruct the Rsvp Service 
 * provider to derive the appropriate default value for the parameter.  Note 
 * that not all values in the FLOWSPEC structure can be defaults. In the
 * ReceivingFlowspec, all parameters can be defaulted except the ServiceType.  
 * In the SendingFlowspec, the MaxSduSize and MinimumPolicedSize can be
 * defaulted. Other defaults may be possible. Refer to the appropriate
 * documentation.
 */
#define QOS_NOT_SPECIFIED     0xFFFFFFFF

#define NULL_QOS_TYPE         0xFFFFFFFD


/*
 * define a value that can be used for the PeakBandwidth, which will map into 
 * positive infinity when the FLOWSPEC is converted into IntServ floating point 
 * format.  We can't use (-1) because that value was previously defined to mean
 * "select the default".
 */
#define   POSITIVE_INFINITY_RATE     0xFFFFFFFE



/*
 * the provider specific structure can have a number of objects in it.
 * Each next structure in the
 * ProviderSpecific will be the QOS_OBJECT_HDR struct that prefaces the actual
 * data with a type and length for that object.  This QOS_OBJECT struct can 
 * repeat several times if there are several objects.  This list of objects
 * terminates either when the buffer length has been reached ( WSABUF ) or
 * an object of type QOS_END_OF_LIST is encountered.
 */
typedef struct  {

    ULONG   ObjectType;
    ULONG   ObjectLength;  /* the length of object buffer INCLUDING 
                            * this header */

} QOS_OBJECT_HDR, *LPQOS_OBJECT_HDR;


/*
 * general QOS objects start at this offset from the base and have a range 
 * of 1000
 */
#define   QOS_GENERAL_ID_BASE         2000

#define   QOS_OBJECT_PRIORITY         (0x00000000 + QOS_GENERAL_ID_BASE)
          /* QOS_PRIORITY structure passed */
#define   QOS_OBJECT_END_OF_LIST      (0x00000001 + QOS_GENERAL_ID_BASE) 
          /* QOS_End_of_list structure passed */
#define   QOS_OBJECT_SD_MODE          (0x00000002 + QOS_GENERAL_ID_BASE) 
          /* QOS_ShapeDiscard structure passed */
#define   QOS_OBJECT_TRAFFIC_CLASS    (0x00000003 + QOS_GENERAL_ID_BASE) 
          /* QOS_Traffic class structure passed */
#define   QOS_OBJECT_DESTADDR         (0x00000004 + QOS_GENERAL_ID_BASE)
          /* QOS_DestAddr structure */
#define   QOS_OBJECT_SHAPER_QUEUE_DROP_MODE	   (0x00000005 + QOS_GENERAL_ID_BASE)
          /* QOS_ShaperQueueDropMode structure */
#define   QOS_OBJECT_SHAPER_QUEUE_LIMIT	           (0x00000006 + QOS_GENERAL_ID_BASE)
          /* QOS_ShaperQueueLimit structure */


/*
 * This structure defines the absolute priorty of the flow.  Priorities in the 
 * range of 0-7 are currently defined. Receive Priority is not currently used, 
 * but may at some point in the future.
 */
typedef struct _QOS_PRIORITY {

    QOS_OBJECT_HDR  ObjectHdr;
    UCHAR           SendPriority;     /* this gets mapped to layer 2 priority.*/
    UCHAR           SendFlags;        /* there are none currently defined.*/
    UCHAR           ReceivePriority;  /* this could be used to decide who 
                                       * gets forwarded up the stack first 
                                       * - not used now */
    UCHAR           Unused;

} QOS_PRIORITY, *LPQOS_PRIORITY;


/*
 * This structure is used to define the behaviour that the traffic
 * control packet shaper will apply to the flow.
 *
 * PS_NONCONF_BORROW - the flow will receive resources remaining 
 *  after all higher priority flows have been serviced. If a 
 *  TokenRate is specified, packets may be non-conforming and
 *  will be demoted to less than best-effort priority.
 *  
 * PS_NONCONF_SHAPE - TokenRate must be specified. Non-conforming
 *  packets will be retianed in the packet shaper until they become
 *  conforming.
 *
 * PS_NONCONF_DISCARD - TokenRate must be specified. Non-conforming
 *  packets will be discarded.
 *
 */

typedef struct _QOS_SD_MODE {

    QOS_OBJECT_HDR   ObjectHdr;
    ULONG            ShapeDiscardMode;

} QOS_SD_MODE, *LPQOS_SD_MODE;

#define TC_NONCONF_BORROW      0
#define TC_NONCONF_SHAPE       1
#define TC_NONCONF_DISCARD     2
#define TC_NONCONF_BORROW_PLUS 3


/*
 * This structure may carry an 802.1 TrafficClass parameter which 
 * has been provided to the host by a layer 2 network, for example, 
 * in an 802.1 extended RSVP RESV message. If this object is obtained
 * from the network, hosts will stamp the MAC headers of corresponding
 * transmitted packets, with the value in the object. Otherwise, hosts
 * may select a value based on the standard Intserv mapping of 
 * ServiceType to 802.1 TrafficClass.
 *
 */

typedef struct _QOS_TRAFFIC_CLASS {

    QOS_OBJECT_HDR   ObjectHdr;
    ULONG            TrafficClass;

} QOS_TRAFFIC_CLASS, *LPQOS_TRAFFIC_CLASS;

/*
 * This structure allows overriding of the default schema used to drop 
 * packets when a flow's shaper queue limit is reached.
 *
 * DropMethod - 
 * 	QOS_SHAPER_DROP_FROM_HEAD - Drop packets from
 * 		the head of the queue until the new packet can be
 * 		accepted into the shaper under the current limit.  This
 * 		behavior is the default.
 * 	QOS_SHAPER_DROP_INCOMING - Drop the incoming, 
 * 		limit-offending packet.
 *
 */

typedef struct _QOS_SHAPER_QUEUE_LIMIT_DROP_MODE {

    QOS_OBJECT_HDR   ObjectHdr;
    ULONG            DropMode;

} QOS_SHAPER_QUEUE_LIMIT_DROP_MODE, *LPQOS_SHAPER_QUEUE_LIMIT_DROP_MODE;

#define QOS_SHAPER_DROP_INCOMING	0
#define QOS_SHAPER_DROP_FROM_HEAD	1

/*
 * This structure allows the default per-flow limit on the shaper queue
 * size to be overridden.
 *
 * QueueSizeLimit - Limit, in bytes, of the size of the shaper queue
 *
 */

typedef struct _QOS_SHAPER_QUEUE_LIMIT {

    QOS_OBJECT_HDR   ObjectHdr;
    ULONG            QueueSizeLimit;

} QOS_SHAPER_QUEUE_LIMIT, *LPQOS_SHAPER_QUEUE_LIMIT;


#endif  /* __QOS_H_ */




