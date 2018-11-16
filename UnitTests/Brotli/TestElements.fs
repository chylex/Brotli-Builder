namespace UnitTests.Brotli.TestElements

open Xunit
open System
open BrotliLib.IO
open BrotliLib.Brotli.Components
open BrotliLib.Brotli.Components.Header


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
        Assert.Equal(ws, Helper.convert ws null WindowSize.Serializer)

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
            Assert.Equal(dl, Helper.convert dl null DataLength.Serializer)

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
