@echo off

set PROTOGEN="%HOME%\..\bin\protogen.exe"

%PROTOGEN% -i:Protocol.proto -o:Protocol.cs
%PROTOGEN% -i:VoteProtocol.proto -o:VoteProtocol.cs
%PROTOGEN% -i:CommenterProtocol.proto -o:CommenterProtocol.cs

if not %ERRORLEVEL%==0 goto on_error

set PROLOGUE=#pragma warning disable 1591
echo %PROLOGUE% > ..\Protocol.cs
type Protocol.cs >> ..\Protocol.cs

echo %PROLOGUE% > ..\Vote\VoteProtocol.cs
type VoteProtocol.cs >> ..\Vote\VoteProtocol.cs

echo %PROLOGUE% > ..\Commenter\CommenterProtocol.cs
type CommenterProtocol.cs >> ..\Commenter\CommenterProtocol.cs

rem �t�@�C���폜
del Protocol.cs
del VoteProtocol.cs
del CommenterProtocol.cs

echo OK

goto on_end

:on_error
echo �G���[���������܂����B

:on_end
pause
