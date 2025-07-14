using UnityEngine;

public class CommandBase 
{
    public string commandId {  get; private set; }
    public string commandDescription {  get; private set; }
    public string commandFormat {  get; private set; }

    public CommandBase(string commandId, string commandDescription, string commandFormat)
    {
        this.commandId = commandId;
        this.commandDescription = commandDescription;
        this.commandFormat = commandFormat;
    }
}
