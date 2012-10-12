
cd ..

xcopy ..\NetSparkle\NetSparkle\Release\lib\net40-full\* externals /R /H /E /Y
xcopy ..\SharpZip\bin\* externals /R /H /E /Y

del /F /Q externals\*.xml

pause
