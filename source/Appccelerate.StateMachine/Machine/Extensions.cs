namespace Appccelerate.StateMachine.Machine
{
    using System;
    using System.Collections.Generic;

    public class Extensions<TState, TEvent>
        where TState : IComparable
        where TEvent : IComparable
    {
        private readonly List<IExtension<TState, TEvent>> extensions = new List<IExtension<TState, TEvent>>();

        public void ForEach(Action<IExtension<TState, TEvent>> action)
        {
            this.extensions.ForEach(action);
        }

        public void Add(IExtension<TState, TEvent> extension)
        {
            this.extensions.Add(extension);
        }

        public void Clear()
        {
            this.extensions.Clear();
        }
    }
}
