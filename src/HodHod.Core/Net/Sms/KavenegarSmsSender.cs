using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Abp.Dependency;

namespace HodHod.Net.Sms;

public class KavenegarSmsSender : ISmsSender, ITransientDependency
{
    private readonly ILogger<KavenegarSmsSender> _logger;

    public KavenegarSmsSender(ILogger<KavenegarSmsSender> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(string number, string message)
    {
        await SendAsyncResult(number, message);
    }

    private async Task<bool> SendAsyncResult(string number, string message)
    {
        try
        {
            var token = message;
            var apiKey = "6A596B434E3764674E57737079706F32306F34714F59417532734E416B4949575261636750646B654C70513D";
            var sender = "20005209";
            var tag = "otp";
            var appname = "سامانه هدهد";
            const string otpTemplate = "« {appName} »\nکد ثبت گزارش:\n{otp}\n{apiKey}\nلغو11";
            var finalMessage = otpTemplate
                .Replace("{appName}", appname)
                .Replace("{otp}", token)
                .Replace("{apiKey}", "#hodhod");
            var url = $"https://api.kavenegar.com/v1/{apiKey}/sms/send.json?receptor={number}&sender={sender}&message={finalMessage}&tag={tag}";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Kavenegar success: {Response}", content);
                return true;
            }
            else
            {
                _logger.LogError("Kavenegar failed: Status={StatusCode}, Body={Response}",
                    response.StatusCode, content);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending SMS to Kavenegar");
            return false;
        }
    }
}