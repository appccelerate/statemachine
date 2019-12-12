namespace Appccelerate.StateMachine.Machine.Transitions
{
    using System;

    /// <summary>
    /// Represents a not fired transition - there was no transition found for an event in the current state or a super state.
    /// </summary>
    /// <typeparam name="TState">ype of the states.</typeparam>
    public class NotFiredTransitionResult<TState> : ITransitionResult<TState>
        where TState : IComparable
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="ITransitionResult{TState}"/> is fired.
        /// </summary>
        /// <value><c>true</c> if fired; otherwise, <c>false</c>.</value>
        public bool Fired => false;
    }
}