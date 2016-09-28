using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebSharper;

namespace BookCollection
{
    [JavaScript, Serializable]
    public class Book
    {
        public int BookId;
        public string Title;
        public string Author;
        public DateTime PublishDate;
        public string ISBN;

        [NonSerialized]
        public bool IsEdited;

        public Book(string title, string author, DateTime publishDate, string isbn)
        {
            Title = title;
            Author = author;
            PublishDate = publishDate;
            ISBN = isbn;
        }

        // required for serialization
        public Book() { }
    }
}