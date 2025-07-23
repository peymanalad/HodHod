namespace HodHod.TokenAuth.Dto;

public class SwitchedAccountAuthenticateResultModel
{
    public string AccessToken { get; set; }

    public string EncryptedAccessToken { get; set; }

    public int ExpireInSeconds { get; set; }
}