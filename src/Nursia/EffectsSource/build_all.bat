SET TwoMGFX="C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\2MGFX.exe"
SET FXC="C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x64\fxc.exe"

rem No defines
%TwoMGFX% DefaultEffect.fx ..\Resources\Effects\DefaultEffect.dx11.mgfxo /Profile:DirectX_11
%TwoMGFX% DefaultEffect.fx ..\Resources\Effects\DefaultEffect.ogl.mgfxo /Profile:OpenGL
%FXC% /T fx_2_0 /Fo ..\Resources\Effects\DefaultEffect.fxb DefaultEffect.fx

rem No lightning, one bone
%TwoMGFX% DefaultEffect.fx ..\Resources\Effects\DefaultEffect_BONES_1.dx11.mgfxo /Defines:BONES=1 /Profile:DirectX_11
%TwoMGFX% DefaultEffect.fx ..\Resources\Effects\DefaultEffect_BONES_1.ogl.mgfxo /Defines:BONES=1 /Profile:OpenGL
%FXC% /T fx_2_0 /D BONES=1 /Fo ..\Resources\Effects\DefaultEffect_BONES_1.fxb DefaultEffect.fx

rem No lightning, two bones
%TwoMGFX% DefaultEffect.fx ..\Resources\Effects\DefaultEffect_BONES_2.dx11.mgfxo /Defines:BONES=2 /Profile:DirectX_11
%TwoMGFX% DefaultEffect.fx ..\Resources\Effects\DefaultEffect_BONES_2.ogl.mgfxo /Defines:BONES=2 /Profile:OpenGL
%FXC% /T fx_2_0 /D BONES=2 /Fo ..\Resources\Effects\DefaultEffect_BONES_2.fxb DefaultEffect.fx

rem No lightning, four bones
%TwoMGFX% DefaultEffect.fx ..\Resources\Effects\DefaultEffect_BONES_4.dx11.mgfxo /Defines:BONES=4 /Profile:DirectX_11
%TwoMGFX% DefaultEffect.fx ..\Resources\Effects\DefaultEffect_BONES_4.ogl.mgfxo /Defines:BONES=4 /Profile:OpenGL
%FXC% /T fx_2_0 /D BONES=4 /Fo ..\Resources\Effects\DefaultEffect_BONES_4.fxb DefaultEffect.fx