using System;
using System.Linq;
using System.Collections.Generic;


namespace GrowingData.Utilities {
    public static class HashsetHelpers {
        public static HashSet<TResult> ToHashSet<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector

        ) {

            return new HashSet<TResult>(source.Select(x => selector(x)));

        }

    }
}