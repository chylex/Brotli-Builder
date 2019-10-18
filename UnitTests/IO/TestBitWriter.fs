namespace UnitTests.IO.TestBitWriter

open Xunit
open System
open BrotliLib.IO


module WriteBits =
    [<Fact>]
    let ``writing bit by bit to existing stream yields correct text representation`` () =
        let stream = BitStream("01")
        let writer = stream.GetWriter()
        writer.WriteBit(true);
        writer.WriteBit(true);
        writer.WriteBit(false);
        writer.WriteBit(true);

        Assert.Equal("011101", stream.ToString())
        
    [<Fact>]
    let ``writing a bit stream to existing stream yields correct text representation`` () =
        let stream = BitStream("01")
        let writer = stream.GetWriter()
        writer.WriteBits(BitStream("1101"))
        
        Assert.Equal("011101", stream.ToString())


module WriteChunk =
    [<Theory>]
    [<InlineData("", 0, 0b0)>]
    [<InlineData("0", 1, 0b0)>]
    [<InlineData("1", 1, 0b1)>]
    [<InlineData("01", 2, 0b10)>]
    [<InlineData("011", 3, 0b110)>]
    [<InlineData("1010", 4, 0b0101)>]
    [<InlineData("11110000", 8, 0b1111)>]
    [<InlineData("00000000111111110000000011111111", 32, 0b11111111000000001111111100000000)>]
    let ``writing a chunk yields correct text and byte array representation`` (expected: string, count: int, value: int) =
        let stream = BitStream()
        let writer = stream.GetWriter()
        writer.WriteChunk(count, value)

        Assert.Equal(expected, stream.ToString())
        Assert.Equal(value, stream.GetReader().NextChunk(count))
        
    [<Fact>]
    let ``writing 33 bit chunk throws exception`` () =
        let writer = BitStream().GetWriter()

        Assert.Throws<ArgumentOutOfRangeException>(fun () -> writer.WriteChunk(33, 0b0) |> ignore)


module AlignToByteBoundary =
    [<Fact>]
    let ``writer is already aligned at the start of stream`` () =
        let stream = BitStream()
        let writer = stream.GetWriter()

        writer.AlignToByteBoundary()
        Assert.Equal(0, stream.Length)

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(3)>]
    [<InlineData(4)>]
    [<InlineData(5)>]
    [<InlineData(6)>]
    [<InlineData(7)>]
    let ``writer aligns itself to end of first byte`` (write: int) =
        let stream = BitStream()
        let writer = stream.GetWriter()

        for _ in 0..write do
            writer.WriteBit(true) |> ignore

        writer.AlignToByteBoundary()
        Assert.Equal(8, stream.Length)

    [<Fact>]
    let ``writer is already aligned at the start of second byte`` () =
        let stream = BitStream()
        let writer = stream.GetWriter()
        
        for _ in 1..8 do
            writer.WriteBit(true) |> ignore

        writer.AlignToByteBoundary()
        Assert.Equal(8, stream.Length)


module WriteAlignedBytes =
    let bytes : obj array seq = seq {
        yield Array.map box [| 0b0uy |]
        yield Array.map box [| 0b1uy |]
        yield Array.map box [| 120uy; 0uy; 255uy; 33uy |]
        yield Array.map box [| 1uy; 2uy; 3uy; 4uy; 5uy; 6uy; 7uy; 8uy; 9uy; 10uy; 11uy; 12uy; 13uy; 14uy; 15uy |]
        yield Array.map box [| 1uy; 2uy; 3uy; 4uy; 5uy; 6uy; 7uy; 8uy; 9uy; 10uy; 11uy; 12uy; 13uy; 14uy; 15uy; 16uy |]
        yield Array.map box [| 1uy; 2uy; 3uy; 4uy; 5uy; 6uy; 7uy; 8uy; 9uy; 10uy; 11uy; 12uy; 13uy; 14uy; 15uy; 16uy; 17uy |]
    }

    [<Fact>]
    let ``writing empty byte array into stream yields correct byte array representation`` () =
        let stream = BitStream()
        let writer = stream.GetWriter()

        writer.WriteAlignedBytes([||])
        Assert.Equal<byte array>([||], stream.ToByteArray())

    [<Theory>]
    [<MemberData("bytes")>]
    let ``writing bytes into stream yields correct byte array representation`` ([<ParamArray>] bytes: byte array) =
        let stream = BitStream()
        let writer = stream.GetWriter()

        writer.WriteAlignedBytes(bytes)
        Assert.Equal<byte array>(bytes, stream.ToByteArray())

    [<Fact>]
    let ``writing empty byte array into unaligned stream skips to next boundary and then writes correct byte sequence with correct final alignment`` () =
        let stream = BitStream("1")
        let writer = stream.GetWriter()

        writer.WriteAlignedBytes([||])
        writer.WriteBit(true)
        Assert.Equal<byte array>(Array.concat [ [| 0b1uy |]; [| 0b1uy |] ], stream.ToByteArray())
        
    [<Theory>]
    [<MemberData("bytes")>]
    let ``writing bytes into unaligned stream skips to next boundary and then writes correct byte sequence with correct final alignment`` ([<ParamArray>] bytes: byte array) =
        let stream = BitStream("1")
        let writer = stream.GetWriter()

        writer.WriteAlignedBytes(bytes)
        writer.WriteBit(true)
        Assert.Equal<byte array>(Array.concat [ [| 0b1uy |]; bytes; [| 0b1uy |] ], stream.ToByteArray())
