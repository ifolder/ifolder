set AXIS_LIB_DIR=.\tools\axis-1_2RC2\lib
set CLASSPATH=
set CLASSPATH=%CLASSPATH%;%AXIS_LIB_DIR%\axis.jar
set CLASSPATH=%CLASSPATH%;%AXIS_LIB_DIR%\axis-ant.jar
set CLASSPATH=%CLASSPATH%;%AXIS_LIB_DIR%\commons-discovery.jar
set CLASSPATH=%CLASSPATH%;%AXIS_LIB_DIR%\commons-logging.jar
set CLASSPATH=%CLASSPATH%;%AXIS_LIB_DIR%\jaxrpc.jar
set CLASSPATH=%CLASSPATH%;%AXIS_LIB_DIR%\log4j-1.2.8.jar
set CLASSPATH=%CLASSPATH%;%AXIS_LIB_DIR%\saaj.jar
set CLASSPATH=%CLASSPATH%;%AXIS_LIB_DIR%\wsdl4j.jar
set CLASSPATH=%CLASSPATH%;.\SwingStoreBrowser.jar

java -classpath %CLASSPATH% com.novell.simias.storebrowser.SwingStoreBrowser
