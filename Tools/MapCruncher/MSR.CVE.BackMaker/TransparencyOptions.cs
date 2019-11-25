using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class TransparencyOptions
    {
        public enum TransparencyMode
        {
            Normal,
            Inverted,
            Off
        }

        private const string TransparencyOptionsTag = "TransparencyOptions";
        private const string UseDocumentTransparencyAttr = "UseDocumentTransparency";
        private const string EnabledAttr = "Enabled";
        private const string InvertedAttr = "Inverted";
        public static RangeInt FuzzRange = new RangeInt(0, 255);
        public static RangeInt HaloSizeRange = new RangeInt(0, 5);
        private DirtyEvent dirtyEvent;
        private bool _enabled;
        private bool _inverted;
        private bool _useDocumentTransparency;
        private FadeOptions _fadeOptions;
        public event TransparencyOptionsChangedDelegate transparencyOptionsChangedEvent;

        public List<TransparencyColor> colorList
        {
            get;
            private set;
        }

        public bool useDocumentTransparency
        {
            get
            {
                return _useDocumentTransparency;
            }
            set
            {
                if (_useDocumentTransparency != value)
                {
                    _useDocumentTransparency = value;
                    SetDirty();
                }
            }
        }

        private void Initialize(DirtyEvent dirty)
        {
            dirtyEvent = dirty;
            colorList = new List<TransparencyColor>();
            _enabled = true;
            _inverted = false;
            _useDocumentTransparency = true;
            _fadeOptions = new FadeOptions(dirty);
        }

        public TransparencyOptions(TransparencyOptions prototype)
        {
            _enabled = prototype._enabled;
            _inverted = prototype._inverted;
            _useDocumentTransparency = prototype._useDocumentTransparency;
            colorList = new List<TransparencyColor>();
            colorList.AddRange(prototype.colorList);
            _fadeOptions = new FadeOptions(prototype._fadeOptions);
        }

        public TransparencyOptions(DirtyEvent dirty)
        {
            Initialize(dirty);
        }

        public TransparencyColor AddColor(Pixel color)
        {
            TransparencyColor transparencyColor = new TransparencyColor(color, 2, 0);
            colorList.Add(transparencyColor);
            SetDirty();
            return transparencyColor;
        }

        public void RemoveColor(TransparencyColor tc)
        {
            colorList.Remove(tc);
            SetDirty();
        }

        public void SetFuzz(TransparencyColor tc, int newValue)
        {
            if (colorList.Contains(tc) && tc.fuzz != newValue)
            {
                colorList[colorList.IndexOf(tc)] = new TransparencyColor(tc.color, newValue, tc.halo);
                SetDirty();
            }
        }

        public void SetHalo(TransparencyColor tc, int newValue)
        {
            if (colorList.Contains(tc) && tc.halo != newValue)
            {
                colorList[colorList.IndexOf(tc)] = new TransparencyColor(tc.color, tc.fuzz, newValue);
                SetDirty();
            }
        }

        public TransparencyMode GetMode()
        {
            if (!_enabled)
            {
                return TransparencyMode.Off;
            }

            if (_inverted)
            {
                return TransparencyMode.Inverted;
            }

            return TransparencyMode.Normal;
        }

        public void SetNormalTransparency()
        {
            _enabled = true;
            _inverted = false;
            SetDirty();
        }

        public void SetInvertedTransparency()
        {
            _enabled = true;
            _inverted = true;
            SetDirty();
        }

        public void SetDisabledTransparency()
        {
            _enabled = false;
            SetDirty();
        }

        public bool ShouldBeTransparent(byte r, byte g, byte b)
        {
            if (!_enabled)
            {
                return false;
            }

            bool flag = false;
            foreach (TransparencyColor current in colorList)
            {
                if (Math.Abs(current.color.r - r) <= current.fuzz &&
                    Math.Abs(current.color.g - g) <= current.fuzz &&
                    Math.Abs(current.color.b - b) <= current.fuzz)
                {
                    flag = true;
                    break;
                }
            }

            return flag != _inverted;
        }

        public void SetDirty()
        {
            if (transparencyOptionsChangedEvent != null)
            {
                transparencyOptionsChangedEvent();
            }

            dirtyEvent.SetDirty();
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(_useDocumentTransparency);
            hash.Accumulate(_enabled);
            if (_enabled)
            {
                hash.Accumulate(_inverted);
                foreach (TransparencyColor current in colorList)
                {
                    current.AccumulateRobustHash(hash);
                }
            }

            _fadeOptions.AccumulateRobustHash(hash);
        }

        public TransparencyOptions(MashupParseContext context, DirtyEvent dirty)
        {
            Initialize(dirty);
            XMLTagReader xMLTagReader = context.NewTagReader("TransparencyOptions");
            _useDocumentTransparency = true;
            context.GetAttributeBoolean("UseDocumentTransparency", ref _useDocumentTransparency);
            _enabled = context.GetRequiredAttributeBoolean("Enabled");
            _inverted = context.GetRequiredAttributeBoolean("Inverted");
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(TransparencyColor.GetXMLTag()))
                {
                    colorList.Add(new TransparencyColor(context));
                }
                else
                {
                    if (xMLTagReader.TagIs(FadeOptions.GetXMLTag()))
                    {
                        _fadeOptions = new FadeOptions(context, dirty);
                    }
                }
            }
        }

        internal static string GetXMLTag()
        {
            return "TransparencyOptions";
        }

        internal void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("TransparencyOptions");
            writer.WriteAttributeString("UseDocumentTransparency",
                _useDocumentTransparency.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Enabled", _enabled.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Inverted", _inverted.ToString(CultureInfo.InvariantCulture));
            foreach (TransparencyColor current in colorList)
            {
                current.WriteXML(writer);
            }

            _fadeOptions.WriteXML(writer);
            writer.WriteEndElement();
        }

        internal bool Effectless()
        {
            return colorList.Count == 0 || GetMode() == TransparencyMode.Off;
        }

        internal FadeOptions GetFadeOptions()
        {
            return _fadeOptions;
        }
    }
}
