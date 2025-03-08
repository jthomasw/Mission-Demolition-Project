using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Nuke : MonoBehaviour
{
    const int LOOKBACK_COUNT = 10;
    static List<Nuke> NUKES = new List<Nuke>();

    [SerializeField] private bool _awake;
    public bool awake
    {
        get { return _awake; }
        private set { _awake = value; }
    }

    private AudioSource audioSource;
    private Vector3 prevPos;
    private List<float> deltas = new List<float>();
    private Rigidbody rigid;
    private SphereCollider triggerCollider;
    public AudioClip clip;
    

    public float expansionSpeed = 20f; // How fast it grows after impact
    public float maxSize = 5f; // Maximum size before disappearing
    public float pushForce = 50f; // Force applied to nearby objects
    private bool hasExploded = false; // Tracks if the explosion has started

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rigid = GetComponent<Rigidbody>();
        rigid.isKinematic = false; // Allow movement
       
        // Add and configure trigger collider
        triggerCollider = GetComponent<SphereCollider>();
        triggerCollider.isTrigger = false; // **Important: Collider should be solid at first**

        awake = true;
        prevPos = transform.position;
        deltas.Add(1000);
        NUKES.Add(this);
    }

    void FixedUpdate()
    {
        if (!awake || hasExploded) return; // Stop tracking movement after explosion

        Vector3 deltaV3 = transform.position - prevPos;
        deltas.Add(deltaV3.magnitude);
        prevPos = transform.position;

        while (deltas.Count > LOOKBACK_COUNT)
        {
            deltas.RemoveAt(0);
        }

        float maxDelta = 0;
        foreach (float f in deltas)
        {
            if (f > maxDelta) maxDelta = f;
        }

        if (maxDelta <= Physics.sleepThreshold)
        {
            awake = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return; // Prevent multiple triggers

        Debug.Log($"Nuke hit: {collision.gameObject.name}");

        // Change collider to trigger mode so it expands without interfering with physics
        triggerCollider.isTrigger = true;

        // Apply force to nearby objects
        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();
        if (otherRb != null)
        {
            Vector3 forceDirection = collision.transform.position - transform.position;
            forceDirection.Normalize();
            otherRb.AddForce(forceDirection * pushForce, ForceMode.Impulse);
        }

        // Start explosion effect
        hasExploded = true;
        audioSource.PlayOneShot(clip);
        StartCoroutine(ExpandAndDestroy());
    }

    private IEnumerator ExpandAndDestroy()
    {
        float currentSize = triggerCollider.radius;

        while (currentSize < maxSize)
        {
            currentSize += expansionSpeed * Time.deltaTime;
            triggerCollider.radius = currentSize;
            yield return null;
        }

        // Make the GameObject invisible by changing its alpha to 0
        Renderer renderer = GetComponent<Renderer>(); // Get the Renderer component
        if (renderer != null)
        {
            Color color = renderer.material.color; // Get the current color of the material
            color.a = 0f; // Set the alpha value to 0 (fully transparent)
            renderer.material.color = color; // Apply the new color back to the material
        }

        yield return new WaitForSeconds(1);
        Destroy(gameObject); // Remove nuke after full expansion
    }

    private void OnDestroy()
    {
        NUKES.Remove(this);
    }

    public static void DESTROY_PROJECTILES()
    {
        foreach (Nuke n in NUKES)
        {
            Destroy(n.gameObject);
        }
    }
}

