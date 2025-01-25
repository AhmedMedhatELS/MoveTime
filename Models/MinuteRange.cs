namespace Models
{
    public class MinuteRange
    {
        #region Properities
        public int MinuteRangeId { get; set; }        
        public int Start { get; set; }
        public int End { get; set; }
        public int Price { get; set; }
        #endregion

        #region Foreign Keys
        public int ShiftHourId { get; set; }
        public ShiftHour ShiftHour { get; set; } = null!;
        #endregion

        #region Relations

        #endregion
    }
}