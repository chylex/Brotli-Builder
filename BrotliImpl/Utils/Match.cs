using System;

namespace BrotliImpl.Utils{
    static class Match{
        public static bool Check(ArraySegment<byte> bytes, int current, int candidate, int length){
            for(int offset = 0; offset < length; offset++){
                if (bytes[current + offset] != bytes[candidate + offset]){
                    return false;
                }
            }

            return true;
        }

        public static int DetermineLength(ArraySegment<byte> bytes, int current, int candidate, int limit){
            int matched = 0;
                
            while(matched < limit && bytes[current] == bytes[candidate]){
                ++current;
                ++candidate;
                ++matched;
            }

            return matched;
        }
    }
}
