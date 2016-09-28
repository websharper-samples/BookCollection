using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebSharper;

namespace BookCollection
{
    public static class Remoting
    {
        static ConcurrentDictionary<int, Book> store =
            new ConcurrentDictionary<int, Book>()
            {
                [0] = new Book(
                    "Expert F# 4.0",
                    "Don Syme, Adam Granicz, Antonio Cisternino",
                    new DateTime(2015, 12, 28),
                    "978-1-484207-41-3")
            };

        [Remote]
        public static Task<Book[]> GetBooks()
        {
            return Task.FromResult(store.Values.ToArray());
        }

        static int nextId;

        [Remote]
        public static Task<int> InsertBook(Book book)
        {
            var id = Interlocked.Increment(ref nextId);
            book.BookId = id;
            store.TryAdd(id, book);
            return Task.FromResult(id);
        }

        [Remote]
        public static Task<bool> DeleteBook(int id)
        {
            Book removed;
            return Task.FromResult(store.TryRemove(id, out removed));
        }

        [Remote]
        public static Task<bool> UpdateBook(Book book)
        {
            if (store.ContainsKey(book.BookId))
            {
                store[book.BookId] = book;
                return Task.FromResult(true);
            }
            else
                return Task.FromResult(false);
        }

    }
}