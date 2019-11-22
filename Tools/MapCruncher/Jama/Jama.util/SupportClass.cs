using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public class SupportClass
{
    [Serializable]
    public class TextNumberFormat
    {
        private enum formatTypes
        {
            General,
            Number,
            Currency,
            Percent
        }

        private NumberFormatInfo numberFormat;
        private int numberFormatType;
        private bool groupingActivated;
        private string separator;
        private int maxIntDigits;
        private int minIntDigits;
        private int maxFractionDigits;
        private int minFractionDigits;

        public bool GroupingUsed
        {
            get
            {
                return groupingActivated;
            }
            set
            {
                groupingActivated = value;
            }
        }

        public int MinIntDigits
        {
            get
            {
                return minIntDigits;
            }
            set
            {
                minIntDigits = value;
            }
        }

        public int MaxIntDigits
        {
            get
            {
                return maxIntDigits;
            }
            set
            {
                maxIntDigits = value;
            }
        }

        public int MinFractionDigits
        {
            get
            {
                return minFractionDigits;
            }
            set
            {
                minFractionDigits = value;
            }
        }

        public int MaxFractionDigits
        {
            get
            {
                return maxFractionDigits;
            }
            set
            {
                maxFractionDigits = value;
            }
        }

        public TextNumberFormat()
        {
            numberFormat = new NumberFormatInfo();
            numberFormatType = 0;
            groupingActivated = true;
            separator = GetSeparator(0);
            maxIntDigits = 127;
            minIntDigits = 1;
            maxFractionDigits = 3;
            minFractionDigits = 0;
        }

        public void setMaximumIntegerDigits(int newValue)
        {
            maxIntDigits = newValue;
            if (newValue <= 0)
            {
                maxIntDigits = 0;
                minIntDigits = 0;
            }
            else
            {
                if (maxIntDigits < minIntDigits)
                {
                    minIntDigits = maxIntDigits;
                }
            }
        }

        public void setMinimumIntegerDigits(int newValue)
        {
            minIntDigits = newValue;
            if (newValue <= 0)
            {
                minIntDigits = 0;
            }
            else
            {
                if (maxIntDigits < minIntDigits)
                {
                    maxIntDigits = minIntDigits;
                }
            }
        }

        public void setMaximumFractionDigits(int newValue)
        {
            maxFractionDigits = newValue;
            if (newValue <= 0)
            {
                maxFractionDigits = 0;
                minFractionDigits = 0;
            }
            else
            {
                if (maxFractionDigits < minFractionDigits)
                {
                    minFractionDigits = maxFractionDigits;
                }
            }
        }

        public void setMinimumFractionDigits(int newValue)
        {
            minFractionDigits = newValue;
            if (newValue <= 0)
            {
                minFractionDigits = 0;
            }
            else
            {
                if (maxFractionDigits < minFractionDigits)
                {
                    maxFractionDigits = minFractionDigits;
                }
            }
        }

        private TextNumberFormat(formatTypes theType, int digits)
        {
            numberFormat = NumberFormatInfo.CurrentInfo;
            numberFormatType = (int)theType;
            groupingActivated = true;
            separator = GetSeparator((int)theType);
            maxIntDigits = 127;
            minIntDigits = 1;
            maxFractionDigits = 3;
            minFractionDigits = 0;
        }

        private TextNumberFormat(formatTypes theType, CultureInfo cultureNumberFormat, int digits)
        {
            numberFormat = cultureNumberFormat.NumberFormat;
            numberFormatType = (int)theType;
            groupingActivated = true;
            separator = GetSeparator((int)theType);
            maxIntDigits = 127;
            minIntDigits = 1;
            maxFractionDigits = 3;
            minFractionDigits = 0;
        }

        public static TextNumberFormat getTextNumberInstance()
        {
            return new TextNumberFormat(formatTypes.Number, 3);
        }

        public static TextNumberFormat getTextNumberCurrencyInstance()
        {
            TextNumberFormat textNumberFormat = new TextNumberFormat(formatTypes.Currency, 3);
            return textNumberFormat.setToCurrencyNumberFormatDefaults(textNumberFormat);
        }

        public static TextNumberFormat getTextNumberPercentInstance()
        {
            TextNumberFormat textNumberFormat = new TextNumberFormat(formatTypes.Percent, 3);
            return textNumberFormat.setToPercentNumberFormatDefaults(textNumberFormat);
        }

        public static TextNumberFormat getTextNumberInstance(CultureInfo culture)
        {
            return new TextNumberFormat(formatTypes.Number, culture, 3);
        }

        public static TextNumberFormat getTextNumberCurrencyInstance(CultureInfo culture)
        {
            TextNumberFormat textNumberFormat = new TextNumberFormat(formatTypes.Currency, culture, 3);
            return textNumberFormat.setToCurrencyNumberFormatDefaults(textNumberFormat);
        }

        public static TextNumberFormat getTextNumberPercentInstance(CultureInfo culture)
        {
            TextNumberFormat textNumberFormat = new TextNumberFormat(formatTypes.Percent, culture, 3);
            return textNumberFormat.setToPercentNumberFormatDefaults(textNumberFormat);
        }

        public object Clone()
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            bool result;
            if (obj == null || GetType() != obj.GetType())
            {
                result = false;
            }
            else
            {
                TextNumberFormat textNumberFormat = (TextNumberFormat)obj;
                result = numberFormat == textNumberFormat.numberFormat &&
                         numberFormatType == textNumberFormat.numberFormatType &&
                         groupingActivated == textNumberFormat.groupingActivated &&
                         separator == textNumberFormat.separator &&
                         maxIntDigits == textNumberFormat.maxIntDigits &&
                         minIntDigits == textNumberFormat.minIntDigits &&
                         maxFractionDigits == textNumberFormat.maxFractionDigits &&
                         minFractionDigits == textNumberFormat.minFractionDigits;
            }

            return result;
        }

        public override int GetHashCode()
        {
            return numberFormat.GetHashCode() ^ numberFormatType ^ groupingActivated.GetHashCode() ^
                   separator.GetHashCode() ^ maxIntDigits ^ minIntDigits ^ maxFractionDigits ^
                   minFractionDigits;
        }

        public string FormatDouble(double number)
        {
            string result;
            if (groupingActivated)
            {
                result = SetIntDigits(
                    number.ToString(GetCurrentFormatString() + GetNumberOfDigits(number), numberFormat));
            }
            else
            {
                result = SetIntDigits(number
                    .ToString(GetCurrentFormatString() + GetNumberOfDigits(number), numberFormat)
                    .Replace(separator, ""));
            }

            return result;
        }

        public string FormatLong(long number)
        {
            string result;
            if (groupingActivated)
            {
                result = SetIntDigits(number.ToString(GetCurrentFormatString() + minFractionDigits,
                    numberFormat));
            }
            else
            {
                result = SetIntDigits(number
                    .ToString(GetCurrentFormatString() + minFractionDigits, numberFormat)
                    .Replace(separator, ""));
            }

            return result;
        }

        private string SetIntDigits(string number)
        {
            string str = "";
            int i = number.IndexOf(numberFormat.NumberDecimalSeparator);
            string text;
            if (i > 0)
            {
                str = number.Substring(i);
                text = number.Substring(0, i).Replace(numberFormat.NumberGroupSeparator, "");
            }
            else
            {
                text = number.Replace(numberFormat.NumberGroupSeparator, "");
            }

            text = text.PadLeft(MinIntDigits, '0');
            if ((i = text.Length - MaxIntDigits) > 0)
            {
                text = text.Remove(0, i);
            }

            if (groupingActivated)
            {
                for (i = text.Length; i > 3; i -= 3)
                {
                    text = text.Insert(i - 3, numberFormat.NumberGroupSeparator);
                }
            }

            text += str;
            string result;
            if (text.Length == 0)
            {
                result = "0";
            }
            else
            {
                result = text;
            }

            return result;
        }

        public static CultureInfo[] GetAvailableCultures()
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures);
        }

        private string GetCurrentFormatString()
        {
            string result = "n";
            switch (numberFormatType)
            {
                case 0:
                    result = "n";
                    break;
                case 1:
                    result = "n";
                    break;
                case 2:
                    result = "c";
                    break;
                case 3:
                    result = "p";
                    break;
            }

            return result;
        }

        private string GetSeparator(int numberFormatType)
        {
            string result = " ";
            switch (numberFormatType)
            {
                case 0:
                    result = numberFormat.NumberGroupSeparator;
                    break;
                case 1:
                    result = numberFormat.NumberGroupSeparator;
                    break;
                case 2:
                    result = numberFormat.CurrencyGroupSeparator;
                    break;
                case 3:
                    result = numberFormat.PercentGroupSeparator;
                    break;
            }

            return result;
        }

        private TextNumberFormat setToCurrencyNumberFormatDefaults(TextNumberFormat format)
        {
            format.maxFractionDigits = 2;
            format.minFractionDigits = 2;
            return format;
        }

        private TextNumberFormat setToPercentNumberFormatDefaults(TextNumberFormat format)
        {
            format.maxFractionDigits = 0;
            format.minFractionDigits = 0;
            return format;
        }

        private int GetNumberOfDigits(double number)
        {
            int num = 0;
            double num2 = Math.Abs(number);
            while (num2 % 1.0 > 0.0)
            {
                num2 *= 10.0;
                num++;
            }

            return num < minFractionDigits ? minFractionDigits :
                num < maxFractionDigits ? num : maxFractionDigits;
        }
    }

    private class BackStringReader : StringReader
    {
        private char[] buffer;
        private int position = 1;

        public BackStringReader(string s) : base(s)
        {
            buffer = new char[position];
        }

        public override int Read()
        {
            int result;
            if (position >= 0 && position < buffer.Length)
            {
                result = buffer[position++];
            }
            else
            {
                result = base.Read();
            }

            return result;
        }

        public override int Read(char[] array, int index, int count)
        {
            int num = buffer.Length - position;
            int result;
            if (count <= 0)
            {
                result = 0;
            }
            else
            {
                if (num > 0)
                {
                    if (count < num)
                    {
                        num = count;
                    }

                    Array.Copy(buffer, position, array, index, num);
                    count -= num;
                    index += num;
                    position += num;
                }

                if (count > 0)
                {
                    count = base.Read(array, index, count);
                    if (count == -1)
                    {
                        if (num == 0)
                        {
                            result = -1;
                        }
                        else
                        {
                            result = num;
                        }
                    }
                    else
                    {
                        result = num + count;
                    }
                }
                else
                {
                    result = num;
                }
            }

            return result;
        }

        public void UnRead(int unReadChar)
        {
            position--;
            buffer[position] = (char)unReadChar;
        }

        public void UnRead(char[] array, int index, int count)
        {
            Move(array, index, count);
        }

        public void UnRead(char[] array)
        {
            Move(array, 0, array.Length - 1);
        }

        private void Move(char[] array, int index, int count)
        {
            for (int i = index + count; i >= index; i--)
            {
                UnRead(array[i]);
            }
        }
    }

    public class StreamTokenizerSupport
    {
        private const string TOKEN = "Token[";
        private const string NOTHING = "NOTHING";
        private const string NUMBER = "number=";
        private const string EOF = "EOF";
        private const string EOL = "EOL";
        private const string QUOTED = "quoted string=";
        private const string LINE = "], Line ";
        private const string DASH = "-.";
        private const string DOT = ".";
        private const int TT_NOTHING = -4;
        private const sbyte ORDINARYCHAR = 0;
        private const sbyte WORDCHAR = 1;
        private const sbyte WHITESPACECHAR = 2;
        private const sbyte COMMENTCHAR = 4;
        private const sbyte QUOTECHAR = 8;
        private const sbyte NUMBERCHAR = 16;
        private const int STATE_NEUTRAL = 0;
        private const int STATE_WORD = 1;
        private const int STATE_NUMBER1 = 2;
        private const int STATE_NUMBER2 = 3;
        private const int STATE_NUMBER3 = 4;
        private const int STATE_NUMBER4 = 5;
        private const int STATE_STRING = 6;
        private const int STATE_LINECOMMENT = 7;
        private const int STATE_DONE_ON_EOL = 8;
        private const int STATE_PROCEED_ON_EOL = 9;
        private const int STATE_POSSIBLEC_COMMENT = 10;
        private const int STATE_POSSIBLEC_COMMENT_END = 11;
        private const int STATE_C_COMMENT = 12;
        private const int STATE_STRING_ESCAPE_SEQ = 13;
        private const int STATE_STRING_ESCAPE_SEQ_OCTAL = 14;
        private const int STATE_DONE = 100;
        public const int TT_EOF = -1;
        public const int TT_EOL = 10;
        public const int TT_NUMBER = -2;
        public const int TT_WORD = -3;
        private sbyte[] attribute = new sbyte[256];
        private bool eolIsSignificant;
        private bool slashStarComments;
        private bool slashSlashComments;
        private bool lowerCaseMode;
        private bool pushedback;
        private int lineno = 1;
        private BackReader inReader;
        private BackStringReader inStringReader;
        private BackInputStream inStream;
        private StringBuilder buf;
        public double nval;
        public string sval;
        public int ttype;

        private int read()
        {
            int result;
            if (inReader != null)
            {
                result = inReader.Read();
            }
            else
            {
                if (inStream != null)
                {
                    result = inStream.Read();
                }
                else
                {
                    result = inStringReader.Read();
                }
            }

            return result;
        }

        private void unread(int ch)
        {
            if (inReader != null)
            {
                inReader.UnRead(ch);
            }
            else
            {
                if (inStream != null)
                {
                    inStream.UnRead(ch);
                }
                else
                {
                    inStringReader.UnRead(ch);
                }
            }
        }

        private void init()
        {
            buf = new StringBuilder();
            ttype = -4;
            WordChars(65, 90);
            WordChars(97, 122);
            WordChars(160, 255);
            WhitespaceChars(0, 32);
            CommentChar(47);
            QuoteChar(39);
            QuoteChar(34);
            ParseNumbers();
        }

        private void setAttributes(int low, int hi, sbyte attrib)
        {
            int num = Math.Max(0, low);
            int num2 = Math.Min(255, hi);
            for (int i = num; i <= num2; i++)
            {
                attribute[i] = attrib;
            }
        }

        private bool isWordChar(int data)
        {
            char c = (char)data;
            return data != -1 && (c > 'ÿ' || attribute[c] == 1 || attribute[c] == 16);
        }

        public StreamTokenizerSupport(StringReader reader)
        {
            string text = "";
            for (int num = reader.Read(); num != -1; num = reader.Read())
            {
                text += (char)num;
            }

            reader.Close();
            inStringReader = new BackStringReader(text);
            init();
        }

        public StreamTokenizerSupport(StreamReader reader)
        {
            inReader = new BackReader(new StreamReader(reader.BaseStream, reader.CurrentEncoding).BaseStream,
                2,
                reader.CurrentEncoding);
            init();
        }

        public StreamTokenizerSupport(Stream stream)
        {
            inStream = new BackInputStream(new BufferedStream(stream), 2);
            init();
        }

        public virtual void CommentChar(int ch)
        {
            if (ch >= 0 && ch <= 255)
            {
                attribute[ch] = 4;
            }
        }

        public virtual void EOLIsSignificant(bool flag)
        {
            eolIsSignificant = flag;
        }

        public virtual int Lineno()
        {
            return lineno;
        }

        public virtual void LowerCaseMode(bool flag)
        {
            lowerCaseMode = flag;
        }

        public virtual int NextToken()
        {
            char c = '\0';
            char c2 = '\0';
            int num = 0;
            int result;
            if (pushedback)
            {
                pushedback = false;
                result = ttype;
            }
            else
            {
                ttype = -4;
                int num2 = 0;
                nval = 0.0;
                sval = null;
                buf.Length = 0;
                do
                {
                    int num3 = read();
                    char c3 = c;
                    c = (char)num3;
                    switch (num2)
                    {
                        case 0:
                            if (num3 == -1)
                            {
                                ttype = -1;
                                num2 = 100;
                            }
                            else
                            {
                                if (c > 'ÿ')
                                {
                                    buf.Append(c);
                                    ttype = -3;
                                    num2 = 1;
                                }
                                else
                                {
                                    if (attribute[c] == 4)
                                    {
                                        num2 = 7;
                                    }
                                    else
                                    {
                                        if (attribute[c] == 1)
                                        {
                                            buf.Append(c);
                                            ttype = -3;
                                            num2 = 1;
                                        }
                                        else
                                        {
                                            if (attribute[c] == 16)
                                            {
                                                ttype = -2;
                                                buf.Append(c);
                                                if (c == '-')
                                                {
                                                    num2 = 2;
                                                }
                                                else
                                                {
                                                    if (c == '.')
                                                    {
                                                        num2 = 4;
                                                    }
                                                    else
                                                    {
                                                        num2 = 3;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (attribute[c] == 8)
                                                {
                                                    c2 = c;
                                                    ttype = c;
                                                    num2 = 6;
                                                }
                                                else
                                                {
                                                    if ((slashSlashComments || slashStarComments) && c == '/')
                                                    {
                                                        num2 = 10;
                                                    }
                                                    else
                                                    {
                                                        if (attribute[c] == 0)
                                                        {
                                                            ttype = c;
                                                            num2 = 100;
                                                        }
                                                        else
                                                        {
                                                            if (c == '\n' || c == '\r')
                                                            {
                                                                lineno++;
                                                                if (eolIsSignificant)
                                                                {
                                                                    ttype = 10;
                                                                    if (c == '\n')
                                                                    {
                                                                        num2 = 100;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (c == '\r')
                                                                        {
                                                                            num2 = 8;
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    if (c == '\r')
                                                                    {
                                                                        num2 = 9;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            break;
                        case 1:
                            if (isWordChar(num3))
                            {
                                buf.Append(c);
                            }
                            else
                            {
                                if (num3 != -1)
                                {
                                    unread(c);
                                }

                                sval = buf.ToString();
                                num2 = 100;
                            }

                            break;
                        case 2:
                            if (num3 == -1 || attribute[c] != 16 || c == '-')
                            {
                                if (attribute[c] == 4 && char.IsNumber(c))
                                {
                                    buf.Append(c);
                                    num2 = 3;
                                }
                                else
                                {
                                    if (num3 != -1)
                                    {
                                        unread(c);
                                    }

                                    ttype = 45;
                                    num2 = 100;
                                }
                            }
                            else
                            {
                                buf.Append(c);
                                if (c == '.')
                                {
                                    num2 = 4;
                                }
                                else
                                {
                                    num2 = 3;
                                }
                            }

                            break;
                        case 3:
                            if (num3 == -1 || attribute[c] != 16 || c == '-')
                            {
                                if (char.IsNumber(c) && attribute[c] == 1)
                                {
                                    buf.Append(c);
                                }
                                else
                                {
                                    if (c == '.' && attribute[c] == 2)
                                    {
                                        buf.Append(c);
                                    }
                                    else
                                    {
                                        if (num3 != -1 && attribute[c] == 4 && char.IsNumber(c))
                                        {
                                            buf.Append(c);
                                        }
                                        else
                                        {
                                            if (num3 != -1)
                                            {
                                                unread(c);
                                            }

                                            try
                                            {
                                                nval = double.Parse(buf.ToString());
                                            }
                                            catch (FormatException)
                                            {
                                            }

                                            num2 = 100;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                buf.Append(c);
                                if (c == '.')
                                {
                                    num2 = 4;
                                }
                            }

                            break;
                        case 4:
                            if (num3 == -1 || attribute[c] != 16 || c == '-' || c == '.')
                            {
                                if (attribute[c] == 4 && char.IsNumber(c))
                                {
                                    buf.Append(c);
                                }
                                else
                                {
                                    if (num3 != -1)
                                    {
                                        unread(c);
                                    }

                                    string text = buf.ToString();
                                    if (text.Equals("-."))
                                    {
                                        unread(46);
                                        ttype = 45;
                                    }
                                    else
                                    {
                                        if (text.Equals(".") && 1 == attribute[c3])
                                        {
                                            ttype = 46;
                                        }
                                        else
                                        {
                                            try
                                            {
                                                nval = double.Parse(text);
                                            }
                                            catch (FormatException)
                                            {
                                            }
                                        }
                                    }

                                    num2 = 100;
                                }
                            }
                            else
                            {
                                buf.Append(c);
                                num2 = 5;
                            }

                            break;
                        case 5:
                            if (num3 == -1 || attribute[c] != 16 || c == '-' || c == '.')
                            {
                                if (num3 != -1)
                                {
                                    unread(c);
                                }

                                try
                                {
                                    nval = double.Parse(buf.ToString());
                                }
                                catch (FormatException)
                                {
                                }

                                num2 = 100;
                            }
                            else
                            {
                                buf.Append(c);
                            }

                            break;
                        case 6:
                            if (num3 == -1 || c == c2 || c == '\r' || c == '\n')
                            {
                                sval = buf.ToString();
                                if (c == '\r' || c == '\n')
                                {
                                    unread(c);
                                }

                                num2 = 100;
                            }
                            else
                            {
                                if (c == '\\')
                                {
                                    num2 = 13;
                                }
                                else
                                {
                                    buf.Append(c);
                                }
                            }

                            break;
                        case 7:
                            if (num3 == -1)
                            {
                                ttype = -1;
                                num2 = 100;
                            }
                            else
                            {
                                if (c == '\n' || c == '\r')
                                {
                                    unread(c);
                                    num2 = 0;
                                }
                            }

                            break;
                        case 8:
                            if (c != '\n' && num3 != -1)
                            {
                                unread(c);
                            }

                            num2 = 100;
                            break;
                        case 9:
                            if (c != '\n' && num3 != -1)
                            {
                                unread(c);
                            }

                            num2 = 0;
                            break;
                        case 10:
                            if (c == '*')
                            {
                                num2 = 12;
                            }
                            else
                            {
                                if (c == '/')
                                {
                                    num2 = 7;
                                }
                                else
                                {
                                    if (num3 != -1)
                                    {
                                        unread(c);
                                    }

                                    ttype = 47;
                                    num2 = 100;
                                }
                            }

                            break;
                        case 11:
                            if (num3 == -1)
                            {
                                ttype = -1;
                                num2 = 100;
                            }
                            else
                            {
                                if (c == '/')
                                {
                                    num2 = 0;
                                }
                                else
                                {
                                    if (c != '*')
                                    {
                                        num2 = 12;
                                    }
                                }
                            }

                            break;
                        case 12:
                            if (c == '*')
                            {
                                num2 = 11;
                            }

                            if (c == '\n')
                            {
                                lineno++;
                            }
                            else
                            {
                                if (num3 == -1)
                                {
                                    ttype = -1;
                                    num2 = 100;
                                }
                            }

                            break;
                        case 13:
                            if (num3 == -1)
                            {
                                sval = buf.ToString();
                                num2 = 100;
                            }
                            else
                            {
                                num2 = 6;
                                if (c == 'a')
                                {
                                    buf.Append(7);
                                }
                                else
                                {
                                    if (c == 'b')
                                    {
                                        buf.Append('\b');
                                    }
                                    else
                                    {
                                        if (c == 'f')
                                        {
                                            buf.Append(12);
                                        }
                                        else
                                        {
                                            if (c == 'n')
                                            {
                                                buf.Append('\n');
                                            }
                                            else
                                            {
                                                if (c == 'r')
                                                {
                                                    buf.Append('\r');
                                                }
                                                else
                                                {
                                                    if (c == 't')
                                                    {
                                                        buf.Append('\t');
                                                    }
                                                    else
                                                    {
                                                        if (c == 'v')
                                                        {
                                                            buf.Append(11);
                                                        }
                                                        else
                                                        {
                                                            if (c >= '0' && c <= '7')
                                                            {
                                                                num = c - '0';
                                                                num2 = 14;
                                                            }
                                                            else
                                                            {
                                                                buf.Append(c);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            break;
                        case 14:
                            if (num3 == -1 || c < '0' || c > '7')
                            {
                                buf.Append((char)num);
                                if (num3 == -1)
                                {
                                    sval = buf.ToString();
                                    num2 = 100;
                                }
                                else
                                {
                                    unread(c);
                                    num2 = 6;
                                }
                            }
                            else
                            {
                                int num4 = num * 8 + (c - '0');
                                if (num4 < 256)
                                {
                                    num = num4;
                                }
                                else
                                {
                                    buf.Append((char)num);
                                    buf.Append(c);
                                    num2 = 6;
                                }
                            }

                            break;
                    }
                } while (num2 != 100);

                if (ttype == -3 && lowerCaseMode)
                {
                    sval = sval.ToLower();
                }

                result = ttype;
            }

            return result;
        }

        public virtual void OrdinaryChar(int ch)
        {
            if (ch >= 0 && ch <= 255)
            {
                attribute[ch] = 0;
            }
        }

        public virtual void OrdinaryChars(int low, int hi)
        {
            setAttributes(low, hi, 0);
        }

        public virtual void ParseNumbers()
        {
            for (int i = 48; i <= 57; i++)
            {
                attribute[i] = 16;
            }

            attribute[46] = 16;
            attribute[45] = 16;
        }

        public virtual void PushBack()
        {
            if (ttype != -4)
            {
                pushedback = true;
            }
        }

        public virtual void QuoteChar(int ch)
        {
            if (ch >= 0 && ch <= 255)
            {
                attribute[ch] = 8;
            }
        }

        public virtual void ResetSyntax()
        {
            OrdinaryChars(0, 255);
        }

        public virtual void SlashSlashComments(bool flag)
        {
            slashSlashComments = flag;
        }

        public virtual void SlashStarComments(bool flag)
        {
            slashStarComments = flag;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("Token[");
            int num = ttype;
            switch (num)
            {
                case -4:
                    stringBuilder.Append("NOTHING");
                    break;
                case -3:
                    stringBuilder.Append(sval);
                    break;
                case -2:
                    stringBuilder.Append("number=");
                    stringBuilder.Append(nval);
                    break;
                case -1:
                    stringBuilder.Append("EOF");
                    break;
                default:
                    if (num == 10)
                    {
                        stringBuilder.Append("EOL");
                    }

                    break;
            }

            if (ttype > 0)
            {
                if (attribute[ttype] == 8)
                {
                    stringBuilder.Append("quoted string=");
                    stringBuilder.Append(sval);
                }
                else
                {
                    stringBuilder.Append('\'');
                    stringBuilder.Append((char)ttype);
                    stringBuilder.Append('\'');
                }
            }

            stringBuilder.Append("], Line ");
            stringBuilder.Append(lineno);
            return stringBuilder.ToString();
        }

        public virtual void WhitespaceChars(int low, int hi)
        {
            setAttributes(low, hi, 2);
        }

        public virtual void WordChars(int low, int hi)
        {
            setAttributes(low, hi, 1);
        }
    }

    public class BackReader : StreamReader
    {
        private char[] buffer;
        private int position = 1;

        public BackReader(Stream streamReader, int size, Encoding encoding) : base(streamReader, encoding)
        {
            buffer = new char[size];
            position = size;
        }

        public BackReader(Stream streamReader, Encoding encoding) : base(streamReader, encoding)
        {
            buffer = new char[position];
        }

        public bool MarkSupported()
        {
            return false;
        }

        public void Mark(int position)
        {
            throw new IOException("Mark operations are not allowed");
        }

        public void Reset()
        {
            throw new IOException("Mark operations are not allowed");
        }

        public override int Read()
        {
            int result;
            if (position >= 0 && position < buffer.Length)
            {
                result = buffer[position++];
            }
            else
            {
                result = base.Read();
            }

            return result;
        }

        public override int Read(char[] array, int index, int count)
        {
            int num = buffer.Length - position;
            int result;
            if (count <= 0)
            {
                result = 0;
            }
            else
            {
                if (num > 0)
                {
                    if (count < num)
                    {
                        num = count;
                    }

                    Array.Copy(buffer, position, array, index, num);
                    count -= num;
                    index += num;
                    position += num;
                }

                if (count > 0)
                {
                    count = base.Read(array, index, count);
                    if (count == -1)
                    {
                        if (num == 0)
                        {
                            result = -1;
                        }
                        else
                        {
                            result = num;
                        }
                    }
                    else
                    {
                        result = num + count;
                    }
                }
                else
                {
                    result = num;
                }
            }

            return result;
        }

        public bool IsReady()
        {
            return position >= buffer.Length || BaseStream.Position >= BaseStream.Length;
        }

        public void UnRead(int unReadChar)
        {
            position--;
            buffer[position] = (char)unReadChar;
        }

        public void UnRead(char[] array, int index, int count)
        {
            Move(array, index, count);
        }

        public void UnRead(char[] array)
        {
            Move(array, 0, array.Length - 1);
        }

        private void Move(char[] array, int index, int count)
        {
            for (int i = index + count; i >= index; i--)
            {
                UnRead(array[i]);
            }
        }
    }

    public class BackInputStream : BinaryReader
    {
        private byte[] buffer;
        private int position = 1;

        public BackInputStream(Stream streamReader, int size) : base(streamReader)
        {
            buffer = new byte[size];
            position = size;
        }

        public BackInputStream(Stream streamReader) : base(streamReader)
        {
            buffer = new byte[position];
        }

        public bool MarkSupported()
        {
            return false;
        }

        public override int Read()
        {
            int result;
            if (position >= 0 && position < buffer.Length)
            {
                result = buffer[position++];
            }
            else
            {
                result = base.Read();
            }

            return result;
        }

        public virtual int Read(sbyte[] array, int index, int count)
        {
            int num = count + index;
            byte[] array2 = ToByteArray(array);
            int num2 = 0;
            while (position < buffer.Length && index < num)
            {
                array2[index++] = buffer[position++];
                num2++;
            }

            if (index < num)
            {
                num2 += base.Read(array2, index, num - index);
            }

            for (int i = 0; i < array2.Length; i++)
            {
                array[i] = (sbyte)array2[i];
            }

            return num2;
        }

        public void UnRead(int element)
        {
            position--;
            if (position >= 0)
            {
                buffer[position] = (byte)element;
            }
        }

        public void UnRead(byte[] array, int index, int count)
        {
            Move(array, index, count);
        }

        public void UnRead(byte[] array)
        {
            Move(array, 0, array.Length - 1);
        }

        public long Skip(long numberOfBytes)
        {
            return BaseStream.Seek(numberOfBytes, SeekOrigin.Current) - BaseStream.Position;
        }

        private void Move(byte[] array, int index, int count)
        {
            for (int i = index + count; i >= index; i--)
            {
                UnRead(array[i]);
            }
        }
    }

    public static Random Random = new Random();

    public static byte[] ToByteArray(sbyte[] sbyteArray)
    {
        byte[] array = null;
        if (sbyteArray != null)
        {
            array = new byte[sbyteArray.Length];
            for (int i = 0; i < sbyteArray.Length; i++)
            {
                array[i] = (byte)sbyteArray[i];
            }
        }

        return array;
    }

    public static byte[] ToByteArray(string sourceString)
    {
        return Encoding.UTF8.GetBytes(sourceString);
    }

    public static byte[] ToByteArray(object[] tempObjectArray)
    {
        byte[] array = null;
        if (tempObjectArray != null)
        {
            array = new byte[tempObjectArray.Length];
            for (int i = 0; i < tempObjectArray.Length; i++)
            {
                array[i] = (byte)tempObjectArray[i];
            }
        }

        return array;
    }

    public static sbyte[] ToSByteArray(byte[] byteArray)
    {
        sbyte[] array = null;
        if (byteArray != null)
        {
            array = new sbyte[byteArray.Length];
            for (int i = 0; i < byteArray.Length; i++)
            {
                array[i] = (sbyte)byteArray[i];
            }
        }

        return array;
    }

    public static void WriteStackTrace(Exception throwable, TextWriter stream)
    {
        stream.Write(throwable.StackTrace);
        stream.Flush();
    }

    public static void Serialize(Stream stream, object objectToSend)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(stream, objectToSend);
    }

    public static void Serialize(BinaryWriter binaryWriter, object objectToSend)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(binaryWriter.BaseStream, objectToSend);
    }

    public static object Deserialize(BinaryReader binaryReader)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        return binaryFormatter.Deserialize(binaryReader.BaseStream);
    }
}
