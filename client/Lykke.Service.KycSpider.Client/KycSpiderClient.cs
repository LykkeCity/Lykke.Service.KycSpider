using System;
using Common.Log;

namespace Lykke.Service.KycSpider.Client
{
    public class KycSpiderClient : IKycSpiderClient, IDisposable
    {
        private readonly ILog _log;

        public KycSpiderClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
