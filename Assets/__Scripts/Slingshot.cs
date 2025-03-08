using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;

public class Slingshot : MonoBehaviour
{
    [SerializeField] private LineRenderer rubber;

    [SerializeField] private Transform firstPoint;

    [SerializeField] private Transform secondPoint;

    //Vector3 rubberMiddle = new Vector3(-10.5f, -7.12f, -0.2f);

    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public GameObject nukePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;
    

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public GameObject nuke;
    public bool aimingMode;
    public string ammo = "projectile";

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;
        rubber.SetPosition(0, firstPoint.position);
        rubber.SetPosition(2, secondPoint.position);
        
    }

    public void Hiroshima() //my girlfriend named it this lol
    {
        if (ammo == "projectile")
        {
            ammo = "nuclear";
        }
        else
        {
            ammo = "projectile";
        }

        Debug.Log($"Switched ammo to: {ammo}");
    }

    void OnMouseEnter()
    {
        //print("Slingshot:OnMouseEnter()");
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        //print("Slingshot:OnMouseExit()");
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true;
        switch (ammo)
        {
            case "projectile":
                projectile = Instantiate(projectilePrefab, launchPos, Quaternion.identity) as GameObject;
                projectile.transform.position = launchPos;
                projectile.GetComponent<Rigidbody>().isKinematic = true;
                
                break;
            case "nuclear":
                nuke = Instantiate(nukePrefab, launchPos, Quaternion.identity) as GameObject;
                nuke.transform.position = launchPos;
                nuke.GetComponent<Rigidbody>().isKinematic = true;
                
                break;
        }

    }

    void Update()
    {
        if (!aimingMode) return;

        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        Vector3 mouseDelta = mousePos3D - launchPos;

        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        Vector3 projPos = launchPos + mouseDelta;

        if (ammo == "projectile" && projectile != null)
        {
            projectile.transform.position = projPos;
        }
        else if (ammo == "nuclear" && nuke != null)
        {
            nuke.transform.position = projPos;
        }

        if (Input.GetMouseButton(0))
        {
            rubber.SetPosition(1, projPos);
        }
        if (Input.GetMouseButtonUp(0))
        {
            aimingMode = false;

            if (ammo == "projectile" && projectile != null)
            {
                LaunchProjectile(projectile, mouseDelta);
            }
            else if (ammo == "nuclear" && nuke != null)
            {
                LaunchProjectile(nuke, mouseDelta);
            }
        }
    }

    private void LaunchProjectile(GameObject proj, Vector3 direction)
    {
        Rigidbody projRB = proj.GetComponent<Rigidbody>();
        projRB.isKinematic = false;
        projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
        projRB.linearVelocity = -direction * velocityMult;


        FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
        FollowCam.POI = proj;

        Instantiate(projLinePrefab, proj.transform);

        MissionDemolition.SHOT_FIRED();

        projectile = null;
        nuke = null;
    }


}
