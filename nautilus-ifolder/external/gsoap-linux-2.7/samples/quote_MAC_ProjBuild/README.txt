Quote_PB: Delayed Stock Quotes Apple Mac OS X Project Builder Project

This folder contains an example Apple Project Builder project to build an
XMethods Delayed Stock Quote client application in C.

The custom build step runs the gSOAP compiler to generate stubs and skeletons.

To add (or modify) the custom build step, open the Quote.pbproj file and:
1. select the "Targets" tab in the "Build" window
2. in the "Targets" folder select "Quote"
3. select "Build Phases" in "Summary"
4. from the menu select "Project" then "New Build Phase"
   and "New Shell Script Build Phase"
5. in the "Script" entry add: "/usr/local/bin/soapcpp2 -c quote.h"

In step 5 you need to enter the path to soapcpp2, which in this example is
/usr/local/bin

To install soapcpp2 in /usr/local/bin, enter these commands:
sudo echo
<enter your admin password>
sudo cp soapcpp2 /usr/local/bin

To build and debug the application, select "Build and Debug". When the link
phase fails, check if soapC.c, soapClient.c (or soapServer.c to build a server)
are present in the project sources. If not, add them from the menu "Project"
"Add Files..."

To view the output of the program while running, select the "Standard I/O" tab
in the debugger.

To change the command-line argument before running, select "Targets" and
"Executables", "Quote".
