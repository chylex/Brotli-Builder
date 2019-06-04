This is a WIP university project that implements libraries and applications that work with [Brotli](https://github.com/google/brotli) compression. Do not consider all APIs to be stable yet, refactoring and improvements to the API and serialization will come later this year.

Before documentation and the APIs themselves reach a more finished state, I would recommend exploring compressed files in *BrotliBuilder*, and understanding the example implementations in *BrotliImpl*.

# Projects

| Project | Type | Framework | Description |
| ------- | :--: | --------- | ----------- |
| BrotliLib | Library | .NET&nbsp;Standard&nbsp;2.0 | APIs for Brotli structure and serialization; main dependency |
| BrotliImpl | Library | .NET&nbsp;Standard&nbsp;2.0 | Example implementations of *encoders* and *transformers* |
| BrotliBuilder | WinForms&nbsp;App | .NET&nbsp;Framework&nbsp;4.7.1 | GUI for analysis of Brotli-compressed files & static dictionary |
| BrotliCalc | Console&nbsp;App | .NET&nbsp;Core&nbsp;2.0 | CLI for batch file processing, analysis, and statistics |

The key principle behind *BrotliLib* is representing the structure of a compressed file using a structure of objects, which is easy to analyze and manipulate.

A cursory glance at the [Brotli format specification](https://tools.ietf.org/html/rfc7932) is recommended before looking into this project. Classes in *BrotliLib* representing the object structure, as well as the labeled bit stream in *BrotliBuilder*, can greatly help understand the specification and the principles behind how Brotli and its decompression process works.

The main library can regenerate parts of the object structure, and serialize it into a Brotli bit stream. Note that some of these processes don't follow the official compressor - the library has no concept of compression levels, and some information is lost during the conversion between the bit stream and its object structure, which may not be regenerated in the same way. There is more work to be done in improvements to the serialization process, and allowing some parts of it to be controlled.

## Guide to Brotli Builder

The application lets you open a compressed file or use one of the example encoders from *BrotliImpl*, as well as apply any of the example transformers once the file is loaded.

![Image of main window](https://repo.chylex.com/brotli-builder-markers.png)

All text fields in the main window support common shortcuts for navigation and selection. Additional shortcuts include:

 - `Ctrl` + `F` to search in text
 - `Ctrl` + `Mouse Wheel Up/Down` to change font size
 - `Ctrl` + `+/-` to change font size
 - `Ctrl` + `0` to reset font size

The bit stream text fields let you navigate the color-coded bit groups:

 - `Ctrl` + `Left Arrow` to jump to previous bit group
 - `Ctrl` + `Right Arrow` to jump to next bit group

# Build Instructions

The project is written primarily in C# and built using Visual Studio 2019. Make sure you have the following components:

 - **.NET Framework 4.7.1 SDK**
 - **[.NET Core 2.0 SDK](https://dotnet.microsoft.com/download)**
 - **F# desktop language support** (optional, only required for unit tests)
