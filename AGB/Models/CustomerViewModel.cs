using System.Collections.Generic;

namespace AparnaGoldBuyers.Models
{
    public class CustomerViewModel
    {
        public CustomerModel Customer { get; set; }
        public List<CustomerOrnamentModel> Ornaments { get; set; }
        public List<CustomerProofModel> Proofs { get; set; }

        public CustomerViewModel()
        {
            Customer = new CustomerModel();
            Ornaments = new List<CustomerOrnamentModel>();
            Proofs = new List<CustomerProofModel>();
        }
    }
}
