<?xml version="1.0" standalone="no"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:template match="/">
    <ADDONCONTAINER>
      <ADDON>
        <PACKAGECONTAINER>
          <PACKAGE ID="9CFE2E89-1ECD-413D-830F-DCB605AD5781" NAME="Windows Phone Power Tools Profiler Config">
            <PACKAGETYPECONTAINER>
              <PACKAGETYPE Name="ARMV4I" ID="ARMV4I" Protected="true">
                <PROPERTYCONTAINER>
                  <PROPERTY ID="RemotePath" Protected="true">\Data\SharedData\PhoneTools\11.0\Profiler</PROPERTY>
                  <PROPERTY ID="RootPath" Protected="true">%CSIDL_COMMON_APPDATA%\WindowsPhonePowerTools\Profiler</PROPERTY>
                  <PROPERTY ID="CommandLine" Protected="true"/>
                  <PROPERTY ID="CPU" Protected="true">ARMV4I</PROPERTY>
                </PROPERTYCONTAINER>
                <FILECONTAINER>
                  <FILE ID="ProfilingTasks.xml" />
                </FILECONTAINER>
              </PACKAGETYPE>

              <PACKAGETYPE Name="X86" ID="X86" Protected="true">
                <PROPERTYCONTAINER>
                  <PROPERTY ID="RemotePath" Protected="true">\Data\SharedData\PhoneTools\11.0\Profiler</PROPERTY>
                  <PROPERTY ID="RootPath" Protected="true">%CSIDL_COMMON_APPDATA%\WindowsPhonePowerTools\Profiler</PROPERTY>
                  <PROPERTY ID="CommandLine" Protected="true"/>
                  <PROPERTY ID="CPU" Protected="true">X86</PROPERTY>
                </PROPERTYCONTAINER>
                <FILECONTAINER>
                  <FILE ID="ProfilingTasks.xml" />
                </FILECONTAINER>
              </PACKAGETYPE>

            </PACKAGETYPECONTAINER>
            <PROPERTYCONTAINER/>
          </PACKAGE>
        </PACKAGECONTAINER>
      </ADDON>
    </ADDONCONTAINER>
  </xsl:template>
</xsl:stylesheet>
