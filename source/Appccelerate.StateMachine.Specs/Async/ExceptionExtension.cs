//-------------------------------------------------------------------------------
// <copyright file="ExceptionExtension.cs" company="Appccelerate">
//   Copyright (c) 2008-2019 Appccelerate
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Appccelerate.StateMachine.Specs.Async
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Appccelerate.StateMachine.AsyncMachine;
    using Appccelerate.StateMachine.AsyncMachine.Extensions;
    using Appccelerate.StateMachine.AsyncMachine.States;
    using Appccelerate.StateMachine.AsyncMachine.Transitions;

    public class ExceptionExtension<TState, TEvent> : AsyncExtensionBase<TState, TEvent>
        where TState : notnull
        where TEvent : notnull
    {
        public List<Exception> GuardExceptions { get; } = new List<Exception>();

        public List<Exception> EntryActionExceptions { get; } = new List<Exception>();

        public List<Exception> ExitActionExceptions { get; } = new List<Exception>();

        public override Task HandlingGuardException(IStateMachineInformation<TState, TEvent> stateMachine, ITransitionDefinition<TState, TEvent> transitionDefinition, ITransitionContext<TState, TEvent> transitionContext, ref Exception exception)
        {
            exception = new WrappedException(exception);

            return Task.CompletedTask;
        }

        public override Task HandledGuardException(IStateMachineInformation<TState, TEvent> stateMachine, ITransitionDefinition<TState, TEvent> transitionDefinition, ITransitionContext<TState, TEvent> transitionContext, Exception exception)
        {
            this.GuardExceptions.Add(exception);

            return Task.CompletedTask;
        }

        public override Task HandlingEntryActionException(IStateMachineInformation<TState, TEvent> stateMachine, IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context, ref Exception exception)
        {
            exception = new WrappedException(exception);

            return Task.CompletedTask;
        }

        public override Task HandledEntryActionException(IStateMachineInformation<TState, TEvent> stateMachine, IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context, Exception exception)
        {
            this.EntryActionExceptions.Add(exception);

            return Task.CompletedTask;
        }

        public override Task HandlingExitActionException(IStateMachineInformation<TState, TEvent> stateMachine, IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context, ref Exception exception)
        {
            exception = new WrappedException(exception);

            return Task.CompletedTask;
        }

        public override Task HandledExitActionException(IStateMachineInformation<TState, TEvent> stateMachine, IStateDefinition<TState, TEvent> stateDefinition, ITransitionContext<TState, TEvent> context, Exception exception)
        {
            this.ExitActionExceptions.Add(exception);

            return Task.CompletedTask;
        }
    }
}