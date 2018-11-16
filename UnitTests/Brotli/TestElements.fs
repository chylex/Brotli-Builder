namespace UnitTests.Brotli.TestElements

open Xunit
open System
open BrotliLib.IO


module Helper =
    let convert (obj: 'a) (context: 'b) (serializer: IBitSerializer<'a, 'b>) =
        let stream = BitStream()
        serializer.ToBits(stream.GetWriter(), obj, context)
        serializer.FromBits(stream.GetReader(), context)

