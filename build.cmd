@ECHO OFF
PUSHD "%~dp0"
IF NOT EXIST .tools CALL init.cmd
.tools\nuget restore
msbuild build.proj /verbosity:n /clp:ShowCommandLine /m:%NUMBER_OF_PROCESSORS% /nr:false /fl /flp:LogFile=MSBuild.log;Verbosity=diag;ShowTimestamp
FOR %%I IN (Debug Release) DO (
  FOR /R %%J IN (bin\%%~I\*.Test.dll) DO (
    vstest.console "%%~J"
  )
  FOR /R %%J IN (bin\%%~I\*.Profile.exe) DO (
    vstest.console "%%~J"
  )
)
POPD
