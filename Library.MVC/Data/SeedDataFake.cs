using Bogus;
using Library.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Library.MVC.Data
{
    public static class SeedDataFake
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if data already exists
            if (context.Books.Any() || context.Members.Any() || context.Loans.Any())
            {
                Console.WriteLine("Database already has data. Skipping fake data generation.");
                return;
            }

            Console.WriteLine("Generating fake data...");

            // FIXED CATEGORIES
            var categories = new[] { "Fiction", "Non-Fiction", "Science", "History", "Biography", "Children", "Technology" };

            // GENERATE FAKE BOOKS (20 books)
            var bookFaker = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
                .RuleFor(b => b.Author, f => f.Name.FullName())
                .RuleFor(b => b.Isbn, f => f.Random.Replace("###-#-##-####"))
                .RuleFor(b => b.Category, f => f.PickRandom(categories))
                .RuleFor(b => b.IsAvailable, f => f.Random.Bool(0.7f)); // 70% chance of being available

            var books = bookFaker.Generate(20);
            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();
            Console.WriteLine($" {books.Count} books generated.");

            // GENERATE FAKE MEMBERS (10 members)
            var memberFaker = new Faker<Member>()
                .RuleFor(m => m.FullName, f => f.Name.FullName())
                .RuleFor(m => m.Email, f => f.Internet.Email())
                .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber("(##) #####-####"));
            var members = memberFaker.Generate(10);
            await context.Members.AddRangeAsync(members);
            await context.SaveChangesAsync();
            Console.WriteLine($" {members.Count} members generated.");

            //  LOAD BOOKS AND MEMBERS FROM DATABASE (to use in loans)
            var allBooks = await context.Books.ToListAsync();
            var allMembers = await context.Members.ToListAsync();

            //  GENERATE FAKE LOANS (15 loans)
            var loanFaker = new Faker<Loan>()
                .RuleFor(l => l.BookId, f => f.PickRandom(allBooks).Id)
                .RuleFor(l => l.MemberId, f => f.PickRandom(allMembers).Id)
                .RuleFor(l => l.LoanDate, f => f.Date.Past(2)) // Up to 2 years ago
                .RuleFor(l => l.DueDate, (f, l) => l.LoanDate.AddDays(14))
                .RuleFor(l => l.ReturnedDate, (f, l) =>
                {
                    // 50% of loans are already returned
                    if (f.Random.Bool(0.5f))
                    {
                        return l.LoanDate.AddDays(f.Random.Int(5, 13));
                    }
                    return null;
                });

            var loans = loanFaker.Generate(15);

            // Ensure at least 3 loans are overdue (DueDate in past, not returned)
            for (int i = 0; i < 3; i++)
            {
                loans[i].LoanDate = DateTime.Now.AddDays(-30);
                loans[i].DueDate = DateTime.Now.AddDays(-5); // Past due!
                loans[i].ReturnedDate = null;
            }

            await context.Loans.AddRangeAsync(loans);
            await context.SaveChangesAsync();
            Console.WriteLine($" {loans.Count} loans generated (including 3 overdue).");
        }
    }
}