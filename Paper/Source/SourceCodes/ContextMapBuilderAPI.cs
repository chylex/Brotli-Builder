// Assume the block types have already been setup.
Assert(builder.BlockTypes[Category.Distance].TypeCount == 𝟑);

var contextMap = new ContextMapBuilder.Distances(blockTypeCount: 𝟑);
// [ 0,0,0,0 | 0,0,0,0 | 0,0,0,0 ]

contextMap.Set(blockType: 𝟎, values: new byte[]{ 𝟎, 𝟏, 𝟐, 𝟑 });
// [ 0,1,2,3 | 0,0,0,0 | 0,0,0,0 ]
//   * * * *

contextMap.Set(blockType: 𝟎, index: 𝟏, value: 𝟎);
// [ 0,0,2,3 | 0,0,0,0 | 0,0,0,0 ]
//     *

contextMap.Set(blockType: 𝟎, range: new IntRange(𝟐, 𝟑), value: 𝟏);
// [ 0,0,1,1 | 0,0,0,0 | 0,0,0,0 ]
//       * *

contextMap.RepeatFirstBlockType(separateTreesPerBlockType: false);
// [ 0,0,1,1 | 0,0,1,1 | 0,0,1,1 ]
//             * * * *   * * * *

contextMap.RepeatFirstBlockType(separateTreesPerBlockType: true);
// [ 0,0,1,1 | 2,2,3,3 | 4,4,5,5 ]
//             * * * *   * * * *

// Build and assign.
builder.DistanceCtxMap = contextMap.Build();
