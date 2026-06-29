namespace Neovolve.Configuration.DependencyInjection.Generator;

using System.Collections;
using System.Collections.Immutable;

/// <summary>
///     The <see cref="EquatableArray{T}" /> struct wraps an <see cref="ImmutableArray{T}" /> with value based equality so
///     it can be cached safely by the incremental generator pipeline.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyList<T>
    where T : IEquatable<T>
{
    private readonly ImmutableArray<T> _array;

    public EquatableArray(ImmutableArray<T> array)
    {
        _array = array;
    }

    public bool Equals(EquatableArray<T> other)
    {
        if (_array.IsDefault)
        {
            return other._array.IsDefault;
        }

        if (other._array.IsDefault)
        {
            return false;
        }

        if (_array.Length != other._array.Length)
        {
            return false;
        }

        for (var index = 0; index < _array.Length; index++)
        {
            if (_array[index].Equals(other._array[index]) == false)
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> other && Equals(other);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)GetItems()).GetEnumerator();
    }

    public override int GetHashCode()
    {
        if (_array.IsDefault)
        {
            return 0;
        }

        var hash = 17;

        foreach (var item in _array)
        {
            hash = (hash * 31) + item.GetHashCode();
        }

        return hash;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private ImmutableArray<T> GetItems()
    {
        return _array.IsDefault ? ImmutableArray<T>.Empty : _array;
    }

    public int Count => _array.IsDefault ? 0 : _array.Length;

    public T this[int index] => _array[index];
}
