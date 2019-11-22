using System;
using System.Collections.Generic;

namespace MSR.CVE.BackMaker
{
    public class ParametricLine
    {
        public class Intersection : IComparable
        {
            public bool IsParallel;
            public double t;

            public int CompareTo(object obj)
            {
                if (!(obj is Intersection))
                {
                    return 1;
                }

                Intersection intersection = (Intersection)obj;
                if (intersection.IsParallel && IsParallel)
                {
                    return 0;
                }

                if (intersection.t == t)
                {
                    return 0;
                }

                if (intersection.t > t)
                {
                    return -1;
                }

                return 1;
            }
        }

        private LatLon s;
        private LatLon d;

        public ParametricLine(LatLon source, LatLon dest)
        {
            s = source;
            d = dest;
        }

        public LatLon t(double t_arg)
        {
            return new LatLon(s.lat + t_arg * (d.lat - s.lat),
                s.lon + t_arg * (d.lon - s.lon));
        }

        public double Length()
        {
            return Math.Sqrt(Math.Pow(d.lon - s.lon, 2.0) + Math.Pow(d.lat - s.lat, 2.0));
        }

        public Intersection LatitudeIntersection(double lat)
        {
            Intersection intersection = new Intersection();
            if (s.lat == d.lat)
            {
                intersection.IsParallel = true;
            }
            else
            {
                intersection.IsParallel = false;
                intersection.t = (lat - s.lat) / (d.lat - s.lat);
            }

            return intersection;
        }

        public Intersection LongitudeIntersection(double lon)
        {
            Intersection intersection = new Intersection();
            if (s.lon == d.lon)
            {
                intersection.IsParallel = true;
            }
            else
            {
                intersection.IsParallel = false;
                intersection.t = (lon - s.lon) / (d.lon - s.lon);
            }

            return intersection;
        }

        public List<LatLon> Interpolate(int NumSteps)
        {
            List<LatLon> list = new List<LatLon>();
            for (int i = 0; i < NumSteps; i++)
            {
                list.Add(t(i / (double)NumSteps));
            }

            return list;
        }
    }
}
