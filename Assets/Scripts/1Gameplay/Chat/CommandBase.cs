using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;


public abstract class CommandBase 
{
    public class Argument
    {
        public Type Type;
        public string Name;
        public int Index;
        public object DefaultValue;

        public Argument(Type type,string name,int index,object defaultValue = null)
        {
            this.Type = type;
            this.Name = name;
            this.Index = index;
            this.DefaultValue = defaultValue;
        }
    }
   
    public CommandType CommandType;
    public bool IsServerCommand => CommandType == CommandType.ServerSide || CommandType == CommandType.ServerSideOnlyAdmin;
    public bool IsAdminCommand  => CommandType == CommandType.ClientSideOnlyAdmin || CommandType == CommandType.ServerSideOnlyAdmin;
    public bool IsLocalCommand => CommandType == CommandType.ClientSideOnlyAdmin || CommandType == CommandType.ClientSide;

    public string CommandId {  get; private set; }
    public string CommandDescription {  get; private set; }
    public Argument[] RequiredArguments {get; private set;}
    public Argument[] OptionalArguments {get; private set;}
    public int ArgumentsCount => RequiredArguments.Length + OptionalArguments.Length;
    public string CommandFormat
    {
        get
        {
            StringBuilder stringBuilder = new StringBuilder();
            for(int i = 0; i < ArgumentsCount;i++)
            {
                if(i < RequiredArguments.Length)
                    stringBuilder.Append($"[{RequiredArguments[i].Name}]");
                else
                    stringBuilder.Append($"({OptionalArguments[i].Name})");
                
                if(i + 1 < ArgumentsCount) 
                    stringBuilder.Append(" ");
            }
            return stringBuilder.ToString();
        }
    }
    protected CommandBase(CommandType commandType, string id, string desc, params (Type type,string name,object defaultValue)[] args)
    {
        CommandId = id;
        CommandDescription = desc;
        CommandType = commandType;
        var required = new List<Argument>();
        var optional = new List<Argument>();
        for(int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if(arg.defaultValue != null)
                optional.Add(new Argument(arg.type,arg.name,i,arg.defaultValue));
            else
                required.Add(new Argument(arg.type,arg.name,i));
        }

        RequiredArguments = required.ToArray();
        OptionalArguments = optional.ToArray();
    }

    public bool Validate(string[] args)
    {
        return Validate(args,out var objects);
    }
    public bool Validate(string[] args,out object[] argObjects)
    {
        if (args.Length < RequiredArguments.Length || args.Length > ArgumentsCount)
        {
            argObjects = default;
            return false;
        }
        
        argObjects = new object[ArgumentsCount];
        for (int i = 0; i < RequiredArguments.Length; i++)
        {
            var arg = RequiredArguments[i];
            if (!TryConvert(args[i],arg.Type, out argObjects[arg.Index]))
                return false;
        }

        List<int> optionalArgs = new List<int>();
        for (int i = RequiredArguments.Length; i < args.Length; i++)
        {
            string argText = args[i].ToLower();
            var arg = OptionalArguments.FirstOrDefault(x => argText.StartsWith($"{x.Name}:"));

            if(arg == null)
                return false;
            else if (!optionalArgs.Contains(arg.Index) && TryConvert(argText.Substring(arg.Name.Length+1), arg.Type, out argObjects[arg.Index]))
                optionalArgs.Add(arg.Index);
            else
                return false;     
        }

        if(optionalArgs.Count != OptionalArguments.Length)
        {
            foreach(var arg in OptionalArguments)
            {
                if(!optionalArgs.Contains(arg.Index))
                    argObjects[arg.Index] = arg.DefaultValue;
            }
        }
        return true;
    }    
    public virtual bool ValidateArgument(string argument, int index)
    {
        return TryGetArgument(index,out Argument arg) && TryConvert(argument, arg.Type, out _);
    }
    public virtual bool TryCompleteArgument(string currentArgument,int index,out string newValue)
    {
        if(TryGetICommandArgument(currentArgument,index,out var result) && !result.IsCorrect)
           return result.TryComplete(out newValue);  
        newValue = default;
        return false;
    }    
    public bool TryGetICommandArgument(string currentArgument,int index,out ICommandArgument result)
    {
        if(TryGetArgument(index,out var arg))
        {
            TryConvert(currentArgument,arg.Type, out var convertResult);
            result = convertResult as ICommandArgument;
            return result != null;
        }
        result = default;
        return false;
    }
    
    public bool TryInvoke(string[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender,out string result)
    {
        if(Validate(args,out var argObjects))
        {
            result = Invoke(argObjects,ref entityCommandBuffer,sender);
            return true;
        }
        result = default;
        return false;
    }
    public Argument GetArgument(int index)
    {
        TryGetArgument(index,out Argument argument);
        return argument;
    }
    protected abstract string Invoke(object[] args, ref EntityCommandBuffer entityCommandBuffer, Entity sender);
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
        }
        result = null;
        return false;
    }
    private bool TryGetArgument(int index, out Argument argument)
    {
        if(index >= 0)
        {
            if(index < RequiredArguments.Length)
            {
                argument = RequiredArguments[index];
                return true;
            }
            if(index < OptionalArguments.Length)
            {
                argument = OptionalArguments[index];
                return true;
            }
        }
        argument = default;
        return false;
    }
}
