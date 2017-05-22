//-------------------------------------------------------------------------------
// <copyright file="StandardFactory.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
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

namespace Appccelerate.StateMachine.Machine
{
    using System;

    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.Contexts;
    using Appccelerate.StateMachine.Machine.Events;
    using Appccelerate.StateMachine.Machine.GuardHolders;
    using Appccelerate.StateMachine.Machine.States;
    using Appccelerate.StateMachine.Machine.Transitions;

    /// <summary>
    /// Standard implementation of the state machine factory.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    // ReSharper disable ClassWithVirtualMembersNeverInherited.Global
    public class StandardFactory<TState, TEvent> : IFactory<TState, TEvent>
    // ReSharper restore ClassWithVirtualMembersNeverInherited.Global
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly IStateMachineInformation<TState, TEvent> stateMachineInformation;
        private readonly IExtensionHost<TState, TEvent> extensionHost;

        public StandardFactory(IStateMachineInformation<TState, TEvent> stateMachineInformation, IExtensionHost<TState, TEvent> extensionHost)
        {
            this.stateMachineInformation = stateMachineInformation;
            this.extensionHost = extensionHost;
        }

        public virtual IState<TState, TEvent> CreateState(TState id)
        {
            return new State<TState, TEvent>(id, this.stateMachineInformation, this.extensionHost);
        }

        public virtual ITransition<TState, TEvent> CreateTransition()
        {
            return new Transition<TState, TEvent>(this.stateMachineInformation, this.extensionHost);
        }

        public virtual IActionHolder CreateActionHolder(Action action)
        {
            return new ArgumentLessActionHolder(action);
        }

        public virtual IActionHolder CreateActionHolder<T>(Action<T> action)
        {
            return new ArgumentActionHolder<T>(action);
        }

        public virtual IActionHolder CreateActionHolder<T>(Action<T> action, T parameter)
        {
            return new ParametrizedActionHolder<T>(action, parameter);    
        }

        public virtual IActionHolder CreateTransitionActionHolder(Action action)
        {
            return new ArgumentLessActionHolder(action);
        }

        public virtual IActionHolder CreateTransitionActionHolder<T>(Action<T> action)
        {
            return new ArgumentActionHolder<T>(action);
        }
        
        public virtual IGuardHolder CreateGuardHolder(Func<bool> guard)
        {
            return new ArgumentLessGuardHolder(guard);
        }

        public virtual IGuardHolder CreateGuardHolder<T>(Func<T, bool> guard)
        {
            return new ArgumentGuardHolder<T>(guard);
        }

        public virtual ITransitionContext<TState, TEvent> CreateTransitionContext(IState<TState, TEvent> state, Missable<TEvent> eventId, object eventArgument, INotifier<TState, TEvent> notifier)
        {
            return new TransitionContext<TState, TEvent>(state, eventId, eventArgument, notifier);
        }

        public virtual StateMachineInitializer<TState, TEvent> CreateStateMachineInitializer(IState<TState, TEvent> initialState, ITransitionContext<TState, TEvent> context)
        {
            return new StateMachineInitializer<TState, TEvent>(initialState, context);
        }
    }
}