using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace LogSluice.Models;

public class VirtualLogList : IList<string>, IReadOnlyList<string>, IList, INotifyCollectionChanged
{
    private readonly List<long> _lineOffsets = new();
    private long _scannedPosition = 0;
    private readonly object _lock = new();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private readonly string _filePath; // Store the path instead of the stream

    public VirtualLogList(string filePath)
    {
        _filePath = filePath;
        _lineOffsets.Add(0);
        ScanForNewLines();
    }

    public void ScanForNewLines()
    {
        lock (_lock)
        {
            try
            {
                using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                if (stream.Length <= _scannedPosition) return;

                int previousCount = Count;
                stream.Seek(_scannedPosition, SeekOrigin.Begin);

                int bufferSize = 65536;
                byte[] buffer = new byte[bufferSize];
                int bytesRead;
                long currentPos = _scannedPosition;

                while ((bytesRead = stream.Read(buffer, 0, bufferSize)) > 0)
                {
                    for (int i = 0; i < bytesRead; i++)
                    {
                        if (buffer[i] == '\n')
                        {
                            _lineOffsets.Add(currentPos + i + 1);
                        }
                    }
                    currentPos += bytesRead;
                }

                _scannedPosition = currentPos;

                if (Count > previousCount)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    });
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }

    public string this[int index]
    {
        get
        {
            lock (_lock)
            {
                if (index < 0 || index >= Count) return string.Empty;

                long start = _lineOffsets[index];
                long end = (index == _lineOffsets.Count - 1) ? _scannedPosition : _lineOffsets[index + 1];
                int length = (int)(end - start);

                if (length <= 0) return string.Empty;


                while (true)
                    try
                    {
                        using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                        byte[] buffer = new byte[length];
                        stream.Seek(start, SeekOrigin.Begin);
                        int read = stream.Read(buffer, 0, length);

                        return Encoding.UTF8.GetString(buffer, 0, read).TrimEnd('\r', '\n');
                    }
                    catch (Exception)
                    {
                        // ignore...
                    }
            }
        }
    }

    public int Count => _lineOffsets.Count > 0 && _lineOffsets[^1] == _scannedPosition
        ? _lineOffsets.Count - 1
        : _lineOffsets.Count;

    public bool IsReadOnly => true;
    public void Add(string item) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();
    public bool Contains(string item) => throw new NotSupportedException();
    public void CopyTo(string[] array, int arrayIndex) => throw new NotSupportedException();
    public IEnumerator<string> GetEnumerator() => throw new NotSupportedException();
    public int IndexOf(string item) => throw new NotSupportedException();
    public void Insert(int index, string item) => throw new NotSupportedException();
    public bool Remove(string item) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    object? IList.this[int index]
    {
        get => this[index];
        set => throw new NotSupportedException();
    }
    bool IList.IsFixedSize => false;
    bool IList.IsReadOnly => true;
    int IList.Add(object? value) => throw new NotSupportedException();
    void IList.Clear() => throw new NotSupportedException();
    bool IList.Contains(object? value) => throw new NotSupportedException();
    int IList.IndexOf(object? value) => throw new NotSupportedException();
    void IList.Insert(int index, object? value) => throw new NotSupportedException();
    void IList.Remove(object? value) => throw new NotSupportedException();
    void IList.RemoveAt(int index) => throw new NotSupportedException();

    bool ICollection.IsSynchronized => true;
    object ICollection.SyncRoot => _lock;

    string IList<string>.this[int index] { get => this[index]; set => throw new NotImplementedException(); }

    void ICollection.CopyTo(Array array, int index) => throw new NotSupportedException();
}