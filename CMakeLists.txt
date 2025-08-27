cmake_minimum_required(VERSION 3.16)
project(CsharpFlow VERSION 1.0.0 LANGUAGES CSharp)

# Set C# language version and framework
set(CMAKE_CSharp_FLAGS "/langversion:8.0")
set(DOTNET_TARGET_FRAMEWORK "net48" CACHE STRING "Target .NET Framework version")
set(CMAKE_DOTNET_TARGET_FRAMEWORK_VERSION "v4.8")

# Configure build type
if(NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE "Release" CACHE STRING "Choose the type of build." FORCE)
    set_property(CACHE CMAKE_BUILD_TYPE PROPERTY STRINGS "Debug" "Release" "RelWithDebInfo" "MinSizeRel")
endif()

# Output directories
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)

# Create Bin directory structure matching the existing project
set(BIN_DEBUG_DIR "${CMAKE_SOURCE_DIR}/Bin/Debug")
set(BIN_RELEASE_DIR "${CMAKE_SOURCE_DIR}/Bin/Release")

file(MAKE_DIRECTORY ${BIN_DEBUG_DIR})
file(MAKE_DIRECTORY ${BIN_RELEASE_DIR})

# Find .NET/Mono compilers
find_program(DOTNET_EXECUTABLE dotnet)
find_program(MSBUILD_EXECUTABLE msbuild)
find_program(XBUILD_EXECUTABLE xbuild)

if(DOTNET_EXECUTABLE)
    message(STATUS "Found .NET CLI: ${DOTNET_EXECUTABLE}")
    set(BUILD_TOOL "dotnet")
elseif(MSBUILD_EXECUTABLE)
    message(STATUS "Found MSBuild: ${MSBUILD_EXECUTABLE}")
    set(BUILD_TOOL "msbuild")
elseif(XBUILD_EXECUTABLE)
    message(STATUS "Found xbuild: ${XBUILD_EXECUTABLE}")
    set(BUILD_TOOL "xbuild")
else()
    message(FATAL_ERROR "No suitable .NET build tool found. Please install dotnet-sdk, mono-devel, or mono-xbuild")
endif()

# Collect source files
file(GLOB_RECURSE FLOW_SOURCES 
    "Interfaces/*.cs"
    "Impl/*.cs" 
    "Logger/*.cs"
    "Properties/*.cs"
)

file(GLOB_RECURSE TEST_SOURCES
    "TestFlow/*.cs"
)

# Create Flow library target
add_library(Flow SHARED ${FLOW_SOURCES})

# Set C# specific properties
set_target_properties(Flow PROPERTIES
    VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.8"
    VS_DOTNET_REFERENCES "System;System.Core;System.Collections.Concurrent"
    FOLDER "Libraries"
)

# Conditional compilation symbols
if(UNITY_BUILD)
    target_compile_definitions(Flow PRIVATE UNITY_EDITOR UNITY_2019_1_OR_NEWER)
    message(STATUS "Building with Unity support")
endif()

# Create test executable
add_executable(TestFlow ${TEST_SOURCES})

set_target_properties(TestFlow PROPERTIES
    VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.8"
    VS_DOTNET_REFERENCES "System;System.Core;nunit.framework"
    FOLDER "Tests"
)

target_link_libraries(TestFlow Flow)

# Custom build commands for different build tools
if(BUILD_TOOL STREQUAL "dotnet")
    # Use dotnet CLI
    add_custom_target(build_flow ALL
        COMMAND ${DOTNET_EXECUTABLE} build Flow.csproj -c $<CONFIG> -o ${CMAKE_SOURCE_DIR}/Bin/$<CONFIG>
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        COMMENT "Building Flow library with dotnet"
        VERBATIM
    )
    
    add_custom_target(build_tests ALL
        COMMAND ${DOTNET_EXECUTABLE} build TestFlow/TestFlow.csproj -c $<CONFIG> -o ${CMAKE_SOURCE_DIR}/Bin/$<CONFIG>
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        COMMENT "Building Flow tests with dotnet"
        DEPENDS build_flow
        VERBATIM
    )

elseif(BUILD_TOOL STREQUAL "msbuild")
    # Use MSBuild
    add_custom_target(build_flow ALL
        COMMAND ${MSBUILD_EXECUTABLE} Flow.csproj /p:Configuration=$<CONFIG> /p:OutputPath=${CMAKE_SOURCE_DIR}/Bin/$<CONFIG>/
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        COMMENT "Building Flow library with MSBuild"
        VERBATIM
    )
    
    add_custom_target(build_tests ALL
        COMMAND ${MSBUILD_EXECUTABLE} TestFlow/TestFlow.csproj /p:Configuration=$<CONFIG> /p:OutputPath=${CMAKE_SOURCE_DIR}/Bin/$<CONFIG>/
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        COMMENT "Building Flow tests with MSBuild"
        DEPENDS build_flow
        VERBATIM
    )

elseif(BUILD_TOOL STREQUAL "xbuild")
    # Use xbuild (legacy Mono)
    add_custom_target(build_flow ALL
        COMMAND ${XBUILD_EXECUTABLE} Flow.csproj /p:Configuration=$<CONFIG> /p:OutputPath=${CMAKE_SOURCE_DIR}/Bin/$<CONFIG>/
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        COMMENT "Building Flow library with xbuild"
        VERBATIM
    )
    
    add_custom_target(build_tests ALL
        COMMAND ${XBUILD_EXECUTABLE} TestFlow/TestFlow.csproj /p:Configuration=$<CONFIG> /p:OutputPath=${CMAKE_SOURCE_DIR}/Bin/$<CONFIG>/
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        COMMENT "Building Flow tests with xbuild"
        DEPENDS build_flow
        VERBATIM
    )
endif()

# Test execution targets
if(DOTNET_EXECUTABLE)
    add_custom_target(test
        COMMAND ${DOTNET_EXECUTABLE} test TestFlow/TestFlow.csproj -c $<CONFIG> --no-build --logger "console;verbosity=normal"
        WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
        COMMENT "Running Flow unit tests with dotnet"
        DEPENDS build_tests
        VERBATIM
    )
else()
    # Try to find NUnit console runner for Mono
    find_program(NUNIT_CONSOLE nunit-console)
    if(NUNIT_CONSOLE)
        add_custom_target(test
            COMMAND ${NUNIT_CONSOLE} ${CMAKE_SOURCE_DIR}/Bin/$<CONFIG>/TestFlow.dll
            WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
            COMMENT "Running Flow unit tests with nunit-console"
            DEPENDS build_tests
            VERBATIM
        )
    else()
        message(STATUS "NUnit console runner not found. Tests can be run manually.")
    endif()
endif()

# Install targets
install(FILES 
    ${CMAKE_SOURCE_DIR}/Bin/$<CONFIG>/Flow.dll
    ${CMAKE_SOURCE_DIR}/Bin/$<CONFIG>/Flow.pdb
    DESTINATION lib
    OPTIONAL
)

install(DIRECTORY 
    ${CMAKE_SOURCE_DIR}/Interfaces/
    DESTINATION include/Flow/Interfaces
    FILES_MATCHING PATTERN "*.cs"
)

# Package configuration
set(CPACK_PACKAGE_NAME "CsharpFlow")
set(CPACK_PACKAGE_VERSION ${PROJECT_VERSION})
set(CPACK_PACKAGE_DESCRIPTION_SUMMARY "C# Coroutine Kernel Library")
set(CPACK_PACKAGE_VENDOR "Christian Schladetsch")
set(CPACK_PACKAGE_DESCRIPTION_FILE "${CMAKE_SOURCE_DIR}/Readme.md")
set(CPACK_RESOURCE_FILE_LICENSE "${CMAKE_SOURCE_DIR}/LICENSE.txt")
set(CPACK_PACKAGE_INSTALL_DIRECTORY "CsharpFlow")

if(UNIX)
    set(CPACK_GENERATOR "DEB;RPM;TGZ")
    set(CPACK_DEBIAN_PACKAGE_MAINTAINER "Christian Schladetsch")
    set(CPACK_DEBIAN_PACKAGE_DEPENDS "mono-runtime (>= 4.6.2) | dotnet-runtime-6.0")
    set(CPACK_RPM_PACKAGE_REQUIRES "mono-core >= 4.6.2")
endif()

include(CPack)

# Print configuration summary
message(STATUS "")
message(STATUS "=== CsharpFlow Build Configuration ===")
message(STATUS "Build tool: ${BUILD_TOOL}")
message(STATUS "Build type: ${CMAKE_BUILD_TYPE}")
message(STATUS "Target framework: ${DOTNET_TARGET_FRAMEWORK}")
message(STATUS "Source directory: ${CMAKE_SOURCE_DIR}")
message(STATUS "Binary directory: ${CMAKE_BINARY_DIR}")
message(STATUS "Output directory: ${CMAKE_SOURCE_DIR}/Bin")
if(UNITY_BUILD)
    message(STATUS "Unity support: ENABLED")
else()
    message(STATUS "Unity support: DISABLED (use -DUNITY_BUILD=ON to enable)")
endif()
message(STATUS "=====================================")
message(STATUS "")
message(STATUS "Build commands:")
message(STATUS "  cmake -B build")  
message(STATUS "  cmake --build build")
message(STATUS "  cmake --build build --target test")
message(STATUS "")