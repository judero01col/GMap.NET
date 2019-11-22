using System;
using System.Diagnostics;

namespace GMap.NET
{
    /// <summary>
    ///     generic for singletons
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        // ctor
        protected Singleton()
        {
            if (Instance != null)
            {
                throw new Exception(
                    "You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\"");
            }
        }

        public static T Instance
        {
            get
            {
                if (SingletonCreator.Exception != null)
                {
                    throw SingletonCreator.Exception;
                }

                return SingletonCreator.Instance;
            }
        }

        class SingletonCreator
        {
            static SingletonCreator()
            {
                try
                {
                    Instance = new T();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Exception = ex.InnerException;
                    }
                    else
                    {
                        Exception = ex;
                    }

                    Trace.WriteLine("Singleton: " + Exception);
                }
            }

            internal static readonly T Instance;
            internal static readonly Exception Exception;
        }
    }
}
