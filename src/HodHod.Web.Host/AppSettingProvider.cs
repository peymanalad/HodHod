using System;
using Microsoft.Extensions.Configuration;

public static class AppSettingProvider
{
    public static string Get(string key, IConfiguration config)
    {
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(envValue))
            return envValue;

        var configKey = key.Replace("__", ":");
        return config[configKey];
    }

    public static bool GetBool(string key, IConfiguration config, bool defaultValue = false)
    {
        var value = Get(key, config);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public static int GetInt(string key, IConfiguration config, int defaultValue = 0)
    {
        var value = Get(key, config);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }
}