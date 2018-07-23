using UnityEngine;
using System.Collections;

public class CameraRotate : MonoBehaviour
{
    public Transform targetObject;
    public Vector3 targetOffset;
    public float averageDistance = 5.0f;
    public float maxDistance = 20;
    public float minDistance = .6f;
    public float xSpeed = 200.0f;
    public float ySpeed = 200.0f;
    public int yMinLimit = 5;
    public int yMaxLimit = 80;
    public int zoomSpeed = 40;
    public float panSpeed = 0.3f;
    public float zoomDampening = 5.0f;
	public float rotateOnOff = 1;
    public float MaxXDegTouchVolecity = 50.0f;
    public float MaxYDegTouchVolecity = 50.0f;
    public float MaxXDegMouseVolecity = 5.0f;

    [SerializeField]
    private float xDeg = 0.0f;
    [SerializeField]
    private float yDeg = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private Quaternion currentRotation;
    private Quaternion desiredRotation;
    private Quaternion rotation;
    private Vector3 position;
	private float idleTimer = 0.0f;
	private float idleSmooth = 0.0f;
    Touch oldTouch1;
    Touch oldTouch2;

    
    void Start() { Init(); }
    void OnEnable() { Init(); }

    public void Init()
    {
		if (!targetObject)
        {
            GameObject go = new GameObject("Cam Target");
			go.transform.position = transform.position + (transform.forward * averageDistance);
			targetObject = go.transform;
        }

		currentDistance = averageDistance;
		desiredDistance = averageDistance;
        
        position = transform.position;
        rotation = transform.rotation;
        currentRotation = transform.rotation;
        desiredRotation = transform.rotation;
       
        xDeg = Vector3.Angle(Vector3.right, transform.right );
        yDeg = Vector3.Angle(Vector3.up, transform.up );
		position = targetObject.position - (rotation * Vector3.forward * currentDistance + targetOffset);
    }

    void Update()
    {
        Touch touch;
        if (1 == Input.touchCount)
        {
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                if (Physics.Raycast(ray, out hit))
                {
                    hit.transform.SendMessage("OnMouseDown");
                }
            }
            if (touch.phase == TouchPhase.Moved)
            {
                var tLimitXDeg = Mathf.Clamp(Input.touches[0].deltaPosition.x, -MaxXDegTouchVolecity, MaxXDegTouchVolecity);
                var tLimitYDeg = Mathf.Clamp(Input.touches[0].deltaPosition.y, -MaxYDegTouchVolecity, MaxYDegTouchVolecity);
                xDeg += tLimitXDeg * xSpeed * 0.02f;
                yDeg -= tLimitYDeg * ySpeed * 0.02f;
                yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
            }
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;
            rotation = Quaternion.Lerp(currentRotation, desiredRotation, 0.02f * zoomDampening);
            transform.rotation = rotation;
            idleSmooth = 0;
            idleTimer = 0;
        }
        else if (2 == Input.touchCount)
        {
            Touch newTouch1 = Input.GetTouch(0);
            Touch newTouch2 = Input.GetTouch(1);

            //第2点刚开始接触屏幕, 只记录，不做处理  
            if (newTouch2.phase == TouchPhase.Began)
            {
                oldTouch2 = newTouch2;
                oldTouch1 = newTouch1;
                return;
            }
            //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型  
            float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
            float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);

            //两个距离之差，为正表示放大手势， 为负表示缩小手势  
            float offset = newDistance - oldDistance;
            desiredDistance -= offset * 0.001f * zoomSpeed * 0.125f * Mathf.Abs(desiredDistance);
            //记住最新的触摸点，下次使用  
            oldTouch1 = newTouch1;
            oldTouch2 = newTouch2;

        }
        else if (5 == Input.touchCount)
        {
            Screen.fullScreen = false;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * 0.5f * zoomSpeed * Mathf.Abs(desiredDistance);
        }
        //else if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
        //{
        //    desiredDistance -= Input.GetAxis("Mouse Y") * 0.02f * zoomSpeed * 0.125f * Mathf.Abs(desiredDistance);
        //}
        else if (Input.GetMouseButton(0))
        {
            var tLimitMouseXDeg = Mathf.Clamp(Input.GetAxis("Mouse X"), -MaxXDegMouseVolecity, MaxXDegMouseVolecity);
            xDeg += tLimitMouseXDeg * xSpeed * 0.1f;
            yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.1f;
            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);

            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;
            rotation = Quaternion.Lerp(currentRotation, desiredRotation, 0.02f * zoomDampening);
            transform.rotation = rotation;

            idleSmooth = 0;
            idleTimer = 0;

        }
        else
        {
            idleTimer += 0.02f;
            if (idleTimer > rotateOnOff && rotateOnOff > 0)
            {
                idleSmooth += (0.02f + idleSmooth) * 0.005f;
                idleSmooth = Mathf.Clamp(idleSmooth, 0, 1);
                xDeg += xSpeed * 0.001f * idleSmooth;
            }
            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;
            rotation = Quaternion.Lerp(currentRotation, desiredRotation, 0.02f * zoomDampening * 2);
            transform.rotation = rotation;
        }

        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, 0.02f  * zoomDampening);
		position = targetObject.position - (rotation * Vector3.forward * currentDistance + targetOffset);
        transform.position = position;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}