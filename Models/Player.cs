namespace OnlinePlayersPool.Models;

public class Player
{
    private long _rank;
    private bool _changed;

    public long Id { get; }

    public long Rank
    {
        get => _rank;
        set
        {
            lock (this)
            {
                _rank = value;
                _changed = true;
            }
        }
    }

    public bool Changed => _changed;

    public LeaderboardMessage GetMessage()
    {
        long rank;
        lock (this)
        {
            _changed = false;
            rank = _rank;
        }

        return new LeaderboardMessage(Id, rank);
    }

    public Player(long id, long initialRank)
    {
        Id = id;
        _rank = initialRank;
    }
}
