using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Encode.Build{
    public sealed class BlockSwitchBuilder{
        public const int FinalCommandLengthPlaceholder = -1;

        public Category Category { get; }
        public int InitialLength { get; private set; }

        public IReadOnlyList<BlockSwitchCommand> Commands => commands;
        public int TypeCount => commands.Count == 0 ? 1 : 1 + commands.Max(command => command.Type);

        private BlockSwitchCommand? LastCommand => commands.Count > 0 ? commands[^1] : null;

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
            var lastCommand = LastCommand;

            if (lastCommand?.Length == FinalCommandLengthPlaceholder){
                throw new InvalidOperationException("Cannot add another block-switch command after the final command.");
            }
            else if (lastCommand?.Type == type){
                commands[^1] = new BlockSwitchCommand(type, length + lastCommand.Length);
            }
            else{
                commands.Add(new BlockSwitchCommand(type, length));
            }

            return this;
        }

        public BlockSwitchBuilder AddFinalBlockSwitch(int type){
            var lastCommand = LastCommand;

            if (lastCommand?.Type == type){
                commands[^1] = new BlockSwitchCommand(type, FinalCommandLengthPlaceholder);
            }
            else if (lastCommand?.Length != FinalCommandLengthPlaceholder){
                commands.Add(new BlockSwitchCommand(type, FinalCommandLengthPlaceholder));
            }
            else{
                throw new InvalidOperationException("Cannot add another block-switch command after the final command.");
            }

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

            if (InitialLength < 1){
                throw new InvalidOperationException("Initial block length must be at least 1.");
            }

            var commandsFinal = commands;

            var tracker = new BlockTypeTracker(typeCount);
            int remainingLength = totalLength - InitialLength;

            var typeCodeFreq = new FrequencyList<BlockTypeCode>();
            var lengthCodeFreq = new FrequencyList<BlockLengthCode>{ BlockLengthCode.MakeCode(InitialLength) };

            foreach(var command in commands){
                var typeCodes = tracker.FindCodes(command.Type);
                var typeCode = typeCodes.Count > 1 ? parameters.BlockTypeCodePicker(typeCodes, typeCodeFreq) : typeCodes[0];

                int length;

                if (command.Length == FinalCommandLengthPlaceholder){
                    length = remainingLength;
                    
                    commandsFinal = new List<BlockSwitchCommand>(commands);
                    commandsFinal[^1] = new BlockSwitchCommand(command.Type, length); // replace the last command with one that has proper length
                }
                else{
                    length = command.Length;
                }

                typeCodeFreq.Add(typeCode);
                lengthCodeFreq.Add(BlockLengthCode.MakeCode(length));

                remainingLength -= length;

                if (remainingLength < 0){
                    long totalCommandLength = InitialLength + commands.Sum(cmd => cmd.Length == FinalCommandLengthPlaceholder ? 0L : cmd.Length);
                    bool hasFinalCommand = commands.Any(cmd => cmd.Length == FinalCommandLengthPlaceholder);

                    throw new InvalidOperationException("Block-switch command lengths exceed the actual amount of symbols in " + Category + " category (total " + totalCommandLength + (hasFinalCommand ? "+final" : "") + ", actual " + totalLength + ").");
                }
            }

            if (remainingLength > 0){
                throw new InvalidOperationException("Block-switch command lengths do not cover the entire " + Category + " category (covered " + (totalLength - remainingLength) + ", remaining " + remainingLength + ").");
            }

            return (new BlockTypeInfo(
                Category,
                typeCount,
                InitialLength,
                parameters.GenerateBlockTypeCodeTree(typeCodeFreq),
                parameters.GenerateBlockLengthCodeTree(lengthCodeFreq)
            ), commandsFinal);
        }
    }
}
