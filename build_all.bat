dotnet --version
dotnet build build\Nursia.MonoGame.sln /p:Configuration=Release --no-incremental
call copy_zip_package_files.bat
rename "ZipPackage" "Nursia.%APPVEYOR_BUILD_VERSION%"
7z a Nursia.%APPVEYOR_BUILD_VERSION%.zip Nursia.%APPVEYOR_BUILD_VERSION%
