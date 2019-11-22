namespace UnitTests.IO.TestBitReader

open Xunit
open System
open BrotliLib.Serialization


module NextBit =
    [<Fact>]
    let ``iterating through stream yields correct bit sequence`` () =
        let reader = BitStream("0011101").GetReader()

        Assert.False(reader.NextBit())
        Assert.False(reader.NextBit())
        Assert.True(reader.NextBit())
        Assert.True(reader.NextBit())
        Assert.True(reader.NextBit())
        Assert.False(reader.NextBit())
        Assert.True(reader.NextBit())
    
    [<Theory>]
    [<InlineData("")>]
    [<InlineData("0")>]
    [<InlineData("1")>]
    [<InlineData("111100001")>]
    let ``iterating past end of stream throws exception`` (bits: string) =
        let reader = BitStream(bits).GetReader()

        for _ in 1..bits.Length do
            reader.NextBit() |> ignore

        Assert.Throws<IndexOutOfRangeException>(fun () -> reader.NextBit() |> ignore)


module NextChunk =
    [<Fact>]
    let ``iterating through stream yields correct chunk sequence`` () =
        let reader = BitStream("11000100001111101").GetReader()

        Assert.Equal(0b0011, reader.NextChunk(4));
        Assert.Equal(0b10,   reader.NextChunk(2));
        Assert.Equal(0b0000, reader.NextChunk(4));
        Assert.Equal(0b111,  reader.NextChunk(3));
        Assert.Equal(0b1011, reader.NextChunk(4));

    [<Fact>]
    let ``iterating past end of stream throws exception`` () =
        let reader = BitStream("11000100001111101").GetReader()
        reader.NextChunk(13) |> ignore

        Assert.Throws<IndexOutOfRangeException>(fun () -> reader.NextChunk(5) |> ignore)

    [<Fact>]
    let ``retrieving 32 bit chunk yields correct value`` () =
        let reader = BitStream("00001111000011110000111100001111").GetReader()

        Assert.Equal(0b11110000111100001111000011110000, reader.NextChunk(32))

    [<Fact>]
    let ``retrieving 33 bit chunk throws exception`` () =
        let reader = BitStream("000011110000111100001111000011110").GetReader()

        Assert.Throws<ArgumentOutOfRangeException>(fun () -> reader.NextChunk(33) |> ignore)


module AlignToByteBoundary =
    [<Fact>]
    let ``reader is already aligned at the start of stream`` () =
        let reader = BitStream("1").GetReader()

        reader.AlignToByteBoundary()
        Assert.True(reader.NextBit())

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(3)>]
    [<InlineData(4)>]
    [<InlineData(5)>]
    [<InlineData(6)>]
    [<InlineData(7)>]
    let ``reader aligns itself to end of first byte`` (read: int) =
        let reader = BitStream("000000001").GetReader()

        for _ in 0..read do
            reader.NextBit() |> ignore

        reader.AlignToByteBoundary()
        Assert.True(reader.NextBit())

    [<Fact>]
    let ``reader is already aligned at the start of second byte`` () =
        let reader = BitStream("000000001").GetReader()
        
        for _ in 1..8 do
            reader.NextBit() |> ignore

        reader.AlignToByteBoundary()
        Assert.True(reader.NextBit())


module NextAlignedByte =
    [<Fact>]
    let ``iterating through stream yields correct byte sequence`` () =
        let reader = BitStream([| 120uy; 0uy; 255uy; 33uy |]).GetReader()

        Assert.Equal(120uy, reader.NextAlignedByte())
        Assert.Equal(  0uy, reader.NextAlignedByte())
        Assert.Equal(255uy, reader.NextAlignedByte())
        Assert.Equal( 33uy, reader.NextAlignedByte())
        
    [<Fact>]
    let ``iterating through unaligned stream skips to next boundary and then yields correct byte sequence`` () =
        let reader = BitStream([| 120uy; 0uy; 255uy; 33uy |]).GetReader()
        reader.NextBit() |> ignore
        
        Assert.Equal(  0uy, reader.NextAlignedByte())
        Assert.Equal(255uy, reader.NextAlignedByte())
        Assert.Equal( 33uy, reader.NextAlignedByte())
        
    [<Fact>]
    let ``iterating past end of stream throws exception`` () =
        let reader = BitStream([| 120uy; 0uy; 255uy; 33uy |]).GetReader()
        reader.NextBit() |> ignore
        
        Assert.Equal(  0uy, reader.NextAlignedByte())
        Assert.Equal(255uy, reader.NextAlignedByte())

        reader.NextBit() |> ignore
        Assert.Throws<IndexOutOfRangeException>(fun () -> reader.NextAlignedByte() |> ignore)
