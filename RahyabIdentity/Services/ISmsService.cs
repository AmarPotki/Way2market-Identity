using System.Threading.Tasks;
namespace RahyabIdentity.Services{
    public interface ISmsService{
        Task<string> SendSms(string phoneNumber, string code);
    }
}