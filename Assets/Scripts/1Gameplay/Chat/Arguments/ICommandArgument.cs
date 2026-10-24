public interface ICommandArgument
{
    public bool TryParse(string value,out object result);
    public string GetNext();
    public string GetPrevious();
    public string Fill();
}