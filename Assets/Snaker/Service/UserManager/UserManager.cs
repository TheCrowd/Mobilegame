using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snaker.Service.Core;

namespace Snaker.Service.UserManager
{
    public class UserManager : ServiceModule<UserManager>
    {
        private UserBean mainUserData;
        public UserBean MainUserData { get { return mainUserData; } }

        public void Init()
        {
            CheckSingleton();
        }


        /// <summary>
        /// update user info
        /// </summary>
        /// <param name="data"></param>
        public void UpdateMainUserData(UserBean data)
        {
            mainUserData = data;
        }

    }
}
