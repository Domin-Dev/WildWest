using System;
using System.Globalization;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public abstract class CommandBase 
{
    public bool IsServerCommand { get; private set; }
    public bool IsAdminCommand { get; private set; }
    public string CommandId {  get; private set; }
    public string CommandDescription {  get; private set; }
    public (Type type,string name)[] Arguments {get; private set;}
    public string CommandFormat
    {
        get
        {
            StringBuilder stringBuilder = new StringBuilder();
            for(int i = 0; i < Arguments.Length;i++)
            {
                stringBuilder.Append($"[{Arguments[i].name}]");
                if(i + 1 < Arguments.Length)
                    stringBuilder.Append(" ");
            }
            return stringBuilder.ToString();
        }
    }
    protected CommandBase(bool serverCommand,bool adminCommand, string id, string desc, params (Type type,string name)[] args)
    {
        CommandId = id;
        CommandDescription = desc;
        Arguments = args;
        this.IsServerCommand = serverCommand;
        this.IsAdminCommand = adminCommand;
    }

    public virtual bool Validate(string[] args)
    {
        if (args.Length != Arguments.Length)
            return false;

        for (int i = 0; i < args.Length; i++)
        {
            if (!TryConvert(args[i], Arguments[i].type, out _))
                return false;
        }
        return true;
    }    
    public virtual int CountValidArguments(string[] args)
    {
        int max = math.min(args.Length,Arguments.Length);
        int counter = 0;
        for (int i = 0; i < max; i++)
        {
            if (TryConvert(args[i], Arguments[i].type, out _))
                counter++;
        }
        return counter;
    }
    public virtual bool ValidateArgument(string argument, int index)
    {
        if (index >= Arguments.Length || !TryConvert(argument, Arguments[index].type, out _))
            return false;
        return true;
    }
    public virtual bool TryCompleteArgument(string currentArgument,int index,out string newValue)
    {
        TryConvert(currentArgument, Arguments[index].type, out var result);
        ICommandArgument commandArgument = result as ICommandArgument;
        if(commandArgument != null && !commandArgument.IsCorrect)
           return commandArgument.TryComplete(out newValue);
       
        newValue = default;
        return false;
    }
    protected bool TryConvert(string value, Type targetType, out object result)
    {
        switch (targetType)
        {
            case Type t when t == typeof(int):
                if (int.TryParse(value, out var intValue))
                {
                    result = intValue;
                    return true;
                }
                break;
            case Type t when t == typeof(float):
                if (float.TryParse(
                    value?.Replace(',', '.'),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var floatValue))
                {
                    result = floatValue;
                    return true;
                }
                break;
            case Type t when t == typeof(string):
                result = value;
                return true;
            case Type t when t == typeof(bool):
                if (bool.TryParse(value, out var boolValue))
                {
                    result = boolValue;
                    return true;
                }
                break;
            case Type t when t == typeof(MyColor):
                if (MyColor.TryParseHex(value, out var colorValue))
                {
                    result = colorValue;
                    return true;
                }
                break;
            case Type t when typeof(ICommandArgument).IsAssignableFrom(t):
                var arg = (ICommandArgument)Activator.CreateInstance(t);
                if(arg.TryParse(value,out var commandArg))
                {
                    result = commandArg;
                    return true;
                }
                else
                {
                    result = arg;
                    return false;
                }
                break;
        }
        result = null;
        return false;
    }
    public abstract string Invoke(string[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender);
}
