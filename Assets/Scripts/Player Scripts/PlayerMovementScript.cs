﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class PlayerMovementScript : MonoBehaviour
{
    [SerializeField]
    float jumpSpeed = 10f;
    [SerializeField]
    float maxJumpHeight = 2f;
    [SerializeField]
    float movementSpeed = 1f;
    [SerializeField]
    float smoothRotationSpeed;
    //[SerializeField]
    //float turnSpeed = 100f;
    [SerializeField]
    float dashSpeed;
    [SerializeField]
    float dashTimer = 0f;
    [SerializeField]
    float dashTimerMax;

    internal RaycastHit hitInfo;

    //float maxGroundAngle = 120f;
    [SerializeField]
    float groundAngle;
    [SerializeField]
    LayerMask groundLayer;
    [SerializeField]
    Vector3 calculatedForward;
    [SerializeField]
    float height = 1.4f;
    //float heightPadding = 0.05f;
    [SerializeField]
    bool isGrounded;

    [SerializeField]
    internal PlayerScript playerScript;

    [SerializeField]
    internal Rigidbody playerRB;
    // Start is called before the first frame update

    void Awake()
    {
        groundLayer = LayerMask.GetMask("Ground");
        playerScript = GetComponent<PlayerScript>();
        playerRB = GetComponent<Rigidbody>();
        calculatedForward = playerRB.transform.forward;
        //init constants
        dashTimerMax = .25f;
        dashSpeed = 15f;
        smoothRotationSpeed = 25f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       DrawDebugLines();
    }

    internal void GroundMovement(Vector2 movementVector2)
    {
        calculatedForward = PlayerHelperFunctions.CalculateForward(playerRB, height, hitInfo, groundLayer);
        float forwardMovement = movementSpeed * movementVector2.magnitude * Time.fixedDeltaTime;
        //Vector3 movementVector = new Vector3(movementVector2.x, 0f, movementVector2.y);
        //move player
        //transform.Translate(movementVector * forwardMovement, Space.World);
        //playerRB.MovePosition(transform.position+(movementVector*forwardMovement));
        playerRB.MovePosition(transform.position+(calculatedForward * forwardMovement));
        //Debug.Log(calculatedForward);
    }


    internal void Strafe(Vector2 movementVector2)
    {
        //Essentially, this is the groundmovement method that doesnt move in the forward direction
        float fowardMovement = movementSpeed * movementVector2.magnitude * Time.fixedDeltaTime;

        //make it relative to camera
        Vector3 directionVector = HelperFunctions.ConvertMoveInputToCam(movementVector2, playerScript.GetCurrentCamera().transform);
        playerRB.MovePosition(transform.position + (directionVector * fowardMovement));
    }

    internal void SmoothPlayerRotation(Vector2 movementVector)
    {
        //We need to rotate the movement vector to ensure that it's pointing in the direction of the camera
        //use y axis
        Vector3 directionVector = HelperFunctions.ConvertMoveInputToCam(movementVector, playerScript.GetCurrentCamera().transform);

        // The step size is equal to speed times frame time.
        var step = 10 * Time.deltaTime;

        //Rotate player, so long as the vector isn't 0. If it's zero, it just resets to facing in the default.
        if (movementVector.x != 0f || movementVector.y != 0f)
        {
            //playerRB.MoveRotation(Quaternion.LookRotation(directionVector));
            playerRB.rotation = Quaternion.Slerp(playerRB.rotation, Quaternion.LookRotation(directionVector), 15f * Time.deltaTime);
        }

        //Debug.Log("debug transform forward"+playerTransform.forward);
    }

    internal void StrafeSmoothPlayerRotation(Vector2 movementVector)
    {
        //We need to rotate the movement vector to ensure that it's pointing in the direction of the camera
        //use y axis
        Vector3 directionVector = HelperFunctions.ConvertMoveInputToCam(movementVector, playerScript.GetCurrentCamera().transform);

        // The step size is equal to speed times frame time.
        var step = 10 * Time.deltaTime;

        //Rotate player, so long as the vector isn't 0. If it's zero, it just resets to facing in the default.
        if (movementVector.x != 0f || movementVector.y != 0f)
        {
            //playerRB.MoveRotation(Quaternion.LookRotation(directionVector));
            playerRB.rotation = Quaternion.Slerp(playerRB.rotation, Quaternion.LookRotation(directionVector), 2f * Time.deltaTime);
        }

        //Debug.Log("debug transform forward"+playerTransform.forward);
    }


    internal float Jump(Vector3 startPosition, float startTime, Vector2 movementInput)
    {
        float distCovered = (Time.time - startTime) * jumpSpeed;
        float fractionOfJourney = distCovered / maxJumpHeight;
        //we calculate a new start position to determine if the player has moved from the original start position
        Vector3 newStartPosition = new Vector3(transform.position.x, startPosition.y, transform.position.z);
        Vector3 endPosition = new Vector3(newStartPosition.x, newStartPosition.y + maxJumpHeight, newStartPosition.z);

        transform.position = Vector3.Lerp(newStartPosition, endPosition, fractionOfJourney);
       // playerRB.MovePosition(endPosition);
        //Allow the player to move in the air because it's fun
        SmoothPlayerRotation(movementInput);

        //move player
        calculatedForward = PlayerHelperFunctions.CalculateForward(playerRB, height, hitInfo, groundLayer);
        float forwardMovement = movementSpeed * movementInput.magnitude * Time.deltaTime;
        transform.Translate(calculatedForward*forwardMovement, Space.World);
        //GroundMovement(movementInput);
        /*Debug.Log("jumping movementvector: "+movementVector);
        Debug.Log("jumping speed: "+forwardMovement);
        Debug.Log("jumping combined: "+movementVector * forwardMovement);*/
        //return progress so that we know when the player is done jumping
        if (PlayerHitCeiling())
        {
            fractionOfJourney = 1.1f;
        }
        return fractionOfJourney;
    }

    /*internal void CalculateForward()
    {
        if (Physics.Raycast(new Vector3(playerRB.transform.position.x, playerRB.transform.position.y - height / 1.5f, playerRB.transform.position.z), -playerRB.transform.up, out hitInfo, 1f, groundLayer))
        {
            calculatedForward = Vector3.Cross(playerRB.transform.right, hitInfo.normal);
        }
        else
        {
            calculatedForward = playerRB.transform.forward;
        }
    }*/

    internal bool PlayerHitCeiling()
    {
        bool hit = false;
        if (Physics.Raycast(new Vector3(playerRB.transform.position.x, playerRB.transform.position.y + height / 1.5f, playerRB.transform.position.z), playerRB.transform.up, out hitInfo, .2f))
        {
            hit = true;
        }
        return hit;
    }
    /*
    internal void CalculateGroundAngle()
    {
        if (isGrounded)
        {
            groundAngle = 90f;
        }
        else
        {
            groundAngle = Vector3.Angle(hitInfo.normal, transform.forward);
        }
    }

    internal void CheckGround()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out hitInfo, height + heightPadding, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
    */
    void DrawDebugLines()
    {
        Debug.DrawLine(transform.position, transform.position - Vector3.up * height * 2, Color.green);
        Debug.DrawLine(transform.position, transform.position+calculatedForward, Color.blue);
        Debug.DrawLine(transform.position, transform.position+playerRB.transform.forward, Color.red);
        Debug.DrawLine(transform.position, transform.position+playerRB.transform.right, Color.white);
        Debug.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal, Color.black);
    }

   /* private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(new Vector3(playerRB.transform.position.x, playerRB.transform.position.y - height/1.5f, playerRB.transform.position.z), .2f);
    } */
}
