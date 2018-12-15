﻿namespace UnitTests.Brotli.TestElements

open Xunit
open System
open BrotliLib.IO
open BrotliLib.Brotli.Components
open BrotliLib.Brotli.Components.Header
open BrotliLib.Brotli.Components.Utils
open BrotliLib.Huffman


module Helper =
    let convert (obj: 'a) (context: 'b) (serializer: IBitSerializer<'a, 'b>) =
        let stream = BitStream()
        serializer.ToBits(stream.GetWriter(), obj, context)
        serializer.FromBits(stream.GetReader(), context)


module WindowSize =
    let values: obj array seq = WindowSize.ValidValues |> Seq.map (fun value -> [| value |])

    [<Theory>]
    [<MemberData("values")>]
    let ``converting to and from bits yields same object`` (ws: WindowSize) =
        Assert.Equal(ws, Helper.convert ws NoContext.Value WindowSize.Serializer)

    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(9)>]
    [<InlineData(25)>]
    let ``constructing window size with invalid amount of bits throws exception`` (wbits: int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> WindowSize(wbits) |> ignore)


module DataLength =
    let values = seq {
        yield! seq { 0..65536 } // all values that fit into 0-4 nibbles
        yield 65537    // 5 nibbles
        yield 1048576  // 5 nibbles
        yield 1048577  // 6 nibbles
        yield 16777216 // 6 nibbles
    }

    [<Fact>]
    let ``converting to and from bits yields same object`` () =
        for bytes in values do // ReSharper doesn't play nice with enumeration disabled
            let dl = DataLength(bytes)
            Assert.Equal(dl, Helper.convert dl NoContext.Value DataLength.Serializer)

    [<Theory>]
    [<InlineData(0, 0)>]
    [<InlineData(1, 4)>]
    [<InlineData(16, 4)>]
    [<InlineData(256, 4)>]
    [<InlineData(4096, 4)>]
    [<InlineData(65536, 4)>]
    [<InlineData(65537, 5)>]
    [<InlineData(1048576, 5)>]
    [<InlineData(1048577, 6)>]
    [<InlineData(16777216, 6)>]
    let ``different lengths use the fewest nibbles necessary`` (bytes: int, nibbles: int) =
        Assert.Equal(nibbles, DataLength(bytes).ChunkNibbles)

    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(16777217)>]
    let ``constructing data length with invalid amount of bytes throws exception`` (bytes: int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> DataLength(bytes) |> ignore)


module VariableLength11Code =
    let values = seq { 1..256 }

    [<Fact>]
    let ``converting to and from bits yields same object`` () =
        for value in values do
            let code = VariableLength11Code(value)
            Assert.Equal(code, Helper.convert code NoContext.Value VariableLength11Code.Serializer)
        
    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(0)>]
    [<InlineData(257)>]
    let ``constructing variable length code with invalid value throws exception`` (value: int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> VariableLength11Code(value) |> ignore)


module DistanceParameters =
    let cartesian a b = [for va in a do for vb in b -> (va, vb)]

    let postfix = seq { 0uy..3uy }
    let directbits = seq { 0uy..15uy }

    [<Fact>]
    let ``converting to and from bits yields same object`` () =
        for (pb, db) in cartesian postfix directbits do
            let parameters = DistanceParameters(pb, db)
            Assert.Equal(parameters, Helper.convert parameters NoContext.Value DistanceParameters.Serializer)
        
    [<Theory>]
    [<InlineData(4, 0)>]
    [<InlineData(0, 16)>]
    [<InlineData(4, 16)>]
    let ``constructing distance parameters with invalid value throws exception`` (pb: byte, db: byte) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> DistanceParameters(pb, db) |> ignore)


module HuffmanTree =
    let simple : obj array seq = seq {
        yield [| HuffmanTree(HuffmanNode.Leaf('a')) |]
        
        yield [| HuffmanTree(HuffmanGenerator<char>.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 1uy)
            HuffmanGenerator<_>.Entry('b', 1uy)
        |])) |]

        yield [| HuffmanTree(HuffmanGenerator<char>.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 1uy)
            HuffmanGenerator<_>.Entry('b', 2uy)
            HuffmanGenerator<_>.Entry('c', 2uy)
        |])) |]

        yield [| HuffmanTree(HuffmanGenerator<char>.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 2uy)
            HuffmanGenerator<_>.Entry('b', 2uy)
            HuffmanGenerator<_>.Entry('c', 2uy)
            HuffmanGenerator<_>.Entry('d', 2uy)
        |])) |]

        yield [| HuffmanTree(HuffmanGenerator<char>.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 1uy)
            HuffmanGenerator<_>.Entry('b', 2uy)
            HuffmanGenerator<_>.Entry('c', 3uy)
            HuffmanGenerator<_>.Entry('d', 3uy)
        |])) |]
    }

    let complex : obj array seq = seq {
        yield [| HuffmanTree(HuffmanGenerator<char>.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 1uy)
            HuffmanGenerator<_>.Entry('b', 2uy)
            HuffmanGenerator<_>.Entry('c', 3uy)
            HuffmanGenerator<_>.Entry('d', 4uy)
            HuffmanGenerator<_>.Entry('e', 4uy)
        |])) |]

        yield [| HuffmanTree(HuffmanGenerator<char>.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('c', 1uy)
            HuffmanGenerator<_>.Entry('a', 3uy)
            HuffmanGenerator<_>.Entry('b', 3uy)
            HuffmanGenerator<_>.Entry('e', 3uy)
            HuffmanGenerator<_>.Entry('d', 4uy)
            HuffmanGenerator<_>.Entry('f', 4uy)
        |])) |]

        yield [| HuffmanTree(HuffmanGenerator<char>.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 3uy)
            HuffmanGenerator<_>.Entry('b', 3uy)
            HuffmanGenerator<_>.Entry('c', 3uy)
            HuffmanGenerator<_>.Entry('d', 3uy)
            HuffmanGenerator<_>.Entry('e', 3uy)
            HuffmanGenerator<_>.Entry('f', 3uy)
            HuffmanGenerator<_>.Entry('g', 3uy)
            HuffmanGenerator<_>.Entry('h', 4uy)
            HuffmanGenerator<_>.Entry('i', 4uy)
        |])) |]
    }

    let context = HuffmanTree<char>.Context(AlphabetSize(256), (fun value -> char value), (fun symbol -> int symbol))

    [<Theory>]
    [<MemberData("simple")>]
    let ``converting to and from bits yields same object (simple)`` (tree: HuffmanTree<char>) =
        Assert.Equal<HuffmanTree<_>>(tree, Helper.convert tree context HuffmanTree<_>.Serializer)

    [<Theory>]
    [<MemberData("complex")>]
    let ``converting to and from bits yields same object (complex)`` (tree: HuffmanTree<char>) =
        Assert.Equal<HuffmanTree<_>>(tree, Helper.convert tree context HuffmanTree<_>.Serializer)


module LiteralContextMode =
    let values: obj array seq = Enum.GetValues(typeof<LiteralContextMode>) :?> (LiteralContextMode[]) |> Seq.map(fun value -> [| value |])

    [<Theory>]
    [<MemberData("values")>]
    let ``converting to and from bits yields same object`` (mode: LiteralContextMode) =
        Assert.Equal(mode, Helper.convert mode NoContext.Value LiteralContextModes.Serializer)
