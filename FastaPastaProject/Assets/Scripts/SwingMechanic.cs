using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMechanic : MonoBehaviour
{
    public float swingSpeed = 2f;
    private FirstPersonController firstPersonController;
    private StarterAssetsInputs _input;
    private CharacterController characterController;
    public LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleavle;
    public Transform gunTip, cameraTip, player;
    private float maxGrappleDistance = 100f;
    private SpringJoint joints;
    private Rigidbody rb;
    public bool isSwinging;
    private bool wasSwingingLastFrame;
    private float speedToTransform;
    private Vector3 currentGrapplePosition;
    public Transform orientation;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    private bool isApplayed = false;

    private Vector3 _storedRigidbodyVelocity;


    private void Start()
    {
        firstPersonController = gameObject.GetComponent<FirstPersonController>();
        _input = gameObject.GetComponent<StarterAssetsInputs>();
        characterController = gameObject.GetComponent<CharacterController>();

        wasSwingingLastFrame = false;

    }

    private void Update()
    {

        if (_input.swing && !wasSwingingLastFrame)
        {
            isSwinging = true;
            StartGrapple();

        }
        else if (!_input.swing && wasSwingingLastFrame && isSwinging)
        {
            isSwinging = false;
            StopGrapple();

        }
        if (isApplayed)
        {
            firstPersonController.ApplyStoredVelocity(_storedRigidbodyVelocity);
            isApplayed = false;
        }
        wasSwingingLastFrame = _input.swing;
//        if (joints != null) AirControll();
        if (firstPersonController._speed >= 15)
        {
            firstPersonController._speed = 15;
        }
    }
    private void LateUpdate()
    {
        DrawRope();
    }
    private void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTip.position, cameraTip.forward, out hit, maxGrappleDistance, whatIsGrappleavle))
        {
            characterController.enabled = false;
            gameObject.AddComponent<Rigidbody>();
            rb = gameObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            speedToTransform = firstPersonController._speed;
            Vector3 directionOfMovement = transform.forward;
            rb.velocity = directionOfMovement * (speedToTransform * swingSpeed);

            grapplePoint = hit.point;
            joints = player.gameObject.AddComponent<SpringJoint>();
            joints.autoConfigureConnectedAnchor = false;
            joints.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            joints.maxDistance = distanceFromPoint * 0.8f;
            joints.minDistance = distanceFromPoint * 0.25f;

            joints.spring = 4.5f;
            joints.damper = 7f;
            joints.massScale = 4.5f;

            lr.positionCount = 2;
        }
    }

    private void DrawRope()
    {
        if (!joints) return;

        currentGrapplePosition = Vector3.Lerp(grapplePoint, grapplePoint, Time.deltaTime * 4f);
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }
    private void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joints);
        if (rb != null)
        {
            _storedRigidbodyVelocity = rb.velocity /4f;
            Destroy(rb);
            isApplayed = true;
        }
        characterController.enabled = true;
        firstPersonController.ResetVerticalVelocity();


    }

  //  private void AirControll()
   // {
   //     // right
   //     if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        // left
      //  if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);

        //if (Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * horizontalThrustForce * Time.deltaTime);
        // extend cable
      //  if (Input.GetKey(KeyCode.S))
      //  {
       //     float extendedDistanceFromPoint = Vector3.Distance(transform.position, grapplePoint) + extendCableSpeed;

       //     joints.maxDistance = extendedDistanceFromPoint * 0.8f;
       //     joints.minDistance = extendedDistanceFromPoint * 0.25f;
       // }
   // }

    public bool IsGrappling()
    {
        return joints != null;
    }
    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}
