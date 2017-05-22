//-------------------------------------------------------------------------------
// <copyright file="IFactory.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.Events
{
    using System;

    using Appccelerate.StateMachine.Machine.ActionHolders;
    using Appccelerate.StateMachine.Machine.GuardHolders;

    public interface IFactory<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        IState<TState, TEvent> CreateState(TState id);

        ITransition<TState, TEvent> CreateTransition();

        IActionHolder CreateActionHolder(Action action);

        IActionHolder CreateActionHolder<T>(Action<T> action);

        IActionHolder CreateActionHolder<T>(Action<T> action, T parameter);

        IActionHolder CreateTransitionActionHolder(Action action);

        IActionHolder CreateTransitionActionHolder<T>(Action<T> action);

        IGuardHolder CreateGuardHolder(Func<bool> guard);

        IGuardHolder CreateGuardHolder<T>(Func<T, bool> guard);
        
        ITransitionContext<TState, TEvent> CreateTransitionContext(IState<TState, TEvent> state, Missable<TEvent> eventId, object eventArgument, INotifier<TState, TEvent> notifier);

        StateMachineInitializer<TState, TEvent> CreateStateMachineInitializer(IState<TState, TEvent> initialState, ITransitionContext<TState, TEvent> context);
    }
}