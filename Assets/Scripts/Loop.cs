using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Loop : MonoBehaviour
{
    [SerializeField]
    private UnityEvent _onLoop;
    [SerializeField,Range(.0001f,100f)] private float _loopTime=1f;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            _onLoop.Invoke();
            yield return new WaitForSeconds(_loopTime);
        }
    }
}
