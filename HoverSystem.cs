using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class HoverSystem : MonoBehaviour
{
    //Public fields
    public bool IsDead;//for managing spawn points/check points
    public GameObject model;
    public float maxHoverDistance;
    public float groundCheckDistance;
    public float groundRotationDistance;
    public LayerMask layerMask;
    public float hoverForce;
    public ForceMode hoverForceMode = ForceMode.Acceleration;
    public Transform[] hoverPoints = new Transform[4];
    public float autoBalancingTime = 1f;//the time it takes the vehicle to balance itself on the x and z axis (the lesser, the faster). 
    public float radius = 0.5f;//the radius of the spherecast we'll use for groundCheck
    [Header("Model base rotation on Y axis")]
    public float modelBaseYRotation = 90f;//due to the bike being set to 90 degrees on the y axis, in order to rotate to properly, you need to make sure the y rotation is always 90 degrees (or 0, if you're using any other models with normal rotation)

    [Header("Artificial gravity")] 
    public ForceMode gravityForce;
    public float maxGravity = 20f;
    public float gravityAccelerationTime = 0.25f;

    //Private fields
    private RaycastHit[] hits = new RaycastHit[4];
    private Rigidbody _rigidBody;
    private RaycastHit _groundHit;//groundCheck raycast
    private float _gravity;
    private bool _isNearGround;//have this parameter for moving, so that player isn't able to fly in the air

    //Properties
    public bool IsNearGround
    {
        get { return _isNearGround; }
    }
    
    //Methods
    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        IsDead = false;
    }
    
    private void Update()
    {
        //Ground check
        if (Physics.Raycast(transform.position, Vector3.down, out _groundHit, groundCheckDistance, layerMask))
        {
            _isNearGround = true;
        }
        else
        {
            _isNearGround = false;
        }
        
        if (!Input.anyKey)//Vehicle auto-balance when nothing is pressed
        {
            //Spherically Interpolate the rotation on x the and z axis to zero, so the bike doesn't constantly roll over 
            if((transform.rotation.x >= 90f || transform.rotation.x <= -90f) || (transform.rotation.z >= 90f || transform.rotation.z <= -90f) )
                Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, transform.rotation.y, 0f), autoBalancingTime * Time.deltaTime);
        }
        
        //Rotation according to player input on ground, when shift is not pressed
        if(!Input.GetKey(KeyCode.LeftShift))//smoother upward driving experience
            ModelRotation();
    }
    
    void FixedUpdate()
    {
        //Apply gravity when not touching the ground
        if (!_isNearGround)
        {
            StartCoroutine(GravityAccelerator());
            _rigidBody.AddForce(Vector3.down * _gravity, gravityForce);
        }
        else
        {
            _gravity = 0;
            GroundRotation();
        }
        
        for (int i = 0; i < 4; i++)//for every hover point, add force to the rigidbody at that hover point's position
            ApplyHoverForce(hoverPoints[i], hits[i]);
    }
     
    private IEnumerator GravityAccelerator()
    {
        if(_gravity < maxGravity)
            _gravity += gravityAccelerationTime;

        yield return new WaitForSeconds(0.1f);
    }

     void ApplyHoverForce(Transform hoverPoint, RaycastHit hit)//if there is ground near the vehicle, this method returns true.
    {
        Debug.DrawRay(hoverPoint.position, -transform.up * maxHoverDistance, Color.yellow);//when hover points don't touch the ground, make yellow DebugRay

        if (Physics.Raycast(transform.position, -transform.up, out hit, maxHoverDistance, layerMask))//raycast for finding ground, done for each hover point separately
        {
            Debug.DrawRay(hoverPoint.position, -transform.up * hit.distance, Color.red);//when hover points do touch the ground, make red DebugRay
            float force = Mathf.Abs(1 / (hit.point.y - hoverPoint.position.y)); //force that keeps the object floating, by increasing the hover force once the hover points come too close to the ground 
            float totalForce = force * hoverForce;
            _rigidBody.AddForceAtPosition(transform.up * totalForce, hoverPoint.position, hoverForceMode);//apply force at the hover points of the rigidbody
        }
    }


     //Method for turning the bike model according to the rotation of the ground beneath it
    void ModelRotation()
    {
        if (Input.GetAxis("Horizontal") > 0f) //  Model Turn Right
        {
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(-35, -modelBaseYRotation, 0), 1 * Time.deltaTime).normalized;
        }
        else
        {
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(0, -modelBaseYRotation, 0), 1 * Time.deltaTime);
        }
        
        if (Input.GetAxis("Horizontal") < 0f) // Model Turn Left
        {
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(35, -modelBaseYRotation, 0), 1 * Time.deltaTime).normalized;
        }
        else
        {
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(0, -modelBaseYRotation, 0), 1 * Time.deltaTime);
        }
        
        if (Input.GetAxis("Vertical") > 0f) // Model Turn Forward
        {
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(0, -modelBaseYRotation, -20), 1 * Time.deltaTime);
        }
        else
        {
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(0, -modelBaseYRotation, 0), 1 * Time.deltaTime);
        }
        
        if (Input.GetAxis("Vertical") < 0f) // Model Turn Backwards
        {
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(0, -modelBaseYRotation, 20), 1 * Time.deltaTime);
        }
        else
        {
            model.transform.localRotation = Quaternion.Lerp(model.transform.localRotation, Quaternion.Euler(0, -modelBaseYRotation, 0), 1 * Time.deltaTime);
        }
    }

    private void GroundRotation()
    {
        RaycastHit hit;
        
        if (Physics.SphereCast(transform.position, radius, -model.transform.up, out hit, groundRotationDistance))
        {
            Vector3 lookAtObject = Vector3.Cross(transform.right, _groundHit.normal);
            if (lookAtObject != Vector3.zero) //we do this in case the vector of the surface we're taking rotation from is not zero, or else we'll get a debug message "Look rotation viewing vector is zero"
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookAtObject, _groundHit.normal), Time.deltaTime * 5.0f);
            }
        }

    }

}
 