using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Shift
    {
        #region Properities
        public int ShiftId { get; set; }
        public string ShiftName { get; set; } = null!;
        public TimeSpan? StartTime { get; set; } = null;
        public TimeSpan? EndTime { get; set; } = null;
        #endregion

        #region Foreign Keys

        #endregion

        #region Relations

        #endregion
    }
}
