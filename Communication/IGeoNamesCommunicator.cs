using System.Threading.Tasks;

namespace CK2LandedTitlesManager.Communication
{
    public interface IGeoNamesCommunicator
    {
        Task<string> GatherExonym(string placeName, string language);
    }
}
