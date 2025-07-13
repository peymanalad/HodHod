using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HodHod.Otp.Dto;

public class OtpLoginResultDto
{
    public string AccessToken { get; set; }
    public string EncryptedAccessToken { get; set; }
    public int ExpireInSeconds { get; set; }
    public string RefreshToken { get; set; }
    public int RefreshTokenExpireInSeconds { get; set; }
    public long UserId { get; set; }
    public string ReturnUrl { get; set; }
}
