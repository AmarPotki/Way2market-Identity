using System.Threading.Tasks;
namespace RahyabIdentity.Services{
    public interface ISmsService
    {
        string SendVerifySms(string mobile, string token);
        string SendSms(string mobile, string text);
    }
}