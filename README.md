This is a WIP university project that implements libraries and applications that work with [Brotli](https://github.com/google/brotli) compression.

Do not consider all APIs to be stable yet. Before documentation and the APIs reach a more finished state, I would recommend **reading the rest of the README**, then exploring compressed files in BrotliBuilder, and understanding the example implementations in BrotliImpl.

| Project | Type | Framework | Description |
| ------- | :--: | --------- | ----------- |
| BrotliLib | Library | .NET&nbsp;Standard&nbsp;2.1 | APIs for Brotli structure and serialization; main dependency |
| BrotliImpl | Library | .NET&nbsp;Standard&nbsp;2.1 | Example implementations of *encoders* and *transformers* |
| BrotliBuilder | WinForms&nbsp;App | .NET&nbsp;Core&nbsp;3.0 | GUI for analysis of Brotli-compressed files & static dictionary |
| BrotliCalc | Console&nbsp;App | .NET&nbsp;Core&nbsp;3.0 | CLI for batch file processing, analysis, and statistics |

A cursory glance at the [Brotli format specification](https://tools.ietf.org/html/rfc7932) is recommended before looking into this project. Classes in *BrotliLib* representing the object structure, as well as the labeled bit stream in *BrotliBuilder*, can greatly help understand the specification and the principles behind how Brotli and its decompression process works.

# Guide to Brotli Builder

The application lets you open a compressed file or use one of the example implementations of encoders and transformers from BrotliImpl.

In addition, it also contains tools such as the **Static Dictionary** dialog, which lets you navigate and search the entire contents of Brotli's embedded dictionary.

![Image of main window](https://repo.chylex.com/brotli-builder-markers.png)

The bit stream field has color-coded groups. The currently selected group has a black background with white text.

Each group corresponds to a *marker* that summarizes its meaning. The selected group's marker is centered and highlighted in the left panel as you navigate inside the text field. Wherever possible, markers use names defined in the [Brotli format specification](https://tools.ietf.org/html/rfc7932) to make them easily searchable.

Opening a compressed file splits the bottom right panel (bit stream and output fields) into two sections labeled **Original** and **Generated**. The **Original** section has the opened file, the **Generated** section has the result of reserializing the generated object structure. Transformations are always applied to the **Generated** section. Note that as there's only one **Marker Info** panel, it will show markers that belong to the section of the most recently highlighted group.

The **Tools** menu lets you clone the contents of **Generated** section into the **Original** section and vice versa, and also compare the complete marker text between the two sections (with **WinMerge** detection to instantly show a diff).

All text fields in the main window support common shortcuts for navigation and selection. Additional shortcuts include:

- `Ctrl` + `F` to search in text
- `Ctrl` + `Mouse Wheel Up/Down` to change font size
- `Ctrl` + `+/-` to change font size
- `Ctrl` + `0` to reset font size

The bit stream text fields let you navigate the color-coded bit groups:

- `Ctrl` + `Left Arrow` to jump to previous bit group
- `Ctrl` + `Right Arrow` to jump to next bit group

# Explore the Library Code

The key principle behind the project is representing the individual components of a compressed file as an object structure, which is easy to analyze and manipulate.

As Brotli is a bit-oriented format, a `BitStream` provides a generic bit collection used across the whole main library. A `BrotliFileStructure` is the root of the object structure of a compressed file, containing:
 - `BrotliFileParameters` with the stream header (currently just the sliding window size) and the static dictionary
 - List of `MetaBlock` objects which hold the actual data in either an uncompressed or compressed form

The main library defines and provides means to the following operations:

| Operation | Converts From | Converts To | Meaning |
| --------- | ------------- | ----------- | ------- |
| **Deserialization** | `BitStream` | `BrotliFileStructure` | Reading a compressed file structure |
| **Serialization** | `BrotliFileStructure` | `BitStream` | Writing a compressed file structure |
| **Encoding** | `byte[]` | `BrotliFileStructure` | Constructing a compressed file structure |
| **Transforming** | `BrotliFileStructure` | `BrotliFileStructure` | Modifying a compressed file structure |

All of these operations are also available as streaming APIs, for when the entire structure does not need to be loaded into memory at once:
 - `BrotliFileReader` for deserialization
 - `BrotliFileWriter` for serialization
 - `BrotliFileStreamEncoder` for encoding
 - `BrotliFileStreamTransformer` for transforming

## Compression Pipeline

An encoder should focus on how to generate *insert&copy* commands and store them in a `MetaBlock` sequence. The *insert&copy* commands generate the uncompressed output from literals (bytes), backward references, and static dictionary references.

A transformer should focus on how to modify parts of the `MetaBlock` header, which changes the representation of *insert&copy* commands in the bit stream. Transformers may also take one `MetaBlock` and split it into multiple smaller ones. Transformers should not, however, modify a `MetaBlock` in a way that changes the uncompressed output.

While this doesn't exactly mimic the way official compressor works, the separated **Encode - (Transform)\* - Serialize** pipeline with `BrotliFileStructure` as the centerpiece lets you think about `MetaBlock` construction in small, focused steps.

It also allows opening a file created by the official compressor, and apply transformations to it to find a more compact representation of it. Please keep in mind that there is more work to be done in terms of consistency, please read the next section.

### Serialization & Transformation Caveat

Some information is lost during the deserialization process, and currently the serialization process does not provide enough control to regenerate that information in the same way.

In addition, performing any transformation inherently regenerates parts of the `MetaBlock` header.

- This is especially apparent with files generated by the official compressor's lowest quality settings, which often use pre-defined codes to save time
- These codes must be able to represent all possible values even if they end up unused, which increases compressed file size
- A `CompressedMetaBlockBuilder`, which facilitates rebuilding an existing `MetaBlock`, must throw these codes out and regenerate them in case a parameter change makes them invalid
- At this moment, there is no control over the regeneration strategy, so it's not possible to keep the inefficient pre-defined codes and the compressed file size is shrunk significantly

To only see the intended effect of a transformation without these side effects, first apply a *rebuild* transformation (`TransformRebuild` in BrotliImpl) to the opened file which only triggers the side effects. The rebuilt structure can then be used as a baseline for comparing subsequent transformations.

# Build from Source Code

The project is written primarily in C# and built using Visual Studio 2019. Make sure you have the following components:

- **[.NET Core 3.0 SDK](https://dotnet.microsoft.com/download)** (included in Visual Studio v16.3)
- **F# desktop language support** (optional, only required for unit tests)

BrotliCalc requires a brotli executable to enable the batch compression command. This repository includes a Windows build based on the [c435f06](https://github.com/google/brotli/commit/c435f06) commit in the `BrotliCalc/Resources/` folder. You can supply your own build either by replacing the file and rebuilding BrotliCalc, or by using the `-brotliexe` command line argument.

Publishing profiles are included for BrotliBuilder and BrotliCalc. The libraries can simply be built in Release configuration to generate the files.
