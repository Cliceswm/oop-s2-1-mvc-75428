using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Library.Domain
{
    public class Member
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public List<Loan> Loans { get; set; } = new List<Loan>();
    }
}
