
namespace AssetBuilder
{
    public enum SecurityContext
    {
        Open,
        ReadOnly,
        Full
    }

    public enum UserSecurityLevel
    {
        Unknown,
        Reviewer,
        Translator,
        Editor,
        Builder,
        Admin
    }
}
