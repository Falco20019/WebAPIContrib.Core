using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using WebApiContrib.Core.Formatter.GoogleProtobuf;
using Xunit;

namespace WebApiContrib.Core.GoogleProtobuf.Tests
{
    // note: the JSON tests are here to verify that the two formatters do not conflict with each other
    public class ProtobufDelimitedTests
    {
        private TestServer _server;
        private readonly MessageParser<Book> _bookParser;

        public ProtobufDelimitedTests()
        {
            _server = new TestServer(new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .ConfigureServices(services => services
                    .AddMvcCore()
                    .AddProtobufFormatters(options => options.UseDelimitedFormat = true)));
            _bookParser = Book.Parser;
        }

        [Fact]
        public async Task GetCollection_X_Protobuf_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/books");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-protobuf"));
            var result = await client.SendAsync(request);
            var stream = await result.Content.ReadAsStreamAsync();
            
            var bookList = new[]
            {
                _bookParser.ParseDelimitedFrom(stream),
                _bookParser.ParseDelimitedFrom(stream)
            };

            Assert.Equal(2, bookList.Length);
            Assert.Equal(Book.Data[0].Author, bookList[0].Author);
            Assert.Equal(Book.Data[0].Title, bookList[0].Title);
            Assert.Equal(Book.Data[1].Author, bookList[1].Author);
            Assert.Equal(Book.Data[1].Title, bookList[1].Title);
        }

        [Fact]
        public async Task GetCollection_Protobuf_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/books");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/protobuf"));
            var result = await client.SendAsync(request);
            var stream = await result.Content.ReadAsStreamAsync();
            var bookList = new[]
            {
                _bookParser.ParseDelimitedFrom(stream),
                _bookParser.ParseDelimitedFrom(stream)
            };

            Assert.Equal(2, bookList.Length);
            Assert.Equal(Book.Data[0].Author, bookList[0].Author);
            Assert.Equal(Book.Data[0].Title, bookList[0].Title);
            Assert.Equal(Book.Data[1].Author, bookList[1].Author);
            Assert.Equal(Book.Data[1].Title, bookList[1].Title);
        }

        [Fact]
        public async Task GetCollection_X_Google_Protobuf_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/books");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-google-protobuf"));
            var result = await client.SendAsync(request);
            var stream = await result.Content.ReadAsStreamAsync();
            var bookList = new[]
            {
                _bookParser.ParseDelimitedFrom(stream),
                _bookParser.ParseDelimitedFrom(stream)
            };

            Assert.Equal(2, bookList.Length);
            Assert.Equal(Book.Data[0].Author, bookList[0].Author);
            Assert.Equal(Book.Data[0].Title, bookList[0].Title);
            Assert.Equal(Book.Data[1].Author, bookList[1].Author);
            Assert.Equal(Book.Data[1].Title, bookList[1].Title);
        }

        [Fact]
        public async Task GetById_Protobuf_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/books/1");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/protobuf"));
            var result = await client.SendAsync(request);
            var book = Book.Parser.ParseDelimitedFrom(await result.Content.ReadAsStreamAsync());

            Assert.NotNull(book);
            Assert.Equal(Book.Data[0].Author, book.Author);
            Assert.Equal(Book.Data[0].Title, book.Title);
        }

        [Fact]
        public async Task GetById_X_Protobuf_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/books/1");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-protobuf"));
            var result = await client.SendAsync(request);
            var book = Book.Parser.ParseDelimitedFrom(await result.Content.ReadAsStreamAsync());

            Assert.NotNull(book);
            Assert.Equal(Book.Data[0].Author, book.Author);
            Assert.Equal(Book.Data[0].Title, book.Title);
        }

        [Fact]
        public async Task GetById_X__Google_Protobuf_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/books/1");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-google-protobuf"));
            var result = await client.SendAsync(request);
            var book = Book.Parser.ParseDelimitedFrom(await result.Content.ReadAsStreamAsync());

            Assert.NotNull(book);
            Assert.Equal(Book.Data[0].Author, book.Author);
            Assert.Equal(Book.Data[0].Title, book.Title);
        }
        [Fact]
        public async Task Post_X_Protobuf_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/books");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-protobuf"));

            var book = new Book
            {
                Author = "Tim Parks",
                Title = "Italian Ways: On and off the Rails from Milan to Palermo"
            };

            MemoryStream stream = new MemoryStream();
            book.WriteDelimitedTo(stream);

            HttpContent data = new StreamContent(stream);

            request.Content = data;
            var result = await client.SendAsync(request);

            var echo = Book.Parser.ParseDelimitedFrom(await result.Content.ReadAsStreamAsync());

            Assert.NotNull(book);
            Assert.Equal(book.Author, echo.Author);
            Assert.Equal(book.Title, echo.Title);
        }

        [Fact]
        public async Task Post_Protobuf_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/books");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/protobuf"));

            var book = new Book
            {
                Author = "Tim Parks",
                Title = "Italian Ways: On and off the Rails from Milan to Palermo"
            };

            MemoryStream stream = new MemoryStream();
            book.WriteDelimitedTo(stream);

            HttpContent data = new StreamContent(stream);

            request.Content = data;
            var result = await client.SendAsync(request);

            var echo = Book.Parser.ParseDelimitedFrom(await result.Content.ReadAsStreamAsync());

            Assert.NotNull(book);
            Assert.Equal(book.Author, echo.Author);
            Assert.Equal(book.Title, echo.Title);
        }

        [Fact]
        public async Task Post_X_Google_Protobuf_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/books");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-google-protobuf"));

            var book = new Book
            {
                Author = "Tim Parks",
                Title = "Italian Ways: On and off the Rails from Milan to Palermo"
            };

            MemoryStream stream = new MemoryStream();
            book.WriteDelimitedTo(stream);

            HttpContent data = new StreamContent(stream);

            request.Content = data;
            var result = await client.SendAsync(request);

            var echo = Book.Parser.ParseDelimitedFrom(await result.Content.ReadAsStreamAsync());

            Assert.NotNull(book);
            Assert.Equal(book.Author, echo.Author);
            Assert.Equal(book.Title, echo.Title);
        }
        [Fact]
        public async Task GetCollection_JSON_Header()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/books");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var result = await client.SendAsync(request);
            var bookList = JsonConvert.DeserializeObject<Book[]>(await result.Content.ReadAsStringAsync());

            Assert.Equal(2, bookList.Length);
            Assert.Equal(Book.Data[0].Author, bookList[0].Author);
            Assert.Equal(Book.Data[0].Title, bookList[0].Title);
            Assert.Equal(Book.Data[1].Author, bookList[1].Author);
            Assert.Equal(Book.Data[1].Title, bookList[1].Title);
        }
    }
}
