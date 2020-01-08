# Custom Types for States and Events
You can use any (non-nullable) type that implements `Equals` and `GetHashCode` methods for states or events.

Here are sample custom state and event classes:
```c#
public class MyState
{
    public MyState(string name)
    {
        this.Name = name;
    }

    private string Name { get; }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((MyState)obj);
    }

    public override int GetHashCode()
    {
        return this.Name != null ? this.Name.GetHashCode() : 0;
    }

    private bool Equals(MyState other)
    {
        return string.Equals(this.Name, other.Name);
    }
}

public class MyEvent
{
    public MyEvent(int identifier)
    {
        this.Identifier = identifier;
    }

    private int Identifier { get; }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((MyEvent)obj);
    }

    public override int GetHashCode()
    {
        return this.Identifier;
    }

    private bool Equals(MyEvent other)
    {
        return this.Identifier == other.Identifier;
    }
}
```

You can use these types now in the state machine:
```c#
var stateMachineDefinitionBuilder = StateMachineBuilder.ForMachine<MyState, MyEvent>();

stateMachineDefinitionBuilder
    .In(new MyState("A"))
        .On(new MyEvent(1)).Goto(new MyState("B"));

stateMachineDefinitionBuilder
    .In(new MyState("B"))
        .ExecuteOnEntry(() => arrivedInStateB = true);

machine = stateMachineDefinitionBuilder
    .WithInitialState(new MyState("A"))
    .Build()
    .CreatePassiveStateMachine();

machine.Start();

machine.Fire(new MyEvent(1)));
```