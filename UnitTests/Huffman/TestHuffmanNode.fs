namespace UnitTests.Huffman.TestHuffmanNode

open Xunit
open System.Collections.Generic
open BrotliLib.Huffman
open BrotliLib.IO


module Helper =
    let leaf = HuffmanNode.Leaf('a')

    let depth1 = HuffmanNode.Path(
        HuffmanNode.Leaf('a'),
        HuffmanNode.Leaf('b')
    )

    let depth2 = HuffmanNode.Path(
        HuffmanNode.Path(
            HuffmanNode.Leaf('a'),
            HuffmanNode.Leaf('b')
        ),
        HuffmanNode.Path(
            HuffmanNode.Leaf('c'),
            HuffmanNode.Leaf('d')
        )
    )


module Traversal =
    [<Fact>]
    let ``a leaf node returns its value without consuming any bits`` () =
        Assert.Equal('a', Helper.leaf.LookupValue(BitStream().GetReader()))
        
    [<Fact>]
    let ``a path of depth 1 returns correct values for each path`` () =
        Assert.Equal('a', Helper.depth1.LookupValue(BitStream("0").GetReader()))
        Assert.Equal('b', Helper.depth1.LookupValue(BitStream("1").GetReader()))
        
    [<Fact>]
    let ``a path of depth 2 returns correct values for each path`` () =
        Assert.Equal('a', Helper.depth2.LookupValue(BitStream("00").GetReader()))
        Assert.Equal('b', Helper.depth2.LookupValue(BitStream("01").GetReader()))
        Assert.Equal('c', Helper.depth2.LookupValue(BitStream("10").GetReader()))
        Assert.Equal('d', Helper.depth2.LookupValue(BitStream("11").GetReader()))

    [<Fact>]
    let ``running out of bits returns default value`` () =
        Assert.Equal(Unchecked.defaultof<char>, Helper.depth2.LookupValue(BitStream("0").GetReader()))


module ValueMap =
    [<Fact>]
    let ``a leaf node returns its value mapped to an empty bit stream`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream())
        }, Helper.leaf.GenerateValueMap())

    [<Fact>]
    let ``a path of depth 1 returns its values mapped to correct bit streams`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("0"))
            yield KeyValuePair('b', BitStream("1"))
        }, Helper.depth1.GenerateValueMap())

    [<Fact>]
    let ``a path of depth 2 returns its values mapped to correct bit streams`` () =
        Assert.Equal<IEnumerable<KeyValuePair<_, _>>>(seq {
            yield KeyValuePair('a', BitStream("00"))
            yield KeyValuePair('b', BitStream("01"))
            yield KeyValuePair('c', BitStream("10"))
            yield KeyValuePair('d', BitStream("11"))
        }, Helper.depth2.GenerateValueMap())
