/* AcceptCertSheetController */

#import <Cocoa/Cocoa.h>
#include "Security/Security.h"
#include "SecurityInterface/SFCertificateView.h"


@interface AcceptCertSheetController : NSWindowController
{
	SecCertificateRef	certRef;

	IBOutlet SFCertificateView	*certView;
    IBOutlet NSTextField		*messageTitle;
    IBOutlet NSTextField		*message;
	
	NSString	*host;
}

- (AcceptCertSheetController *) initWithCert:(SecCertificateRef)CertRef forHost:(NSString *)Host;

- (IBAction)accept:(id)sender;
- (IBAction)decline:(id)sender;
@end
