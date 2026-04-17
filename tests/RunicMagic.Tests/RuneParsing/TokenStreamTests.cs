using FluentAssertions;
using RunicMagic.Controller.RuneParsing;
using Xunit;

namespace RunicMagic.Tests.RuneParsing
{
    public class TokenStreamTests
    {
        [Fact]
        public void Next_ReturnsTokensInOrder()
        {
            var tokenStream = new TokenStream("token1 token2 token3");

            tokenStream.Next().Should().Be("token1");
            tokenStream.Next().Should().Be("token2");
            tokenStream.Next().Should().Be("token3");
        }

        [Fact]
        public void Next_ReturnsNull_WhenExhausted()
        {
            var tokenStream = new TokenStream("token1");

            tokenStream.Next().Should().Be("token1");
            tokenStream.Next().Should().BeNull();
        }

        [Fact]
        public void Next_ReturnsNull_WhenEmpty()
        {
            var tokenStream = new TokenStream("");

            tokenStream.Next().Should().BeNull();
        }

        [Fact]
        public void InsertAtCursor_InsertsTokensAtCurrentPosition()
        {
            var tokenStream = new TokenStream("token1 token2");

            tokenStream.Next().Should().Be("token1");

            tokenStream.InsertAtCursor(["inserted1"]);

            tokenStream.Next().Should().Be("inserted1");
            tokenStream.Next().Should().Be("token2");
        }

        [Fact]
        public void InsertAtCursor_MaintainsCorrectOrderAfterInsertion()
        {
            var tokenStream = new TokenStream("token1 token2");

            tokenStream.Next().Should().Be("token1");

            tokenStream.InsertAtCursor(["inserted1", "inserted2"]);

            tokenStream.Next().Should().Be("inserted1");
            tokenStream.Next().Should().Be("inserted2");
            tokenStream.Next().Should().Be("token2");
        }

        [Fact]
        public void InsertAtCursor_WorksWhenCursorIsAtStart()
        {
            var tokenStream = new TokenStream("token1 token2");

            tokenStream.InsertAtCursor(["inserted1"]);

            tokenStream.Next().Should().Be("inserted1");
            tokenStream.Next().Should().Be("token1");
            tokenStream.Next().Should().Be("token2");
        }

        [Fact]
        public void InsertAtCursor_WorksWhenCursorIsAtEnd()
        {
            var tokenStream = new TokenStream("token1");

            tokenStream.Next().Should().Be("token1");
            tokenStream.Next().Should().BeNull();

            tokenStream.InsertAtCursor(["inserted1"]);

            tokenStream.Next().Should().Be("inserted1");
        }

        [Fact]
        public void InsertAtCursor_Works_OnEmptyStream()
        {
            var tokenStream = new TokenStream("");

            tokenStream.InsertAtCursor(["inserted1"]);

            tokenStream.Next().Should().Be("inserted1");
        }

        [Fact]
        public void Backtrack_MovesCursorBack()
        {
            var tokenStream = new TokenStream("token1 token2");

            tokenStream.Next().Should().Be("token1");
            tokenStream.Next().Should().Be("token2");

            tokenStream.Backtrack();

            tokenStream.Next().Should().Be("token2");
        }
    }
}
