using Abp.Application.Services.Dto;

namespace HodHod.Authorization.Users.Dto;

public interface IGetLoginAttemptsInput : ISortedResultRequest
{
    string Filter { get; set; }
}

