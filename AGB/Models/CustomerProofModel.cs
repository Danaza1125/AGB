using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AparnaGoldBuyers.Models
{
    public class CustomerProofModel
    {
        public int CustomersProofId { get; set; }
        public int CustomerId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public int Active { get; set; } = 1;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}