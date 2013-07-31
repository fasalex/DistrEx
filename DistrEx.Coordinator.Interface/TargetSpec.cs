﻿using System.Threading;
using DistrEx.Common;

namespace DistrEx.Coordinator.Interface
{
    public abstract class TargetSpec
    {
        public TargetedInstruction<TArgument, TResult> Do<TArgument, TResult>(
            Instruction<TArgument, TResult> instruction)
        {
            return Do(CreateInstructionSpec(instruction));
        }

        protected TargetedInstruction<TArgument, TResult> Do<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction)
        {
            return TargetedInstruction<TArgument, TResult>.Create(this, instruction);
        }

        public abstract void TransportAssemblies<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction);

        public abstract void ClearAssemblies();

        protected abstract InstructionSpec<TArgument, TResult> CreateInstructionSpec<TArgument, TResult>(Instruction<TArgument, TResult> instruction);

        public abstract Future<TResult> Invoke<TArgument, TResult>(InstructionSpec<TArgument, TResult> instruction,
                                                                   TArgument argument);
    }
}
