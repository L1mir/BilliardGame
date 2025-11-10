using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PocketLockDownModifier : GameModifier
{
    [Header("Lock Settings")]
    [SerializeField] private int minLockedPockets = 1;
    [SerializeField] private int maxLockedPockets = 3;
    
    private List<GameObject> lockedPockets = new List<GameObject>();

    protected override void OnActivate()
    {
        var pockets = GameObject.FindGameObjectsWithTag("Pocket");
        if (pockets.Length == 0)
        {
            Debug.LogError("No pockets found with tag 'Pocket'!");
            return;
        }

        int pocketsToLock = Mathf.Min(
            Random.Range(minLockedPockets, maxLockedPockets + 1),
            pockets.Length
        );

        var rnd = new System.Random();
        var shuffledPockets = pockets.OrderBy(x => rnd.Next()).ToArray();

        for (int i = 0; i < pocketsToLock; i++)
        {
            var pocket = shuffledPockets[i];
            var renderer = pocket.GetComponent<MeshRenderer>();
            var collider = pocket.GetComponent<CapsuleCollider>();
            
            if (renderer != null) renderer.enabled = true;
            if (collider != null) collider.enabled = true;
            
            lockedPockets.Add(pocket);
        }
    }

    public void UsePocketLockDown()
    {
        Activate();
    }
    
    protected override void OnDeactivate()
    {
        foreach (var pocket in lockedPockets)
        {
            if (pocket != null)
            {
                var renderer = pocket.GetComponent<MeshRenderer>();
                var collider = pocket.GetComponent<CapsuleCollider>();
                
                if (renderer != null) renderer.enabled = false;
                if (collider != null) collider.enabled = false;
            }
        }
        lockedPockets.Clear();
    }
}