using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AparnaGoldBuyers.Models
{
    public class CustomerOrnamentModel
    {
        public int CustomersOrnamentId { get; set; }
        public int CustomerId { get; set; }
        public string OrnamentPath { get; set; }
        public string grams { get; set; }
        public string purity { get; set; }
        public string buyingPrice { get; set; }
        public int Active { get; set; } = 1;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}