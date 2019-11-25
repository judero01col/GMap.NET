using System;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class PresentFailureCode : Present, IDisposable
    {
        private string provenance;

        public Exception exception
        {
            get;
        }

        public PresentFailureCode(PresentFailureCode innerFailure, string provenance)
        {
            this.provenance = provenance + innerFailure.provenance;
            exception = innerFailure.exception;
        }

        public PresentFailureCode(Exception ex)
        {
            exception = ex;
        }

        public PresentFailureCode(string str) : this(new Exception(str))
        {
        }

        public void Dispose()
        {
        }

        public override string ToString()
        {
            return provenance + ": " + exception.Message;
        }

        public Present Duplicate(string refCredit)
        {
            return this;
        }

        public static string DescribeResult(Present result)
        {
            if (result == null)
            {
                return "Processing";
            }

            if (result is PresentFailureCode)
            {
                return ((PresentFailureCode)result).ToString();
            }

            return "Complete";
        }

        public static PresentFailureCode FailedCast(Present result, string provenance)
        {
            if (result is PresentFailureCode)
            {
                return new PresentFailureCode((PresentFailureCode)result, provenance);
            }

            return new PresentFailureCode(string.Format("Unexpected type {0} at {1}", result.GetType(), provenance));
        }
    }
}
