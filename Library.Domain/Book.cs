using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Library.Domain
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Author { get; set; }

        public string Isbn { get; set; }

        public string Category { get; set; }

        public bool IsAvailable { get; set; }

        public List<Loan> Loans { get; set; } = new();
    }
}

