@echo off

:: Check if the correct number of arguments are provided
if "%~2"=="" (
    echo Usage: runner.bat hotel_ids destination_ids
    exit /b 1
)

:: Assign the positional arguments to variables
set HOTEL_IDS=%1
set DESTINATION_IDS=%2

:: Run the C# executable with the provided arguments
.\bin\Release\net8.0\win-x64\publish\Test.exe %HOTEL_IDS% %DESTINATION_IDS%