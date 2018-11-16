namespace UnitTests.Brotli.TestElements

open Xunit
open System
open BrotliLib.IO
open BrotliLib.Brotli.Components


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
