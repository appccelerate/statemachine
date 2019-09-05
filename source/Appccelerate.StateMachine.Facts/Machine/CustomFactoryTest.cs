namespace Appccelerate.StateMachine.Facts.Machine
{
    using System;
    using FakeItEasy;
    using StateMachine.Machine;
    using StateMachine.Machine.ActionHolders;
    using StateMachine.Machine.Contexts;
    using StateMachine.Machine.Events;
    using StateMachine.Machine.GuardHolders;
    using StateMachine.Machine.States;
    using Xunit;

    public class CustomFactoryTest
    {
        [Fact]
        public void CustomFactoryIsUsedForParameterlessActions()
        {
            var actionHolder = A.Fake<IActionHolder>();
            var factory = new FakeFactory();
            factory.SetParameterlessActionHolder(actionHolder);

            var stateMachine = new StateMachineDefinitionBuilder<string, int>()
                .WithCustomFactory(factory)
                .WithConfiguration(x =>
                    x.In("A")
                        .ExecuteOnExit(ParameterlessMethod)
                        .On(1)
                        .Goto("B"))
                .WithConfiguration(x =>
                    x.In("B")
                        .ExecuteOnEntry(ParameterlessMethod))
                .Build()
                .CreatePassiveStateMachine();

            stateMachine.Initialize("A");
            stateMachine.Start();

            stateMachine.Fire(1, "SomeArgument");

            A.CallTo(() =>
                    actionHolder.Execute("SomeArgument"))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void CustomFactoryIsUsedForParameterizedActions()
        {
            var actionHolder = A.Fake<IActionHolder>();
            var factory = new FakeFactory();
            factory.SetParameterizedActionHolder(actionHolder);

            var stateMachine = new StateMachineDefinitionBuilder<string, int>()
                .WithCustomFactory(factory)
                .WithConfiguration(x =>
                    x.In("A")
                        .ExecuteOnExitParametrized(ParameterizedMethod, "hello")
                        .On(1)
                        .Goto("B"))
                .WithConfiguration(x =>
                    x.In("B")
                        .ExecuteOnEntryParametrized(ParameterizedMethod, "world"))
                .Build()
                .CreatePassiveStateMachine();

            stateMachine.Initialize("A");
            stateMachine.Start();

            stateMachine.Fire(1, "SomeArgument");

            A.CallTo(() =>
                    actionHolder.Execute("SomeArgument"))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void CustomFactoryIsUsedForArgumentActions()
        {
            var actionHolder = A.Fake<IActionHolder>();
            var factory = new FakeFactory();
            factory.SetArgumentActionHolder(actionHolder);

            var stateMachine = new StateMachineDefinitionBuilder<string, int>()
                .WithCustomFactory(factory)
                .WithConfiguration(x =>
                    x.In("A")
                        .ExecuteOnExit<string>(ParameterizedMethod)
                        .On(1)
                        .Goto("B"))
                .WithConfiguration(x =>
                    x.In("B")
                        .ExecuteOnEntry<string>(ParameterizedMethod))
                .Build()
                .CreatePassiveStateMachine();

            stateMachine.Initialize("A");
            stateMachine.Start();

            stateMachine.Fire(1, "SomeArgument");

            A.CallTo(() =>
                    actionHolder.Execute("SomeArgument"))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void CustomFactoryIsUsedForTransitionParameterlessActions()
        {
            var actionHolder = A.Fake<IActionHolder>();
            var factory = new FakeFactory();
            factory.SetParameterlessTransitionActionHolder(actionHolder);

            var stateMachine = new StateMachineDefinitionBuilder<string, int>()
                .WithCustomFactory(factory)
                .WithConfiguration(x =>
                    x.In("A")
                        .On(1)
                        .Goto("B")
                        .Execute(ParameterlessMethod))
                .WithConfiguration(x =>
                    x.In("B"))
                .Build()
                .CreatePassiveStateMachine();

            stateMachine.Initialize("A");
            stateMachine.Start();

            stateMachine.Fire(1, "SomeArgument");

            A.CallTo(() =>
                    actionHolder.Execute("SomeArgument"))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CustomFactoryIsUsedForTransitionActions()
        {
            var actionHolder = A.Fake<IActionHolder>();
            var factory = new FakeFactory();
            factory.SetTransitionActionHolder(actionHolder);

            var stateMachine = new StateMachineDefinitionBuilder<string, int>()
                .WithCustomFactory(factory)
                .WithConfiguration(x =>
                    x.In("A")
                        .On(1)
                        .Goto("B")
                        .Execute<string>(ParameterizedMethod))
                .WithConfiguration(x =>
                    x.In("B"))
                .Build()
                .CreatePassiveStateMachine();

            stateMachine.Initialize("A");
            stateMachine.Start();

            stateMachine.Fire(1, "SomeArgument");

            A.CallTo(() =>
                    actionHolder.Execute("SomeArgument"))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CustomFactoryIsUsedForParameterlessGuardHolder()
        {
            var guardHolder = A.Fake<IGuardHolder>();
            var factory = new FakeFactory();
            factory.SetParameterlessGuardHolder(guardHolder);

            var stateMachine = new StateMachineDefinitionBuilder<string, int>()
                .WithCustomFactory(factory)
                .WithConfiguration(x =>
                    x.In("A")
                        .On(1)
                        .If(() => true)
                        .Goto("B"))
                .WithConfiguration(x =>
                    x.In("B"))
                .Build()
                .CreatePassiveStateMachine();

            stateMachine.Initialize("A");
            stateMachine.Start();

            stateMachine.Fire(1, "SomeArgument");

            A.CallTo(() =>
                    guardHolder.Execute("SomeArgument"))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CustomFactoryIsUsedForGuardHolder()
        {
            var guardHolder = A.Fake<IGuardHolder>();
            var factory = new FakeFactory();
            factory.SetGuardHolder(guardHolder);

            var stateMachine = new StateMachineDefinitionBuilder<string, int>()
                .WithCustomFactory(factory)
                .WithConfiguration(x =>
                    x.In("A")
                        .On(1)
                        .If<string>(p => true)
                        .Goto("B"))
                .WithConfiguration(x =>
                    x.In("B"))
                .Build()
                .CreatePassiveStateMachine();

            stateMachine.Initialize("A");
            stateMachine.Start();

            stateMachine.Fire(1, "SomeArgument");

            A.CallTo(() =>
                    guardHolder.Execute("SomeArgument"))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CustomFactoryIsUsedForTransitionContext()
        {
            var actionHolder = A.Fake<IActionHolder>();
            var factory = new FakeFactory();
            factory.SetTransitionActionHolder(actionHolder);
            factory.SetEventArgumentOfTransitionContext("ArgumentFromFakedTransitionContext");

            var stateMachine = new StateMachineDefinitionBuilder<string, int>()
                .WithCustomFactory(factory)
                .WithConfiguration(x =>
                    x.In("A")
                        .On(1)
                        .Goto("B")
                        .Execute<string>(ParameterizedMethod))
                .WithConfiguration(x =>
                    x.In("B"))
                .Build()
                .CreatePassiveStateMachine();

            stateMachine.Initialize("A");
            stateMachine.Start();

            stateMachine.Fire(1, "RealArgument");

            A.CallTo(() =>
                    actionHolder.Execute("ArgumentFromFakedTransitionContext"))
                .MustHaveHappenedOnceExactly();
        }

        private static void ParameterlessMethod()
        {
        }

        private static void ParameterizedMethod(string param)
        {
        }

        private class FakeFactory : IFactory<string, int>
        {
            private IActionHolder parameterlessActionHolder = A.Fake<IActionHolder>();
            private IActionHolder parameterizedActionHolder = A.Fake<IActionHolder>();
            private IActionHolder argumentActionHolder = A.Fake<IActionHolder>();

            private IActionHolder parameterlessTransitionActionHolder = A.Fake<IActionHolder>();
            private IActionHolder transitionActionHolder = A.Fake<IActionHolder>();

            private IGuardHolder parameterlessGuardHolder = A.Fake<IGuardHolder>();
            private IGuardHolder guardHolder = A.Fake<IGuardHolder>();

            private string overridenEventArgument;

            public void SetParameterlessActionHolder(IActionHolder action)
            {
                this.parameterlessActionHolder = action;
            }

            public void SetParameterizedActionHolder(IActionHolder action)
            {
                this.parameterizedActionHolder = action;
            }

            public void SetArgumentActionHolder(IActionHolder action)
            {
                this.argumentActionHolder = action;
            }

            public void SetParameterlessTransitionActionHolder(IActionHolder action)
            {
                this.parameterlessTransitionActionHolder = action;
            }

            public void SetTransitionActionHolder(IActionHolder action)
            {
                this.transitionActionHolder = action;
            }

            public void SetParameterlessGuardHolder(IGuardHolder action)
            {
                this.parameterlessGuardHolder = action;
            }

            public void SetGuardHolder(IGuardHolder action)
            {
                this.guardHolder = action;
            }

            public void SetEventArgumentOfTransitionContext(string eventArgumentToUse)
            {
                this.overridenEventArgument = eventArgumentToUse;
            }

            public IActionHolder CreateActionHolder(Action action)
            {
                return this.parameterlessActionHolder;
            }

            public IActionHolder CreateActionHolder<T>(Action<T> action)
            {
                return this.argumentActionHolder;
            }

            public IActionHolder CreateActionHolder<T>(Action<T> action, T parameter)
            {
                return this.parameterizedActionHolder;
            }

            public IActionHolder CreateTransitionActionHolder(Action action)
            {
                return this.parameterlessTransitionActionHolder;
            }

            public IActionHolder CreateTransitionActionHolder<T>(Action<T> action)
            {
                return this.transitionActionHolder;
            }

            public IGuardHolder CreateGuardHolder(Func<bool> guard)
            {
                return this.parameterlessGuardHolder;
            }

            public IGuardHolder CreateGuardHolder<T>(Func<T, bool> guard)
            {
                return this.guardHolder;
            }

            public ITransitionContext<string, int> CreateTransitionContext(IStateDefinition<string, int> stateDefinition, Missable<int> eventId, object eventArgument, INotifier<string, int> notifier)
            {
                var eventArgumentToUse = eventArgument;
                if (this.overridenEventArgument != null)
                {
                    eventArgumentToUse = this.overridenEventArgument;
                }

                return new TransitionContext<string, int>(stateDefinition, eventId, eventArgumentToUse, notifier);
            }

            public StateMachineInitializer<string, int> CreateStateMachineInitializer(IStateDefinition<string, int> initialState, ITransitionContext<string, int> context)
            {
                return new StateMachineInitializer<string, int>(initialState, context);
            }
        }
    }
}
