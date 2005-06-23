#import "AcceptCertSheetController.h"

@implementation AcceptCertSheetController


- (AcceptCertSheetController *) initWithCert:(SecCertificateRef)CertRef forHost:(NSString *)Host
{
	if ((self = [super initWithWindowNibName:@"AcceptCert"]) != nil)
	{
		certRef = CertRef;
		host = Host;
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

	[messageTitle setStringValue:[NSString stringWithFormat:
		NSLocalizedString(@"iFolder cannot verify the identity of the iFolder Server \"%@\".", 
			@"Accept Certificate Message"), host]];

	[message setStringValue:[NSString stringWithFormat:
		NSLocalizedString(@"The certificate for this iFolder Server was signed by an unknown certifying authority.  You might be connecting to a server that is pretending to be \"%@\" which could put your confidential information at risk.   Before accepting this certificate, you should check with your system administrator.  Do you want to accept this certificate permanently and continue to connect?", 
			@"Accept Certificate Details"), host]];
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
