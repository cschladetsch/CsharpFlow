// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

namespace Flow {
    /// <inheritdoc />
    /// <summary>
    ///     A Node is a Group that steps all referenced Generators when it itself is Stepped,
    ///     and similarly for Pre and Post.
    /// </summary>
    public interface INode
        : IGroup {
        void Add(params IGenerator[] gens);
        new INode AddTo(IGroup group);
        new INode Named(string name);
    }
}