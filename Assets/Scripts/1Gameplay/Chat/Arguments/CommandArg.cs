using System.Linq;

public class CommandArg : ICommandArgument
{
    public string CommandID;
    public bool IsCorrect => isCorrect;
    private bool isCorrect;
    public CommandArg()
    {
        isCorrect = false;
    }
    public bool TryComplete(out string value)
    {
        if(string.IsNullOrEmpty(CommandID)) 
        {
            value = DebugController.Commands.Last().Key;
            return true;
        }

        var list = DebugController.Commands.Where(x => x.Key.StartsWith(CommandID)).ToList();
        if(list.Count == 1)
        {
            value = list[0].Key;
            return true;
        }
        value = default;
        return false;
    }
    public string GetNext()
    {
        throw new System.NotImplementedException();
    }
    public string GetPrevious()
    {
        throw new System.NotImplementedException();
    }
    public bool TryParse(string value,out object result)
    {
        if(value != null)
        {
            value = value.ToLower();
            if(DebugController.Commands.ContainsKey(value))
            {
                isCorrect = true;
                CommandID = value;
                result = this; 
                return true;
            }
        }
        isCorrect = false;
        CommandID = value;
        result = this;
        return false;
    }
}
