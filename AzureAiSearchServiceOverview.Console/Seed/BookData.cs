using AzureAiSearchServiceOverview.Console.Models;

namespace AzureAiSearchServiceOverview.Console.Seed;

public static class BookData
{
    public static List<Book> GetSampleBooks() =>
    [
        new Book
        {
            Id = "1",
            Name = "The Lord of the Rings: The Fellowship of the Ring",
            Description =
                "The first part of J.R.R. Tolkien’s epic saga, following Frodo Baggins as he embarks on a perilous journey to destroy the One Ring.",
            Author = "J.R.R. Tolkien",
            PageCount = 423,
            Genres = "Fantasy, Adventure, Fiction"
        },

        new Book
        {
            Id = "2",
            Name = "The Lord of the Rings: The Two Towers",
            Description =
                "The second installment of Tolkien’s masterpiece, where the Fellowship is broken and the quest continues across Middle-earth.",
            Author = "J.R.R. Tolkien",
            PageCount = 352,
            Genres = "Fantasy, Adventure, Fiction"
        },

        new Book
        {
            Id = "3",
            Name = "The Lord of the Rings: The Return of the King",
            Description =
                "The final volume of The Lord of the Rings, concluding the epic struggle between the forces of good and Sauron’s darkness.",
            Author = "J.R.R. Tolkien",
            PageCount = 416,
            Genres = "Fantasy, Adventure, Epic"
        },

        new Book
        {
            Id = "4",
            Name = "The Hobbit",
            Description =
                "Bilbo Baggins is swept into an unexpected adventure with dwarves to reclaim their homeland from the dragon Smaug.",
            Author = "J.R.R. Tolkien",
            PageCount = 310,
            Genres = "Fantasy, Adventure, Fiction"
        },

        new Book
        {
            Id = "5",
            Name = "Dune",
            Description =
                "Frank Herbert’s legendary science fiction novel about politics, religion, and ecology on the desert planet Arrakis.",
            Author = "Frank Herbert",
            PageCount = 688,
            Genres = "Science Fiction, Adventure, Classic"
        },

        new Book
        {
            Id = "6",
            Name = "Neuromancer",
            Description =
                "William Gibson’s cyberpunk classic that introduced the Matrix and redefined science fiction for a digital age.",
            Author = "William Gibson",
            PageCount = 271,
            Genres = "Science Fiction, Cyberpunk, Thriller"
        },

        new Book
        {
            Id = "7",
            Name = "Foundation",
            Description =
                "Isaac Asimov’s visionary tale of the fall and rebirth of a galactic empire through the science of psychohistory.",
            Author = "Isaac Asimov",
            PageCount = 296,
            Genres = "Science Fiction, Classic, Space Opera"
        },

        new Book
        {
            Id = "8",
            Name = "Ender's Game",
            Description =
                "A young boy is trained through battle simulations to lead humanity’s defense against an alien species.",
            Author = "Orson Scott Card",
            PageCount = 324,
            Genres = "Science Fiction, Military, Adventure"
        },

        new Book
        {
            Id = "9",
            Name = "Ready Player One",
            Description =
                "In a dystopian future, Wade Watts hunts for an Easter egg inside the OASIS, a massive virtual reality world.",
            Author = "Ernest Cline",
            PageCount = 374,
            Genres = "Science Fiction, Adventure, Gaming"
        },

        new Book
        {
            Id = "10",
            Name = "The Martian",
            Description =
                "An astronaut stranded on Mars must use his ingenuity and engineering skills to survive until rescue.",
            Author = "Andy Weir",
            PageCount = 369,
            Genres = "Science Fiction, Survival, Adventure"
        }
    ];
}