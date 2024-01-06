using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMechanic : MonoBehaviour
{
    private FirstPersonController firstPersonController;
    private StarterAssetsInputs _input;
    private CharacterController characterController;
    public LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleavle;
    public Transform gunTip, cameraTip, player;
    private float maxGrappleDistance = 100f;
    private SpringJoint joints;
    public Rigidbody rb;
    public bool isSwinging;
    private bool wasSwingingLastFrame;


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
            characterController.enabled = false;
            gameObject.AddComponent<Rigidbody>();
            rb = gameObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
        else if (!_input.swing && wasSwingingLastFrame)
        {
            isSwinging = false;
            StopGrapple();
        }
        wasSwingingLastFrame = _input.swing;

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
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }
    private void StopGrapple()
    {
        lr.positionCount = 0;
        Destroy(joints);
        if (rb != null)
        {
            Destroy(rb);
        }
        characterController.enabled = true;
    }
}
