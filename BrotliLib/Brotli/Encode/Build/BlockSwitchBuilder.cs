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
        public Category Category { get; }
        public int InitialLength { get; private set; }

        public IReadOnlyList<BlockSwitchCommand> Commands => commands;
        public BlockSwitchCommand? LastCommand => commands.Count > 0 ? commands[^1] : null;

        public int TypeCount => commands.Count == 0 ? 1 : 1 + commands.Max(command => command.Type);
        public long TotalLength => InitialLength + commands.Sum(cmd => cmd.IsFinalPlaceholder ? 0L : cmd.Length);

        // Fields

        private readonly List<BlockSwitchCommand> commands = new List<BlockSwitchCommand>();

        // Construction

        public BlockSwitchBuilder(BlockTypeInfo info){
            this.Category = info.Category;
            this.InitialLength = info.InitialLength;

            if (InitialLength == BlockTypeInfo.Empty[Category].InitialLength){
                InitialLength = 0;
            }
        }

        public BlockSwitchBuilder(BlockTypeInfo info, IReadOnlyList<BlockSwitchCommand> commands) : this(info){
            this.commands.AddRange(commands);
        }

        // Commands

        public BlockSwitchBuilder Reset(){
            InitialLength = 0;
            commands.Clear();
            return this;
        }

        public BlockSwitchBuilder SetInitialLength(int initialLength){
            InitialLength = initialLength;
            return this;
        }

        public BlockSwitchBuilder AddBlock(byte type, int length){
            if (type == 0 && commands.Count == 0){
                InitialLength += length;
                return this;
            }

            var lastCommand = LastCommand;

            if (lastCommand?.IsFinalPlaceholder == true){
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

        public BlockSwitchBuilder AddFinalBlock(byte type){
            var lastCommand = LastCommand;

            if (lastCommand?.Type == type){
                commands[^1] = new BlockSwitchCommand(type);
            }
            else if (lastCommand?.IsFinalPlaceholder != true){
                commands.Add(new BlockSwitchCommand(type));
            }
            else{
                throw new InvalidOperationException("Cannot add another block-switch command after the final command.");
            }

            return this;
        }

        // Building

        private bool CheckIsEmpty(int totalLength){
            return commands.Count switch{
                0 => true,
                1 => commands[0].Type == 0 && (commands[0].IsFinalPlaceholder || commands[0].Length >= totalLength),
                _ => false
            };
        }

        public (BlockTypeInfo Info, IReadOnlyList<BlockSwitchCommand> Commands) Build(int totalLength, BrotliCompressionParameters parameters){
            if (CheckIsEmpty(totalLength)){
                return (BlockTypeInfo.Empty[Category], Array.Empty<BlockSwitchCommand>());
            }

            int typeCount = TypeCount;

            if (typeCount <= 1){
                throw new InvalidOperationException("Cannot generate block-switch chain that only refers to 1 block type.");
            }

            if (InitialLength < 1){
                throw new InvalidOperationException("Initial block length must be at least 1.");
            }

            if (InitialLength >= totalLength){
                throw new InvalidOperationException("Initial block length must not cover or exceed all symbols (" + InitialLength + " >= " + totalLength + ").");
            }

            var commandsFinal = new List<BlockSwitchCommand>(commands);

            var tracker = new BlockTypeTracker(typeCount);
            int remainingLength = totalLength - InitialLength;

            var typeCodeFreq = new FrequencyList<BlockTypeCode>();
            var lengthCodeFreq = new FrequencyList<BlockLengthCode>{ BlockLengthCode.MakeCode(InitialLength) };

            bool previousCommandReachedEnd = false;

            foreach(var command in commands){
                var typeCodes = tracker.FindCodes(command.Type);
                var typeCode = typeCodes.Count > 1 ? parameters.BlockTypeCodePicker(typeCodes, typeCodeFreq) : typeCodes[0];

                int length;

                if (command.IsFinalPlaceholder){
                    length = remainingLength;
                    commandsFinal[^1] = new BlockSwitchCommand(command.Type, length); // replace the last command with one that has proper length
                }
                else{
                    length = command.Length;
                }

                typeCodeFreq.Add(typeCode);
                lengthCodeFreq.Add(BlockLengthCode.MakeCode(length));

                remainingLength -= length;

                if (remainingLength <= 0){
                    if (!previousCommandReachedEnd){
                        previousCommandReachedEnd = true;
                    }
                    else{
                        bool hasFinalCommand = commands.Any(cmd => cmd.IsFinalPlaceholder);
                        string totalLengthStr = TotalLength + (hasFinalCommand ? "+final" : "");

                        throw new InvalidOperationException("Non-last block-switch command length exceeded the actual amount of symbols in " + Category + " category (total " + totalLengthStr + ", actual " + totalLength + ").");
                    }
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
