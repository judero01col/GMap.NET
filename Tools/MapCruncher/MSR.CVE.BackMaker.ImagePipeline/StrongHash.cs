using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class StrongHash : IRobustHash
    {
        public const string xmlTag = "StrongHash";
        private const string ValueAttr = "Value";
        private MemoryStream ms = new MemoryStream();
        private byte[] hashValue;
        private int shortHashValue;
        private static UTF8Encoding encoding = new UTF8Encoding();

        public static bool operator ==(StrongHash h1, StrongHash h2)
        {
            return h1.Equals(h2);
        }

        public static bool operator !=(StrongHash h1, StrongHash h2)
        {
            return !h1.Equals(h2);
        }

        private void accBytes(byte[] buf)
        {
            ms.Write(buf, 0, buf.Length);
        }

        public void Accumulate(int input)
        {
            accBytes(BitConverter.GetBytes(input));
        }

        public void Accumulate(long input)
        {
            accBytes(BitConverter.GetBytes(input));
        }

        public void Accumulate(Size size)
        {
            Accumulate(size.Width);
            Accumulate(size.Height);
        }

        public void Accumulate(double value)
        {
            Accumulate(value.GetHashCode());
        }

        public void Accumulate(string value)
        {
            accBytes(encoding.GetBytes(value));
        }

        public void Accumulate(bool value)
        {
            accBytes(BitConverter.GetBytes(value));
        }

        public override int GetHashCode()
        {
            DoHash();
            return shortHashValue;
        }

        private void DoHash()
        {
            if (hashValue == null)
            {
                HashAlgorithm hashAlgorithm = new SHA1Managed();
                byte[] array = ms.ToArray();
                hashAlgorithm.ComputeHash(array, 0, array.Length);
                hashValue = hashAlgorithm.Hash;
                ComputeShortHashValue();
                ms.Dispose();
                ms = null;
            }
        }

        private void ComputeShortHashValue()
        {
            int num = 0;
            for (int i = 0; i < hashValue.Length; i++)
            {
                num = num * 131 + hashValue[i];
            }

            shortHashValue = num;
        }

        public override bool Equals(object obj)
        {
            if (obj is StrongHash)
            {
                StrongHash strongHash = (StrongHash)obj;
                DoHash();
                strongHash.DoHash();
                return ArraysEqual(strongHash);
            }

            return false;
        }

        private bool ArraysEqual(StrongHash rh2)
        {
            bool result = true;
            if (hashValue.Length != rh2.hashValue.Length)
            {
                result = false;
            }
            else
            {
                for (int i = 0; i < hashValue.Length; i++)
                {
                    if (hashValue[i] != rh2.hashValue[i])
                    {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }

        public override string ToString()
        {
            StrongHash strongHash = new StrongHash();
            strongHash.ms.Write(ms.GetBuffer(), 0, (int)ms.Length);
            strongHash.DoHash();
            return ByteArrayToHex(strongHash.hashValue);
        }

        public static string ByteArrayToHex(byte[] byteArray)
        {
            StringBuilder stringBuilder = new StringBuilder(byteArray.Length * 2);
            for (int i = 0; i < byteArray.Length; i++)
            {
                byte b = byteArray[i];
                int index = (b >> 4) & 15;
                int index2 = b & 15;
                stringBuilder.Append("0123456789ABCDEF"[index]);
                stringBuilder.Append("0123456789ABCDEF"[index2]);
            }

            return stringBuilder.ToString();
        }

        public StrongHash()
        {
        }

        public StrongHash(MashupParseContext context)
        {
            XMLTagReader xMLTagReader = context.NewTagReader("StrongHash");
            hashValue = Convert.FromBase64String(context.GetRequiredAttribute("Value"));
            ComputeShortHashValue();
            xMLTagReader.SkipAllSubTags();
        }

        public void WriteXML(XmlTextWriter writer)
        {
            DoHash();
            writer.WriteStartElement("StrongHash");
            writer.WriteAttributeString("Value", Convert.ToBase64String(hashValue));
            writer.WriteEndElement();
        }
    }
}
