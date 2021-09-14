using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public bool firstTap, pcInput;

    public JumpStats jumpStats;
    public Raycaster raycaster;
    public FallOption fallOption;

    public GameObject triggerGroup;
    public Transform pivot;

    public bool tap { get { if (pcInput) { return Input.GetMouseButtonDown(0); } else { if (Input.touchCount > 0) { return true; } } return false; } }
    public Rigidbody Rigidbody => _rigidbody ?? (_rigidbody = GetComponent<Rigidbody>());
    private Rigidbody _rigidbody;

    private float _tweenTime;
    private bool fallRotate;
    [HideInInspector] public float trueAngle;
    public static UnityEvent OnCollideHilt = new UnityEvent();
    private IEnumerator Start()
    {
        _tweenTime = jumpStats.tweenTime;
        yield return new WaitUntil(() => tap);
        firstTap = true;
        Rigidbody.isKinematic = false;
    }
    private void Update()
    {
        CalculateTrueAngle();
        transform.position = new Vector3(0, transform.position.y, transform.position.z);
        raycaster.CalculateFallVelocity(transform, jumpStats.jumpLenght);
        if (tap)
        {
            if (transform.position.y + 3.0f < jumpStats.jumpMax) Jump();
            else Jump(true);
        }
        if (fallRotate) fallOption.FreeFall(trueAngle, pivot.transform);
    }
    public void Jump(bool onTop = false)
    {
        DOTween.KillAll();
        fallRotate = false;
        pivot.DORotate(Vector3.right * RequiredAngle(), _tweenTime, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            fallRotate = true;
        });
        transform.DOMoveY(raycaster.targetPoint, Mathf.Abs(raycaster.targetPoint - transform.position.y) / jumpStats.fallSpeed).SetDelay(_tweenTime);
        if (!onTop)
        {
            transform.DOJump(transform.position + Vector3.forward * jumpStats.jumpLenght, jumpStats.jumpPower, 1, _tweenTime).SetEase(Ease.Linear);
        }
        else
        {
            transform.DOMoveZ(15.0f, _tweenTime).SetRelative().SetEase(Ease.Linear);
        }
    }
    public float RequiredAngle()
    {
        float reqAngle = 0;
        if (trueAngle >= 0 && trueAngle < 90)
        {
            reqAngle = (90 - trueAngle) + 270;
            _tweenTime = jumpStats.tweenTime;
        }
        else if (trueAngle >= 90 && trueAngle < 180)
        {
            reqAngle = (180 - trueAngle) + 180;
            _tweenTime = jumpStats.tweenTime;
        }
        else if (trueAngle >= 180 && trueAngle < 270)
        {
            reqAngle = (360 - trueAngle) + 315;
            _tweenTime *= 1.61f;
        }
        else
        {
            reqAngle = (360 - trueAngle) + 360;
            _tweenTime = jumpStats.tweenTime;
        }
        return reqAngle + 10;
    }
    public void CalculateTrueAngle()
    {
        float fakeAngleX = pivot.rotation.eulerAngles.x;
        float fakeAngleY = pivot.rotation.eulerAngles.y;
        float fakeAngleZ = pivot.rotation.eulerAngles.z;
        if (fakeAngleY == 180 || fakeAngleZ == 180)
        {
            if (fakeAngleX < 90) { trueAngle = (90 - fakeAngleX) + 90; }
            else if (fakeAngleX <= 360 && fakeAngleX >= 270) { trueAngle = (360 - fakeAngleX) + 180; }
        }
        else { trueAngle = fakeAngleX; }
    }
    bool cooldown;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Ground")) return;
        DOTween.KillAll();
        triggerGroup.SetActive(fallRotate = false);
        StartCoroutine(WaitForTap());
    }
    public IEnumerator WaitForTap()
    {
        yield return new WaitUntil(() => tap);
        yield return new WaitForSeconds(0.2f);
        triggerGroup.SetActive(fallRotate = true);
    }
    private void OnCollisionEnter(Collision other)
    {
        DOTween.KillAll();
        fallRotate = false;
        OnCollideHilt.Invoke();
        pivot.DORotate((360 - trueAngle) * Vector3.right, 0.7f, RotateMode.LocalAxisAdd);
        transform.DOMove(transform.position + transform.forward * -5.0f + transform.up * 3.0f, 0.6f).OnComplete(() =>
        {
            triggerGroup.SetActive(fallRotate = true);
            transform.DOMoveY(raycaster.currentPoint, Mathf.Abs(raycaster.targetPoint - transform.position.y) / jumpStats.fallSpeed);
        });
    }
}
[System.Serializable]
public class FallOption
{
    public float acceleration = 4.8f;
    public float turnSpeed = 120.0f;
    public float normalArea = 1.2f;
    public float slowAreaSpeed = 0.8f;
    public float slowAreaPreSpeed = 0.6f;
    public float accelerateAreaSpeed = 2.4f;
    public Vector2 slowPrepare = new Vector2(0, 30);
    public Vector2 slowArea = new Vector2(30, 90);
    public Vector2 accelerateArea = new Vector2(90, 315);
    public void FreeFall(float trueAngle, Transform transform)
    {
        if (trueAngle > slowPrepare.x && trueAngle < slowPrepare.y) acceleration = slowAreaPreSpeed;
        else if (trueAngle >= slowArea.x && trueAngle < slowArea.y) acceleration = slowAreaSpeed;
        else if (trueAngle >= accelerateArea.x && trueAngle < accelerateArea.y) acceleration = accelerateAreaSpeed;
        else acceleration = normalArea;
        transform.Rotate(turnSpeed * acceleration * Time.deltaTime * Vector3.right);
    }
}
[System.Serializable]
public class Raycaster
{
    public float targetPoint;
    public float currentPoint;
    public float jumpPoint;
    public void CalculateFallVelocity(Transform transform, float jumpWidth)
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        RaycastHit origin, jumpray;
        if (Physics.Raycast(transform.position + Vector3.up * 2.0f, Vector3.down, out origin, 300.0f, layerMask))
        {
            Debug.DrawRay(transform.position + Vector3.up * 2.0f, Vector3.down * origin.distance, Color.green);
            currentPoint = origin.point.y;
        }
        if (Physics.Raycast(transform.position + Vector3.up * 2.0f + Vector3.forward * jumpWidth, Vector3.down, out jumpray, 300.0f, layerMask))
        {
            Debug.DrawRay(transform.position + Vector3.up * 2.0f + Vector3.forward * jumpWidth, Vector3.down * jumpray.distance, Color.red);
            jumpPoint = jumpray.point.y;
        }
        SetTargetPos();
    }
    public void SetTargetPos()
    {
        if (currentPoint < jumpPoint) targetPoint = jumpPoint;
        else if (currentPoint > jumpPoint) targetPoint = jumpPoint;
        else targetPoint = currentPoint;
    }
}
[System.Serializable]
public class JumpStats
{
    public float jumpMax = 80.0f;
    public float jumpLenght = 15.0f;
    public float jumpPower = 10.0f;
    public float fallSpeed = 20.0f;
    public float tweenTime = 2.0f;
}