using System;

namespace MSR.CVE.BackMaker.MCDebug
{
    internal class WeakHashableObject
    {
        private WeakReference obj;
        private int hashCode;

        public WeakHashableObject(object target)
        {
            obj = new WeakReference(target);
            hashCode = target.GetHashCode();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object other)
        {
            object target = obj.Target;
            if (target == null)
            {
                return false;
            }

            if (other is WeakHashableObject)
            {
                return target.Equals(((WeakHashableObject)other).obj.Target);
            }

            return target.Equals(other);
        }

        public override string ToString()
        {
            object target = obj.Target;
            return target.ToString();
        }
    }
}
