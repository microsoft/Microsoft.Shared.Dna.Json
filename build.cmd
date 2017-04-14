@ECHO OFF
SETLOCAL
PUSHD "%~dp0"
IF NOT EXIST .tools CALL init.cmd
IF NOT DEFINED VS140COMNTOOLS GOTO NEEDVS
IF NOT DEFINED DevEnvDir CALL "%VS140COMNTOOLS%\VsDevCmd.bat"
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
GOTO END
:NEEDVS
ECHO Install Visual Studio 2015 to build.
GOTO END
:END
POPD
ENDLOCAL
