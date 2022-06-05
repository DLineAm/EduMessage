namespace EduMessage.Services
{
    public static class FeatureFormatter
    {
        public static string GetFormattedString(string prefix)
        {
            return "(!" + prefix + "[])";
        }
    }
}