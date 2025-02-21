#if SORTIFY_COLLECTIONS
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sortify
{
    [Serializable]
    public struct SerializableKeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    [Serializable]
    public class SDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, ISerializationCallbackReceiver, IReadOnlyDictionary<TKey, TValue>
    {
        [SerializeField] private List<SerializableKeyValuePair<TKey, TValue>> _entries = new List<SerializableKeyValuePair<TKey, TValue>>();
        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public void OnBeforeSerialize()
        {
            _entries.Clear();
            foreach (var kvp in _dictionary)
                _entries.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
        }

        public void OnAfterDeserialize()
        {
            _dictionary.Clear();
            foreach (var entry in _entries)
            {
                if (!_dictionary.ContainsKey(entry.Key))
                {
                    _dictionary.Add(entry.Key, entry.Value);
                }
                else
                {
                    Debug.LogWarning($"Duplicate key '{entry.Key}' found during deserialization.");
                }
            }
        }

        // Implementation of IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                _dictionary[key] = value;
                int index = _entries.FindIndex(e => EqualityComparer<TKey>.Default.Equals(e.Key, key));
                if (index >= 0)
                {
                    _entries[index] = new SerializableKeyValuePair<TKey, TValue>(key, value);
                }
                else
                {
                    _entries.Add(new SerializableKeyValuePair<TKey, TValue>(key, value));
                }
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            if (_dictionary.ContainsKey(key))
                throw new ArgumentException($"An element with the key '{key}' already exists.");

            _dictionary.Add(key, value);
            _entries.Add(new SerializableKeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.Remove(key))
            {
                int index = _entries.FindIndex(entry => EqualityComparer<TKey>.Default.Equals(entry.Key, key));
                if (index >= 0)
                {
                    _entries.RemoveAt(index);
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
            _entries.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (_dictionary.TryGetValue(item.Key, out TValue value))
                return EqualityComparer<TValue>.Default.Equals(value, item.Value);

            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (Contains(item))
                return Remove(item.Key);

            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        // Implementation of IReadOnlyDictionary<TKey, TValue>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;

        // Implementation of non-generic IDictionary interface
        object IDictionary.this[object key]
        {
            get
            {
                if (IsCompatibleKey(key))
                    return this[(TKey)key];
                return null;
            }
            set
            {
                if (!IsCompatibleKey(key))
                    throw new ArgumentException("Invalid key type.");

                if (!(value is TValue))
                    throw new ArgumentException("Invalid value type.");

                this[(TKey)key] = (TValue)value;
            }
        }

        bool IDictionary.IsReadOnly => IsReadOnly;
        bool IDictionary.IsFixedSize => false;

        ICollection IDictionary.Keys => (ICollection)Keys;
        ICollection IDictionary.Values => (ICollection)Values;

        void IDictionary.Add(object key, object value)
        {
            if (!IsCompatibleKey(key))
                throw new ArgumentException("Invalid key type.");

            if (!(value is TValue))
                throw new ArgumentException("Invalid value type.");

            Add((TKey)key, (TValue)value);
        }

        bool IDictionary.Contains(object key)
        {
            if (IsCompatibleKey(key))
                return ContainsKey((TKey)key);
            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(_dictionary.GetEnumerator());
        }

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
                Remove((TKey)key);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Rank != 1)
                throw new ArgumentException("Array must be single-dimensional.");

            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException("Array must have zero-based indexing.");

            if (index < 0 || index > array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (array.Length - index < Count)
                throw new ArgumentException("The array is too small.");

            if (array is KeyValuePair<TKey, TValue>[] pairs)
            {
                CopyTo(pairs, index);
            }
            else if (array is DictionaryEntry[] dictEntryArray)
            {
                int i = index;
                foreach (var kvp in _dictionary)
                {
                    dictEntryArray[i++] = new DictionaryEntry(kvp.Key, kvp.Value);
                }
            }
            else if (array is object[] objects)
            {
                int i = index;
                try
                {
                    foreach (var kvp in _dictionary)
                    {
                        objects[i++] = new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException("Invalid array type.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid array type.");
            }
        }

        int ICollection.Count => Count;
        object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;
        bool ICollection.IsSynchronized => false;

        // Helper methods
        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return key is TKey;
        }

        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                this.enumerator = enumerator;
            }

            public DictionaryEntry Entry => new DictionaryEntry(Key, Value);

            public object Key => enumerator.Current.Key;

            public object Value => enumerator.Current.Value;

            public object Current => Entry;

            public bool MoveNext() => enumerator.MoveNext();

            public void Reset() => enumerator.Reset();
        }

        // Additional methods
        public bool ContainsValue(TValue value)
        {
            return _dictionary.ContainsValue(value);
        }

        public bool AddOrUpdate(TKey key, TValue value)
        {
            if (_dictionary.ContainsKey(key))
            {
                this[key] = value;
                return false;
            }
            else
            {
                Add(key, value);
                return true;
            }
        }

        public void AddRange(IDictionary<TKey, TValue> other)
        {
            foreach (var kvp in other)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        public int RemoveRange(IEnumerable<TKey> keys)
        {
            int removedCount = 0;
            foreach (var key in keys)
            {
                if (Remove(key))
                {
                    removedCount++;
                }
            }
            return removedCount;
        }
    }
}
#endif