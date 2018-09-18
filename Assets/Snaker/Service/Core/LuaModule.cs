using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snaker.Service.Core
{
    public class LuaModule:BusinessModule
    {
        private object m_args = null;
        internal LuaModule(string name) : base(name) {
        }

        public override void Create(object args = null)
        {
            base.Create(args);
            m_args = args;

            //TODO: load Lua scripts with specific name!
        }

        public override void Release()
        {
            base.Release();

            //TODO: release Lua scripts with specific name!
        }
    }
}
