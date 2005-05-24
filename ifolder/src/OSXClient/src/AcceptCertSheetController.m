#import "AcceptCertSheetController.h"

@implementation AcceptCertSheetController


- (AcceptCertSheetController *) initWithCert:(SecCertificateRef)CertRef
{
	if ((self = [super initWithWindowNibName:@"AcceptCert"]) != nil)
	{
		certRef = CertRef;
	}
	return self;
}

//===================================================================
// awakeFromNib
// When this class is loaded from the nib, startup simias and wait
// since our app isn't useful without simias
//===================================================================
-(void)awakeFromNib
{
	[certView setCertificate:certRef];
	[certView setDisplayTrust:NO];
	[certView setEditableTrust:NO];
}

- (IBAction)accept:(id)sender
{
	[[self window] orderOut:nil];
	[NSApp endSheet:[self window] returnCode:1];
}

- (IBAction)decline:(id)sender
{
	[[self window] orderOut:nil];
	[NSApp endSheet:[self window] returnCode:0];
}

@end
