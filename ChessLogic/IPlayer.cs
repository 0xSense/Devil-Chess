using System;
using Rudzoft.ChessLib.Types;

public interface IPlayer 
{
    public Move GetMove(Rudzoft.ChessLib.IPosition pos);
}