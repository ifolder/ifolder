#ifndef _CONVIDS_H_
#define _CONVIDS_H_

/* Function prototypes */
STATUS FS_ConvertGuidToID(NDSid_t *guid, LONG *idp);
STATUS FS_ConvertIDToGuid(LONG id, NDSid_t *guid);
STATUS CFS_ConvertGuidToID( LONG *guid, LONG id);
#endif /* _CONVIDS_H_ */
