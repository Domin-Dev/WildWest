using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEngine;


public enum CommandType
{
    ServerSide = 0,
    ClientSide = 1,
    ServerSideOnlyAdmin = 2,
    ClientSideOnlyAdmin = 3,
}



public delegate string Command(ref EntityCommandBuffer ecb,Entity sender);
public delegate string Command<T1>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1);
public delegate string Command<T1, T2>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1, T2 arg2);
public delegate string Command<T1, T2, T3>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1, T2 arg2, T3 arg3);
public delegate string Command<T1, T2, T3, T4>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
public delegate string Command<T1, T2, T3, T4,T5>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1, T2 arg2, T3 arg3, T4 arg4,T5 arg5);
public delegate string Command<T1, T2, T3, T4,T5,T6>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1, T2 arg2, T3 arg3, T4 arg4,T5 arg5,T6 arg6);


public class DebugCommand : CommandBase
{
    public Command command { private set; get; }
    public DebugCommand(string commandId, string commandDescription, Command command, CommandType commandType = CommandType.ServerSideOnlyAdmin) : base(commandType, commandId, commandDescription)
    {
        this.command = command;   
    }
    protected override string Invoke(object[] args,ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        return command.Invoke(ref entityCommandBuffer,sender);
    }
}

public class DebugCommand<T1> : CommandBase
{
    public Command<T1> command { private set; get; }

    public DebugCommand(
        string commandId, string commandDescription, string argName1,
        Command<T1> command,
        DefaultValue<T1> defaultValue1 = null,
        CommandType commandType = CommandType.ServerSideOnlyAdmin)
        : base(commandType, commandId, commandDescription,
        (typeof(T1), argName1, defaultValue1 == null ? null : defaultValue1.Value))
    {
        this.command = command;
    }

    protected override string Invoke(object[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        return command.Invoke(ref entityCommandBuffer, sender, (T1)args[0]);
    }
}

public class DebugCommand<T1, T2> : CommandBase
{
    public Command<T1, T2> command { private set; get; }

    public DebugCommand(
        string commandId, string commandDescription,
        string argName1, string argName2,
        Command<T1, T2> command,
        DefaultValue<T1> defaultValue1 = null,
        DefaultValue<T2> defaultValue2 = null,
        CommandType commandType = CommandType.ServerSideOnlyAdmin)
        : base(commandType, commandId, commandDescription,
            (typeof(T1), argName1, defaultValue1 == null ? null : defaultValue1.Value),
            (typeof(T2), argName2, defaultValue2 == null ? null : defaultValue2.Value))
    {
        this.command = command;
    }

    protected override string Invoke(object[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        return command.Invoke(ref entityCommandBuffer, sender, (T1)args[0], (T2)args[1]);
    }
}

public class DebugCommand<T1, T2, T3> : CommandBase
{
    public Command<T1, T2, T3> command { private set; get; }

    public DebugCommand(
        string commandId, string commandDescription,
        string argName1, string argName2, string argName3,
        Command<T1, T2, T3> command,
        DefaultValue<T1> defaultValue1 = null,
        DefaultValue<T2> defaultValue2 = null,
        DefaultValue<T3> defaultValue3 = null,
        CommandType commandType = CommandType.ServerSideOnlyAdmin)
        : base(commandType, commandId, commandDescription,
            (typeof(T1), argName1, defaultValue1 == null ? null : defaultValue1.Value),
            (typeof(T2), argName2, defaultValue2 == null ? null : defaultValue2.Value),
            (typeof(T3), argName3, defaultValue3 == null ? null : defaultValue3.Value))
    {
        this.command = command;
    }

    protected override string Invoke(object[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        return command.Invoke(ref entityCommandBuffer, sender, (T1)args[0], (T2)args[1], (T3)args[2]);
    }
}

public class DebugCommand<T1, T2, T3, T4> : CommandBase
{
    public Command<T1, T2, T3, T4> command { private set; get; }

    public DebugCommand(
        string commandId, string commandDescription,
        string argName1, string argName2, string argName3, string argName4,
        Command<T1, T2, T3, T4> command,
        DefaultValue<T1> defaultValue1 = null,
        DefaultValue<T2> defaultValue2 = null,
        DefaultValue<T3> defaultValue3 = null,
        DefaultValue<T4> defaultValue4 = null,
        CommandType commandType = CommandType.ServerSideOnlyAdmin)
        : base(commandType, commandId, commandDescription,
            (typeof(T1), argName1, defaultValue1 == null ? null : defaultValue1.Value),
            (typeof(T2), argName2, defaultValue2 == null ? null : defaultValue2.Value),
            (typeof(T3), argName3, defaultValue3 == null ? null : defaultValue3.Value),
            (typeof(T4), argName4, defaultValue4 == null ? null : defaultValue4.Value))
    {
        this.command = command;
    }

    protected override string Invoke(object[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        return command.Invoke(ref entityCommandBuffer, sender,(T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]);
    }
}

public class DebugCommand<T1, T2, T3, T4, T5> : CommandBase
{
    public Command<T1, T2, T3, T4, T5> command { private set; get; }

    public DebugCommand(
        string commandId, string commandDescription,
        string argName1, string argName2, string argName3, string argName4, string argName5,
        Command<T1, T2, T3, T4, T5> command,
        DefaultValue<T1> defaultValue1 = null,
        DefaultValue<T2> defaultValue2 = null,
        DefaultValue<T3> defaultValue3 = null,
        DefaultValue<T4> defaultValue4 = null,
        DefaultValue<T5> defaultValue5 = null,
        CommandType commandType = CommandType.ServerSideOnlyAdmin)
        : base(commandType, commandId, commandDescription,
            (typeof(T1), argName1, defaultValue1 == null ? null : defaultValue1.Value),
            (typeof(T2), argName2, defaultValue2 == null ? null : defaultValue2.Value),
            (typeof(T3), argName3, defaultValue3 == null ? null : defaultValue3.Value),
            (typeof(T4), argName4, defaultValue4 == null ? null : defaultValue4.Value),
            (typeof(T5), argName5, defaultValue5 == null ? null : defaultValue5.Value))
    {
        this.command = command;
    }

    protected override string Invoke(object[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        return command.Invoke(ref entityCommandBuffer, sender,(T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4]);
    }
}

public class DebugCommand<T1, T2, T3, T4, T5, T6> : CommandBase
{
    public Command<T1, T2, T3, T4, T5, T6> command { private set; get; }

    public DebugCommand(
        string commandId, string commandDescription,
        string argName1, string argName2, string argName3,
        string argName4, string argName5, string argName6,
        Command<T1, T2, T3, T4, T5, T6> command,
        DefaultValue<T1> defaultValue1 = null,
        DefaultValue<T2> defaultValue2 = null,
        DefaultValue<T3> defaultValue3 = null,
        DefaultValue<T4> defaultValue4 = null,
        DefaultValue<T5> defaultValue5 = null,
        DefaultValue<T6> defaultValue6 = null,
        CommandType commandType = CommandType.ServerSideOnlyAdmin)
        : base(commandType, commandId, commandDescription,
            (typeof(T1), argName1, defaultValue1 == null ? null : defaultValue1.Value),
            (typeof(T2), argName2, defaultValue2 == null ? null : defaultValue2.Value),
            (typeof(T3), argName3, defaultValue3 == null ? null : defaultValue3.Value),
            (typeof(T4), argName4, defaultValue4 == null ? null : defaultValue4.Value),
            (typeof(T5), argName5, defaultValue5 == null ? null : defaultValue5.Value),
            (typeof(T6), argName6, defaultValue6 == null ? null : defaultValue6.Value))
    {
        this.command = command;
    }

    protected override string Invoke(object[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        return command.Invoke(ref entityCommandBuffer, sender,(T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5]);
    }
}
