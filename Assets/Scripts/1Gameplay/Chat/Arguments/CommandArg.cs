public class CommandArg : ICommandArgument
{
    public string CommandID;
    public string Fill()
    {
        throw new System.NotImplementedException();
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
        value = value.ToLower();
        if(DebugController.Commands.ContainsKey(value))
        {
            CommandID = value;
            result = this; 
            return true;
        }
        result = default;
        return false;
    }
}
