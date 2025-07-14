using System.Text.RegularExpressions;

namespace HodHod;

public static class PhoneNumberHelper
{
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // keep digits only
        var digits = Regex.Replace(input, @"\D", "");
        if (digits.StartsWith("0"))
        {
            digits = "98" + digits.Substring(1);
        }
        else if (!digits.StartsWith("98"))
        {
            // if missing leading 98 but not starting with 0, assume digits already without country code
            digits = "98" + digits;
        }

        return digits;
    }
}