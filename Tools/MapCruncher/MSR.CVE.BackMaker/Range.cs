using System;

namespace MSR.CVE.BackMaker
{
    public abstract class Range<T> where T : IComparable
    {
        public delegate bool Escape(T value);

        public T min;
        public T max;

        public Range(T min, T max)
        {
            this.min = min;
            this.max = max;
        }

        public T Parse(MashupParseContext context, string fieldName)
        {
            return Parse(context, fieldName, context.GetRequiredAttribute(fieldName), null);
        }

        public T Parse(MashupParseContext context, string fieldName, string str)
        {
            return Parse(context, fieldName, str, null);
        }

        public T Parse(MashupParseContext context, string fieldName, string str, Escape escape)
        {
            if (str == null)
            {
                throw new InvalidMashupFile(context, string.Format("Field {0} value absent", fieldName));
            }

            T result;
            try
            {
                T t = Parse(str);
                if (escape != null && escape(t))
                {
                    result = t;
                }
                else
                {
                    if (t.CompareTo(min) < 0 || t.CompareTo(max) > 0)
                    {
                        throw new OverflowException();
                    }

                    result = t;
                }
            }
            catch (OverflowException)
            {
                throw new InvalidMashupFile(context,
                    string.Format("Field {0} value {1} out of range [{2},{3}]",
                        new object[] {fieldName, str, min, max}));
            }
            catch (Exception ex)
            {
                throw new InvalidMashupFile(context,
                    string.Format("Field {0} value {1} cannot be parsed: {2}", fieldName, str, ex.Message));
            }

            return result;
        }

        public T Constrain(T value)
        {
            if (value.CompareTo(min) < 0)
            {
                value = min;
            }

            if (value.CompareTo(max) > 0)
            {
                value = max;
            }

            return value;
        }

        internal void CheckValid(T value)
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                throw new OverflowException(
                    string.Format("Value {1} out of range [{2},{3}]", value, min, max));
            }
        }

        protected abstract T Parse(string str);
    }
}
