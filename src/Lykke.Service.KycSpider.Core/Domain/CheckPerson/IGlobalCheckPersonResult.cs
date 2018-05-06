namespace Lykke.Service.KycSpider.Core.Domain.CheckPerson
{
    public interface IGlobalCheckPersonResult
    {
        IGlobalCheckPersonRequest Request { get; }
        ICheckPersonResultDiff PepDiff { get; }
        ICheckPersonResultDiff CrimeDiff { get; }
        ICheckPersonResultDiff SanctionDiff { get; }
    }
}
