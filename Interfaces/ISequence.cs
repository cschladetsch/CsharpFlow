namespace Flow {
    /// <inheritdoc />
    /// <summary>
    ///     A Sequence is a node that steps only the first generator until it is
    ///     completed, then moves in to the next.
    ///     When the sequence is empty it Completes.
    /// </summary>
    public interface ISequence
        : INode {
    }
}