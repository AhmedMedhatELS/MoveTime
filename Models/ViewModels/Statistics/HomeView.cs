using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels.Statistics
{
    public class HomeView
    {
        #region 1st section
        public int TotalKids { get; set; }
        public int BlackList { get; set; }
        public int Approved { get; set; }
        public int NotApproved { get; set; }
        #endregion
        #region 2nd section
        public List<int> WeekProfit { get; set; } = [];
        public List<string> WeekDaysName { get; set; } = [];
        #endregion
        #region 3rd section
        public int MaleKids { get; set; }
        public int FemaleKids { get; set; }
        public int KidsSub { get; set; }
        public int MaleSub { get; set; }
        public int FemaleSub { get; set; }
        #endregion
    }
}
