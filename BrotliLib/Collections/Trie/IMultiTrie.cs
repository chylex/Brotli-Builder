using System.Collections.Generic;

namespace BrotliLib.Collections.Trie{
    public interface IMultiTrie<K, V>{
        IReadOnlyList<V> Find(IEnumerable<K> key);
    }
}
