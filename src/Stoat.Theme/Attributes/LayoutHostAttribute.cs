namespace Stoat.Theme.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class LayoutHostAttribute(Type control) : Attribute
{
    public Type LayoutControl { get; set; } = control;
}