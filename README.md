# WITSML Converter

This repo provides a library and tool for converting WITSML documents between versions 1.4, 2.0, and 2.1 of the WITSML standard.

These conversions are implemented as XML transformations with additional processing being done in code.

## Requirements

The library, Petrolink.WitsmlConverter, is built against .NET Standard 2.0. The command line tool is built against .NET 6.0 (LTS).

All projects are written using C# 11 and require either Microsoft Visual Studio/Build Tools 2022 version 17.4, or the .NET 7 SDK to build.

## Usage

The Petrolink.WitsmlConverter library can be included as a dependency for a C# project. This library provides a static WitsmlTransformer class that can be used to perform the transformations.

If a command line tool is needed, the Petrolink.WitsmlConverter.Tool project, which builds the WitsmlConvert.exe file, can be used. This tool is merely a wrapper around the WitsmlTransformer class. Execute `WitsmlConvert.exe --help` for usage details.

## Building

The repository provides a Visual Studio 2022 solution containing 4 projects:
* Petrolink.WitsmlConverter - The WITSML converter library.
* Petrolink.WitsmlConverter.Mappings - Contains generated mapping files used by Petrolink.WitsmlConverter.
* Petrolink.WitsmlConverter.Tests - Tests for Petrolink.WitsmlConverter.
* Petrolink.WitsmlConverter.Tool - The command line wrapper around Petrolink.WitsmlConverter. Builds WitsmlConvert.exe.

Simply open `WitsmlConverter.sln` in Visual Studio and build the project, or use the `dotnet build` command to build the project.