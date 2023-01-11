//-----------------------------------------------------------------------
// <copyright file="StandardRulebook.cs">
//     Copyright (c) Michael Szvetits. All rights reserved.
// </copyright>
// <author>Michael Szvetits</author>
//-----------------------------------------------------------------------
namespace Chess.Model.Rule
{
    using Chess.Model.Command;
    using Chess.Model.Data;
    using Chess.Model.Game;
    using Chess.Model.Piece;
    using Chess.Model.Visitor;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System;

    /// <summary>
    /// Represents the standard chess rulebook.
    /// </summary>
    public class StandardRulebook : IRulebook
    {
        /// <summary>
        /// Represents the check rule of a standard chess game.
        /// </summary>
        private readonly CheckRule checkRule;

        /// <summary>
        /// Represents the end rule of a standard chess game.
        /// </summary>
        private readonly EndRule endRule;

        /// <summary>
        /// Represents the movement rule of a standard chess game.
        /// </summary>
        private readonly MovementRule movementRule;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardRulebook"/> class.
        /// </summary>
        public StandardRulebook()
        {
            var threatAnalyzer = new ThreatAnalyzer();
            var castlingRule = new CastlingRule(threatAnalyzer);
            var enPassantRule = new EnPassantRule();
            var promotionRule = new PromotionRule();

            this.checkRule = new CheckRule(threatAnalyzer);
            this.movementRule = new MovementRule(castlingRule, enPassantRule, promotionRule, threatAnalyzer);
            this.endRule = new EndRule(this.checkRule, this.movementRule);
        }

        /// <summary>
        /// Creates a new chess game according to the standard rulebook.
        /// </summary>
        /// <returns>The newly created chess game.</returns>
        public ChessGame CreateTraditionalGame()
        {
            IEnumerable<PlacedPiece> makeBaseLine(int row, Color color)
            {
                yield return new PlacedPiece(new Position(row, 0), new Rook(color));
                yield return new PlacedPiece(new Position(row, 1), new Knight(color));
                yield return new PlacedPiece(new Position(row, 2), new Bishop(color));
                yield return new PlacedPiece(new Position(row, 3), new Queen(color));
                yield return new PlacedPiece(new Position(row, 4), new King(color));
                yield return new PlacedPiece(new Position(row, 5), new Bishop(color));
                yield return new PlacedPiece(new Position(row, 6), new Knight(color));
                yield return new PlacedPiece(new Position(row, 7), new Rook(color));
            }

            IEnumerable<PlacedPiece> makePawns(int row, Color color) =>
                Enumerable.Range(0, 8).Select(
                    i => new PlacedPiece(new Position(row, i), new Pawn(color))
                );

            IImmutableDictionary<Position, ChessPiece> makePieces(int pawnRow, int baseRow, Color color)
            {
                var pawns = makePawns(pawnRow, color);
                var baseLine = makeBaseLine(baseRow, color);
                var pieces = baseLine.Union(pawns);
                var empty = ImmutableSortedDictionary.Create<Position, ChessPiece>(PositionComparer.DefaultComparer);
                return pieces.Aggregate(empty, (s, p) => s.Add(p.Position, p.Piece));
            }

            var whitePlayer = new Player(Color.White);
            var whitePieces = makePieces(1, 0, Color.White);
            var blackPlayer = new Player(Color.Black);
            var blackPieces = makePieces(6, 7, Color.Black);
            var board = new Board(whitePieces.AddRange(blackPieces));

            return new ChessGame(board, whitePlayer, blackPlayer);
        }

        /// <summary>
        /// Creates a new chess game according to the 960 rulebook.
        /// </summary>
        /// <returns>The newly created chess game.</returns>
        public ChessGame Create960Game()
        {
            IEnumerable<PlacedPiece> makeBaseLine(int row, Color color)
            {
                Random randomCol = new Random();
                int genRandCol;
                int Rook1Col;
                int Rook2Col;

                List<int> availableColumns = new List<int>();
                List<int> kingColumns = new List<int>();

                for (int i = 0; i <= 7; i++)
                {
                    availableColumns.Add(i);
                }


                // This code block places rooks and king
                {
                    genRandCol = availableColumns[randomCol.Next(0, availableColumns.Count - 1)];
                    Rook1Col = genRandCol;

                    yield return new PlacedPiece(new Position(row, genRandCol), new Rook(color));
                    availableColumns.Remove(genRandCol);

                    genRandCol = availableColumns[randomCol.Next(0, availableColumns.Count - 1)];

                    // makes sure rooks are placed in valid columns
                    if (genRandCol == Rook1Col - 1)
                    {
                        if (genRandCol - 1 >= 0) genRandCol--;
                        else genRandCol += 3;
                    }
                    else if (genRandCol == Rook1Col + 1)
                    {
                        if (genRandCol + 1 <= 7) genRandCol++;
                        else genRandCol -= 3;
                    }
                    Rook2Col = genRandCol;

                    yield return new PlacedPiece(new Position(row, genRandCol), new Rook(color));
                    availableColumns.Remove(genRandCol);

                    // Makes sure rook1 value is lower than rook2 so that we can properly get available columns for the king
                    if (Rook2Col < Rook1Col)
                    {
                        int temp = Rook1Col;
                        Rook1Col = Rook2Col;
                        Rook2Col = temp;
                    }

                    // Gets available columns for the king in between the rooks
                    for (int col = Rook1Col + 1; col < Rook2Col; col++)
                    {
                        kingColumns.Add(col);
                    }

                    genRandCol = kingColumns[randomCol.Next(0, kingColumns.Count - 1)];
                    yield return new PlacedPiece(new Position(row, genRandCol), new King(color));
                    availableColumns.Remove(genRandCol);
                }

                // This code block places the bishops
                {
                    List<int> evenCol = new List<int>();
                    List<int> oddCol = new List<int>();
                    foreach (int availableCol in availableColumns)
                    {
                        if (availableCol % 2 == 0) evenCol.Add(availableCol);
                        else oddCol.Add(availableCol);
                    }

                    genRandCol = evenCol[randomCol.Next(0, evenCol.Count - 1)];
                    yield return new PlacedPiece(new Position(row, genRandCol), new Bishop(color));
                    availableColumns.Remove(genRandCol);

                    genRandCol = oddCol[randomCol.Next(0, oddCol.Count - 1)];
                    yield return new PlacedPiece(new Position(row, genRandCol), new Bishop(color));
                    availableColumns.Remove(genRandCol);
                }
                
                genRandCol = availableColumns[randomCol.Next(0, availableColumns.Count - 1)];
                yield return new PlacedPiece(new Position(row, genRandCol), new Knight(color));
                availableColumns.Remove(genRandCol);

                genRandCol = availableColumns[randomCol.Next(0, availableColumns.Count - 1)];
                yield return new PlacedPiece(new Position(row, genRandCol), new Queen(color));
                availableColumns.Remove(genRandCol);

                genRandCol = availableColumns[randomCol.Next(0, availableColumns.Count - 1)];
                yield return new PlacedPiece(new Position(row, genRandCol), new Knight(color));
                availableColumns.Remove(genRandCol);

            }

            IEnumerable<PlacedPiece> makePawns(int row, Color color) =>
                Enumerable.Range(0, 8).Select(
                    i => new PlacedPiece(new Position(row, i), new Pawn(color))
                );

            IImmutableDictionary<Position, ChessPiece> makePieces(int pawnRow, int baseRow, Color color)
            {
                var pawns = makePawns(pawnRow, color);
                var baseLine = makeBaseLine(baseRow, color);
                var pieces = baseLine.Union(pawns);
                var empty = ImmutableSortedDictionary.Create<Position, ChessPiece>(PositionComparer.DefaultComparer);
                return pieces.Aggregate(empty, (s, p) => s.Add(p.Position, p.Piece));
            }

            var whitePlayer = new Player(Color.White);
            var whitePieces = makePieces(1, 0, Color.White);
            var blackPlayer = new Player(Color.Black);
            var blackPieces = makePieces(6, 7, Color.Black);
            var board = new Board(whitePieces.AddRange(blackPieces));

            return new ChessGame(board, whitePlayer, blackPlayer);
        }



        /// <summary>
        /// Gets the status of a chess game, according to the standard rulebook.
        /// </summary>
        /// <param name="game">The game state to be analyzed.</param>
        /// <returns>The current status of the game.</returns>
        public Status GetStatus(ChessGame game)
        {
            return this.endRule.GetStatus(game);
        }

        /// <summary>
        /// Gets all possible updates (i.e., future game states) for a chess piece on a specified position,
        /// according to the standard rulebook.
        /// </summary>
        /// <param name="game">The current game state.</param>
        /// <param name="position">The position to be analyzed.</param>
        /// <returns>A sequence of all possible updates for a chess piece on the specified position.</returns>
        public IEnumerable<Update> GetUpdates(ChessGame game, Position position)
        {
            var piece = game.Board.GetPiece(position, game.ActivePlayer.Color);
            var updates = piece.Map(
                p =>
                {
                    var moves = this.movementRule.GetCommands(game, p);
                    var turnEnds = moves.Select(c => new SequenceCommand(c, EndTurnCommand.Instance));
                    var records = turnEnds.Select
                    (
                        c => new SequenceCommand(c, new SetLastUpdateCommand(new Update(game, c)))
                    );
                    var futures = records.Select(c => c.Execute(game).Map(g => new Update(g, c)));
                    return futures.FilterMaybes().Where
                    (
                        e => !this.checkRule.Check(e.Game, e.Game.PassivePlayer)
                    );
                }
            );

            return updates.GetOrElse(Enumerable.Empty<Update>());
        }
    }
}