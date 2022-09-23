using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownDamageGrenade : ThrownGrenade
{
    public string grenadeItemKey = "HE_grenade";
    [Space]
    public float explosionRadius;
    public float explosionForce;
    public int maxDamage;
    public int minDamage;

    private void OnEnable()
    {
        if (pv.IsMine)
        {
            OnActivated += Explode;
        }
    }

    private void OnDisable()
    {
        if (pv.IsMine)
        {
            OnActivated -= Explode;
        }
    }

    private void Explode()
    {
        var objsInRadius = Physics.OverlapSphere(transform.position, explosionRadius);
        List<GameObject> objsInFOV = new List<GameObject>();
        foreach(var obj in objsInRadius)
        {
            //Check fov
            bool inFOV = true;
            /*
            if(Physics.Raycast(transform.position, obj.GetComponent<Collider>().bounds.center, out var hitInfo))
            {
                inFOV = hitInfo.transform == obj.transform;
            }
            
            Debug.DrawRay(transform.position, obj.GetComponent<Collider>().bounds.center - transform.position, Color.red, 10f);
            */
            if (inFOV)
            {
                objsInFOV.Add(obj.gameObject);
            }
        }

        foreach(var obj in objsInFOV)
        {
            if(obj.TryGetComponent<Health>(out var health))
            {
                health.Attack(CalculateDamage(obj.transform.position), pv.Owner, transform.position, grenadeItemKey);
            }
        }
    }

    private int CalculateDamage(Vector3 targetPos)
    {
        return maxDamage;

        //Dst to target will always be <= radius

        float t = (Vector3.Distance(transform.position, targetPos) / explosionRadius);
        return Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, t));
    }
}
