using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask countersLayerMask;

    public static Player Instance
    {
        get; private set;
    }

    public event EventHandler<OnSelectedCounterChangeEventArgs> OnSelectedCounterChange;
    public class OnSelectedCounterChangeEventArgs : EventArgs
    {
        public ClearCounter selectedCounter;
    }

    public bool isWalking = false;

    private Vector3 lastInteractDirection;
    private ClearCounter selectedCounter;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more than 1 player instance");
        }
        Instance = this;
    }

    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (selectedCounter != null)
        {
            selectedCounter.Interact();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDirection = new(inputVector.x, 0, inputVector.y);

        if (moveDirection != Vector3.zero)
        {
            lastInteractDirection = moveDirection;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDirection, out RaycastHit raycastHit, interactDistance, countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
            {
                if (clearCounter != selectedCounter)
                {
                    setSelectedCounter(clearCounter);
                }
            }
            else
            {
                setSelectedCounter(null);
            }
        }
        else
        {
            setSelectedCounter(null);
        }

    }

    private void setSelectedCounter(ClearCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChange?.Invoke(this, new OnSelectedCounterChangeEventArgs { selectedCounter = selectedCounter });
    }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDirection = new(inputVector.x, 0, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .65f;
        float playerHeight = 2f;
        Vector3 point2 = transform.position + Vector3.up * playerHeight;
        bool canMove = !Physics.CapsuleCast(transform.position, point2, playerRadius, moveDirection, moveDistance);

        if (!canMove) // try moving on the edges
        {
            Vector3 moveDirX = new(moveDirection.x, 0, 0); // normalize if you want to keep the speed
            canMove = !Physics.CapsuleCast(transform.position, point2, playerRadius, moveDirX, moveDistance);
            if (canMove)
            {
                moveDirection = moveDirX;
            }
            else
            {
                Vector3 moveDirZ = new(0, 0, moveDirection.z);
                canMove = !Physics.CapsuleCast(transform.position, point2, playerRadius, moveDirZ, moveDistance);
                if (canMove)
                {
                    moveDirection = moveDirZ;
                }
                else
                {
                    // cant move
                }
            }
        }

        if (canMove)
        {
            transform.position += moveDirection * moveDistance;
        }

        isWalking = moveDirection != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
    }

    public bool IsWalking()
    {
        return isWalking;
    }
}
