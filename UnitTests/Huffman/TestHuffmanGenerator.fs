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
        

module FromFrequencies =
    [<Fact>]
    let ``1 character returns itself`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream())
        }, HuffmanGenerator.FromFrequencies([|
            HuffmanGenerator<_>.SymbolFreq('a', 5)
        |]).GenerateValueMap())

    [<Fact>]
    let ``2 characters return correct bit streams`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("0"))
            yield KeyValuePair('b', BitStream("1"))
        }, HuffmanGenerator.FromFrequencies([|
            HuffmanGenerator<_>.SymbolFreq('b', 2);
            HuffmanGenerator<_>.SymbolFreq('a', 1)
        |]).GenerateValueMap())

    [<Fact>]
    let ``3 characters return correct bit streams`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("00"))
            yield KeyValuePair('b', BitStream("01"))
            yield KeyValuePair('c', BitStream("1"))
        }, HuffmanGenerator.FromFrequencies([|
            HuffmanGenerator<_>.SymbolFreq('b', 2);
            HuffmanGenerator<_>.SymbolFreq('a', 1);
            HuffmanGenerator<_>.SymbolFreq('c', 5);
        |]).GenerateValueMap())

    [<Fact>]
    let ``many characters return correct bit path lengths`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair(' ', 3)
            yield KeyValuePair('a', 3)
            yield KeyValuePair('e', 3)
            yield KeyValuePair('f', 4)
            yield KeyValuePair('h', 4)
            yield KeyValuePair('i', 4)
            yield KeyValuePair('m', 4)
            yield KeyValuePair('n', 4)
            yield KeyValuePair('s', 4)
            yield KeyValuePair('t', 4)
            yield KeyValuePair('l', 5)
            yield KeyValuePair('o', 5)
            yield KeyValuePair('p', 5)
            yield KeyValuePair('r', 5)
            yield KeyValuePair('u', 5)
            yield KeyValuePair('x', 5)
        }, HuffmanGenerator.FromFrequencies(
            "this is an example of a huffman tree".ToCharArray()
            |> Seq.countBy(fun x -> x)
            |> Seq.map(fun (char, count) -> HuffmanGenerator<_>.SymbolFreq(char, count))
            |> Seq.toArray
        ).GenerateValueMap() |> Seq.map(fun (kvp) -> KeyValuePair(kvp.Key, kvp.Value.Length)) |> Seq.sortBy(fun (kvp) -> (kvp.Value, kvp.Key)))

    [<Fact>]
    let ``0 characters throw exception`` () =
        Assert.Throws<ArgumentException>(fun () -> HuffmanGenerator<char>.FromFrequencies([||]) |> ignore)


module FromFrequenciesCanonical =
    [<Fact>]
    let ``limiting worst case depth 6 tree to depth 6 results in no change`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("0"))
            yield KeyValuePair('b', BitStream("10"))
            yield KeyValuePair('c', BitStream("110"))
            yield KeyValuePair('d', BitStream("1110"))
            yield KeyValuePair('e', BitStream("11110"))
            yield KeyValuePair('f', BitStream("111110"))
            yield KeyValuePair('g', BitStream("111111"))
        }, HuffmanGenerator.FromFrequenciesCanonical([|
            HuffmanGenerator<_>.SymbolFreq('a', 13)
            HuffmanGenerator<_>.SymbolFreq('b', 8)
            HuffmanGenerator<_>.SymbolFreq('c', 5)
            HuffmanGenerator<_>.SymbolFreq('d', 3)
            HuffmanGenerator<_>.SymbolFreq('e', 2)
            HuffmanGenerator<_>.SymbolFreq('f', 1)
            HuffmanGenerator<_>.SymbolFreq('g', 1)
        |], 6uy).GenerateValueMap())

    [<Fact>]
    let ``limiting worst case depth 6 tree to depth 5 generates correct bit paths`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("0"))
            yield KeyValuePair('b', BitStream("10"))
            yield KeyValuePair('c', BitStream("110"))
            yield KeyValuePair('d', BitStream("11100"))
            yield KeyValuePair('e', BitStream("11101"))
            yield KeyValuePair('f', BitStream("11110"))
            yield KeyValuePair('g', BitStream("11111"))
        }, HuffmanGenerator.FromFrequenciesCanonical([|
            HuffmanGenerator<_>.SymbolFreq('a', 13)
            HuffmanGenerator<_>.SymbolFreq('b', 8)
            HuffmanGenerator<_>.SymbolFreq('c', 5)
            HuffmanGenerator<_>.SymbolFreq('d', 3)
            HuffmanGenerator<_>.SymbolFreq('e', 2)
            HuffmanGenerator<_>.SymbolFreq('f', 1)
            HuffmanGenerator<_>.SymbolFreq('g', 1)
        |], 5uy).GenerateValueMap())

    [<Fact>]
    let ``limiting worst case depth 6 tree to depth 4 generates correct bit paths`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("0"))
            yield KeyValuePair('b', BitStream("100"))
            yield KeyValuePair('c', BitStream("101"))
            yield KeyValuePair('d', BitStream("1100"))
            yield KeyValuePair('e', BitStream("1101"))
            yield KeyValuePair('f', BitStream("1110"))
            yield KeyValuePair('g', BitStream("1111"))
        }, HuffmanGenerator.FromFrequenciesCanonical([|
            HuffmanGenerator<_>.SymbolFreq('a', 13)
            HuffmanGenerator<_>.SymbolFreq('b', 8)
            HuffmanGenerator<_>.SymbolFreq('c', 5)
            HuffmanGenerator<_>.SymbolFreq('d', 3)
            HuffmanGenerator<_>.SymbolFreq('e', 2)
            HuffmanGenerator<_>.SymbolFreq('f', 1)
            HuffmanGenerator<_>.SymbolFreq('g', 1)
        |], 4uy).GenerateValueMap())

    [<Theory>]
    [<InlineData(0uy)>]
    [<InlineData(32uy)>]
    let ``invalid maximum depths throw exception`` (maxDepth: byte) =
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> HuffmanGenerator<char>.FromFrequenciesCanonical([||], maxDepth) |> ignore)

    [<Fact>]
    let ``limiting tree with 5 symbols to depth 2 throws exception`` () =
        let freq = [|
            HuffmanGenerator<_>.SymbolFreq('a', 5)
            HuffmanGenerator<_>.SymbolFreq('b', 3)
            HuffmanGenerator<_>.SymbolFreq('c', 2)
            HuffmanGenerator<_>.SymbolFreq('d', 1)
            HuffmanGenerator<_>.SymbolFreq('e', 1)
        |]

        Assert.Throws<ArgumentException>(fun () -> HuffmanGenerator.FromFrequenciesCanonical(freq, 2uy) |> ignore)
