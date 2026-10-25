public interface ICommandArgument
{
    public bool IsCorrect {get;}
    public bool TryParse(string value,out object result);
    public string GetNext();
    public string GetPrevious();
    public bool TryComplete(out string value);
}