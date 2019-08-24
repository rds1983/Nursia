SET TwoMGFX="C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2MGFX.exe"
SET FXC="C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x64\fxc.exe"

SET output=%1
IF "%2" NEQ "" SET output=%output%_%2
IF "%3" NEQ "" SET output=%output%_%3
IF "%4" NEQ "" SET output=%output%_%3

SET defines=
IF "%2" NEQ "" SET defines=/Defines:%2
IF "%3" NEQ "" SET defines=%defines%;%3
IF "%4" NEQ "" SET defines=%defines%;%4

%TwoMGFX% %1.fx ..\Resources\Effects\%output%.dx11.mgfxo %defines% /Profile:DirectX_11
%TwoMGFX% %1.fx ..\Resources\Effects\%output%.ogl.mgfxo %defines% /Profile:OpenGL

SET defines=
IF "%2" NEQ "" SET defines=/D %2
IF "%3" NEQ "" SET defines=%defines% /D %3
IF "%4" NEQ "" SET defines=%defines% /D %4

%FXC% /T fx_2_0 %defines% /Fo ..\Resources\Effects\%output%.fxb %1.fx