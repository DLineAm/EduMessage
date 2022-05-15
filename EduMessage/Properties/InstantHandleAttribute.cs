using System;

namespace EduMessage.Annotations
{
    /// <summary>
    /// Tells the code analysis engine if the parameter is completely handled when the invoked method is on stack.
    /// If the parameter is a delegate, indicates that delegate can only be invoked during method execution
    /// (the delegate can be invoked zero or multiple times, but not stored to some field and invoked later,
    /// when the containing method is no longer on the execution stack).
    /// If the parameter is an enumerable, indicates that it is enumerated while the method is executed.
    /// If <see cref="RequireAwait"/> is true, the attribute will only takes effect if the method invocation is located under the 'await' expression.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class InstantHandleAttribute : Attribute
    {
        /// <summary>
        /// Require the method invocation to be used under the 'await' expression for this attribute to take effect on code analysis engine.
        /// Can be used for delegate/enumerable parameters of 'async' methods.
        /// </summary>
        public bool RequireAwait { get; set; }
    }
}