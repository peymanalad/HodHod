using HodHod.EntityFrameworkCore;

namespace HodHod.Migrations.Seed.Host;

public class InitialHostDbBuilder
{
    private readonly HodHodDbContext _context;

    public InitialHostDbBuilder(HodHodDbContext context)
    {
        _context = context;
    }

    public void Create()
    {
        new DefaultEditionCreator(_context).Create();
        new DefaultLanguagesCreator(_context).Create();
        new HostRoleAndUserCreator(_context).Create();
        new DefaultSettingsCreator(_context).Create();

        _context.SaveChanges();
    }
}

