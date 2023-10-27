using System;
using Rudzoft.ChessLib.Types;

public interface IOpponent : IPlayer
{
    public bool IsTurn();
    public void BeginPonder();
}