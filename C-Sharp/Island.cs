using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Globalization;
using UnityEngine.AI;
using System.Numerics;

public class Island : MonoBehaviour
{
    // Referanse til UDP-mottakskomponenten for å hente koordinater
    public UDPReceive myUDPReceive;
    // NavMeshAgent for å navigere på NavMesh
    public NavMeshAgent myNavMeshAgent;
    public float stock = 30; // Skaleringsfaktor for posisjon
    public int cX = 0; // Indeks for X-koordinaten
    public int cY = 1; // Indeks for Y-koordinaten
    public int yRot = 0; // Indeks for rotasjon

    // Oppdateres hver ramme
    void Update()
    {
        // Henter koordinater fra UDP-mottak
        string[] myCords = myUDPReceive.cords;

        // Konverterer koordinater fra streng til flyttall og skalerer dem
        float posX = float.Parse(myCords[cX], CultureInfo.InvariantCulture.NumberFormat) * stock;
        float posY = float.Parse(myCords[cY], CultureInfo.InvariantCulture.NumberFormat) * stock;
        float rot = float.Parse(myCords[yRot], CultureInfo.InvariantCulture.NumberFormat);

        if (!myNavMeshAgent.hasPath || ReachedDestinationOrGaveUp())
        {
            // Oppdater posisjon og rotasjon til agenten
            UnityEngine.Vector3 desPos = new UnityEngine.Vector3(posX, 2, posY);
            transform.position = desPos;
            transform.rotation = UnityEngine.Quaternion.Euler(0, rot * Mathf.Rad2Deg, 0);
        }
    }

    // Funksjon som sjekker om agenten har nådd målet
    public bool ReachedDestinationOrGaveUp()
    {
        if (!myNavMeshAgent.pathPending)
        {
            if (myNavMeshAgent.remainingDistance <= myNavMeshAgent.stoppingDistance)
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
