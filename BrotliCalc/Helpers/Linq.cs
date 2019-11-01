using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BrotliCalc.Helpers{
    static class Linq{
        public static int MaxThreads { get; set; } = int.MaxValue; // TODO add command line argument

        public static IEnumerable<(TA a, TB b)> Cartesian<TA, TB>(this IEnumerable<TA> me, IReadOnlyList<TB> other){
            foreach(var a in me){
                foreach(var b in other){
                    yield return (a, b);
                }
            }
        }

        public static IEnumerable<(int index, T ele)> WithIndex<T>(this IEnumerable<T> me){
            int index = 0;

            foreach(var element in me){
                yield return (index++, element);
            }
        }

        public static ParallelQuery<T> Parallelize<T>(this IEnumerable<T> me){
            var query = Partitioner.Create(me, EnumerablePartitionerOptions.NoBuffering).AsParallel().WithMergeOptions(ParallelMergeOptions.NotBuffered);

            if (MaxThreads != int.MaxValue){
                query = query.WithDegreeOfParallelism(MaxThreads);
            }

            return query;
        }
    }
}
