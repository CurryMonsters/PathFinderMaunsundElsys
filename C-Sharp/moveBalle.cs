using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Globalization;
using UnityEngine.AI;
using System.Numerics;
using System.Diagnostics.Tracing;
using UnityEditor.Callbacks;

[RequireComponent(typeof(LineRenderer))]
public class moveBalle : MonoBehaviour
{
    public NavMeshAgent myNavMeshAgent;
    public UDPReceive udpReceive;
    public GameObject destinationMarker;
    public GameObject mainCamera;
    private LineRenderer myLineRenderer;
    public float stock = 100;
    public UnityEngine.Vector3 desPos;

    public GameObject sonarVFXPrefab;
    public GameObject boatVisuals;
    public GameObject foamObj;
    ParticleSystem.EmissionModule foamSystem;
    public float foamMultiplier = 10;
    public float sonarDuration;
    public float sonarRange;
    public float pathHeight = 3f;
    public bool isMoveAble = true;
    bool drawnPath = false;
    int navHasPath = 0;
    UnityEngine.Vector3 cameraPos;
    UnityEngine.Quaternion cameraRot;

    // Start is called before the first frame update
    void Start()
    {
        myNavMeshAgent = GetComponent<NavMeshAgent>();
        myLineRenderer = GetComponent<LineRenderer>();
        myLineRenderer.positionCount = 0;
        myLineRenderer.startWidth = 0.15f;
        myLineRenderer.endWidth = 0.15f;
        foamSystem = foamObj.GetComponent<ParticleSystem>().emission;
        cameraPos = mainCamera.transform.position;
        cameraRot = mainCamera.transform.rotation;
    }
    

    // Den henter nye koordinater fra UDP-mottaket,
    // oppdaterer agentens posisjon og rotasjon, sjekker om en path eksisterer, og tegner den ved behov.
    void Update()
    {
        string[] myCords = udpReceive.cords;
        float posX = float.Parse(myCords[0], CultureInfo.InvariantCulture.NumberFormat) * stock;
        float posY = float.Parse(myCords[1], CultureInfo.InvariantCulture.NumberFormat) * stock;
        
        if (isMoveAble)
        {
            desPos = new UnityEngine.Vector3(posX, 1, posY);
            destinationMarker.transform.position = desPos;

            float desRot = float.Parse(myCords[2], CultureInfo.InvariantCulture.NumberFormat);
            destinationMarker.transform.rotation = UnityEngine.Quaternion.Euler(0, desRot * Mathf.Rad2Deg, 0);
            
            myNavMeshAgent.ResetPath();

            float myPosX = float.Parse(myCords[3], CultureInfo.InvariantCulture.NumberFormat) * stock;
            float myPosY = float.Parse(myCords[4], CultureInfo.InvariantCulture.NumberFormat) * stock;
            float rot = float.Parse(myCords[5], CultureInfo.InvariantCulture.NumberFormat);

            transform.rotation = UnityEngine.Quaternion.Euler(0, rot * Mathf.Rad2Deg, 0);
            transform.position = new UnityEngine.Vector3(myPosX, 1, myPosY);
            mainCamera.transform.position = cameraPos;
            mainCamera.transform.rotation = cameraRot;
            mainCamera.GetComponent<CameraOrbiter>().rotateSpeed = 0;
        }

        if (myNavMeshAgent.hasPath)
        {
            navHasPath = 1;
        }
        else
        {
            navHasPath = 0;
        }

        if (myNavMeshAgent.hasPath && drawnPath == false)
        {
            DrawPath(); // Tegner pathen når agenten har en path å følge.
            drawnPath = true;
        }
        foamSystem.rateOverTime = navHasPath * foamMultiplier + 30;
    }

    public void startSim()
    {
        // Starter simuleringen ved å deaktivere fri bevegelse øyer, båten og målet og setter destinasjonen for NavMeshAgent.
        isMoveAble = false;
        myNavMeshAgent.SetDestination(desPos);
        mainCamera.GetComponent<CameraOrbiter>().rotateSpeed = 6;

        if (!myNavMeshAgent.hasPath) drawnPath = false;
    }

    public void resimulate()
    {
        // Reaktiverer fri bevegelse og tilbakestiller pathen hvis agenten har nådd målet eller gitt opp.
        if (ReachedDestinationOrGaveUp())
        {
            isMoveAble = true;
            myLineRenderer.positionCount = 0;
        }
    }

    public bool ReachedDestinationOrGaveUp()
    {
        // Sjekker om NavMeshAgent har nådd målet eller gitt opp navigasjonen.
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

    //visuell representasjon av pathen til båten
    void DrawPath()
    {
        // Tegner pathen som NavMeshAgent følger.
        int pathLength = myNavMeshAgent.path.corners.Length;
        myLineRenderer.positionCount = pathLength; // Setter antall punkter i linjen.
        myLineRenderer.SetPosition(0, transform.position); // Startpunkt for linjen.

        if (pathLength < 2) return;

        for (int i = 0; i < pathLength; i++)
        {
            // Plasserer hvert punkt langs pathen
            UnityEngine.Vector3 pointPos = new UnityEngine.Vector3(
                myNavMeshAgent.path.corners[i].x,
                myNavMeshAgent.path.corners[i].y + pathHeight,
                myNavMeshAgent.path.corners[i].z);
            myLineRenderer.SetPosition(i, pointPos);
        }
    }
}
