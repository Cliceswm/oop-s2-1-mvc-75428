using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Domain;
using Library.MVC.Controllers;
using Library.MVC.Data;
using Library.Tests.Helpers;

namespace Library.Tests.Controllers
{
    public class LoansControllerTests
    {
        [Fact]
        public async Task Create_Post_WithSameBookTwice_ReturnsError()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();

            var book = new Book { Title = "Test Book", Author = "Author", Category = "Fiction", Isbn = "123-456", IsAvailable = true };
            var member1 = new Member { FullName = "Member 1", Email = "m1@email.com" };
            var member2 = new Member { FullName = "Member 2", Email = "m2@email.com" };

            context.Books.Add(book);
            context.Members.AddRange(member1, member2);
            await context.SaveChangesAsync();

            var controller = new LoansController(context);

            // First loan - active
            var loan1 = new Loan
            {
                BookId = book.Id,
                MemberId = member1.Id,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                ReturnedDate = null
            };
            await controller.Create(loan1);

            // Second loan - same book
            var loan2 = new Loan
            {
                BookId = book.Id,
                MemberId = member2.Id,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14)
            };

            // Act
            var result = await controller.Create(loan2);

            // Assert
            Assert.IsType<ViewResult>(result);

            var loans = await context.Loans.ToListAsync();
            Assert.Single(loans);
        }

        [Fact]
        public async Task ReturnBook_MarksLoanAsReturnedAndBookAvailable()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();

            var book = new Book { Title = "Test Book", Author = "Author", Category = "Fiction", Isbn = "123-456", IsAvailable = false };
            var member = new Member { FullName = "Member", Email = "m@email.com" };

            context.Books.Add(book);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            var loan = new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = DateTime.Now.AddDays(-10),
                DueDate = DateTime.Now.AddDays(4),
                ReturnedDate = null
            };

            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            var controller = new LoansController(context);

            // Act
            var result = await controller.ReturnBook(loan.Id);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);

            var updatedLoan = await context.Loans.FindAsync(loan.Id);
            Assert.NotNull(updatedLoan.ReturnedDate);

            var updatedBook = await context.Books.FindAsync(book.Id);
            Assert.True(updatedBook.IsAvailable);
        }

        [Fact]
        public async Task Index_ShowsOverdueLoans_Correctly()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();

            var book = new Book { Title = "Book", Author = "Author", Category = "Fiction", Isbn = "123-456", IsAvailable = true };
            var member = new Member { FullName = "Member", Email = "m@email.com" };
            context.Books.Add(book);
            context.Members.Add(member);

            // Overdue loan
            context.Loans.Add(new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = DateTime.Now.AddDays(-20),
                DueDate = DateTime.Now.AddDays(-5),
                ReturnedDate = null
            });

            await context.SaveChangesAsync();

            var controller = new LoansController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Loan>>(viewResult.Model);
            Assert.Single(model);

            // Check if ViewData contains overdue count
            Assert.NotNull(viewResult.ViewData["OverdueCount"]);
            Assert.Equal(1, viewResult.ViewData["OverdueCount"]);
        }
    }
}