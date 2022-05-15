using System;

namespace EduMessage.Annotations
{
    /// <summary>
    /// <para>
    /// Defines the code search template using the Structural Search and Replace syntax.
    /// It allows you to find and, if necessary, replace blocks of code that match a specific pattern.
    /// Search and replace patterns consist of a textual part and placeholders.
    /// Textural part must contain only identifiers allowed in the target language and will be matched exactly (white spaces, tabulation characters, and line breaks are ignored).
    /// Placeholders allow matching variable parts of the target code blocks.
    /// A placeholder has the following format: $placeholder_name$- where placeholder_name is an arbitrary identifier.
    /// </para>
    /// <para>
    /// Available placeholders:
    /// <list type="bullet">
    /// <item>$this$ - expression of containing type</item>
    /// <item>$thisType$ - containing type</item>
    /// <item>$member$ - current member placeholder</item>
    /// <item>$qualifier$ - this placeholder is available in the replace pattern and can be used to insert qualifier expression matched by the $member$ placeholder.
    /// (Note that if $qualifier$ placeholder is used, then $member$ placeholder will match only qualified references)</item>
    /// <item>$expression$ - expression of any type</item>
    /// <item>$identifier$ - identifier placeholder</item>
    /// <item>$args$ - any number of arguments</item>
    /// <item>$arg$ - single argument</item>
    /// <item>$arg1$ ... $arg10$ - single argument</item>
    /// <item>$stmts$ - any number of statements</item>
    /// <item>$stmt$ - single statement</item>
    /// <item>$stmt1$ ... $stmt10$ - single statement</item>
    /// <item>$name{Expression, 'Namespace.FooType'}$ - expression with 'Namespace.FooType' type</item>
    /// <item>$expression{'Namespace.FooType'}$ - expression with 'Namespace.FooType' type</item>
    /// <item>$name{Type, 'Namespace.FooType'}$ - 'Namespace.FooType' type</item>
    /// <item>$type{'Namespace.FooType'}$ - 'Namespace.FooType' type</item>
    /// <item>$statement{1,2}$ - 1 or 2 statements</item>
    /// </list>
    /// </para>
    /// <para>
    /// Note that you can also define your own placeholders of the supported types and specify arguments for each placeholder type.
    /// This can be done using the following format: $name{type, arguments}$. Where 'name' - is the name of your placeholder,
    /// 'type' - is the type of your placeholder (one of the following: Expression, Type, Identifier, Statement, Argument, Member),
    /// 'arguments' - arguments list for your placeholder. Each placeholder type supports it's own arguments, check examples below for mode details.
    /// Placeholder type may be omitted and determined from the placeholder name, if name has one of the following prefixes:
    /// <list type="bullet">
    /// <item>expr, expression - expression placeholder, e.g. $exprPlaceholder{}$, $expressionFoo{}$</item>
    /// <item>arg, argument - argument placeholder, e.g. $argPlaceholder{}$, $argumentFoo{}$</item>
    /// <item>ident, identifier - identifier placeholder, e.g. $identPlaceholder{}$, $identifierFoo{}$</item>
    /// <item>stmt, statement - statement placeholder, e.g. $stmtPlaceholder{}$, $statementFoo{}$</item>
    /// <item>type - type placeholder, e.g. $typePlaceholder{}$, $typeFoo{}$</item>
    /// <item>member - member placeholder, e.g. $memberPlaceholder{}$, $memberFoo{}$</item>
    /// </list>
    /// </para>
    /// <para>
    /// Expression placeholder arguments:
    /// <list type="bullet">
    /// <item>expressionType - string value in single quotes, specifies full type name to match (empty string by default)</item>
    /// <item>exactType - boolean value, specifies if expression should have exact type match (false by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item>$myExpr{Expression, 'Namespace.FooType', true}$ - defines expression placeholder, matching expressions of the 'Namespace.FooType' type with exact matching.</item>
    /// <item>$myExpr{Expression, 'Namespace.FooType'}$ - defines expression placeholder, matching expressions of the 'Namespace.FooType' type or expressions which can be implicitly converted to 'Namespace.FooType'.</item>
    /// <item>$myExpr{Expression}$ - defines expression placeholder, matching expressions of any type.</item>
    /// <item>$exprFoo{'Namespace.FooType', true}$ - defines expression placeholder, matching expressions of the 'Namespace.FooType' type with exact matching.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Type placeholder arguments:
    /// <list type="bullet">
    /// <item>type - string value in single quotes, specifies full type name to match (empty string by default)</item>
    /// <item>exactType - boolean value, specifies if expression should have exact type match (false by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item>$myType{Type, 'Namespace.FooType', true}$ - defines type placeholder, matching 'Namespace.FooType' types with exact matching.</item>
    /// <item>$myType{Type, 'Namespace.FooType'}$ - defines type placeholder, matching 'Namespace.FooType' types or types, which can be implicitly converted to 'Namespace.FooType'.</item>
    /// <item>$myType{Type}$ - defines type placeholder, matching any type.</item>
    /// <item>$typeFoo{'Namespace.FooType', true}$ - defines types placeholder, matching 'Namespace.FooType' types with exact matching.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Identifier placeholder arguments:
    /// <list type="bullet">
    /// <item>nameRegex - string value in single quotes, specifies regex to use for matching (empty string by default)</item>
    /// <item>nameRegexCaseSensitive - boolean value, specifies if name regex is case sensitive (true by default)</item>
    /// <item>type - string value in single quotes, specifies full type name to match (empty string by default)</item>
    /// <item>exactType - boolean value, specifies if expression should have exact type match (false by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item>$myIdentifier{Identifier, 'my.*', false, 'Namespace.FooType', true}$ - defines identifier placeholder, matching identifiers (ignoring case) starting with 'my' prefix with 'Namespace.FooType' type.</item>
    /// <item>$myIdentifier{Identifier, 'my.*', true, 'Namespace.FooType', true}$ - defines identifier placeholder, matching identifiers (case sensitively) starting with 'my' prefix with 'Namespace.FooType' type.</item>
    /// <item>$identFoo{'my.*'}$ - defines identifier placeholder, matching identifiers (case sensitively) starting with 'my' prefix.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Statement placeholder arguments:
    /// <list type="bullet">
    /// <item>minimalOccurrences - minimal number of statements to match (-1 by default)</item>
    /// <item>maximalOccurrences - maximal number of statements to match (-1 by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item>$myStmt{Statement, 1, 2}$ - defines statement placeholder, matching 1 or 2 statements.</item>
    /// <item>$myStmt{Statement}$ - defines statement placeholder, matching any number of statements.</item>
    /// <item>$stmtFoo{1, 2}$ - defines statement placeholder, matching 1 or 2 statements.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Argument placeholder arguments:
    /// <list type="bullet">
    /// <item>minimalOccurrences - minimal number of arguments to match (-1 by default)</item>
    /// <item>maximalOccurrences - maximal number of arguments to match (-1 by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item>$myArg{Argument, 1, 2}$ - defines argument placeholder, matching 1 or 2 arguments.</item>
    /// <item>$myArg{Argument}$ - defines argument placeholder, matching any number of arguments.</item>
    /// <item>$argFoo{1, 2}$ - defines argument placeholder, matching 1 or 2 arguments.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Member placeholder arguments:
    /// <list type="bullet">
    /// <item>docId - string value in single quotes, specifies XML documentation id of the member to match (empty by default)</item>
    /// </list>
    /// Examples:
    /// <list type="bullet">
    /// <item>$myMember{Member, 'M:System.String.IsNullOrEmpty(System.String)'}$ - defines member placeholder, matching 'IsNullOrEmpty' member of the 'System.String' type.</item>
    /// <item>$memberFoo{'M:System.String.IsNullOrEmpty(System.String)'}$ - defines member placeholder, matching 'IsNullOrEmpty' member of the 'System.String' type.</item>
    /// </list>
    /// </para>
    /// <para>
    /// For more information please refer to the <a href="https://www.jetbrains.com/help/resharper/Navigation_and_Search__Structural_Search_and_Replace.html">Structural Search and Replace</a> article.
    /// </para>
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method
        | AttributeTargets.Constructor
        | AttributeTargets.Property
        | AttributeTargets.Field
        | AttributeTargets.Event
        | AttributeTargets.Interface
        | AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Enum,
        AllowMultiple = true,
        Inherited = false)]
    public sealed class CodeTemplateAttribute : Attribute
    {
        public CodeTemplateAttribute(string searchTemplate)
        {
            SearchTemplate = searchTemplate;
        }

        /// <summary>
        /// Structural search pattern to use in the code template.
        /// Pattern includes textual part, which must contain only identifiers allowed in the target language,
        /// and placeholders, which allow matching variable parts of the target code blocks.
        /// </summary>
        public string SearchTemplate { get; }

        /// <summary>
        /// FormattedMessageContent to show when the search pattern was found.
        /// You can also prepend the message Text with "Error:", "Warning:", "Suggestion:" or "Hint:" prefix to specify the pattern severity.
        /// Code patterns with replace template produce suggestions by default.
        /// However, if replace template is not provided, then warning severity will be used.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Structural search replace pattern to use in code template replacement.
        /// </summary>
        public string ReplaceTemplate { get; set; }

        /// <summary>
        /// Replace message to show in the light bulb.
        /// </summary>
        public string ReplaceMessage { get; set; }

        /// <summary>
        /// Apply code formatting after code replacement.
        /// </summary>
        public bool FormatAfterReplace { get; set; } = true;

        /// <summary>
        /// Whether similar code blocks should be matched.
        /// </summary>
        public bool MatchSimilarConstructs { get; set; }

        /// <summary>
        /// Automatically insert namespace import directives or remove qualifiers that become redundant after the template is applied.
        /// </summary>
        public bool ShortenReferences { get; set; }

        /// <summary>
        /// String to use as a suppression key.
        /// By default the following suppression key is used 'CodeTemplate_SomeType_SomeMember',
        /// where 'SomeType' and 'SomeMember' are names of the associated containing type and member to which this attribute is applied.
        /// </summary>
        public string SuppressionKey { get; set; }
    }
}