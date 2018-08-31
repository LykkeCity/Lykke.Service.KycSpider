using JetBrains.Annotations;

namespace Lykke.Service.KycSpider.Client
{
    /// <summary>
    /// KycSpider client interface.
    /// </summary>
    [PublicAPI]
    public interface IKycSpiderClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - IKycSpiderApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application Api interface</summary>
        IKycSpiderApi Api { get; }
    }
}
