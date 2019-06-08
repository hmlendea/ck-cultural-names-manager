namespace CK2LandedTitlesManager.BusinessLogic
{
    public interface INameCleaner
    {
        string Normalise(string name);
        
        string Clean(string name);
    }
}