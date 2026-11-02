namespace Stoat.Theme.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ViewFallbackAttribute(Type control) : Attribute
{
    public Type Control { get; set; } = control;
}