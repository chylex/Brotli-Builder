using System;
using System.Collections.Generic;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Utils{
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

        public CategoryMap<U> Select<U>(Func<T, U> mapper){
            return new CategoryMap<U>(category => mapper(this[category]));
        }

        public override bool Equals(object obj){
            return obj is CategoryMap<T> map &&
                   CollectionHelper.Equal(values, map.values);
        }

        public override int GetHashCode(){
            return CollectionHelper.HashCode(values);
        }
    }
}
