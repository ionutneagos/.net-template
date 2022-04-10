namespace Web.Attribute
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SwaggerExcludeAttribute : Attribute { }
}
