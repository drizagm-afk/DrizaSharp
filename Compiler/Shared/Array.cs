using System.Collections;
using System.Runtime.CompilerServices;

namespace DrzSharp.Compiler;

//>>>> ARRAY BUILDER <<<<
public static class ArrayBuilder
{
    public static ArrayBuilder<T> Create<T>(int capacity = 4) where T : notnull
    => new(capacity);
}
public sealed class ArrayBuilder<T>(int capacity = 4) where T : notnull
{
    private T[] _array = new T[capacity];
    public int Count { get; private set; } = 0;

    private int Grow(int inc = 1)
    {
        var start = Count;

        Count += inc;
        if (_array.Length == 0)
            _array = new T[Math.Max(4, inc)];
        while (Count > _array.Length)
            Array.Resize(ref _array, _array.Length * 2);

        return start;
    }
    public int Add(T item)
    {
        var start = Grow();

        _array[start] = item;
        return start;
    }
    public (int, int) AddRange(params ReadOnlySpan<T> items)
    {
        var start = Grow(items.Length);
        items.CopyTo(_array.AsSpan(start));

        return (start, items.Length);
    }
    public (int, int) AddRange(IEnumerable<T> items)
    {
        var start = Count;
        var len = 0;
        foreach (var item in items)
        {
            Grow();
            _array[start + len++] = item;
        }
        return (start, len);
    }

    public T[] ToArray()
    {
        T[] res = new T[Count];
        Array.Copy(_array, res, Count);
        Count = 0;

        return res;
    }
    public T[] MoveToArray()
    {
        var res = _array;
        _array = Array.Empty<T>();
        Count = 0;

        return res;
    }

    public ArrayView<T> ToView()
    {
        var len = Count;
        return new(ToArray(), len);
    }
    public ArrayView<T> MoveToView()
    {
        var len = Count;
        return new(MoveToArray(), len);
    }
}

//>>>> ARRAY VIEW <<<<
public static class ArrayView
{
    public static ArrayView<T> AsView<T>(this T[] array) where T : notnull
    => new(array, array.Length);

    public static ArrayView<T> Create<T>(ReadOnlySpan<T> items) where T : notnull
    {
        var builder = new ArrayBuilder<T>(items.Length);
        builder.AddRange(items);

        return builder.MoveToView();
    }
}

[CollectionBuilder(typeof(ArrayView), "Create")]
public readonly struct ArrayView<T> : IEnumerable<T>, IEquatable<ArrayView<T>>
where T : notnull
{
    private readonly T[]? _array;
    public readonly int Length;

    internal ArrayView(T[] array, int length)
    {
        _array = array;
        Length = length;
    }

    //ENUMERABLE
    public T this[int i]
    {
        get
        {
            if ((uint)i >= (uint)Length)
                throw new ArgumentOutOfRangeException();

            return _array![i];
        }
    }

    //ENUMERATOR
    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator(ArrayView<T> slice) : IEnumerator<T>
    {
        private readonly ArrayView<T> _slice = slice;
        private int _index = -1;

        public readonly T Current => _slice[_index];
        readonly object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            _index++;
            return _index < _slice.Length;
        }
        public void Reset()
        => _index = -1;
        public readonly void Dispose() { }
    }

    //EQUALITY
    public bool Equals(ArrayView<T> other)
    {
        //LENGTH CHECK
        if (Length != other.Length)
            return false;
        else if (Length == 0)
            return true;

        //VALUE CHECK
        if (_array == other._array)
            return true;

        var comparer = EqualityComparer<T>.Default;

        for (int i = 0; i < Length; i++)
        {
            if (!comparer.Equals(_array![i], other._array![i]))
                return false;
        }

        return true;
    }

    public static bool operator ==(ArrayView<T> left, ArrayView<T> right)
    => left.Equals(right);
    public static bool operator !=(ArrayView<T> left, ArrayView<T> right)
    => !(left == right);

    public override bool Equals(object? obj)
    => obj is ArrayView<T> other && Equals(other);
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Length);

        for (int i = 0; i < Length; i++)
            hash.Add(_array![i]);

        return hash.ToHashCode();
    }
}