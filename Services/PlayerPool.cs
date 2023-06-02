using OnlinePlayersPool.Models;

namespace OnlinePlayersPool.Services;

public interface IPlayerPool
{
    int OnlinePlayersCount { get; }
    void AddPlayer(Player player);
    bool RemovePlayer(long id);
    void ChangePlayerRank(long id, long oldRank, long newRank);
    IEnumerable<Player> IterateThroughChanged();
}

internal class PlayerPool : IPlayerPool
{
    private readonly Dictionary<long, LinkedListNode<Player>> _nodeVsId = new(5000);
    private readonly LinkedList<Player> _pool = new();

    public int OnlinePlayersCount => _pool.Count;

    public void AddPlayer(Player player)
    {
        var newNode = new LinkedListNode<Player>(player);

        var addBefore = _pool.First;

        while (addBefore?.Value.Rank < player.Rank)
        {
            addBefore = addBefore.Next;
        }

        if (addBefore is null)
        {
            _pool.AddLast(newNode);
        }
        else
        {
            _pool.AddBefore(addBefore, newNode);
        }

        _nodeVsId.TryAdd(player.Id, newNode);
    }

    public bool RemovePlayer(long id)
    {
        if (_nodeVsId.Remove(id, out var node))
        {
            _pool.Remove(node);
            return true;
        }

        return false;
    }

    public void ChangePlayerRank(long id, long oldRank, long newRank)
    {
        if (!_nodeVsId.TryGetValue(id, out var node))
        {
            if (oldRank < newRank)
            {
                PushPlayersDown(oldRank, newRank);
            }
            else if (oldRank > newRank)
            {
                PushPlayersUp(oldRank, newRank);
            }
            return;
        }

        if (node.Value.Rank < newRank)
        {
            IncreasePlayerRank(node, newRank);
        }
        else if (node.Value.Rank > newRank)
        {
            DecreasePlayerRank(node, newRank);
        }
    }

    private void PushPlayersDown(long oldRank, long newRank)
    {
        if (_pool.Last is null || _pool.Last.Value.Rank < oldRank)
        {
            return;
        }

        var node = _pool.First;
        while (node?.Value.Rank < oldRank)
        {
            node = node.Next;
        }

        while (node?.Value.Rank < newRank)
        {
            node.Value.Rank--;
            node = node.Next;
        }
    }

    private void PushPlayersUp(long oldRank, long newRank)
    {
        if (_pool.First is null || _pool.First.Value.Rank > oldRank)
        {
            return;
        }

        var node = _pool.Last;
        while (node?.Value.Rank > oldRank)
        {
            node = node.Previous;
        }

        while (node?.Value.Rank > newRank)
        {
            node.Value.Rank++;
            node = node.Previous;
        }
    }

    private void IncreasePlayerRank(LinkedListNode<Player> node, long newRank)
    {
        var addAfter = node;

        while (addAfter.Next?.Value.Rank < newRank)
        {
            addAfter = addAfter.Next;
            addAfter.Value.Rank--;
        }

        if (node != addAfter)
        {
            _pool.Remove(node);
            node.Value.Rank = newRank;
            _pool.AddAfter(addAfter, node);
        }
        else
        {
            node.Value.Rank = newRank;
        }
    }

    private void DecreasePlayerRank(LinkedListNode<Player> node, long newRank)
    {
        var addBefore = node;

        while (addBefore.Previous?.Value.Rank > newRank)
        {
            addBefore = addBefore.Previous;
            addBefore.Value.Rank++;
        }

        if (node != addBefore)
        {
            _pool.Remove(node);
            node.Value.Rank = newRank;
            _pool.AddBefore(addBefore, node);
        }
        else
        {
            node.Value.Rank = newRank;
        }
    }

    public IEnumerable<Player> IterateThroughChanged()
    {
        for (var node = _pool.First; node != null; node = node.Next)
        {
            if (node.Value.Changed)
            {
                yield return node.Value;
            }
        }
    }
}
