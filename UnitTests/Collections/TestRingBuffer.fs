namespace UnitTests.Collections.TestRingBuffer

open Xunit
open System
open BrotliLib.Collections


module InitializationAndAccess =
    [<Fact>]
    let ``constructing with one value initializes the buffer correctly`` () =
        let buffer = RingBuffer.From(10)

        Assert.Equal(1, buffer.Length)
        Assert.Equal(10, buffer.Front)
        Assert.Equal(10, buffer.Back)

        Assert.Equal(10, buffer.[0])
        Assert.Equal(10, buffer.[1])
        Assert.Equal(10, buffer.[2])

    [<Fact>]
    let ``constructing with two values initializes the buffer correctly`` () =
        let buffer = RingBuffer.From(10, 20)

        Assert.Equal(2, buffer.Length)
        Assert.Equal(20, buffer.Front)
        Assert.Equal(10, buffer.Back)

        Assert.Equal(10, buffer.[0])
        Assert.Equal(20, buffer.[1])
        Assert.Equal(10, buffer.[2])

    [<Fact>]
    let ``constructing with three values initializes the buffer correctly`` () =
        let buffer = RingBuffer.From(10, 20, 30)

        Assert.Equal(3, buffer.Length)
        Assert.Equal(30, buffer.Front)
        Assert.Equal(10, buffer.Back)

        Assert.Equal(10, buffer.[0])
        Assert.Equal(20, buffer.[1])
        Assert.Equal(30, buffer.[2])

    [<Fact>]
    let ``constructing with no parameters throws exception`` () =
        Assert.Throws<ArgumentException>(fun () -> RingBuffer.From() |> ignore)

    [<Fact>]
    let ``accessing a negative index throws exception`` () =
        let buffer = RingBuffer.From(10, 20, 30)
        Assert.Throws<IndexOutOfRangeException>(fun () -> buffer.[-1] |> ignore)


module Mutability =
    [<Fact>]
    let ``pushing to a buffer of size 1 replaces the stored value`` () =
        let buffer = RingBuffer.From(10)

        buffer.Push(90)
        Assert.Equal(90, buffer.Front)

    [<Fact>]
    let ``pushing to a buffer of size 2 inserts the value and shifts the rest`` () =
        let buffer = RingBuffer.From(10, 20)

        buffer.Push(90)
        Assert.Equal(90, buffer.Front)
        Assert.Equal(20, buffer.Back)

        buffer.Push(80)
        Assert.Equal(80, buffer.Front)
        Assert.Equal(90, buffer.Back)

    [<Fact>]
    let ``pushing to a buffer of size 3 inserts the value and shifts the rest`` () =
        let buffer = RingBuffer.From(10, 20, 30)

        buffer.Push(90)
        Assert.Equal(90, buffer.Front)
        Assert.Equal(30, buffer.[1])
        Assert.Equal(20, buffer.Back)

        buffer.Push(80)
        Assert.Equal(80, buffer.Front)
        Assert.Equal(90, buffer.[1])
        Assert.Equal(30, buffer.Back)

    [<Fact>]
    let ``accessing a negative index after pushing throws exception`` () =
        let buffer = RingBuffer.From(10, 20, 30)

        buffer.Push(90)
        buffer.Push(80)
        Assert.Throws<IndexOutOfRangeException>(fun () -> buffer.[-1] |> ignore)
