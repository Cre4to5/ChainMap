using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ChainMapLib
{
    public struct ChainMap<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _mainDictionary = new();
        private readonly List<IDictionary<TKey, TValue>> _additionalDictionaries = new();
        private ReadOnlyCollection<IDictionary<TKey, TValue>> _allDictionaries
        {
            get
            {
                List<IDictionary<TKey, TValue>> temp = new();
                temp.Add(_mainDictionary);
                temp.AddRange(_additionalDictionaries);
                return new(temp);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var temp = _mainDictionary.Select(x => x.Key).ToList();
                temp.AddRange(_additionalDictionaries.SelectMany(x => x.Keys).ToList());
                return temp.Distinct().ToList();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var temp = _mainDictionary.Select(x => x.Value).ToList();
                temp.AddRange(_additionalDictionaries.SelectMany(x => x.Values).ToList());
                return temp.Distinct().ToList();
            }
        }

        public int Count => _allDictionaries.Sum(a => a.Count);

        public int CountDictionaries => _allDictionaries.Count;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get
            {
                if(_mainDictionary.ContainsKey(key)) return _mainDictionary[key];
                foreach(var x in _additionalDictionaries)
                    if(x.ContainsKey(key)) return x[key];
                throw new KeyNotFoundException($"Key: '{key}' wasn't found");
            }
            set
            {
                if (_mainDictionary.ContainsKey(key)) _mainDictionary[key] = value;
                foreach (var x in _additionalDictionaries)
                    if (x.ContainsKey(key)) _mainDictionary[key] = value;
            }
        }

        public ChainMap(params IDictionary<TKey, TValue>[] dictionaries)
        {
            if (dictionaries.Length > 0)
            {
                _mainDictionary = (Dictionary<TKey,TValue>)dictionaries.First();
                _additionalDictionaries = dictionaries.Skip(1).ToList();
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (_mainDictionary.ContainsKey(key)) throw new ArgumentException($"Key: '{key}' already exists in main dictionary!");
            _mainDictionary[key] = value;
        }
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }
        public bool TryAdd(KeyValuePair<TKey,TValue> item)
        {
            if(_mainDictionary.ContainsKey(item.Key)) return false;
            _mainDictionary[item.Key] = item.Value;
            return true;
        }
        public void AddDictionary(IDictionary<TKey, TValue> dictionary, int index)
        {
            if (index < 0) _additionalDictionaries.Add(dictionary);
            else if (index > _additionalDictionaries.Count - 1) _additionalDictionaries.Insert(0, dictionary);
            else _additionalDictionaries.Insert(index, dictionary);
        }

        public bool ContainsKey(TKey key)
        {
            return _allDictionaries.Any(x => x.ContainsKey(key));
        }
        public bool ContainsValue(TValue value)
        {
            return _allDictionaries.Any(x => ((Dictionary<TKey, TValue>)x).ContainsValue(value));
        }

        public bool Remove(TKey key)
        {
            if(!_mainDictionary.ContainsKey(key)) return false;
            _mainDictionary.Remove(key);
            return true;
        }
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }
        public void RemoveDictionary(int index)
        {
            if (index >= 0 && index < _additionalDictionaries.Count)
            {
                _additionalDictionaries.RemoveAt(index);
            }
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            foreach (var dict in _allDictionaries)
            {
                if (dict.ContainsKey(key))
                {
                    value = dict[key];
                    return true;
                }
            }
            value = default;
            return false;
        }

        public void Clear()
        {
            _mainDictionary.Clear();
        }
        public void ClearDictionaries()
        {
            _mainDictionary.Clear();
            _additionalDictionaries.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public ReadOnlyCollection<IDictionary<TKey,TValue>> GetDictionaries()
        {
            return _allDictionaries;
        }
        public ReadOnlyDictionary<TKey,TValue> GetDictionary(int index)
        {
            return new(_allDictionaries[index]);
        }
        public Dictionary<TKey, TValue> GetMainDictionary() => _mainDictionary;

        public Dictionary<TKey,TValue> Merge()
        {
            Dictionary<TKey, TValue> temp = new();
            foreach (var dict in _allDictionaries)
            {
                foreach (var item in dict)
                {
                    if(!temp.ContainsKey(item.Key)) temp[item.Key] = item.Value;
                }
            }
            return temp;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in _mainDictionary)
            {
                yield return new(item.Key, item.Value);
            }
            foreach (var item in _additionalDictionaries)
            {
                foreach (var item1 in item)
                {
                    yield return new(item1.Key, item1.Value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}