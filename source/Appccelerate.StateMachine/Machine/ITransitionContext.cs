//-------------------------------------------------------------------------------
// <copyright file="ITransitionContext.cs" company="Appccelerate">
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

    /// <summary>
    /// Provides information about the current transition.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    public interface ITransitionContext<TState, TEvent>
        where TState : IComparable where TEvent : IComparable
    {
        IState<TState, TEvent> State { get; }

        Missable<TEvent> EventId { get; }

        object EventArgument { get; }

        void AddRecord(TState stateId, RecordType recordType);

        string GetRecords();

        void OnExceptionThrown(Exception exception);

        void OnTransitionBegin();
    }
}