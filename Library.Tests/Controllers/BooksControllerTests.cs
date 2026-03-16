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
    public class BooksControllerTests
    {
        [Fact]
        public async Task Index_SearchByTitle_ReturnsMatchingBooks()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();

            var books = new List<Book>
            {
                new Book { Title = "Harry Potter", Author = "J.K. Rowling", Category = "Fiction", Isbn = "123-456", IsAvailable = true },
                new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", Category = "Fantasy", Isbn = "789-012", IsAvailable = true },
                new Book { Title = "Harry Potter and the Chamber", Author = "J.K. Rowling", Category = "Fiction", Isbn = "345-678", IsAvailable = true }
            };

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            // Act
            var result = await controller.Index(search: "Harry", category: null, availability: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Book>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.All(model, b => Assert.Contains("Harry", b.Title));
        }

        [Fact]
        public async Task Index_FilterByCategory_ReturnsOnlyBooksInThatCategory()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();

            var books = new List<Book>
            {
                new Book { Title = "Fiction Book", Author = "Author 1", Category = "Fiction", Isbn = "123-456", IsAvailable = true },
                new Book { Title = "Science Book", Author = "Author 2", Category = "Science", Isbn = "789-012", IsAvailable = true },
                new Book { Title = "Another Fiction", Author = "Author 3", Category = "Fiction", Isbn = "345-678", IsAvailable = false }
            };

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            // Act
            var result = await controller.Index(search: null, category: "Fiction", availability: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Book>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.All(model, b => Assert.Equal("Fiction", b.Category));
        }

        [Fact]
        public async Task Index_FilterByAvailability_ReturnsOnlyAvailableBooks()
        {
            // Arrange
            using var context = TestDbContextFactory.CreateInMemoryDbContext();

            var books = new List<Book>
            {
                new Book { Title = "Available Book", Author = "Author 1", Category = "Fiction", Isbn = "123-456", IsAvailable = true },
                new Book { Title = "On Loan Book", Author = "Author 2", Category = "Science", Isbn = "789-012", IsAvailable = false },
                new Book { Title = "Another Available", Author = "Author 3", Category = "History", Isbn = "345-678", IsAvailable = true }
            };

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();

            var controller = new BooksController(context);

            // Act
            var result = await controller.Index(search: null, category: null, availability: "Available");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Book>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            Assert.All(model, b => Assert.True(b.IsAvailable));
        }
    }
}