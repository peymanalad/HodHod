using System.Globalization;

namespace HodHod.Localization;

public interface IApplicationCulturesProvider
{
    CultureInfo[] GetAllCultures();
}

