namespace UnitTests.Numbers.TestAlphabetSize

open Xunit
open System
open BrotliLib.Numbers


module Behavior =
    [<Theory>]
    [<InlineData(0, 0uy)>]
    [<InlineData(1, 0uy)>]
    [<InlineData(2, 1uy)>]
    [<InlineData(3, 2uy)>]
    [<InlineData(4, 2uy)>]
    [<InlineData(5, 3uy)>]
    [<InlineData(6, 3uy)>]
    [<InlineData(7, 3uy)>]
    [<InlineData(8, 3uy)>]
    [<InlineData(9, 4uy)>]
    [<InlineData(16, 4uy)>]
    [<InlineData(17, 5uy)>]
    [<InlineData(32, 5uy)>]
    [<InlineData(33, 6uy)>]
    [<InlineData(256, 8uy)>]
    [<InlineData(704, 10uy)>]
    let ``alphabet of specified size calculates amount of bits per symbol correctly`` (symbols: int, bits: byte) =
        Assert.Equal(bits, AlphabetSize(symbols).BitsPerSymbol)

    [<Fact>]
    let ``constructing alphabet with negative amount of symbols throws exception`` () =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> AlphabetSize(-1) |> ignore)
