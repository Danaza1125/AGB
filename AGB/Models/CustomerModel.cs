using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AparnaGoldBuyers.Models
{
    public class CustomerModel
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string AccountDetails { get; set; }
        public string CustomerDate { get; set; }
        public int Active { get; set; } = 1;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}