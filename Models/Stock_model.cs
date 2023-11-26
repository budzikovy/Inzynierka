namespace Inz_Fn.Models
{
    public class Stock_model
    { 
        public double c {  get; set; }
        public double h { get; set; }
        public double l { get; set; }
        public int n { get; set; }
        public double o { get; set; }
        public Boolean otc { get; set; }
        public long t { get; set; }
        public long v { get; set; }
        public double vw { get; set; }

    }
    public class ApiResponse
    {
        public List<Stock_model> Stock_models { get; set; }
    }
}
