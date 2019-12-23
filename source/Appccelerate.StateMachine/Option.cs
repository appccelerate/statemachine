namespace Appccelerate.StateMachine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Option<T>
    {
        public static Option<T> Some(T value) => new Option<T>(new[] { value });

        public static Option<T> None => new Option<T>(new T[0]);

        private readonly IEnumerable<T> values;

        public Option(
            IEnumerable<T> values)
        {
            this.values = values;
        }

        public bool HasValue
            => this.values != null
               && this.values.Any();

        private T Value
            => !this.HasValue
                ? throw new InvalidOperationException("Maybe does not have a value")
                : this.values.Single();

        public Option<T2> Map<T2>(Func<T, T2> f)
            => this.HasValue
                ? Option<T2>.Some(f(this.Value))
                : Option<T2>.None;
    }
}