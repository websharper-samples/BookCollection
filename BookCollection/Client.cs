using System;
using System.Threading.Tasks;
using WebSharper;
using WebSharper.JQuery;
using WebSharper.UI;
using WebSharper.UI.Client;
using static WebSharper.UI.Client.Html;

namespace BookCollection
{
    [JavaScript]
    public class App
    {
        static ListModel<int, Book> Books = new ListModel<int, Book>(b => b.BookId);
        static Var<string> Message = Var.Create("");

        static async Task RefreshList()
        {
            Books.Set((await Remoting.GetBooks()));
        }

        static string DateToString(DateTime date) =>
            $"{date.Year}-{date.Month.ToString().PadLeft(2, '0')}-{date.Day.ToString().PadLeft(2, '0')}";

        static WebSharper.UI.Doc DisplayBookDoc(Book book)
        {
            return new Template.Index.ListItem()
                .Title(book.Title)
                .Author(book.Author)
                .PublishDate(DateToString(book.PublishDate))
                .ISBN(book.ISBN)
                .Remove(async (el, ev) =>
                {
                    Message.Value = $"Removing book '{book.Title}'";
                    var res = await Remoting.DeleteBook(book.BookId);
                    Books.Remove(book);
                    if (res)
                        Message.Value = $"Removed book '{book.Title}'";
                    else
                        Message.Value = $"Book '{book.Title}' was already removed";
                })
                .Edit((el, ev) =>
                {
                    book.IsEdited = true;
                    Books.Add(book);
                })
                .Doc();
        }

        static WebSharper.UI.Doc EditBookDoc(Book book)
        {
            var editTitle = Var.Create(book.Title); // new Vars for editing details
            var editAuthor = Var.Create(book.Author);
            var editPublishDate = Var.Create(DateToString(book.PublishDate));
            var editISBN = Var.Create(book.ISBN);
            return new Template.Index.EditListItem()
                .Title(editTitle)
                .Author(editAuthor)
                .PublishDate(editPublishDate)
                .ISBN(editISBN)
                .Update(async (el, ev) =>
                {
                    book.Title = editTitle.Value;
                    book.Author = editAuthor.Value;
                    book.PublishDate = DateTime.Parse(editPublishDate.Value);
                    book.ISBN = editISBN.Value;
                    book.IsEdited = false;
                    var res = await Remoting.UpdateBook(book);
                    if (res)
                    {
                        Books.Add(book);
                        Message.Value = $"Updated book '{book.Title}'";
                    }
                    else
                    {
                        Books.Remove(book);
                        Message.Value = $"Book '{book.Title}' has not been found, removed";
                    }
                })
                .Cancel((el, ev) =>
                {
                    book.IsEdited = false;
                    Books.Add(book);
                })
                .Doc();
        }

        [SPAEntryPoint]
        public static void Main()
        {
            Task.Run(RefreshList);
            var newTitle = Var.Create("");
            var newAuthor = Var.Create("");
            var newPublishDate = Var.Create(DateToString(DateTime.Now));
            var newISBN = Var.Create("");
            new Template.Index.Main()
                .ListContainer(
                    Books.View.DocSeqCached((View<Book> bookView) =>
                        bookView.Doc(book =>
                            book.IsEdited ? EditBookDoc(book) : DisplayBookDoc(book)
                        )
                    )
                )
                .Title(newTitle)
                .Author(newAuthor)
                .PublishDate(newPublishDate)
                .ISBN(newISBN)
                .Message(Message.View)
                .Add(async (el, ev) =>
                {
                    var publishDate = DateTime.Parse(newPublishDate.Value);
                    var book = new Book(newTitle.Value, newAuthor.Value, publishDate, newISBN.Value);
                    Message.Value = "Adding book";
                    book.BookId = await Remoting.InsertBook(book);
                    Message.Value = "Added " + book.Title;
                    Books.Add(book);
                    newTitle.Value = newAuthor.Value = newISBN.Value = "";
                    newPublishDate.Value = DateToString(DateTime.Now);
                })
                .Refresh(async (el, ev) => {
                    await RefreshList();
                    Message.Value = "Collection updated";
                })
                .Doc()
            .RunById("main");
        }
    }
}
