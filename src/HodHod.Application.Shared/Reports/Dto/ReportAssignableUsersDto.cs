using System.Collections.Generic;

namespace HodHod.Reports.Dto;

public class ReportAssignableUsersDto
{
    public List<SimpleUserDto> Superiors { get; set; }
    public List<SimpleUserDto> Peers { get; set; }
    public List<SimpleUserDto> Subordinates { get; set; }

    public ReportAssignableUsersDto()
    {
        Superiors = new List<SimpleUserDto>();
        Peers = new List<SimpleUserDto>();
        Subordinates = new List<SimpleUserDto>();
    }
}