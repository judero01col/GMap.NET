using System.Collections.Generic;

namespace MSR.CVE.BackMaker
{
    public abstract class TransformationStyleFactory
    {
        private static List<TransformationStyle> transformationStyles = new List<TransformationStyle>();

        private static void init()
        {
            if (transformationStyles.Count == 0)
            {
                transformationStyles.Add(new AutomaticTransformationStyle());
                transformationStyles.Add(new AffineTransformationStyle());
                transformationStyles.Add(new HomographicTransformationStyle());
            }
        }

        public static TransformationStyle getTransformationStyle(int i)
        {
            init();
            if (i < 0 || i >= transformationStyles.Count)
            {
                i = 0;
            }

            return transformationStyles[i];
        }

        public static TransformationStyle getDefaultTransformationStyle()
        {
            return getTransformationStyle(0);
        }

        public static TransformationStyle ReadFromXMLAttribute(MashupParseContext context)
        {
            init();
            string attribute = context.reader.GetAttribute(TransformationStyle.TransformationStyleNameAttr);
            if (attribute != null)
            {
                for (int i = 0; i < transformationStyles.Count; i++)
                {
                    if (transformationStyles[i].getXmlName() == attribute)
                    {
                        return transformationStyles[i];
                    }
                }

                throw new InvalidMashupFile(context,
                    string.Format("Invalid attribute value {1} for {0}",
                        TransformationStyle.TransformationStyleNameAttr,
                        attribute));
            }

            return transformationStyles[0];
        }
    }
}
