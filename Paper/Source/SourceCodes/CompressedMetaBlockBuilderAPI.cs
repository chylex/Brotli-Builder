var state = new BrotliGlobalState(BrotliFileParameters.Default);
var builder = new CompressedMetaBlockBuilder(state);

// Generates command 1 that outputs "This␣is␣" from literals "This␣",
// and a copy part that produces "is␣" by copying the last 3 literals.
builder.AddInsertCopy(
    Literal.FromString("This ", UTF8),
    copyLength: 𝟑,
    copyDistance: 𝟑);

// Generates command 2 with literals "test" and no copy part.
builder.AddInsert(Literal.FromString("test", UTF8));

// Every command must either have a copy part, or be the last in a meta-block.
// Subsequent commands will be automatically merged until either
// the meta-block is built, or a command introduces a copy part.

// Merges literals "ing" into command 2.
// The command now produces the text "testing".
builder.AddInsert(Literal.FromString("ing", UTF8));

// The previous command is still missing a copy part.
// This finds "␣data" in the dictionary, encodes it into a copy part,
// and merges with command 2 which will now output "testing␣data".
var dictionary = BrotliFileParameters.Default.Dictionary.Index;
var results = dictionary.Find(UTF8.GetBytes(" data"), minLength: 𝟓);
builder.AddCopy(results.First());

// The previous command now has a copy part, so this
// generates command 3 with literals "!!".
builder.AddInsert(Literal.FromString("!!", UTF8));

// We can check the output size before building.
Assert(builder.OutputSize == "This is testing data!!".Length);

// Build the meta-block and obtain the state for building the next meta-block.
var (metaBlock,nextState) = builder.Build(BrotliCompressionParameters.Default);
