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
	certView = [[SFCertificateView alloc] init];
	[certView setCertificate:certRef];
	[certView setDisplayTrust:NO];
	[certView setEditableTrust:NO];

	[certBox setContentView:certView];
	[certBox sizeToFit];

	NSRect rect = [certBox frame];
	rect.size.height += 200;
	[certBox setFrame:rect];
	[certBox setNeedsDisplay:YES];
	[certBox scrollPoint:NSMakePoint(0, [certBox frame].size.height)];
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
