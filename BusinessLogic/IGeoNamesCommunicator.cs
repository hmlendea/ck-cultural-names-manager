using System.Collections.Generic;
using System.Threading.Tasks;

namespace CK2LandedTitlesManager.BusinessLogic
{
    public interface IGeoNamesCommunicator
    {
        Task<string> TryGatherExonym(string titleId, string cultureId);

        Task<string> GatherExonym(string titleId, string cultureId);

        // TODO: Remove
        IDictionary<string, string[]> CultureLanguages { get; }
    }
}
