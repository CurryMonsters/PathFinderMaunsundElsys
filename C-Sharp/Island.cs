using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Globalization;
using UnityEngine.AI;
using System.Numerics;
public class Island : MonoBehaviour
{
    public UDPReceive myUDPReceive;
    public NavMeshAgent myNavMeshAgent;
    public float stock = 30;
    public int cX = 0;
    public int cY = 1;
    public int yRot = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string[] myCords = myUDPReceive.cords;

        float posX = float.Parse(myCords[cX], CultureInfo.InvariantCulture.NumberFormat) * stock;
        float posY = float.Parse(myCords[cY], CultureInfo.InvariantCulture.NumberFormat) * stock;
        float rot = float.Parse(myCords[yRot], CultureInfo.InvariantCulture.NumberFormat);

        if(!myNavMeshAgent.hasPath || ReachedDestinationOrGaveUp()){
            UnityEngine.Vector3 desPos = new UnityEngine.Vector3(posX,2,posY);
            transform.position = desPos;
            transform.rotation = UnityEngine.Quaternion.Euler(0, rot * Mathf.Rad2Deg,0);
        }
    }

    public bool ReachedDestinationOrGaveUp()
    {

        if (!myNavMeshAgent.pathPending)
        {  
            if (myNavMeshAgent.remainingDistance <=  myNavMeshAgent.stoppingDistance)
            {
                if (!myNavMeshAgent.hasPath || myNavMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
