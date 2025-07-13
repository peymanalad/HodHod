using GraphQL.Types;
using HodHod.Dto;

namespace HodHod.Types;

public class OrganizationUnitType : ObjectGraphType<OrganizationUnitDto>
{
    public OrganizationUnitType()
    {
        Name = "OrganizationUnitType";

        Field(x => x.Id);
        Field(x => x.Code);
        Field(x => x.DisplayName);
        Field(x => x.TenantId, nullable: true);
    }
}

