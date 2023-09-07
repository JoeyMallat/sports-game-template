using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraftSystem : MonoBehaviour
{
    [SerializeField] DraftClass _upcomingDraftClass;

    private void Start()
    {
        _upcomingDraftClass = new DraftClass(UnityEngine.Random.Range(50, 80));
    }
}
