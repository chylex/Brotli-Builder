namespace UnitTests.Brotli.TestUtils

open Xunit
open System
open BrotliLib.Brotli.Components.Utils


module AlphabetSize =
    [<Theory>]
    [<InlineData(0, 0)>]
    [<InlineData(1, 0)>]
    [<InlineData(2, 1)>]
    [<InlineData(3, 2)>]
    [<InlineData(4, 2)>]
    [<InlineData(5, 3)>]
    [<InlineData(6, 3)>]
    [<InlineData(7, 3)>]
    [<InlineData(8, 3)>]
    [<InlineData(9, 4)>]
    [<InlineData(16, 4)>]
    [<InlineData(17, 5)>]
    [<InlineData(32, 5)>]
    [<InlineData(33, 6)>]
    [<InlineData(256, 8)>]
    [<InlineData(704, 10)>]
    let ``alphabet of specified size calculates amount of bits per symbol correctly`` (symbols: int, bits: int) =
        Assert.Equal(bits, AlphabetSize(symbols).BitsPerSymbol)

    [<Fact>]
    let ``constructing alphabet with negative amount of symbols throws exception`` () =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> AlphabetSize(-1) |> ignore)
