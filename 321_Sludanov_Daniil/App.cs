using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Builders;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace _UP_Luzin_321
{
    public partial class App : Application
    {
        public int CurrentUserID { get; set; }

        public void SetCurrentUserID(int ID)
        {
            CurrentUserID = ID;
        }
    }
}
