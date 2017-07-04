using System;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class Cowboy : MonoBehaviour, IInputClickHandler
{

    public float WalkSpeed = 1f;
    public float MarginOfError = .1f;
    public float TimeToTurn = .75f;

    private float _timeStartedLerping;
    private float _ground;

    private bool _turning;
    private Quaternion _startRotation;
    private Quaternion _targetRotation;

    private bool _walking;
    private float _timeToWalk;
    private Vector3 _start;
    private Vector3 _destination;

    private Animator _anim;
    private Rigidbody _rigidbody;

    private void Start()
    {
        InputManager.Instance.AddGlobalListener(gameObject);

        _rigidbody = GetComponent<Rigidbody>();
        _ground = transform.position.y;        
    }

    void LateUpdate()
    {
        if (_turning)
        {
            if (TurnForFrame(_startRotation))
            {
                StopTurning();
                StartWalking();
            }
        }

        if (_walking)
        {
            if (MoveForFrame(_destination))
            {
                StopWalking();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            // ignore collisions in the Y direction
            if (!AlmostEquals(contact.point.y, _ground, MarginOfError))
            {
                StopWalking();
                _rigidbody.freezeRotation = true;
            }
        }
    }

    public void MoveCharacterToPoint(Vector3 newLoc)
    {
        if (_anim == null) _anim = gameObject.GetComponent<Animator>();

        // Don't let the cowboy leave the plane that he is on
        newLoc.y = _ground;
        _destination = newLoc;

        StartTurning(newLoc);
    }

    private void StartWalking()
    {
        _start = transform.position;
        _timeStartedLerping = Time.time;
        _timeToWalk = Vector3.Distance(_start, _destination) / WalkSpeed;

        _walking = true;
        _anim.SetFloat("Forward", 1);
    }

    private void StopWalking()
    {
        _walking = false;
        _anim.SetFloat("Forward", 0);
    }

    private bool MoveForFrame(Vector3 destination)
    {
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / _timeToWalk;

        _rigidbody.MovePosition(Vector3.Lerp(_start, destination, percentageComplete));

        if (percentageComplete >= 1.0f)
        {
            // done turning
            return true;
        }

        return false;
    }

    private void StartTurning(Vector3 locationToFace)
    {
        _startRotation = transform.rotation;
        _targetRotation = Quaternion.LookRotation(locationToFace - transform.position);
        _timeStartedLerping = Time.time;

        _anim.SetFloat("Turn", 1);
        _turning = true;
    }

    private void StopTurning()
    {
        _anim.SetFloat("Turn", 0);
        _turning = false;
    }

    private bool TurnForFrame(Quaternion startRotation)
    {
        float timeSinceStarted = Time.time - _timeStartedLerping;
        float percentageComplete = timeSinceStarted / TimeToTurn;

        transform.rotation = Quaternion.Slerp(startRotation, _targetRotation, percentageComplete);

        if (percentageComplete >= 1.0f)
        {
            // done turning
            return true;
        }

        return false;
    }

    private static bool AlmostEquals(float double1, float double2, float precision)
    {
        return Math.Abs(double1 - double2) <= precision;
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        MoveCharacterToPoint(GazeManager.Instance.HitPosition);
    }
}
