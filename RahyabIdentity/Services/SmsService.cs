using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MizbanWS;
namespace RahyabIdentity.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger _logger;
        public SmsService(ILogger logger)
        {
            _logger = logger;
        }
        public async Task<string> SendSms(string phoneNumber, string code)
        {
            var pp = new WSSMSSoapClient(WSSMSSoapClient.EndpointConfiguration.WSSMSSoap);
            var refNum = await pp.sendsmsAsync("mpi_rahyab", "rahyabrayan", phoneNumber, $"your code:{code}", "50002120222220", "41");
            if (refNum[0].ToString().Length > 4)
                return "ok";
            _logger.LogError("ErrorCode:" + refNum[0]);
            return "مشکل در ارسال پیامک";
        }
    }
}