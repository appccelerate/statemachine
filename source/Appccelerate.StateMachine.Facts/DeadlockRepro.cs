namespace Appccelerate.StateMachine.Facts
{
    using System.Threading.Tasks;
    using Appccelerate.StateMachine.AsyncMachine;
    using Xunit;

    // https://github.com/appccelerate/statemachine/issues/40
    public class DeadlockRepro
    {
        private AsyncActiveStateMachine<int, int> machine;

        private TaskCompletionSource<int> myTcs = new TaskCompletionSource<int>();

        public DeadlockRepro()
        {
            var builder = new StateMachineDefinitionBuilder<int, int>();

            builder
                .In(0).On(1).Execute(() => myTcs.SetResult(0));

            machine = builder
                .WithInitialState(0)
                .Build()
                .CreateActiveStateMachine();
        }

        [Fact]
        public async Task DoesNotDeadlock()
        {
            await machine.Fire(1);
            await machine.Start();
            await myTcs.Task.ConfigureAwait(false);
            await machine.Stop();
        }
    }
}