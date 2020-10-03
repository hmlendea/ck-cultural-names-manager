namespace CKCulturalNamesManager.BusinessLogic
{
    public interface INameValidator
    {
        bool IsNameValid(string name);
        
        bool IsNameValid(string name, string cultureId);
    }
}
