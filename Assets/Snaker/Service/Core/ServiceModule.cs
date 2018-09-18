using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snaker.Service.Core
{
    // Service is a singleton
    public abstract class ServiceModule<T>:Module where T:ServiceModule<T>,new()
    {
        private static T ms_instance = default(T);

        public static T Instance
        {
            get {
                if (ms_instance == null) {
                    ms_instance = new T();
                }
                return ms_instance;
            }
        }

        protected void CheckSingleton() {
            if (ms_instance == null)
            {
                var exception = new Exception("ServiceModule<<" + typeof(T).Name + ">>cannot initalize, because it is a singleton class");
            }
        }
    }
}
