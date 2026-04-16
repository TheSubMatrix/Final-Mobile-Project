using System;

namespace MatrixUtils.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : Attribute
    {
        public ProvideAttribute(){}
    }
}