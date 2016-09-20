using System;
using System.Threading.Tasks;
using WebSharper;
using WebSharper.JQuery;
using WebSharper.UI.Next;
using WebSharper.UI.Next.Client;
using WebSharper.UI.Next.CSharp;
using static WebSharper.UI.Next.CSharp.Client.Html;
using System.Linq;

namespace BookCollection
{
    [JavaScript]
    public class Client
    {
        ListModel<int, BookModel> Books = new ListModel<int, BookModel>(b => b.Book.Value.BookId);

        async Task RefreshList()
        {
            Books.Set((await Remoting.GetBooks()).Select(b => new BookModel(b)));
        }

        BookModel NewBook = new BookModel(Book.Empty);
        Var<string> Message = Var.Create("");

        public WebSharper.UI.Next.Doc DisplayBookDoc(BookModel model)
        {
            return Template.Index.EditListItem.Doc(
                Title: model.Title,
                Author: model.Author,
                PublishDate: model.PublishDate,
                ISBN: model.ISBN,
                Update: async (el, ev) =>
                {
                    model.IsEdited = false;
                    var book = model.Book.Value;
                    var res = await Remoting.UpdateBook(book);
                    if (res)
                    {
                        Books.Add(model);
                        Message.Value = $"Updated book '{book.Title}'";
                    }
                    else
                    {
                        Books.Remove(model);
                        Message.Value = $"Book '{book.Title}' has not been found, removed";
                    }
                },
                Cancel: (el, ev) =>
                {
                    model.IsEdited = false;
                    Books.Add(model);
                }
            );
        }

        public WebSharper.UI.Next.Doc EditBookDoc(BookModel model)
        {
            return Template.Index.ListItem.Doc(
                Title: model.Title.View,
                Author: model.Author.View,
                PublishDate: model.PublishDate.View,
                ISBN: model.ISBN.View,
                Remove: async (el, ev) =>
                {
                    var book = model.Book.Value;
                    Message.Value = $"Removing book '{book.Title}'";
                    var res = await Remoting.DeleteBook(book.BookId);
                    Books.Remove(model);
                    if (res)
                        Message.Value = $"Removed book '{book.Title}'";
                    else
                        Message.Value = $"Book '{book.Title}' was already removed";
                },
                Edit: (el, ev) =>
                {
                    model.IsEdited = true;
                    Books.Add(model);
                }
            );
        }

        public WebSharper.UI.Next.Doc MainDoc()
        {
            Task.Run(RefreshList);

            var newBook = new BookModel(Book.Empty);
            var message = Var.Create("");
            return Template.Index.Main.Doc(
                ListContainer:
                    Books.View.DocSeqCached((BookModel model) =>
                    {
                        if (model.IsEdited)
                            return DisplayBookDoc(model);
                        else
                            return EditBookDoc(model);
                    }
                    ),
                Title: newBook.Title,
                Author: newBook.Author,
                PublishDate: newBook.PublishDate,
                ISBN: newBook.ISBN,
                Message: message.View,
                Add: async (el, ev) =>
                {
                    message.Value = "Adding book";
                    var book = newBook.Book.Value;
                    book.BookId = await Remoting.InsertBook(book);
                    message.Value = $"Added '{book.Title}' with id {book.BookId}";
                    Books.Add(new BookModel(book));
                    newBook.Book.Value = Book.Empty;
                },
                Clear: (el, ev) => { newBook.Book.Value = Book.Empty; },
                Refresh: async (el, ev) => await RefreshList()
            );
        }

        static public IControlBody Main() => new Client().MainDoc();
    }
}