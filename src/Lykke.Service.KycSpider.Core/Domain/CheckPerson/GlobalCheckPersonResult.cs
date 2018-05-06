namespace Lykke.Service.KycSpider.Core.Domain.CheckPerson
{
    public class GlobalCheckPersonResult : IGlobalCheckPersonResult
    {
        public IGlobalCheckPersonRequest Request { get; set; }
        public ICheckPersonResultDiff PepDiff { get; set; }
        public ICheckPersonResultDiff CrimeDiff { get; set; }
        public ICheckPersonResultDiff SanctionDiff { get; set; }
    }
}
