set AXIS_LIBS=c:\projects\SwingStoreBrowser\tools\axis-1_2RC2\lib
set AXISCP=.
set AXISCP=%AXISCP%;%AXIS_LIBS%\axis.jar
set AXISCP=%AXISCP%;%AXIS_LIBS%\commons-discovery.jar
set AXISCP=%AXISCP%;%AXIS_LIBS%\commons-logging.jar
set AXISCP=%AXISCP%;%AXIS_LIBS%\jaxrpc.jar
set AXISCP=%AXISCP%;%AXIS_LIBS%\saaj.jar
set AXISCP=%AXISCP%;%AXIS_LIBS%\log4j-1.2.8.jar
set AXISCP=%AXISCP%;%AXIS_LIBS%\wsdl4j.jar
set CLASSPATH=%CLASSPATH%;%AXISCP%

java -cp %CLASSPATH% org.apache.axis.wsdl.WSDL2Java -v -o c:\projects\SwingStoreBrowser\src c:\projects\SwingStoreBrowser\external\SimiasBrowser.wsdl
