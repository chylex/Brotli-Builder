namespace UnitTests.IO.TestBitStream

open Xunit
open System
open BrotliLib.IO


module Representations =
    [<Theory>]
    [<InlineData((*  0*)"")>]
    [<InlineData((*  1*)"0")>]
    [<InlineData((*  2*)"00")>]
    [<InlineData((*  3*)"000")>]
    [<InlineData((*  4*)"0000")>]
    [<InlineData((*  8*)"11000000")>]
    [<InlineData((* 16*)"1110000011110000")>]
    [<InlineData((* 32*)"11110000111100001111000011110000")>]
    [<InlineData((* 64*)"1111100011110000111100001111000011110000111100001111000011110000")>]
    [<InlineData((*128*)"11111100111100001111000011110000111100001111000011110000111100001111000011110000111100001111000011110000111100001111000011110000")>]
    [<InlineData((*129*)"111111101111000011110000111100001111000011110000111100001111000011110000111100001111000011110000111100001111000011110000111100001")>]
    let ``constructing from string yields same length and string representation`` (bits: string) =
        let stream = BitStream(bits)
        Assert.Equal(bits.Length, stream.Length)
        Assert.Equal(bits, stream.ToString())
        
    [<Theory>]
    [<InlineData("1100 ")>]
    [<InlineData("1111211")>]
    [<InlineData("0000_1111")>]
    let ``constructing from string with invalid characters throws exception`` (bits: string) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> BitStream(bits) |> ignore)
        
    [<Theory>]
    [<InlineData()>]
    [<InlineData(0b00000000uy)>]
    [<InlineData(0b11100110uy)>]
    [<InlineData(0b00000000uy, 0b11111111uy)>]
    [<InlineData(0b11111111uy, 0b00000000uy, 0b11111111uy)>]
    [<InlineData(0b00000000uy, 0b11111111uy, 0b00000000uy, 0b11111111uy)>]
    [<InlineData(0b11111111uy, 0b00000000uy, 0b11111111uy, 0b00000000uy, 0b11111111uy)>]
    [<InlineData(0b10101010uy, 0b01010101uy, 0b10101010uy, 0b01010101uy, 0b10101010uy, 0b01010101uy, 0b10101010uy, 0b01010101uy, 0b10101010uy)>]
    let ``constructing from byte array yields same length and byte array representation`` ([<ParamArray>] bytes: byte array) =
        let stream = BitStream(bytes)
        Assert.Equal(bytes.Length * 8, stream.Length)
        Assert.Equal<byte array>(bytes, stream.ToByteArray())
        
    [<Theory>]
    [<InlineData("0", 0b00000000uy)>]
    [<InlineData("1", 0b00000001uy)>]
    [<InlineData("11", 0b00000011uy)>]
    [<InlineData("1111111", 0b01111111uy)>]
    [<InlineData("11111111", 0b11111111uy)>]
    [<InlineData("01010101", 0b10101010uy)>]
    [<InlineData("111111110", 0b11111111uy, 0b00000000uy)>]
    [<InlineData("111111111", 0b11111111uy, 0b00000001uy)>]
    [<InlineData("11110000111100001111001", 0b00001111uy, 0b00001111uy, 0b01001111uy)>]
    let ``constructing from string yields correct byte array representation with zero padding`` (bits: string, [<ParamArray>] bytes: byte array) =
        Assert.Equal<byte array>(bytes, BitStream(bits).ToByteArray())
        

module Equality =
    let equal : obj array seq = seq {
        yield [| "" |]
        yield [| "0" |]
        yield [| "1" |]
        yield [| "10101010" |]
        yield [| "000000000" |]
    }

    let notequal : obj array seq = seq {
        yield [| ""; "0" |]
        yield [| "0"; "1" |]
        yield [| "00"; "000" |]
        yield [| "10101010"; "01010101" |]
        yield [| "00000000"; "0000000000" |]
        yield [| "000000001"; "000000000" |]
    }

    [<Theory>]
    [<MemberData("equal")>]
    let ``two streams are equal if they have same contents`` (bits: string) =
        Assert.True(BitStream(bits).Equals(BitStream(bits)))
        
    [<Theory>]
    [<MemberData("notequal")>]
    let ``two streams are not equal if they have different contents`` (first: string, second: string) =
        Assert.False(BitStream(first).Equals(BitStream(second)))

    [<Theory>]
    [<MemberData("equal")>]
    let ``two equal streams have the same hash code`` (bits: string) =
        Assert.Equal(BitStream(bits).GetHashCode(), BitStream(bits).GetHashCode())
        
    [<Theory>]
    [<MemberData("notequal")>]
    let ``two different streams should generally have different hash codes`` (first: string, second: string) =
        Assert.NotEqual(BitStream(first).GetHashCode(), BitStream(second).GetHashCode())
        

module Mutability =
    [<Theory>]
    [<InlineData("")>]
    [<InlineData("1", true)>]
    [<InlineData("0", false)>]
    [<InlineData("10", true, false)>]
    [<InlineData("11110000", true, true, true, true, false, false, false, false)>]
    [<InlineData("11110000111100001", true, true, true, true, false, false, false, false, true, true, true, true, false, false, false, false, true)>]
    let ``appending few bits to empty stream yields correct text representation`` (expected: string, [<ParamArray>] values: bool array) =
        let stream = BitStream()
        Array.iter (stream.Add) <| values

        Assert.Equal(expected, stream.ToString())

    [<Fact>]
    let ``appending many bits to empty stream yields correct text representation`` () =
        let stream = BitStream()

        for _ in 0..256 do
            stream.Add(true)

        Assert.Equal(String.replicate 257 "1", stream.ToString())

    [<Theory>]
    [<InlineData("", "")>]
    [<InlineData("0", "")>]
    [<InlineData("0", "1")>]
    [<InlineData("11110000", "1010")>]
    let ``appending stream to another stream in either direction yields correct text representation`` (first: string, second: string) =
        let firstsecond = BitStream(first)
        firstsecond.AddAll(BitStream(second))

        let secondfirst = BitStream(second)
        secondfirst.AddAll(BitStream(first))

        Assert.Equal(first + second, firstsecond.ToString())
        Assert.Equal(second + first, secondfirst.ToString())


module Cloning =
    [<Theory>]
    [<InlineData("")>]
    [<InlineData("0")>]
    [<InlineData("1")>]
    [<InlineData("101010101")>]
    let ``cloned stream is equal to original stream but not the same object`` (bits: string) =
        let original = BitStream(bits)
        let clone = original.Clone()

        Assert.Equal<BitStream>(original, clone)
        Assert.NotSame(original, clone)

    [<Fact>]
    let ``appending to cloned stream keeps the original stream intact`` () =
        let original = BitStream("11100")
        let clone = original.Clone()
        clone.Add(true)

        Assert.Equal("11100", original.ToString())
        Assert.Equal("111001", clone.ToString())
