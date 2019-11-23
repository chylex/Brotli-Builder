namespace UnitTests.Brotli.TestElements

open Xunit
open System
open BrotliLib.Brotli.Components
open BrotliLib.Brotli.Components.Data
open BrotliLib.Brotli.Components.Header
open BrotliLib.Brotli.Components.Utils
open BrotliLib.Collections.Huffman
open BrotliLib.Numbers
open BrotliLib.Serialization


module Helper =
    let convert (obj: 'a) (context: 'b) (serializer: BitSerializer<'a, 'b>) (deserializer: BitDeserializer<'a, 'b>) =
        let stream = BitStream()
        serializer.Invoke(stream.GetWriter(), obj, context)
        deserializer.Invoke(stream.GetReader(), context)

    let cartesian a b = [for va in a do for vb in b -> (va, vb)]


module WindowSize =
    let values: obj array seq = WindowSize.BitsRange.Values |> Seq.map (fun value -> [| WindowSize(value) |])

    [<Theory>]
    [<MemberData("values")>]
    let ``converting to and from bits yields same object`` (ws: WindowSize) =
        Assert.Equal(ws, Helper.convert ws NoContext.Value WindowSize.Serialize WindowSize.Deserialize)

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
            Assert.Equal(dl, Helper.convert dl NoContext.Value DataLength.Serialize DataLength.Deserialize)

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
            Assert.Equal(code, Helper.convert code NoContext.Value VariableLength11Code.Serialize VariableLength11Code.Deserialize)
        
    [<Theory>]
    [<InlineData(-1)>]
    [<InlineData(0)>]
    [<InlineData(257)>]
    let ``constructing variable length code with invalid value throws exception`` (value: int) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> VariableLength11Code(value) |> ignore)


module DistanceParameters =
    let postfix = seq { 0uy..3uy }
    let directbits = seq { 0uy..15uy }

    [<Fact>]
    let ``converting to and from bits yields same object`` () =
        for (pb, db) in Helper.cartesian postfix directbits do
            let parameters = DistanceParameters(pb, db)
            Assert.Equal(parameters, Helper.convert parameters NoContext.Value DistanceParameters.Serialize DistanceParameters.Deserialize)
        
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
        Assert.Equal<HuffmanTree<_>>(tree, Helper.convert tree context HuffmanTree<_>.Serialize HuffmanTree<_>.Deserialize)

    [<Theory>]
    [<MemberData("complex")>]
    let ``converting to and from bits yields same object (complex)`` (tree: HuffmanTree<char>) =
        Assert.Equal<HuffmanTree<_>>(tree, Helper.convert tree context HuffmanTree<_>.Serialize HuffmanTree<_>.Deserialize)


module LiteralContextMode =
    let values: obj array seq = Enum.GetValues(typeof<LiteralContextMode>) :?> (LiteralContextMode[]) |> Seq.map(fun value -> [| value |])

    [<Theory>]
    [<MemberData("values")>]
    let ``converting to and from bits yields same object`` (mode: LiteralContextMode) =
        Assert.Equal(mode, Helper.convert mode NoContext.Value LiteralContextModes.Serialize LiteralContextModes.Deserialize)


module InsertCopyLengths =
    let icCodes = seq { 0..703 } |> Seq.map(fun value -> InsertCopyLengthCode(value)) |> Seq.toArray

    let insertRanges = [|
        seq { 0..0 }
        seq { 1..1 }
        seq { 2..2 }
        seq { 3..3 }
        seq { 4..4 }
        seq { 5..5 }
        seq { 6..7 }
        seq { 8..9 }
        seq { 10..13 }
        seq { 14..17 }
        seq { 18..25 }
        seq { 26..33 }
        seq { 34..49 }
        seq { 50..65 }
        seq { 66..97 }
        seq { 98..129 }
        seq { 130..193 }
        seq { 194..321 }
        seq { 322..577 }
        seq { 578..1089 }
        seq { 1090..2113 }
        seq { yield! seq { 2114..2200 }; yield! seq { 6100..6209 } }
        seq { yield! seq { 6210..6300 }; yield! seq { 22000..22593 } }
        seq { yield! seq { 22594..23000 }; yield! seq { 16799000..16799809 } }
    |]

    let copyRanges = [|
        seq { 2..2 }
        seq { 3..3 }
        seq { 4..4 }
        seq { 5..5 }
        seq { 6..6 }
        seq { 7..7 }
        seq { 8..8 }
        seq { 9..9 }
        seq { 10..11 }
        seq { 12..13 }
        seq { 14..17 }
        seq { 18..21 }
        seq { 22..29 }
        seq { 30..37 }
        seq { 38..53 }
        seq { 54..69 }
        seq { 70..101 }
        seq { 102..133 }
        seq { 134..197 }
        seq { 198..325 }
        seq { 326..581 }
        seq { 582..1093 }
        seq { 1094..2117 }
        seq { yield! seq { 2118..2200 }; yield! seq { 16779000..16779333 } }
    |]

    let icCodesObj : obj array seq = seq {
        yield! icCodes |> Seq.chunkBySize 32 |> Seq.map(fun (icCode) -> [| box icCode |])
    }

    let insertRangesObj : obj array seq = seq {
        yield! insertRanges |> Seq.indexed |> Seq.map(fun (index, seq) -> [| box index; box seq |])
    }

    let copyRangesObj : obj array seq = seq {
        yield! copyRanges |> Seq.indexed |> Seq.map(fun (index, seq) -> [| box index; box seq |])
    }

    [<Theory>]
    [<MemberData("icCodesObj")>]
    let ``converting insert and copy codes to and from compacted codes is consistent`` (icCodeRange: InsertCopyLengthCode seq) =
        for original in icCodeRange do
            let dczStrategy =
                match original.UseDistanceCodeZero with
                | true  -> DistanceCodeZeroStrategy.ForceEnabled
                | false -> DistanceCodeZeroStrategy.Disable
            
            let reconstructed = InsertCopyLengthCode(original.InsertCode, original.CopyCode, dczStrategy)
            Assert.Equal(original.CompactedCode, reconstructed.CompactedCode)
    
    [<Theory>]
    [<MemberData("insertRangesObj")>]
    let ``converting insert lengths to and from bits yields same object`` (insertCode: int, insertRange: int seq) =
        let validCodes = icCodes |> Seq.filter(fun icCode -> icCode.InsertCode = insertCode) |> Seq.toArray
        Assert.Equal((if insertCode < 8 then 40 else 24), validCodes.Length)

        for insertLength in insertRange do
            for icCode in validCodes do
                let lengths = InsertCopyLengths(insertLength, copyRanges.[icCode.CopyCode] |> Seq.head)

                Assert.True(lengths.CanEncodeUsing(icCode))
                Assert.Equal(lengths, Helper.convert lengths icCode InsertCopyLengths.Serialize InsertCopyLengths.Deserialize)
    
    [<Theory>]
    [<MemberData("copyRangesObj")>]
    let ``converting copy lengths to and from bits yields same object`` (copyCode: int, copyRange: int seq) =
        let validCodes = icCodes |> Seq.filter(fun icCode -> icCode.CopyCode = copyCode) |> Seq.toArray
        Assert.Equal((if copyCode < 16 then 32 else 24), validCodes.Length)

        for copyLength in copyRange do
            for icCode in validCodes do
                let lengths = InsertCopyLengths(insertRanges.[icCode.InsertCode] |> Seq.head, copyLength)

                Assert.True(lengths.CanEncodeUsing(icCode))
                Assert.Equal(lengths, Helper.convert lengths icCode InsertCopyLengths.Serialize InsertCopyLengths.Deserialize)


module DistanceCodeComplex =
    let distanceParameters : obj array seq = Helper.cartesian DistanceParameters.postfix DistanceParameters.directbits |> Seq.map(fun (pb, db) -> [| DistanceParameters(pb, db) |])
    
    let valueRanges = [|
        seq { 1..10000 }
        seq { 100000..100100 }
        seq { 500000..500100 }
        seq { 17025000..1702600 }
    |]

    [<Theory>]
    [<MemberData("distanceParameters")>]
    let ``constructed complex distance codes can encode the value they were generated from`` (parameters: DistanceParameters) =
        for range in valueRanges do
            for value in range do
                let offset = value + parameters.DirectCodeCount;
                let code = DistanceCode.Complex.ForValue(&parameters, offset)

                Assert.True(code.CanEncodeValue(null, offset))
