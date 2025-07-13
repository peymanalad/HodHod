using System.Threading.Tasks;
using Abp.Application.Services;
using HodHod.Otp.Dto;

namespace HodHod.Otp;

public interface IOtpAppService : IApplicationService
{
    Task SendOtpAsync(SendOtpInput input);
    Task<OtpLoginResultDto> LoginAsync(OtpLoginInput input);
}