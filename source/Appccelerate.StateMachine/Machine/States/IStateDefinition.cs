//-------------------------------------------------------------------------------
// <copyright file="IStateDefinition.cs" company="Appccelerate">
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

namespace Appccelerate.StateMachine.Machine.States
{
    using System;
    using System.Collections.Generic;
    using ActionHolders;
    using Transitions;

    public interface IStateDefinition<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        TState Id { get; }

        IReadOnlyDictionary<TEvent, IEnumerable<ITransitionDefinition<TState, TEvent>>> Transitions { get; }

        IEnumerable<TransitionInfo<TState, TEvent>> TransitionInfos { get; }

        int Level { get; }

        IStateDefinition<TState, TEvent> InitialState { get; }

        HistoryType HistoryType { get; }

        IStateDefinition<TState, TEvent> SuperState { get; }

        IEnumerable<IStateDefinition<TState, TEvent>> SubStates { get; }

        IEnumerable<IActionHolder> EntryActions { get; }

        IEnumerable<IActionHolder> ExitActions { get; }
    }
}