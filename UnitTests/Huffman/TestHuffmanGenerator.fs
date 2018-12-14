namespace UnitTests.Huffman.TestHuffmanGenerator

open Xunit
open System
open System.Collections.Generic
open BrotliLib.Huffman
open BrotliLib.IO


module FromBitCountsCanonical =
    [<Fact>]
    let ``1 character with length (0) generates empty bit stream`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream(""))
        }, HuffmanGenerator.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 0uy)
        |]).GenerateValueMap())

    [<Fact>]
    let ``2 characters with lengths (1, 1) generate correct bit paths`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("0"))
            yield KeyValuePair('b', BitStream("1"))
        }, HuffmanGenerator.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 1uy);
            HuffmanGenerator<_>.Entry('b', 1uy)
        |]).GenerateValueMap())

    [<Fact>]
    let ``3 characters with lengths (2, 2, 1) generate correct bit paths`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('c', BitStream("0"))
            yield KeyValuePair('a', BitStream("10"))
            yield KeyValuePair('b', BitStream("11"))
        }, HuffmanGenerator.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 2uy);
            HuffmanGenerator<_>.Entry('b', 2uy);
            HuffmanGenerator<_>.Entry('c', 1uy)
        |]).GenerateValueMap())

    [<Fact>]
    let ``4 characters with lengths (2, 2, 2, 2) generate correct bit paths`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("00"))
            yield KeyValuePair('b', BitStream("01"))
            yield KeyValuePair('c', BitStream("10"))
            yield KeyValuePair('d', BitStream("11"))
        }, HuffmanGenerator.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 2uy);
            HuffmanGenerator<_>.Entry('b', 2uy);
            HuffmanGenerator<_>.Entry('c', 2uy);
            HuffmanGenerator<_>.Entry('d', 2uy)
        |]).GenerateValueMap())

    [<Fact>]
    let ``4 characters with lengths (2, 2, 2, 2) generate correct bit paths with correct symbol order`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("00"))
            yield KeyValuePair('b', BitStream("01"))
            yield KeyValuePair('c', BitStream("10"))
            yield KeyValuePair('d', BitStream("11"))
        }, HuffmanGenerator.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('d', 2uy);
            HuffmanGenerator<_>.Entry('a', 2uy);
            HuffmanGenerator<_>.Entry('c', 2uy);
            HuffmanGenerator<_>.Entry('b', 2uy)
        |]).GenerateValueMap())

    [<Fact>]
    let ``4 characters with lengths (2, 1, 3, 3) generate correct bit paths`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('b', BitStream("0"))
            yield KeyValuePair('a', BitStream("10"))
            yield KeyValuePair('c', BitStream("110"))
            yield KeyValuePair('d', BitStream("111"))
        }, HuffmanGenerator.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('a', 2uy);
            HuffmanGenerator<_>.Entry('b', 1uy);
            HuffmanGenerator<_>.Entry('c', 3uy);
            HuffmanGenerator<_>.Entry('d', 3uy)
        |]).GenerateValueMap())

    [<Fact>]
    let ``4 characters with lengths (2, 1, 3, 3) generate correct bit paths with correct symbol order`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('b', BitStream("0"))
            yield KeyValuePair('a', BitStream("10"))
            yield KeyValuePair('c', BitStream("110"))
            yield KeyValuePair('d', BitStream("111"))
        }, HuffmanGenerator.FromBitCountsCanonical([|
            HuffmanGenerator<_>.Entry('d', 3uy);
            HuffmanGenerator<_>.Entry('b', 1uy);
            HuffmanGenerator<_>.Entry('a', 2uy);
            HuffmanGenerator<_>.Entry('c', 3uy)
        |]).GenerateValueMap())

    [<Fact>]
    let ``2 characters with lengths (1, 0) throw exception`` () =
        let f () =
            HuffmanGenerator.FromBitCountsCanonical([|
                HuffmanGenerator<_>.Entry('a', 1uy);
                HuffmanGenerator<_>.Entry('b', 0uy)
            |])

        Assert.Throws<ArgumentException>(fun () -> f() |> ignore)

    [<Fact>]
    let ``2 characters with lengths (1, 2) throw exception`` () =
        let f () =
            HuffmanGenerator.FromBitCountsCanonical([|
                HuffmanGenerator<_>.Entry('a', 1uy);
                HuffmanGenerator<_>.Entry('b', 2uy)
            |])

        Assert.Throws<ArgumentException>(fun () -> f() |> ignore)

    [<Fact>]
    let ``3 characters with lengths (1, 1, 1) throw exception`` () =
        let f () =
            HuffmanGenerator.FromBitCountsCanonical([|
                HuffmanGenerator<_>.Entry('a', 1uy);
                HuffmanGenerator<_>.Entry('b', 1uy);
                HuffmanGenerator<_>.Entry('c', 1uy)
            |])

        Assert.Throws<ArgumentException>(fun () -> f() |> ignore)
