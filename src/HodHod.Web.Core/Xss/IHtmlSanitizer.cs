using Abp.Dependency;

namespace HodHod.Web.Xss;

public interface IHtmlSanitizer : ITransientDependency
{
    string Sanitize(string html);
}

