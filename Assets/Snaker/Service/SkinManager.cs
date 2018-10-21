using Snaker.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snaker.Service
{
    class SkinManager : ServiceModule<SkinManager>
    {
        private int skinType = 0;
        public int SkinType { get { return skinType; } }
        public void Init()
        {
            CheckSingleton();
        }

        public void UpdateSkinType(int extSkinType)
        {
            skinType = extSkinType;
        }
    }
}
