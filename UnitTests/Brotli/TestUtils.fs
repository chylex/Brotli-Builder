namespace UnitTests.Brotli.TestUtils

open Xunit
open System
open System.Collections.Generic
open BrotliLib.Collections


module MoveToFront =
    let maps : obj array seq = seq {
        yield Array.map box [|   0uy;  0uy;   0uy;  0uy;   0uy;  0uy; (**)   0uy; 0uy; 0uy;  0uy; 0uy; 0uy |]
        yield Array.map box [|   0uy;  1uy;   2uy;  0uy;   1uy;  2uy; (**)   0uy; 1uy; 2uy;  2uy; 2uy; 2uy |]
        yield Array.map box [|   5uy;  4uy;   4uy;  3uy;   1uy;  0uy; (**)   5uy; 5uy; 0uy;  5uy; 4uy; 4uy |]
        yield Array.map box [|  10uy; 10uy;  10uy; 20uy;  20uy; 20uy; (**)  10uy; 0uy; 0uy; 20uy; 0uy; 0uy |]
        yield Array.map box [| 255uy;  0uy; 255uy;  0uy; 255uy;  0uy; (**) 255uy; 1uy; 1uy;  1uy; 1uy; 1uy |]
    }

    [<Theory>]
    [<MemberData("maps")>]
    let ``encode produces expected result`` ([<ParamArray>] elements: byte array) =
        let map = elements.[..(elements.Length / 2 - 1)]
        MoveToFront.Encode.Invoke(map)

        Assert.Equal<IEnumerable<byte>>(elements.[(elements.Length / 2)..], map)

    [<Theory>]
    [<MemberData("maps")>]
    let ``decode produces expected result`` ([<ParamArray>] elements: byte array) =
        let map = elements.[(elements.Length / 2)..]
        MoveToFront.Decode.Invoke(map)

        Assert.Equal<IEnumerable<byte>>(elements.[..(elements.Length / 2 - 1)], map)

    [<Theory>]
    [<MemberData("maps")>]
    let ``encode followed by decode produces same values`` ([<ParamArray>] elements: byte array) =
        let map = elements.[..(elements.Length / 2 - 1)]
        MoveToFront.Encode.Invoke(map)
        MoveToFront.Decode.Invoke(map)

        Assert.Equal<IEnumerable<byte>>(elements.[..(elements.Length / 2 - 1)], map)
