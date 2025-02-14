global using Litium.Accelerator.Extensions;

using System;

namespace Litium.Accelerator.Extensions
{
    public static class TestExtensions
    {
        public static string UniqueString()
        {
            return "X_" + Guid.NewGuid().ToString("N");
        }

        public static string UniqueString(this object self) => UniqueString();
    }
}
