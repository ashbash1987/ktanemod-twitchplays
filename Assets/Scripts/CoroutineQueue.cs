using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineQueue : MonoBehaviour
{   
    private void Awake()
    {
        _coroutineQueue = new Queue<IEnumerator>();
    }  

    private void Update()
    {
        if (!_processing && _coroutineQueue.Count > 0)
        {
            _processing = true;
            StartCoroutine(ProcessQueueCoroutine());
        }
    }

    public void AddToQueue(IEnumerator subcoroutine)
    {
        _coroutineQueue.Enqueue(subcoroutine);
    }

    private IEnumerator ProcessQueueCoroutine()
    {
        while (_coroutineQueue.Count > 0)
        {
            IEnumerator coroutine = _coroutineQueue.Dequeue();
            while (coroutine.MoveNext())
            {
                yield return coroutine.Current;
            }
        }

        _processing = false;
    }

    private Queue<IEnumerator> _coroutineQueue = null;
    private bool _processing = false;
}
