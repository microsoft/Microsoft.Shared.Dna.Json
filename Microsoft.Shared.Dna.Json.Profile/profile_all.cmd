@ECHO OFF
ECHO Group,Profile,MeanElapsedMilliseconds,Valid
FOR %%I IN (JsonDna) DO (
  FOR %%J IN (ReadLogical ReadIntegral ReadFloat ReadText ReadArray ReadObject ReadComplex WriteLogical WriteIntegral WriteFloat WriteText WriteArray WriteObject WriteComplex) DO (
    Microsoft.Shared.Dna.Json.Profile.exe %%I %%J 100000
  )
)
