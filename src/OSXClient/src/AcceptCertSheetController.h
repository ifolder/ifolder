/* AcceptCertSheetController */

#import <Cocoa/Cocoa.h>
#include "Security/Security.h"
#include "SecurityInterface/SFCertificateView.h"


@interface AcceptCertSheetController : NSWindowController
{
	SecCertificateRef	certRef;
	SFCertificateView	*certView;

    IBOutlet NSBox			*certBox;
    IBOutlet NSScrollView	*certScrollView;
	
}

- (AcceptCertSheetController *) initWithCert:(SecCertificateRef)CertRef;

- (IBAction)accept:(id)sender;
- (IBAction)decline:(id)sender;
@end
