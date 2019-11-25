using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     LithuaniaHybridOldMap, from 2010 data, provider
    /// </summary>
    public class LithuaniaHybridOldMapProvider : LithuaniaMapProviderBase
    {
        public static readonly LithuaniaHybridOldMapProvider Instance;

        LithuaniaHybridOldMapProvider()
        {
        }

        static LithuaniaHybridOldMapProvider()
        {
            Instance = new LithuaniaHybridOldMapProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("35C5C685-E868-4AC7-97BE-10A9A37A81B5");

        public override string Name
        {
            get;
        } = "LithuaniaHybridMapOld";

        GMapProvider[] _overlays;

        public override GMapProvider[] Overlays
        {
            get
            {
                if (_overlays == null)
                {
                    _overlays = new GMapProvider[]
                    {
                        LithuaniaOrtoFotoOldMapProvider.Instance, LithuaniaHybridMapProvider.Instance
                    };
                }

                return _overlays;
            }
        }

        #endregion
    }
}
