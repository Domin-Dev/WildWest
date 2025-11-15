using System;
using System.Globalization;
using Unity.Entities;
using UnityEngine;

public abstract class CommandBase 
{
    public bool isServerCommand { get; private set; }
    public bool isAdminCommand { get; private set; }
    public string commandId {  get; private set; }
    public string commandDescription {  get; private set; }
    public string commandFormat {  get; private set; } 
    public Type[] argumentTypes { get; private set; }

    protected CommandBase(bool serverCommand,bool adminCommand, string id, string desc, string format, params Type[] args)
    {
        commandId = id;
        commandDescription = desc;
        commandFormat = format;
        argumentTypes = args;
        this.isServerCommand = serverCommand;
        this.isAdminCommand = adminCommand;
    }

    public virtual bool Validate(string[] args)
    {
        if (args.Length != argumentTypes.Length)
            return false;

        for (int i = 0; i < args.Length; i++)
        {
            if (!TryConvert(args[i], argumentTypes[i], out _))
                return false;
        }
        return true;
    }

    protected bool TryConvert(string value, Type targetType, out object result)
    {
        if (targetType == typeof(int))
        {
            if (int.TryParse(value, out var tmp)) { result = tmp; return true; }
        }
        else if (targetType == typeof(float))
        {
            var normalized = value.Replace(',', '.');          
            if (float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var tmp)) { result = tmp; return true; }
        }
        else if (targetType == typeof(string))
        {
            result = value; return true;
        }
        else if (targetType == typeof(bool))
        {
            if (bool.TryParse(value, out var tmp)) { result = tmp; return true; }
        }
        result = null;
        return false;
    }

    public abstract string Invoke(string[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender);
}
