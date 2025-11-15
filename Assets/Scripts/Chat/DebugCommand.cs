using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEngine;


public delegate string Command(ref EntityCommandBuffer ecb,Entity sender);
public delegate string Command<T1>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1);
public delegate string Command<T1, T2>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1, T2 arg2);
public delegate string Command<T1, T2, T3>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1, T2 arg2, T3 arg3);
public delegate string Command<T1, T2, T3, T4>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
public delegate string Command<T1, T2, T3, T4,T5>(ref EntityCommandBuffer ecb, Entity sender, T1 arg1, T2 arg2, T3 arg3, T4 arg4,T5 arg5);



public class DebugCommand : CommandBase
{
    public Command command { private set; get; }


    public DebugCommand(string commandId, string commandDescription, string commandFormat, Command command, bool serwerCommand = true, bool adminCommand = true) : base(serwerCommand,adminCommand,commandId, commandDescription, commandFormat)
    {
        this.command = command;
      
    }
    public override string Invoke(string[] args,ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        return command.Invoke(ref entityCommandBuffer,sender);
    }
}
public class DebugCommand<T1> : CommandBase
{
    public Command<T1> command { private set; get; }
    public DebugCommand(string commandId, string commandDescription, string commandFormat, Command<T1> command,bool serwerCommand = true,bool adminCommand = true) : base(serwerCommand,adminCommand, commandId, commandDescription, commandFormat,typeof(T1))
    {
        this.command = command;
    }
    public override string Invoke(string[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        object arg1;
        TryConvert(args[0],typeof(T1),out arg1);
        return command.Invoke(ref entityCommandBuffer,sender, (T1)arg1);
    }
}

public class DebugCommand<T1,T2> : CommandBase
{
    public Command<T1,T2> command { private set; get; }
    public DebugCommand(string commandId, string commandDescription, string commandFormat, Command<T1,T2> command, bool serwerCommand = true, bool adminCommand = true) : base(serwerCommand,adminCommand,commandId, commandDescription, commandFormat,typeof(T1),typeof(T2))
    {
        this.command = command;
    }
    public override string Invoke(string[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        object arg1,arg2;
        TryConvert(args[0], typeof(T1), out arg1);
        TryConvert(args[1], typeof(T2), out arg2);

        return command.Invoke(ref entityCommandBuffer,sender,(T1)arg1,(T2)arg2);
    }
}

public class DebugCommand<T1,T2,T3> : CommandBase
{
    public Command<T1, T2,T3> command { private set; get; }
    public DebugCommand(string commandId, string commandDescription, string commandFormat, Command<T1, T2, T3> command, bool serwerCommand = true, bool adminCommand = true) : base(serwerCommand,adminCommand, commandId, commandDescription, commandFormat, typeof(T1), typeof(T2),typeof(T3))
    {
        this.command = command;
    }
    public override string Invoke(string[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        object arg1, arg2, arg3;
        TryConvert(args[0], typeof(T1), out arg1);
        TryConvert(args[1], typeof(T2), out arg2);
        TryConvert(args[2], typeof(T3), out arg3);

        return command.Invoke(ref entityCommandBuffer,sender,(T1)arg1, (T2)arg2,(T3)arg3);
    }
}

public class DebugCommand<T1, T2, T3, T4> : CommandBase
{
    public Command<T1, T2, T3, T4> command { private set; get; }
    public DebugCommand(string commandId, string commandDescription, string commandFormat, Command<T1, T2, T3,T4> command, bool serwerCommand = true, bool adminCommand = true) : base(serwerCommand,adminCommand, commandId, commandDescription, commandFormat, typeof(T1), typeof(T2), typeof(T3), typeof(T4))
    {
        this.command = command;
    }
    public override string Invoke(string[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        object arg1, arg2, arg3, arg4;
        TryConvert(args[0], typeof(T1), out arg1);
        TryConvert(args[1], typeof(T2), out arg2);
        TryConvert(args[2], typeof(T3), out arg3);
        TryConvert(args[3], typeof(T4), out arg4);

        return command.Invoke(ref entityCommandBuffer,sender,(T1)arg1, (T2)arg2, (T3) arg3,(T4) arg4);
    }
}


public class DebugCommand<T1, T2, T3, T4,T5> : CommandBase
{
    public Command<T1, T2, T3, T4 , T5> command { private set; get; }
    public DebugCommand(string commandId, string commandDescription, string commandFormat, Command<T1, T2, T3, T4, T5> command, bool serwerCommand = true, bool adminCommand = true) : base(serwerCommand, adminCommand, commandId, commandDescription, commandFormat, typeof(T1), typeof(T2), typeof(T3), typeof(T4),typeof(T5))
    {
        this.command = command;
    }
    public override string Invoke(string[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender)
    {
        object arg1, arg2, arg3, arg4, arg5;
        TryConvert(args[0], typeof(T1), out arg1);
        TryConvert(args[1], typeof(T2), out arg2);
        TryConvert(args[2], typeof(T3), out arg3);
        TryConvert(args[3], typeof(T4), out arg4);
        TryConvert(args[4], typeof(T5), out arg5);


        return command.Invoke(ref entityCommandBuffer, sender, (T1)arg1, (T2)arg2, (T3)arg3, (T4)arg4,(T5) arg5);
    }
}