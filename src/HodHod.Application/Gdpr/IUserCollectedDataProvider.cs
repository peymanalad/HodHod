using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using HodHod.Dto;

namespace HodHod.Gdpr;

public interface IUserCollectedDataProvider
{
    Task<List<FileDto>> GetFiles(UserIdentifier user);
}
