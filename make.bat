@echo off

if  "%~1"=="" (

call :find_msbuild

call :build_nlp_loader

call :build_solution

call :post_run_script

EXIT /B 0
)
if  "%~1"=="all" (

call :find_msbuild

call :build_nlp_loader

call :build_solution

call :post_run_script

EXIT /B 0
)
if  "%~1"=="BetterAW" (

call :find_msbuild

call :build_solution BetterAW

call :post_run_script

EXIT /B 0
)
if  "%~1"=="DictionaryHandler" (

call :find_msbuild

call :build_solution DictionaryHandler

call :post_run_script

EXIT /B 0
)
if  "%~1"=="nlp_loader" (

call :build_nlp_loader

call :post_run_script

EXIT /B 0
)
if  "%~1"=="sharp_injector" (

call :find_msbuild

call :build_solution sharp_injector

call :post_run_script

EXIT /B 0
)
if  "%~1"=="WinAPIHooks" (

call :find_msbuild

call :build_solution WinAPIHooks

call :post_run_script

EXIT /B 0
)
if  "%~1"=="clean" (

rmdir /s /q "bin"
rmdir /s /q "nlp_loader/build"

call :find_msbuild

call :build_solution Clean

EXIT /B 0
)
if  "%~1"=="Targets" (

echo --------------------------------------------------------
echo All available targets:
echo --------------------------------------------------------
echo all: Builds all targets
echo BetterAW: Builds the BetterAW library
echo DictionaryHandler: Builds the DictionaryHandler library
echo nlp_loader: Builds the nlp wrapper library
echo sharp_injector: Builds the C# entry point library
echo WinAPIHooks: Builds the C++ WinAPIHooks library
echo clean: Cleans the builds

EXIT /B 0
)
if  "%~1"=="Help" (

echo To build all run ./make.bat
echo To build a specific target run ./make.bat ^<target^>
echo All targets are listed under ./make.bat Targets

EXIT /B 0
)
EXIT /B 0

:build_nlp_loader
set origDir=%cd%
cd nlp_loader
if not exist "build" mkdir "build"
cd build
cmake ..
cmake --build . --config Release
cd %origDir%
EXIT /B 0

:find_msbuild
if not "%VSWHERE_LOCATION%"=="" (FOR /F "tokens=* USEBACKQ" %%F IN (`"%VSWHERE_LOCATION%" -latest -property installationPath`) DO (SET msbuild="%%F\MSBuild\Current\Bin\amd64\MSBuild.exe")) else (
    if not "%programfiles(x86)%" == "" (
        FOR /F "tokens=* USEBACKQ" %%F IN (`"%programfiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath`) DO (SET msbuild="%%F\MSBuild\Current\Bin\amd64\MSBuild.exe")
        ) else (
        FOR /F "tokens=* USEBACKQ" %%F IN (`"%programfiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath`) DO (SET msbuild="%%F\MSBuild\Current\Bin\amd64\MSBuild.exe")
        )
)
EXIT /B 0

:build_solution
if "%~1"=="" (%msbuild% BetterAppWriter.sln -property:Configuration=Debug /p:Platform=x86) else (%msbuild% BetterAppWriter.sln -property:Configuration=Debug /p:Platform=x86 -t:%~1)
EXIT /B 0

:get_all_build_files
set files=("nlp_loader\build\Release\nlp.dll" "Lib\nlp.dll" "sharp_injector\bin\Debug\Microsoft.Xaml.Behaviors.dll" "Microsoft.Xaml.Behaviors.dll" "sharp_injector\bin\Debug\sharp_injector.dll" "Lib\sharp_injector.dll" "sharp_injector\bin\Debug\Translations\Dansk.lang" "Lib\Translations\Dansk.lang")
EXIT /B 0

:post_run_script
if not exist "bin/Lib/Translations" (mkdir "bin/Lib/Translations")
call :get_all_build_files
setlocal EnableDelayedExpansion 
set /a counter=0
  FOR %%f IN %files% DO (
    if !counter! == 0 (
        set counter=1
        set "f0=%%~f"
    ) else (
        if !counter! == 1 (
            SET "f1=%%~f"
            if exist "!f0!" (copy "!f0!" "bin/!f1!")
            set /a counter=0
        )
    )
)
EXIT /B 0