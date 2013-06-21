﻿/* Copyright 2010-2013 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson
{
    /// <summary>
    /// Represents a BSON document that is deserialized lazily.
    /// </summary>
    [Serializable]
    [BsonSerializer(typeof(LazyBsonDocumentSerializer))]
    public class LazyBsonDocument : BsonDocument, IDisposable
    {
        // private fields
        private bool _disposed;
        private IByteBuffer _slice;
        private List<IDisposable> _disposableItems = new List<IDisposable>();
        private BsonBinaryReaderSettings _readerSettings = BsonBinaryReaderSettings.Defaults;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LazyBsonDocument"/> class.
        /// </summary>
        /// <param name="slice">The slice.</param>
        /// <exception cref="System.ArgumentNullException">slice</exception>
        /// <exception cref="System.ArgumentException">LazyBsonDocument cannot be used with an IByteBuffer that needs disposing.</exception>
        public LazyBsonDocument(IByteBuffer slice)
        {
            if (slice == null)
            {
                throw new ArgumentNullException("slice");
            }

            _slice = slice;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyBsonDocument"/> class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public LazyBsonDocument(byte[] bytes)
            : this(new ByteArrayBuffer(bytes, 0, bytes.Length, true))
        {
        }

        // public properties
        /// <summary>
        /// Gets the number of elements.
        /// </summary>
        public override int ElementCount
        {
            get
            {
                EnsureDataIsAccessible();
                return base.ElementCount;
            }
        }

        /// <summary>
        /// Gets the elements.
        /// </summary>
        public override IEnumerable<BsonElement> Elements
        {
            get
            {
                EnsureDataIsAccessible();
                return base.Elements;
            }
        }

        /// <summary>
        /// Gets the element names.
        /// </summary>
        public override IEnumerable<string> Names
        {
            get
            {
                EnsureDataIsAccessible();
                return base.Names;
            }
        }

        /// <summary>
        /// Gets the raw values (see BsonValue.RawValue).
        /// </summary>
        [Obsolete("Use Values instead.")]
        public override IEnumerable<object> RawValues
        {
            get
            {
                EnsureDataIsAccessible();
                return base.RawValues;
            }
        }

        /// <summary>
        /// Gets the slice.
        /// </summary>
        /// <value>
        /// The slice.
        /// </value>
        public IByteBuffer Slice
        {
            get { return _slice; }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        public override IEnumerable<BsonValue> Values
        {
            get
            {
                EnsureDataIsAccessible();
                return base.Values;
            }
        }

        // public indexers
        /// <summary>
        /// Gets or sets a value by position.
        /// </summary>
        /// <param name="index">The position.</param>
        /// <returns>The value.</returns>
        public override BsonValue this[int index]
        {
            get
            {
                EnsureDataIsAccessible();
                return base[index];
            }
            set
            {
                EnsureDataIsAccessible();
                base[index] = value;
            }
        }

        /// <summary>
        /// Gets the value of an element or a default value if the element is not found.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="defaultValue">The default value to return if the element is not found.</param>
        /// <returns>Teh value of the element or a default value if the element is not found.</returns>
        [Obsolete("Use GetValue(string name, BsonValue defaultValue) instead.")]
        public override BsonValue this[string name, BsonValue defaultValue]
        {
            get
            {
                EnsureDataIsAccessible();
                return base[name, defaultValue];
            }
        }

        /// <summary>
        /// Gets or sets a value by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The value.</returns>
        public override BsonValue this[string name]
        {
            get
            {
                EnsureDataIsAccessible();
                return base[name];
            }
            set
            {
                EnsureDataIsAccessible();
                base[name] = value;
            }
       }

        // public methods
        /// <summary>
        /// Adds an element to the document.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Add(BsonElement element)
        {
            EnsureDataIsAccessible();
            return base.Add(element);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public override BsonDocument Add(Dictionary<string, object> dictionary)
        {
            EnsureDataIsAccessible();
            return base.Add(dictionary);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keys">Which keys of the hash table to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public override BsonDocument Add(Dictionary<string, object> dictionary, IEnumerable<string> keys)
        {
            EnsureDataIsAccessible();
            return base.Add(dictionary, keys);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public override BsonDocument Add(IDictionary<string, object> dictionary)
        {
            EnsureDataIsAccessible();
            return base.Add(dictionary);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keys">Which keys of the hash table to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public override BsonDocument Add(IDictionary<string, object> dictionary, IEnumerable<string> keys)
        {
            EnsureDataIsAccessible();
            return base.Add(dictionary, keys);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public override BsonDocument Add(IDictionary dictionary)
        {
            EnsureDataIsAccessible();
            return base.Add(dictionary);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="keys">Which keys of the hash table to add.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public override BsonDocument Add(IDictionary dictionary, IEnumerable keys)
        {
            EnsureDataIsAccessible();
            return base.Add(dictionary, keys);
        }

        /// <summary>
        /// Adds a list of elements to the document.
        /// </summary>
        /// <param name="elements">The list of elements.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange instead.")]
        public override BsonDocument Add(IEnumerable<BsonElement> elements)
        {
            EnsureDataIsAccessible();
            return base.Add(elements);
        }

        /// <summary>
        /// Adds a list of elements to the document.
        /// </summary>
        /// <param name="elements">The list of elements.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        [Obsolete("Use AddRange(IEnumerable<BsonElement> elements) instead.")]
        public override BsonDocument Add(params BsonElement[] elements)
        {
            EnsureDataIsAccessible();
            return base.Add(elements);
        }

        /// <summary>
        /// Creates and adds an element to the document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The value of the element.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Add(string name, BsonValue value)
        {
            EnsureDataIsAccessible();
            return base.Add(name, value);
        }

        /// <summary>
        /// Creates and adds an element to the document, but only if the condition is true.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The value of the element.</param>
        /// <param name="condition">Whether to add the element to the document.</param>
        /// <returns>The document (so method calls can be chained).</returns>
        public override BsonDocument Add(string name, BsonValue value, bool condition)
        {
            EnsureDataIsAccessible();
            return base.Add(name, value, condition);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument AddRange(Dictionary<string, object> dictionary)
        {
            EnsureDataIsAccessible();
            return base.AddRange(dictionary);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument AddRange(IDictionary dictionary)
        {
            EnsureDataIsAccessible();
            return base.AddRange(dictionary);
        }

        /// <summary>
        /// Adds a list of elements to the document.
        /// </summary>
        /// <param name="elements">The list of elements.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument AddRange(IEnumerable<BsonElement> elements)
        {
            EnsureDataIsAccessible();
            return base.AddRange(elements);
        }

        /// <summary>
        /// Adds elements to the document from a dictionary of key/value pairs.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument AddRange(IEnumerable<KeyValuePair<string, object>> dictionary)
        {
            EnsureDataIsAccessible();
            return base.AddRange(dictionary);
        }

        /// <summary>
        /// Clears the document (removes all elements).
        /// </summary>
        public override void Clear()
        {
            EnsureDataIsAccessible();
            base.Clear();
        }

        /// <summary>
        /// Creates a shallow clone of the document (see also DeepClone).
        /// </summary>
        /// <returns>
        /// A shallow clone of the document.
        /// </returns>
        public override BsonValue Clone()
        {
            if (_slice != null)
            {
                return new LazyBsonDocument(CloneSlice());
            }
            else
            {
                return base.Clone();
            }
        }

        /// <summary>
        /// Compares this document to another document.
        /// </summary>
        /// <param name="other">The other document.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates whether this document is less than, equal to, or greather than the other.
        /// </returns>
        public override int CompareTo(BsonDocument other)
        {
            EnsureDataIsAccessible();
            return base.CompareTo(other);
        }

        /// <summary>
        /// Compares the BsonDocument to another BsonValue.
        /// </summary>
        /// <param name="other">The other BsonValue.</param>
        /// <returns>A 32-bit signed integer that indicates whether this BsonDocument is less than, equal to, or greather than the other BsonValue.</returns>
        public override int CompareTo(BsonValue other)
        {
            EnsureDataIsAccessible();
            return base.CompareTo(other);
        }

        /// <summary>
        /// Tests whether the document contains an element with the specified name.
        /// </summary>
        /// <param name="name">The name of the element to look for.</param>
        /// <returns>
        /// True if the document contains an element with the specified name.
        /// </returns>
        public override bool Contains(string name)
        {
            EnsureDataIsAccessible();
            return base.Contains(name);
        }

        /// <summary>
        /// Tests whether the document contains an element with the specified value.
        /// </summary>
        /// <param name="value">The value of the element to look for.</param>
        /// <returns>
        /// True if the document contains an element with the specified value.
        /// </returns>
        public override bool ContainsValue(BsonValue value)
        {
            EnsureDataIsAccessible();
            return base.ContainsValue(value);
        }

        /// <summary>
        /// Creates a deep clone of the document (see also Clone).
        /// </summary>
        /// <returns>
        /// A deep clone of the document.
        /// </returns>
        public override BsonValue DeepClone()
        {
            if (_slice != null)
            {
                return new LazyBsonDocument(CloneSlice());
            }
            else
            {
                return base.DeepClone();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets an element of this document.
        /// </summary>
        /// <param name="index">The zero based index of the element.</param>
        /// <returns>
        /// The element.
        /// </returns>
        public override BsonElement GetElement(int index)
        {
            EnsureDataIsAccessible();
            return base.GetElement(index);
        }

        /// <summary>
        /// Gets an element of this document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <returns>
        /// A BsonElement.
        /// </returns>
        public override BsonElement GetElement(string name)
        {
            EnsureDataIsAccessible();
            return base.GetElement(name);
        }

        /// <summary>
        /// Gets an enumerator that can be used to enumerate the elements of this document.
        /// </summary>
        /// <returns>
        /// An enumerator.
        /// </returns>
        public override IEnumerator<BsonElement> GetEnumerator()
        {
            EnsureDataIsAccessible();
            return base.GetEnumerator();
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            EnsureDataIsAccessible();
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the value of an element.
        /// </summary>
        /// <param name="index">The zero based index of the element.</param>
        /// <returns>
        /// The value of the element.
        /// </returns>
        public override BsonValue GetValue(int index)
        {
            EnsureDataIsAccessible();
            return base.GetValue(index);
        }

        /// <summary>
        /// Gets the value of an element.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <returns>
        /// The value of the element.
        /// </returns>
        public override BsonValue GetValue(string name)
        {
            EnsureDataIsAccessible();
            return base.GetValue(name);
        }

        /// <summary>
        /// Gets the value of an element or a default value if the element is not found.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="defaultValue">The default value returned if the element is not found.</param>
        /// <returns>
        /// The value of the element or the default value if the element is not found.
        /// </returns>
        public override BsonValue GetValue(string name, BsonValue defaultValue)
        {
            EnsureDataIsAccessible();
            return base.GetValue(name, defaultValue);
        }

        /// <summary>
        /// Inserts a new element at a specified position.
        /// </summary>
        /// <param name="index">The position of the new element.</param>
        /// <param name="element">The element.</param>
        public override void InsertAt(int index, BsonElement element)
        {
            EnsureDataIsAccessible();
            base.InsertAt(index, element);
        }

        /// <summary>
        /// Merges another document into this one. Existing elements are not overwritten.
        /// </summary>
        /// <param name="document">The other document.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Merge(BsonDocument document)
        {
            EnsureDataIsAccessible();
            return base.Merge(document);
        }

        /// <summary>
        /// Merges another document into this one, specifying whether existing elements are overwritten.
        /// </summary>
        /// <param name="document">The other document.</param>
        /// <param name="overwriteExistingElements">Whether to overwrite existing elements.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Merge(BsonDocument document, bool overwriteExistingElements)
        {
            EnsureDataIsAccessible();
            return base.Merge(document, overwriteExistingElements);
        }

        /// <summary>
        /// Removes an element from this document (if duplicate element names are allowed
        /// then all elements with this name will be removed).
        /// </summary>
        /// <param name="name">The name of the element to remove.</param>
        public override void Remove(string name)
        {
            EnsureDataIsAccessible();
            base.Remove(name);
        }

        /// <summary>
        /// Removes an element from this document.
        /// </summary>
        /// <param name="index">The zero based index of the element to remove.</param>
        public override void RemoveAt(int index)
        {
            EnsureDataIsAccessible();
            base.RemoveAt(index);
        }

        /// <summary>
        /// Removes an element from this document.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public override void RemoveElement(BsonElement element)
        {
            EnsureDataIsAccessible();
            base.RemoveElement(element);
        }

        /// <summary>
        /// Sets the value of an element.
        /// </summary>
        /// <param name="index">The zero based index of the element whose value is to be set.</param>
        /// <param name="value">The new value.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Set(int index, BsonValue value)
        {
            EnsureDataIsAccessible();
            return base.Set(index, value);
        }

        /// <summary>
        /// Sets the value of an element (an element will be added if no element with this name is found).
        /// </summary>
        /// <param name="name">The name of the element whose value is to be set.</param>
        /// <param name="value">The new value.</param>
        /// <returns>
        /// The document (so method calls can be chained).
        /// </returns>
        public override BsonDocument Set(string name, BsonValue value)
        {
            EnsureDataIsAccessible();
            return base.Set(name, value);
        }

        /// <summary>
        /// Sets an element of the document (replaces any existing element with the same name or adds a new element if an element with the same name is not found).
        /// </summary>
        /// <param name="element">The new element.</param>
        /// <returns>
        /// The document.
        /// </returns>
        public override BsonDocument SetElement(BsonElement element)
        {
            EnsureDataIsAccessible();
            return base.SetElement(element);
        }

        /// <summary>
        /// Sets an element of the document (replacing the existing element at that position).
        /// </summary>
        /// <param name="index">The zero based index of the element to replace.</param>
        /// <param name="element">The new element.</param>
        /// <returns>
        /// The document.
        /// </returns>
        public override BsonDocument SetElement(int index, BsonElement element)
        {
            EnsureDataIsAccessible();
            return base.SetElement(index, element);
        }

        /// <summary>
        /// Tries to get an element of this document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The element.</param>
        /// <returns>
        /// True if an element with that name was found.
        /// </returns>
        public override bool TryGetElement(string name, out BsonElement value)
        {
            EnsureDataIsAccessible();
            return base.TryGetElement(name, out value);
        }

        /// <summary>
        /// Tries to get the value of an element of this document.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="value">The value of the element.</param>
        /// <returns>
        /// True if an element with that name was found.
        /// </returns>
        public override bool TryGetValue(string name, out BsonValue value)
        {
            EnsureDataIsAccessible();
            return base.TryGetValue(name, out value);
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_slice != null)
                    {
                        _slice.Dispose();
                        _slice = null;
                    }
                    if (_disposableItems != null)
                    {
                        _disposableItems.ForEach(x => x.Dispose());
                        _disposableItems = null;
                    }
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Throws if disposed.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException"></exception>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        // private methods
        private IByteBuffer CloneSlice()
        {
            return _slice.GetSlice(0, _slice.Length);
        }

        private LazyBsonArray DeserializeLazyBsonArray(BsonBinaryReader bsonReader)
        {
            var slice = bsonReader.ReadRawBsonArray();
            var nestedArray = new LazyBsonArray(slice);
            _disposableItems.Add(nestedArray);
            return nestedArray;
        }

        private LazyBsonDocument DeserializeLazyBsonDocument(BsonBinaryReader bsonReader)
        {
            var slice = bsonReader.ReadRawBsonDocument();
            var nestedDocument = new LazyBsonDocument(slice);
            _disposableItems.Add(nestedDocument);
            return nestedDocument;
        }

        private void DeserializeThisLevel()
        {
            var elements = new List<BsonElement>();

            using (var stream = new ByteBufferStream(_slice, ownsByteBuffer: false))
            using (var bsonReader = new BsonBinaryReader(stream, _readerSettings))
            {
                bsonReader.ReadStartDocument();
                BsonType bsonType;
                while ((bsonType = bsonReader.ReadBsonType()) != BsonType.EndOfDocument)
                {
                    var name = bsonReader.ReadName();
                    BsonValue value;
                    switch (bsonType)
                    {
                        case BsonType.Array: value = DeserializeLazyBsonArray(bsonReader); break;
                        case BsonType.Document: value = DeserializeLazyBsonDocument(bsonReader); break;
                        default: value = (BsonValue)BsonValueSerializer.Instance.Deserialize(bsonReader, typeof(BsonValue), null); break;
                    }
                    elements.Add(new BsonElement(name, value));
                }
                bsonReader.ReadEndDocument();
            }

            var slice = _slice;
            try
            {
                _slice = null;
                AddRange(elements);
                slice.Dispose();
            }
            catch
            {
                try { Clear(); } catch { }
                _slice = slice;
                throw;
            }
        }

        private void EnsureDataIsAccessible()
        {
            ThrowIfDisposed();
            EnsureThisLevelHasBeenDeserialized();
        }

        private void EnsureThisLevelHasBeenDeserialized()
        {
            if (_slice != null)
            {
                DeserializeThisLevel();
            }
        }
    }
}
