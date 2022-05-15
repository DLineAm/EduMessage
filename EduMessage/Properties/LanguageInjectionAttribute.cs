using System;

namespace EduMessage.Annotations
{
    /// <summary>
    /// Indicates that the marked parameter, field, or property is accepting a string literal
    /// containing code fragment in a language specified by the <see cref="InjectedLanguage"/>.
    /// </summary>
    /// <example><code>
    /// void Foo([LanguageInjection(InjectedLanguage.CSS, Prefix = "body{", Suffix = "}")] string cssProps)
    /// {
    ///   // cssProps should only contains a list of CSS properties
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class LanguageInjectionAttribute : Attribute
    {
        public LanguageInjectionAttribute(InjectedLanguage injectedLanguage)
        {
            InjectedLanguage = injectedLanguage;
        }

        /// <summary>Specify a language of injected code fragment.</summary>
        public InjectedLanguage InjectedLanguage { get; }
        /// <summary>Specify a string that "precedes" injected string literal.</summary>
        [CanBeNull] public string Prefix { get; set; }
        /// <summary>Specify a string that "follows" injected string literal.</summary>
        [CanBeNull] public string Suffix { get; set; }
    }
}