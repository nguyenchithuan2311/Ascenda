@echo off
setlocal enabledelayedexpansion

set firstElement=false
set lastElement=false
set count=0

for %%A in (%*) do (
    set /a count+=1
)

set first=%1
if "%first%" == "none" (
    set firstElement=true
)

set last=%*
for %%A in (%*) do set last=%%A

set result=

if "%last%" == "none" (
    set lastElement=true
)
if "%firstElement%" == "true" (
    if "%lastElement%" == "true" (
        .\bin\Release\net8.0\win-x64\publish\Test.exe none none
    ) else (
        set i=1
        set result=
        for %%A in (%*) do (
            if !i! equ 1 (
                set result=%%A
            ) else (
                set result=!result!,%%A
            )
            set /a i+=1
        )
        .\bin\Release\net8.0\win-x64\publish\Test.exe !result!
    )
) else (
    if "%lastElement%" == "true" (
        set i=1
        set result=
        for %%A in (%*) do (
        if !i! equ 1 (
                        set result=%%A
                    ) else (
                        set result=!result!,%%A
                    )
                    set /a i+=1
        )
        .\bin\Release\net8.0\win-x64\publish\Test.exe !result!
    ) else (
        set /a midIndex=count / 2 + 1
        set i=1
        set result=
        
        for %%A in (%*) do (
            if !i! equ 1 (
                set result=%%A
            ) else if !i! equ !midIndex! (
                set result=!result! %%A
            ) else (
                set result=!result!,%%A
            )
            set /a i+=1
        )
        .\bin\Release\net8.0\win-x64\publish\Test.exe !result!
    )
)
