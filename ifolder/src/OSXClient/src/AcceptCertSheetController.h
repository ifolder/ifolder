/* AcceptCertSheetController */

#import <Cocoa/Cocoa.h>
#include "Security/Security.h"
#include "SecurityInterface/SFCertificateView.h"


@interface AcceptCertSheetController : NSWindowController
{
	SecCertificateRef	certRef;

	IBOutlet SFCertificateView	*certView;
}

- (AcceptCertSheetController *) initWithCert:(SecCertificateRef)CertRef;

- (IBAction)accept:(id)sender;
- (IBAction)decline:(id)sender;
@end
