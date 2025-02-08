using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommand : CommandBase
{
    public Action command {private set; get; }  
    public DebugCommand(string commandId, string commandDescription, string commandFormat,Action command) :base(commandId, commandDescription, commandFormat)
    {
        this.command = command;
        
    }
    public void Invoke()
    {
        command.Invoke();
    }
}
public class DebugCommand<T1> : CommandBase
{
    public Action<T1> command { private set; get; }
    public DebugCommand(string commandId, string commandDescription, string commandFormat, Action<T1> command) : base(commandId, commandDescription, commandFormat)
    {
        this.command = command;
    }
    public void Invoke(T1 t1)
    {
        command.Invoke(t1);
    }
}

public class DebugCommand<T1,T2> : CommandBase
{
    public Action<T1,T2> command { private set; get; }
    public DebugCommand(string commandId, string commandDescription, string commandFormat, Action<T1,T2> command) : base(commandId, commandDescription, commandFormat)
    {
        this.command = command;
    }
    public void Invoke(T1 t1,T2 t2)
    {
        command.Invoke(t1,t2);
    }
}

public class DebugCommand<T1,T2,T3> : CommandBase
{
    public Action<T1, T2,T3> command { private set; get; }
    public DebugCommand(string commandId, string commandDescription, string commandFormat, Action<T1, T2, T3> command) : base(commandId, commandDescription, commandFormat)
    {
        this.command = command;
    }
    public void Invoke(T1 t1, T2 t2, T3 t3)
    {
        command.Invoke(t1, t2, t3);
    }
}