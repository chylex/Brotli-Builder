using BrotliLib.Brotli.Components.Header;

namespace BrotliLib.Brotli.Parameters.Heuristics{
    public static class ContextMapHeuristics{
        public delegate bool DecideFeature(ContextMap contextMap);
        public delegate ContextMap.RunResolution DecideRuns(ContextMap.RunDecider decider);

        public static class MTF{
            public static DecideFeature Disable { get; } = _ => false;
            public static DecideFeature Enable  { get; } = _ => true;
        }

        public static class RLE{
            public static DecideRuns Disable { get; } = decider => decider.Resolve(run => run.Reject());
            public static DecideRuns KeepAll { get; } = decider => decider.Resolve(run => run.Accept());

            public static DecideRuns SplitOneAboveBoundary { get; } = decider => decider.Resolve(original => {
                var nextDown = new ContextMap.Run(original.Length - 1);
                return original.Code == nextDown.Code ? original.Accept() : nextDown.Accept();
            });
        }
    }
}
