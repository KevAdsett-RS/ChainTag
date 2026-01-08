using System;
using PurrNet;

public interface IStateBinding
{
    void Bind();
    void Unbind();
}
public class VarStateBinding<T> : IStateBinding
{
    private readonly SyncVar<T> _var;
    private readonly Action<T> _handler;
    
    public VarStateBinding(SyncVar<T> var, Action<T> handler)
    {
        _var = var;
        _handler = handler;
    }
    
    public void Bind()
    {
        _var.onChanged += _handler;
        _handler?.Invoke(_var);
    }

    public void Unbind()
    {
        _var.onChanged -= _handler;
    }
}

public class ListStateBinding<T> : IStateBinding
{
    private readonly SyncList<T> _list;
    private readonly SyncList<T>.SyncListChanged<T> _handler;
    
    public ListStateBinding(SyncList<T> list, SyncList<T>.SyncListChanged<T> handler)
    {
        _list = list;
        _handler = handler;
    }
    
    public void Bind()
    {
        _list.onChanged += _handler;
        for (var i = 0; i < _list.Count; i++)
        {
            _handler.Invoke(SyncListChange<T>.Added(_list[i], i));
        }
    }

    public void Unbind()
    {
        _list.onChanged -= _handler;
    }
}

public class DictionaryStateBinding<T1, T2> : IStateBinding
{
    private readonly SyncDictionary<T1, T2> _dict;
    private readonly SyncDictionary<T1, T2>.SyncDictionaryChanged<T1, T2> _handler;
    
    public DictionaryStateBinding(SyncDictionary<T1, T2> dict, SyncDictionary<T1, T2>.SyncDictionaryChanged<T1, T2> handler)
    {
        _dict = dict;
        _handler = handler;
    }
    
    public void Bind()
    {
        _dict.onChanged += _handler;

        foreach (var keyValuePair in _dict)
        {
            _handler.Invoke(new SyncDictionaryChange<T1, T2>(SyncDictionaryOperation.Added, keyValuePair.Key, keyValuePair.Value));
        }
    }

    public void Unbind()
    {
        _dict.onChanged -= _handler;
    }
}

