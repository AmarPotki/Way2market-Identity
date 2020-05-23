﻿using System.Threading.Tasks;
using Kavenegar;
using Kavenegar.Core.Exceptions;

using Microsoft.Extensions.Logging;
using Kavenegar;
using Kavenegar.Core.Exceptions;

namespace RahyabIdentity.Services
{
    public class SmsService : ISmsService
    {
        private readonly KavenegarApi _api;
        public SmsService()
        {
            _api = new KavenegarApi("6B4D69387132536F3667477771414B6333466B5750515158446D4555445650594E546F577A335931714D493D");
        }
        public string SendVerifySms(string mobile, string token)
        {
            try
            {
                _api.VerifyLookup(mobile, token, "checkidentity");

                return "";
            }
            catch (ApiException ex)
            {
                // در صورتی که خروجی وب سرویس 200 نباشد این خطارخ می دهد.
                return ex.Message;
            }
            catch (HttpException ex)
            {
                // در زمانی که مشکلی در برقرای ارتباط با وب سرویس وجود داشته باشد این خطا رخ می دهد
                return ex.Message;
            }
        }
        public string SendSms(string mobile, string text)
        {
            try
            {
                _api.VerifyLookup(mobile, text, "peygiryreminder");

                return "";
            }
            catch (ApiException ex)
            {
                // در صورتی که خروجی وب سرویس 200 نباشد این خطارخ می دهد.
                return ex.Message;
            }
            catch (HttpException ex)
            {
                // در زمانی که مشکلی در برقرای ارتباط با وب سرویس وجود داشته باشد این خطا رخ می دهد
                return ex.Message;
            }
        }

        //var pp = new WSSMSSoapClient(WSSMSSoapClient.EndpointConfiguration.WSSMSSoap);
        //var refNum = await pp.sendsmsAsync("mpi_rahyab", "rahyabrayan", phoneNumber, $"your code:{code}", "50002120222220", "41");
        //if (refNum[0].ToString().Length > 4)
        //    return "ok";
        //_logger.LogError("ErrorCode:" + refNum[0]);
        //return "مشکل در ارسال پیامک";
    }
}
