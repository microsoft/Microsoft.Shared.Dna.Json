@ECHO OFF
SETLOCAL
PUSHD "%~dp0"
IF NOT EXIST .tools CALL init.cmd
msbuild -help >NUL 2>&1
IF ERRORLEVEL 9009 GOTO NEEDVS
vstest.console -? >NUL 2>&1
IF ERRORLEVEL 9009 GOTO NEEDVS
.tools\nuget restore
msbuild build.proj /verbosity:n /clp:ShowCommandLine /m:%NUMBER_OF_PROCESSORS% /nr:false /fl /flp:LogFile=MSBuild.log;Verbosity=diag;ShowTimestamp
FOR %%I IN (Debug Release) DO (
  FOR /R %%J IN (bin\%%~I\*.Test.dll) DO (
    vstest.console "%%~J"
  )
)
GOTO END
:NEEDVS
ECHO Run %~nx0 from a Visual Studio developer command prompt.
GOTO END
:END
POPD
ENDLOCAL
