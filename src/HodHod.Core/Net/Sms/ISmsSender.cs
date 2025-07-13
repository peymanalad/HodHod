using System.Threading.Tasks;

namespace HodHod.Net.Sms;

public interface ISmsSender
{
    Task SendAsync(string number, string message);
}

