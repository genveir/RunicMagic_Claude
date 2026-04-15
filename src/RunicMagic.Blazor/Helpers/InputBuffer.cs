namespace RunicMagic.Blazor.Helpers;

public class InputBuffer
{
    private List<Node> buffer = [];

    private int maxSize = 20;
    private int? retrievalIndex = null;

    public void Add(string value)
    {
        bool exists = false;
        Node highestOrder = new Node("nop");
        foreach (var node in buffer)
        {
            node.IncrementOrder();
            if (node.Order > highestOrder.Order)
            {
                highestOrder = node;
            }

            if (node.Value == value)
            {
                exists = true;
                node.ResetOrder();
            }
        }

        if (!exists)
        {
            if (buffer.Count >= maxSize)
            {
                buffer.Remove(highestOrder);
            }
            buffer.Add(new Node(value));
        }

        retrievalIndex = null;
        buffer.Sort();
    }

    public string? GetNext()
    {
        if (buffer.Count == 0)
        {
            return null;
        }

        if (retrievalIndex == null)
        {
            retrievalIndex = 0;
        }
        else retrievalIndex++;

        if (retrievalIndex >= buffer.Count)
        {
            retrievalIndex = 0;
        }

        return buffer[retrievalIndex.Value].Value;
    }

    public string? GetPrevious()
    {
        if (buffer.Count == 0)
        {
            return null;
        }

        if (retrievalIndex == null)
        {
            retrievalIndex = buffer.Count - 1;
        }
        else retrievalIndex--;

        if (retrievalIndex < 0)
        {
            retrievalIndex = buffer.Count - 1;
        }

        return buffer[retrievalIndex.Value].Value;
    }

    private class Node : IComparable<Node>
    {
        public string Value { get; }
        public int Order { get; private set; }

        public Node(string value)
        {
            Value = value;
            Order = 0;
        }

        public void IncrementOrder()
        {
            Order++;
        }

        public void ResetOrder()
        {
            Order = 0;
        }

        public int CompareTo(Node? other)
        {
            if (other == null) return 1;
            return Order.CompareTo(other.Order);
        }
    }
}


