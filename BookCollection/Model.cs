using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSharper;
using WebSharper.UI.Next;
using WebSharper.UI.Next.CSharp;

namespace BookCollection
{
    [JavaScript, Serializable]
    public class Book
    {
        public int BookId = 0; // workaround, will not be needed soon, see https://github.com/intellifactory/websharper/issues/611
        public string Title;
        public string Author;
        public DateTime PublishDate;
        public string ISBN;

        public Book(string title, string author, DateTime publishDate, string isbn)
        {
            Title = title;
            Author = author;
            PublishDate = publishDate;
            ISBN = isbn;
        }

        // required for serialization
        public Book() { }

        public static Book Empty => new Book("", "", DateTime.Today, "");
    }

    [JavaScript]
    public class BookModel
    {
        public Var<Book> Book { get; private set; }
        public bool IsEdited { get; set; }

        public BookModel(Book b)
        {
            Book = Var.Create(b);
        }

        static string DateToString(DateTime date) =>
            $"{date.Year}-{date.Month.ToString().PadLeft(2, '0')}-{date.Day.ToString().PadLeft(2, '0')}";

        public IRef<string> Title => Book.Lens(b => b.Title, (b, v) => { b.Title = v; return b; });
        public IRef<string> Author => Book.Lens(b => b.Author, (b, v) => { b.Author = v; return b; });
        public IRef<string> PublishDate =>
            Book.Lens(b => DateToString(b.PublishDate), (b, v) => { b.PublishDate = DateTime.Parse(v); return b; });
        public IRef<string> ISBN => Book.Lens(b => b.ISBN, (b, v) => { b.ISBN = v; return b; });
    }
}