﻿using System;
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

        public static char Id(this Category category){
            return category switch{
                Category.Literal    => 'L',
                Category.InsertCopy => 'I',
                Category.Distance   => 'D',
                _ => throw new InvalidOperationException("Invalid category: " + category)
            };
        }
    }
}
