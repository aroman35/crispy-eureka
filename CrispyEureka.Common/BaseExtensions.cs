using System;
using System.Collections.Generic;

namespace CrispyEureka.Common
{
    public static class BaseExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (var item in sequence)
            {
                action(item);
            }
        }
    }
}