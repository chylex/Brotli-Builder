using System;
using System.Collections.Generic;

namespace BrotliLib.Brotli.Components.Utils{
    /// <summary>
    /// Three main types of codes in the insert&amp;copy command whose symbols are represented by Huffman coding trees.
    /// </summary>
    public enum Category{
        Literal,
        InsertCopy,
        Distance
    }

    public static class Categories{
        public static readonly IList<Category> LID = new Category[]{
            Category.Literal, Category.InsertCopy, Category.Distance
        };

        public static int HuffmanTreesPerBlockType(this Category category){
            switch(category){
                case Category.Literal:    return 64;
                case Category.InsertCopy: return  1;
                case Category.Distance:   return  4;
                default: throw new InvalidOperationException("Invalid category: " + category);
            }
        }

        public static void Deconstruct<T>(this KeyValuePair<Category, T> kvp, out Category category, out T value){
            category = kvp.Key;
            value = kvp.Value;
        }
    }

    /// <summary>
    /// Utility class to map values of type <typeparamref name="T"/> to the categories.
    /// </summary>
    public sealed class CategoryMap<T>{
        public T this[Category cat]{
            get => values[(int)cat];
            private set => values[(int)cat] = value;
        }

        public IEnumerable<T> Values{
            get{
                foreach(Category category in Categories.LID){
                    yield return this[category];
                }
            }
        }

        private readonly T[] values = new T[Categories.LID.Count];

        public CategoryMap(Func<Category, T> mapper){
            foreach(Category category in Categories.LID){
                this[category] = mapper(category);
            }
        }

        public KeyValuePair<Category, T> Pick(Category category){
            return new KeyValuePair<Category, T>(category, this[category]);
        }

        public CategoryMap<U> Select<U>(Func<Category, T, U> mapper){
            return new CategoryMap<U>(category => mapper(category, this[category]));
        }
    }
}
