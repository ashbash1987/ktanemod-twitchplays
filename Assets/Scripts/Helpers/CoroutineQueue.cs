using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineQueue : MonoBehaviour
{
    public CoroutineCanceller coroutineCanceller = null;

    private void Awake()
    {
        _coroutineQueue = new Queue<IEnumerator>();
    }  

    private void Update()
    {
        if (!_processing && _coroutineQueue.Count > 0)
        {
            _processing = true;
            _activeCoroutine = StartCoroutine(ProcessQueueCoroutine());
        }
    }

    public void AddToQueue(IEnumerator subcoroutine)
    {
        _coroutineQueue.Enqueue(subcoroutine);
    }

    public void CancelFutureSubcoroutines()
    {
        _coroutineQueue.Clear();
    }

    public void StopQueue()
    {
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }

        _processing = false;

        coroutineCanceller.ResetCancel();
    }

    private IEnumerator ProcessQueueCoroutine()
    {
        coroutineCanceller.ResetCancel();

        while (_coroutineQueue.Count > 0)
        {
            IEnumerator coroutine = _coroutineQueue.Dequeue();
            while (coroutine.MoveNext())
            {
                yield return coroutine.Current;
            }
        }

        _processing = false;
        _activeCoroutine = null;

        coroutineCanceller.ResetCancel();
    }

    private Queue<IEnumerator> _coroutineQueue = null;
    private bool _processing = false;
    private Coroutine _activeCoroutine = null;
}
