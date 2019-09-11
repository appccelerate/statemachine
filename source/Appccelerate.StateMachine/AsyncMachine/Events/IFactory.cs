//-------------------------------------------------------------------------------
// <copyright file="IFactory.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.AsyncMachine.Events
{
    using System;
    using System.Threading.Tasks;
    using ActionHolders;
    using GuardHolders;
    using States;

    public interface IFactory<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        IActionHolder CreateActionHolder(Func<Task> action);

        IActionHolder CreateActionHolder(Action action);

        IActionHolder CreateActionHolder<T>(Action<T> action);

        IActionHolder CreateActionHolder<T>(Action<T> action, T parameter);

        IActionHolder CreateTransitionActionHolder(Action action);

        IActionHolder CreateTransitionActionHolder<T>(Action<T> action);

        IActionHolder CreateActionHolder<T>(Func<T, Task> action);

        IActionHolder CreateActionHolder<T>(Func<T, Task> action, T parameter);

        IActionHolder CreateTransitionActionHolder(Func<Task> action);

        IActionHolder CreateTransitionActionHolder<T>(Func<T, Task> action);

        IGuardHolder CreateGuardHolder(Func<bool> guard);

        IGuardHolder CreateGuardHolder(Func<Task<bool>> guard);

        IGuardHolder CreateGuardHolder<T>(Func<T, Task<bool>> guard);

        IGuardHolder CreateGuardHolder<T>(Func<T, bool> guard);

        ITransitionContext<TState, TEvent> CreateTransitionContext(IStateDefinition<TState, TEvent> stateDefinition, Missable<TEvent> eventId, object eventArgument, INotifier<TState, TEvent> notifier);

        StateMachineInitializer<TState, TEvent> CreateStateMachineInitializer(IStateDefinition<TState, TEvent> initialState, ITransitionContext<TState, TEvent> context);
    }
}