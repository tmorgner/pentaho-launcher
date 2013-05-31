pentaho-launcher
================

A small wrapper around the .bat files that start our applications

After mixed experiences with launch4j and hard times supporting that
launcher, the safest option is to have a bat-file that is user-editable
when needed and does not magical things.

A bat-file has a few drawbacks in comparison to a native application:

 * It opens a console window, which is technical and looks ugly
 * It makes it hard to define or maintain file associations

The default code in here is geared towards the Pentaho Report Designer.
It tries to launch the 'Report-Designer.bat' file, which must be in the
same directory as the .exe file.




Technical documentation

The launcher's configuration can be tuned via the .exe.conf file.
The following properties are defined:

 * Extensions : A semi-colon separated list of file extensions, including
                the leading dot.
 * Executable : The name of the bat-file that should be started.
 * JavaVersion: The Java-Version that should be used if neither a JAVA_HOME
                nor a PENTAHO_JAVA_HOME is defined and no local JRE can be
                found. 
 * FixJavaHome: If no JAVA_HOME is defined, shall the launcher try to come
                up with an temporary JAVA_HOME variable.
 * ProgramId  : The program-id must be a unique ID to identify the application.
 

When starting, the launcher goes through the following steps:

	(1) Load the configuration
	(2) Validate the existence of PENTAHO_JAVA_HOME and JAVA_HOME variables
  	  If neither of these variables are defined, the launcher tries to guess
	    whether a JDK of the correct version may be installed by checking the
  	  registry (if FixJavaHome is True). 
	(3) The launcher then tries to register the defined file extensions with 
  	  itself.
	(4) It finally launches the specified executable/bat-file.    
 



 [1] Documentation about the Program-Id:
     http://msdn.microsoft.com/en-gb/library/windows/desktop/cc144152%28v=vs.85%29.aspx