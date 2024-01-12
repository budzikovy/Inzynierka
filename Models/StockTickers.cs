namespace Inz_Fn.Models
{
    public class StockTickers
    {
        public string T { get; set; }
        public float c { get; set; }
        public float h { get; set; }
        public float l { get; set; }
        public float n { get; set; }
        public float o { get; set; }
        public Boolean otc { get; set; }
        public float t { get; set; }
        public float v { get; set; }
        public float vw { get; set; }
        public bool isFavourite { get; set; }
        public float dailyChange
        {
            get
            {
                if (o != 0)
                {
                    return (c - o) / o * 100;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}