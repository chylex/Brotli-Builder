﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Encode.Build{
    public sealed class BlockSwitchBuilder{
        public const int FinalBlockSwitchLengthPlaceholder = -1;

        public Category Category { get; }
        public int InitialLength { get; private set; }

        public IReadOnlyList<BlockSwitchCommand> Commands => commands;
        
        public int TypeCount => commands.Count == 0 ? 1 : 1 + commands.Max(command => command.Type);

        // Fields

        private readonly List<BlockSwitchCommand> commands = new List<BlockSwitchCommand>();

        // Construction

        public BlockSwitchBuilder(BlockTypeInfo info){
            this.Category = info.Category;
            this.InitialLength = info.InitialLength;
        }

        public BlockSwitchBuilder(BlockTypeInfo info, IReadOnlyList<BlockSwitchCommand> commands) : this(info){
            this.commands.AddRange(commands);
        }

        // Commands

        public BlockSwitchBuilder Reset(){
            InitialLength = BlockTypeInfo.Empty[Category].InitialLength;
            commands.Clear();
            return this;
        }

        public BlockSwitchBuilder SetInitialLength(int initialLength){
            InitialLength = initialLength;
            return this;
        }

        public BlockSwitchBuilder AddBlockSwitch(int type, int length){
            commands.Add(new BlockSwitchCommand(type, length));
            return this;
        }

        public BlockSwitchBuilder AddFinalBlockSwitch(int type){
            commands.Add(new BlockSwitchCommand(type, FinalBlockSwitchLengthPlaceholder));
            return this;
        }

        // Building

        public (BlockTypeInfo Info, IReadOnlyList<BlockSwitchCommand> Commands) Build(int totalLength, BrotliCompressionParameters parameters){
            if (commands.Count == 0){
                return (BlockTypeInfo.Empty[Category], Array.Empty<BlockSwitchCommand>());
            }

            int typeCount = TypeCount;

            if (typeCount <= 1){
                throw new InvalidOperationException("Cannot generate block-switch chain that only refers to 1 block type.");
            }

            var commandsFinal = commands;

            var tracker = new BlockTypeTracker(typeCount);
            int remainingLength = totalLength - InitialLength;

            var typeCodeList = new FrequencyList<BlockTypeCode>();
            var lengthCodeList = new FrequencyList<BlockLengthCode>{ BlockLengthCode.MakeCode(InitialLength) };

            for(int index = 0; index < commands.Count; index++){
                var command = commands[index];

                var codes = tracker.FindCodes(command.Type);
                var code = parameters.BlockTypeCodePicker(codes, typeCodeList);

                typeCodeList.Add(code);

                int length;

                if (command.Length == FinalBlockSwitchLengthPlaceholder){
                    if (index != commands.Count - 1){
                        throw new InvalidOperationException("Block-switch command that is marked as final must be the last.");
                    }

                    length = remainingLength;
                    
                    commandsFinal = new List<BlockSwitchCommand>(commands);
                    commandsFinal[^1] = new BlockSwitchCommand(command.Type, length); // replace the last command with one that has proper length
                }
                else{
                    length = command.Length;
                }

                lengthCodeList.Add(BlockLengthCode.MakeCode(length));
                remainingLength -= length;
            }

            if (remainingLength > 0){
                throw new InvalidOperationException("Block-switch command lengths do not cover the entire " + Category + " category (covered " + (totalLength - remainingLength) + ", remaining " + remainingLength + ").");
            }

            return (new BlockTypeInfo(
                Category,
                typeCount,
                InitialLength,
                HuffmanTree<BlockTypeCode>.FromSymbols(typeCodeList),
                HuffmanTree<BlockLengthCode>.FromSymbols(lengthCodeList)
            ), commandsFinal);
        }
    }
}
