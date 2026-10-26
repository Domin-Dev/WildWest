public interface ICommandArgument
{
    public bool IsCorrect {get;}
    public bool TryParse(string value,out object result);
    public bool TryGetNext(out string value);
    public bool TryGetPrevious(out string value);
    public bool TryComplete(out string value);
}