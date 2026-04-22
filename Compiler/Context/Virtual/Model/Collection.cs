using System.Collections;
using DrzSharp.Compiler.Default.Parser;

namespace DrzSharp.Compiler.Virtual;

//========================
//    EMPTY COLLECTIONS
//========================
public static class Empty
{
    public static readonly IReadOnlyDictionary<string, int> IdByName = new Dictionary<string, int>(0);
    public static readonly IReadOnlyDictionary<string, List<int>> IdListByName = new Dictionary<string, List<int>>(0);

    public static readonly IReadOnlyDictionary<GenName, int> IdByGenName = new Dictionary<GenName, int>(0);
    public static readonly IReadOnlyDictionary<GenName, List<int>> IdListByGenName = new Dictionary<GenName, List<int>>(0);

    public static readonly IReadOnlyList<int> IdList = new List<int>(0);

    //VIRTUAL COLLECTIONS
    public static readonly VCollection<UType> UsageList = new VCollectionEdit<UType>(0);
}

//========================
//   VIRTUAL COLLECTION
//========================
public interface VCollection<out T> : IEnumerable<T>
{
    public T this[int i] { get; }
    public int Count { get; }
}
public sealed class VCollectionEdit<T>(int capacity = 4) : VCollection<T>
{
    private T[] _array = new T[capacity];

    //COLLECTION VIEW METHODS
    public int Count { get; private set; } = 0;
    public T this[int i]
    {
        get => _array[i];
        set => _array[i] = value;
    }

    //COLLECTION METHODS
    public bool Contains(T item)
    => IndexOf(item) >= 0;
    public int IndexOf(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_array[i], item))
                return i;
        }
        return -1;
    }

    private void Grow()
    {
        Count++;
        if (_array.Length == 0)
            Array.Resize(ref _array, 4);
        else if (Count > _array.Length)
            Array.Resize(ref _array, _array.Length * 2);
    }

    public int Add(T item)
    {
        var start = Count;
        Grow();
        
        _array[start] = item;
        return start;
    }
    public (int, int) AddRange(params IEnumerable<T> items)
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

    public int Insert(int index, T item)
    {
        Grow();

        Array.Copy(_array, index, _array, index + 1, Count - index - 1);
        _array[index] = item;

        return index;
    }
    public (int, int) InsertRange(int index, IEnumerable<T> items)
    {
        var len = 0;
        foreach (var item in items)
            Insert(index + len++, item);

        return (index, len);
    }

    public bool Remove(T item)
    {
        var id = IndexOf(item);
        if (id < 0)
            return false;

        RemoveAt(id);
        return true;
    }
    public void RemoveAt(int index)
    {
        if (index < Count--)
            Array.Copy(_array, index + 1, _array, index, Count - index);

        _array[Count] = default!;
    }
    public void Clear()
    {
        Array.Clear(_array);
        Count = 0;
    }

    //ENUMERATOR
    public IEnumerator<T> GetEnumerator()
    => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator()
    => GetEnumerator();
    private struct Enumerator(VCollection<T> collection) : IEnumerator<T>
    {
        private readonly VCollection<T> _collection = collection;
        private int _index = -1;

        public readonly T Current => _collection[_index];
        readonly object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            _index++;
            return _index < _collection.Count;
        }
        public void Reset()
        => _index = 0;
        public readonly void Dispose() { }
    }
}