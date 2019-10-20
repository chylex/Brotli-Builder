namespace BrotliLib.Numbers{
    public static class Log2{
        /**
         * Calculates log2 of an integer. Returns the result rounded down to the nearest integer, or 0 if the provided value is <= 0.
         */
        public static byte Floor(int value){
            byte result = 0;
            
            while((value >>= 1) > 0){
                ++result;
            }

            return result;
        }
    }
}
