//===========================================================================//
//                       FreeFlyCamera (Version 1.2)                         //
//                        (c) 2019 Sergey Stafeyev                           //
//===========================================================================//
//                                                                           //
//                Slightly modified to fit Buildwise's needs.                //
//                                                                           //
//===========================================================================//

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Buildwise.Core
{
    [RequireComponent(typeof(Camera))]
    public class FreeFlyCamera : MonoBehaviour
    {
        #region UI

        [Space]

        [SerializeField]
        [Tooltip("The script is currently active")]
        private bool _active = true;

        [Space]

        [SerializeField]
        [Tooltip("Camera rotation by mouse movement is active")]
        private bool _enableRotation = true;

        [SerializeField]
        [Tooltip("Sensitivity of mouse rotation")]
        private float _mouseSense = 0.3f;

        [Space]

        [SerializeField]
        [Tooltip("Camera zooming in/out by 'Mouse Scroll Wheel' is active")]
        private bool _enableTranslation = true;

        [SerializeField]
        [Tooltip("Velocity of camera zooming in/out")]
        private float _translationSpeed = 55f;

        [Space]

        [SerializeField]
        [Tooltip("Camera movement by 'W','A','S','D','Q','E' keys is active")]
        private bool _enableMovement = true;

        [SerializeField]
        [Tooltip("Camera movement speed")]
        private float _movementSpeed = 10f;

        [SerializeField]
        [Tooltip("Speed of the quick camera movement when holding the 'Left Shift' key")]
        private float _boostedSpeed = 50f;

        [Space]

        [SerializeField]
        [Tooltip("Acceleration at camera movement is active")]
        private bool _enableSpeedAcceleration = true;

        [SerializeField]
        [Tooltip("Rate which is applied during camera movement")]
        private float _speedAccelerationFactor = 1.5f;

        #endregion UI

        private CursorLockMode _wantedMode;
        private bool _hasEscBeenPressedRecently = false;

        private float _currentIncrease = 1;
        private float _currentIncreaseMem = 0;

        private Vector3 _initPosition;
        private Vector3 _initRotation;

        private float previousScroll = 0;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_boostedSpeed < _movementSpeed)
                _boostedSpeed = _movementSpeed;
        }
#endif

        BWControls inputActions;

        private void Start()
        {
            _initPosition = transform.position;
            _initRotation = transform.eulerAngles;

            inputActions = new BWControls();
            inputActions.InGame.Enable();
            inputActions.InGame.escape.performed += SetCursorState;
            inputActions.InGame.reset_position.performed += ResetPosition;
        }

        private void ResetPosition(InputAction.CallbackContext context)
        {
            transform.position = _initPosition;
            transform.eulerAngles = _initRotation;
        }

        private void SetCursorState(InputAction.CallbackContext context)
        {
            if (_hasEscBeenPressedRecently)
            {
                _wantedMode = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = _wantedMode = CursorLockMode.None;
                _hasEscBeenPressedRecently = true;
                StartCoroutine(RecordEscKeyForAMoment());
            }

            // Apply cursor state
            Cursor.lockState = _wantedMode;
            // Hide cursor when locking
            Cursor.visible = CursorLockMode.Locked != _wantedMode;
        }

        private void OnEnable()
        {
            if (_active)
                _wantedMode = CursorLockMode.None;
        }

        // Apply requested cursor state
        /*
        private void SetCursorState()
        {

            if (Input.GetKeyDown(KeyCode.Escape) && _hasEscBeenPressedRecently)
            {
                _wantedMode = CursorLockMode.Locked;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = _wantedMode = CursorLockMode.None;
                _hasEscBeenPressedRecently = true;
                StartCoroutine(RecordEscKeyForAMoment());
            }

            // Apply cursor state
            Cursor.lockState = _wantedMode;
            // Hide cursor when locking
            Cursor.visible = CursorLockMode.Locked != _wantedMode;
        }
        */

        private IEnumerator RecordEscKeyForAMoment()
        {
            yield return new WaitForSeconds(0.5f);
            _hasEscBeenPressedRecently = false;
        }

        private void CalculateCurrentIncrease(bool moving)
        {
            _currentIncrease = Time.deltaTime;

            if (!_enableSpeedAcceleration || _enableSpeedAcceleration && !moving)
            {
                _currentIncreaseMem = 0;
                return;
            }

            _currentIncreaseMem += Time.deltaTime * (_speedAccelerationFactor - 1);
            _currentIncrease = Time.deltaTime + Mathf.Pow(_currentIncreaseMem, 3) * Time.deltaTime;
        }

        private void Update()
        {
            if (!_active)
                return;

            //SetCursorState();

            /*
            if (Cursor.visible)
                return;
            */
            // Translation
            if (_enableTranslation)
            {
                float currentScroll = inputActions.InGame.zoom.ReadValue<float>();
                float scrollDelta = currentScroll - previousScroll;
                transform.Translate(Vector3.forward * scrollDelta * Time.deltaTime * _translationSpeed);
                //transform.Translate(Vector3.forward * Input.mouseScrollDelta.y * Time.deltaTime * _translationSpeed);
                previousScroll = currentScroll;
            }

            // Movement
            if (_enableMovement)
            {
                Vector3 deltaPosition = Vector3.zero;
                float currentSpeed = _movementSpeed;

                Vector2 moveInput = inputActions.InGame.move.ReadValue<Vector2>();
                float verticalMoveInput = inputActions.InGame.move_vertical.ReadValue<float>();

                if (inputActions.InGame.boost_speed.IsPressed())
                    currentSpeed = _boostedSpeed;

                if (moveInput.y > 0)
                    deltaPosition += transform.forward;

                if (moveInput.y < 0)
                    deltaPosition -= transform.forward;

                if (moveInput.x < 0)
                    deltaPosition -= transform.right;

                if (moveInput.x > 0)
                    deltaPosition += transform.right;

                if (verticalMoveInput > 0)
                    deltaPosition += transform.up;

                if (verticalMoveInput < 0)
                    deltaPosition -= transform.up;

                // Calc acceleration
                CalculateCurrentIncrease(deltaPosition != Vector3.zero);

                transform.position += deltaPosition * currentSpeed * _currentIncrease;
            }

            // Rotation
            if (_enableRotation)
            {
                Vector2 pointerPositionDelta = inputActions.InGame.point_delta.ReadValue<Vector2>();
                // Pitch
                /*
                transform.rotation *= Quaternion.AngleAxis(
                    -Input.GetAxis("Mouse Y") * _mouseSense,
                    Vector3.right
                );*/
                transform.rotation *= Quaternion.AngleAxis(
                    -pointerPositionDelta.y * _mouseSense,
                    Vector3.right
                );

                // Paw
                /*
                transform.rotation = Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y + Input.GetAxis("Mouse X") * _mouseSense,
                    transform.eulerAngles.z
                );
                */
                transform.rotation = Quaternion.Euler(
                    transform.eulerAngles.x,
                    transform.eulerAngles.y + pointerPositionDelta.x * _mouseSense,
                    transform.eulerAngles.z
                );
            }

            // Return to init position
            /*
            if (Input.GetKeyDown(_initPositonButton))
            {
                transform.position = _initPosition;
                transform.eulerAngles = _initRotation;
            }
            */
        }
    }
}