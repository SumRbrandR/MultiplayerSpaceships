using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LaserV3 : NetworkBehaviour
{
    [SerializeField]
    ParticleSystem hitSparks;
    [SerializeField]
    Transform raycastPoint;

    /// <summary>
    /// Direction to travel.
    /// </summary>
    private Vector3 _direction;
    /// <summary>
    /// Distance remaining to catch up. This is calculated from a passed time and move rate.
    /// </summary>
    private float _passedTime = 0f;
    /// <summary>
    /// In this example the projectile moves at a flat rate of 5f.
    /// </summary>
    [SerializeField]
    private float MOVE_RATE = 5f;

    [SerializeField]
    AudioSource whirSource;

    /// <summary>
    /// Initializes this projectile.
    /// </summary>
    /// <param name="direction">Direction to travel.</param>
    /// <param name="passedTime">How far in time this projectile is behind te prediction.</param>
    public void Initialize(Vector3 direction, float passedTime, Color laserColor, NetworkConnection LocalConnection)
    {
        GetComponentInChildren<Renderer>().material.SetColor("_EmissionColor", laserColor * 80);

        whirSource.pitch = Random.Range(0.8f, 1f);

        _direction = direction;
        _passedTime = passedTime;
        GetComponent<NetworkObject>().SetLocalOwnership(LocalConnection);

    }

    private void Awake()
    {
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        Move();
        Debug.DrawLine(raycastPoint.transform.position, transform.position, Color.green, 5);

    }

    /// <summary>
    /// Move the projectile each frame. This would be called from Update.
    /// </summary>
    private void Move()
    {
        //Frame delta, nothing unusual here.
        float delta = Time.deltaTime;

        //See if to add on additional delta to consume passed time.
        float passedTimeDelta = 0f;
        if (_passedTime > 0f)
        {
            /* Rather than use a flat catch up rate the
             * extra delta will be based on how much passed time
             * remains. This means the projectile will accelerate
             * faster at the beginning and slower at the end.
             * If a flat rate was used then the projectile
             * would accelerate at a constant rate, then abruptly
             * change to normal move rate. This is similar to using
             * a smooth damp. */

            /* Apply 8% of the step per frame. You can adjust
             * this number to whatever feels good. */
            float step = (_passedTime * 0.08f);
            _passedTime -= step;

            /* If the remaining time is less than half a delta then
             * just append it onto the step. The change won't be noticeable. */
            if (_passedTime <= (delta / 2f))
            {
                step += _passedTime;
                _passedTime = 0f;
            }
            passedTimeDelta = step;
        }

        //Move the projectile using moverate, delta, and passed time delta.
        transform.position += _direction * (MOVE_RATE * (delta + passedTimeDelta));
    }
    /// <summary>
    /// Handles collision events.
    /// </summary>
    private void OnTriggerEnter(Collider collision)
    {

        if (collision.GetComponentInParent<LaserV3>() != null)
        {
            // Debug.Log("Not continuing: hit laser");
            return;
        }
        else if (collision.TryGetComponent<NetworkObject>(out NetworkObject nob))
        {
            if (nob.Owner == GetComponent<NetworkObject>().Owner)
            {
                //  Debug.Log("Not continuing: owner of target");
                return;
            }
            /*else
            {
            Debug.Log("Continuing: Not owner");

            }
        }
        else
        {
            Debug.Log("Continuing: good target");
*/
        }

        /* These projectiles are instantiated locally, as in,
         * they are not networked. Because of this there is a very
         * small chance the occasional projectile may not align with
         * 100% accuracy. But, the differences are generally
         * insignifcant and will not affect gameplay. */
        //If client show visual effects, play impact audio.
        if (InstanceFinder.IsClient)
        {
            /*if(collision.TryGetComponent<ShipPart>(out ShipPart sp))
            {
                if(sp.damageHudCounterpart!=null && sp.damageHudCounterpart.TryGetComponent<DamageHologram>(out DamageHologram dh))
                {
                    dh.UpdateCounterpart();
                }
            }*/
           /* if (collision.gameObject.TryGetComponent<MainBody>(out MainBody body))
            {
                print("hit");
              //  body.TryActivateCamera();
            }*/


            if (Physics.Linecast(transform.position, raycastPoint.transform.position, out RaycastHit hit))
            {
                //collision.transform.root.GetComponentInChildren<DamageHologram>()?.ChangeCounterpartColor(collision.GetComponent<ShipPart>().damageHudCounterpart, collision.GetComponent<ShipPart>());

                Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Vector3 pos = hit.point;
                Instantiate(hitSparks, pos, rot);
            }


        }

        //If server check to damage hit objects.
        if (InstanceFinder.IsServer)
        {
            if (collision.gameObject.TryGetComponent<ShipPart>(out ShipPart ps))
            {
                ps.hitPoints -= 12;
                print(ps.name);
               // ps.DestroyIfDead();

            }
        }

        



        //Destroy projectile (probably pool it instead).
        Destroy(gameObject);
    }

}
