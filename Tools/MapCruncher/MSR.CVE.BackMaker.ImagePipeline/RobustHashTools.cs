namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class RobustHashTools
    {
        public static bool StaticEquals(IRobustlyHashable o0, object o1)
        {
            if (o1 is IRobustlyHashable)
            {
                IRobustHash robustHash = Hash(o0);
                IRobustHash obj = Hash((IRobustlyHashable)o1);
                return robustHash.Equals(obj);
            }

            return false;
        }

        internal static string DebugString(IFuture _future)
        {
            DebugHash debugHash = new DebugHash();
            _future.AccumulateRobustHash(debugHash);
            return debugHash.ToString();
        }

        public static IRobustHash Hash(IRobustlyHashable hashable)
        {
            IRobustHash robustHash = new StrongHash();
            hashable.AccumulateRobustHash(robustHash);
            return robustHash;
        }

        public static int GetHashCode(IRobustlyHashable hashable)
        {
            return Hash(hashable).GetHashCode();
        }
    }
}
