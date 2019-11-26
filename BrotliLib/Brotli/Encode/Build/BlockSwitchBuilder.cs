using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Encode.Build{
    public sealed class BlockSwitchBuilder{
        public int InitialLength { get; private set; }
        public IEnumerable<BlockSwitchCommand> Commands => commands;

        private readonly Category category;
        private readonly IList<BlockSwitchCommand> commands = new List<BlockSwitchCommand>();

        public BlockSwitchBuilder(BlockTypeInfo info){
            this.category = info.Category;
            this.InitialLength = info.InitialLength;
        }

        public BlockSwitchBuilder SetInitialLength(int initialLength){
            InitialLength = initialLength;
            return this;
        }

        public BlockSwitchBuilder AddBlockSwitch(BlockSwitchCommand command){
            commands.Add(command);
            return this;
        }

        public BlockTypeInfo Build(){
            if (commands.Count == 0){
                return BlockTypeInfo.Empty[category];
            }

            int count = 1 + commands.Max(command => command.Type);

            if (count <= 1){
                throw new InvalidOperationException("Cannot generate block-switch chain that only refers to 1 block type.");
            }

            var tracker = new BlockTypeTracker(count);

            var typeCodeList = new FrequencyList<int>();
            var lengthCodeList = new FrequencyList<BlockLengthCode>{ BlockLengthCode.MakeCode(InitialLength) };

            foreach(var command in commands){
                var codes = tracker.FindCodes(command.Type);
                var code = codes.First(); // TODO good strategy?

                typeCodeList.Add(code);
                lengthCodeList.Add(BlockLengthCode.MakeCode(command.Length));
            }

            return new BlockTypeInfo(
                category,
                count,
                InitialLength,
                HuffmanTree<int>.FromSymbols(typeCodeList),
                HuffmanTree<BlockLengthCode>.FromSymbols(lengthCodeList)
            );
        }
    }
}
