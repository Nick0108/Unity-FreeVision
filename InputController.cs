using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [Header("目标对象")]
    public Transform TargetTransform;
    [Header("主摄像头")]
    public Camera MainCamera;
    [Header("与目标的偏移量")]
    public Vector3 TargetOffset = Vector3.zero;
    [Header("离目标的距离")]
    public float Distance = 10.0f;
    [Header("离目标的最小距离")][Range(0f,10f)]
    public float MinDistance = 2f;
    [Header("离目标的最远距离")][Range(10f, 30f)]
    public float MaxDistance = 15f;
    [Header("放大缩小的速率")][Range(0f,5f)]
    public float ZoomSpeed = 1f;
    [Header("X轴的速率")][Range(200f, 300f)]
    public float XSpeed = 250.0f;
    [Header("Y轴的速率")][Range(80f, 150f)]
    public float YSpeed = 120.0f;
    [Header("是否允许y轴旋转")]
    public bool AllowRotateY = true;
    [Header("相机向下最大角度")][Range(-90.0f, 90f)]
    public float YMinLimit = 0f;
    [Header("相机向上最大角度")][Range(0.0f, 90f)]
    public float YMaxLimit = 90f;

    public float FocusTargetX = 0f;
    public float FocusTargetY = 0f;
    public float FocusDistance = 10f;
    
    /*
     （2018.07.07） 必要说明：============================================================================================================
     1.（_targetX，_targetY） 的变化是来源于屏幕的滑动，因此这里的X，Y是基于屏幕的二维轴，这个值表示将要旋转到的位置
     2.（_x,_y）中是_x在空间中代表水平方向转动值，_y在空间中代表垂直方向转动值，但是！！
        根据Rotation.eulerAngles的定义，在世界坐标的前提下，在水平方向旋转则意味着绕Y轴(绿轴)旋转，即应改变的是Rotation.eulerAngles.y的值
        同理，在水平方向旋转则意味着绕X轴（红轴）旋转，即应改变的是Rotation.eulerAngles.x的值[ps.物品朝向与z轴（蓝轴）保持平行]
        因此，
                屏幕上的水平滑动值而获得的目标值 _targetX  =>  影响摄像机实际水平旋转值 _x => 将平滑计算后的值赋值给 Rotation.eulerAngles.y
                屏幕上的水平滑动值而获得的目标值 _targetY  =>  影响摄像机实际水平旋转值 _y => 将平滑计算后的值赋值给 Rotation.eulerAngles.x
                另外，请注意，在这里的_targetX，_targetY是（人为）滑动值，但是_x, _y，Rotation.eulerAngles.y,Rotation.eulerAngles.x 都是“角度值”

                         _targetX
                        ┏━━━━━━━━┓                            y轴（绿轴）
                        ┃         ┃                               │  
                        ┃         ┃ _targetY                      │_____z轴（蓝轴）
                        ┃         ┃                             ╱  
                        ┗━━━━━━━━┛               x轴（红轴） ╱      

        另外，该计算，必须先算出摄像头Rotation和Distance的值才能计算出position的值（position = rotation * new Vector3(0.0f, 0.0f, -Distance)）
        ========================================================================================================================================
         */

    private float _x = 0.0f;
    private float _y = 0.0f;
    private float _targetX = 0f;
    private float _targetY = 0f;
    private float _targetDistance = 0f;
    private float _xVelocity = 1f;
    private float _yVelocity = 1f;
    private float _zoomVelocity = 1f;

    private bool _inspectFurniture = false;
    private bool _isHitFurniture = false;

    private bool _isClickSthDown = false;
    private bool _isInspectClickSthElse = false;
    private Vector3 MouseClickDownPosition;

    RaycastHit _hitInfo;
    //RaycastHit _ClickhitInfo;

    private Transform _targetBeginTransform;
    private StaticTransform _cameraBeginTransform;
    private StaticTransform _cameraLastTransform;

    private enum MouseButton
    {
        LEFT_MOUSE_BUTTON = 0,
        RIGHT_MOUSE_BUTTON = 1,
        MIDDLE_MOUSE_BUTTON = 2
    }

    private struct StaticTransform
    {
        private Vector3 _position;
        public Vector3 Position
        {
            get{ return _position;}
        }
        private Quaternion _rotation;
        public Quaternion Rotation
        {
            get{ return _rotation;}
        }
        private float _distance;
        public float Distance
        {
            get { return _distance; }
        }

        public StaticTransform(Vector3 pPosition, Quaternion pRotation, float pDistance)
        {
            _position = pPosition;
            _rotation = pRotation;
            _distance = pDistance;
        }
    }


    // Use this for initialization
    void Start()
    {
        if(MainCamera == null)
        {
            MainCamera = Camera.main;
        }
        if (TargetTransform != null)
        {
            _targetBeginTransform = TargetTransform;
        }
        else
        {
            Debug.LogError("没有设置任何观察目标");
        }
        var angles = MainCamera.transform.eulerAngles;
        //初始化用于计算rotation的两个值，但是由于后面用Quaternion.Euler(_y, _x, 0)时是反的，所以这里也是反的
        _targetY = _y = angles.x;
        _targetX = _x = ClampAngle(angles.y, YMinLimit, YMaxLimit);
        TargetOffset.y = MainCamera.transform.position.y;
        //_targetDistance = Distance;
        _targetDistance = Mathf.Clamp(Vector3.Distance(MainCamera.transform.position, TargetTransform.position), MinDistance, MaxDistance); 
        Distance = Mathf.Clamp(_targetDistance,MinDistance,MaxDistance);
        _cameraBeginTransform = new StaticTransform(MainCamera.transform.position, MainCamera.transform.rotation, Distance);
    }

    // Update is called once per frame
    void Update()
    {
        _isHitFurniture = IsHitGameObjectByTag(ColorController.TARGET_TAG);

        AllMouseReaction(TargetTransform, AllowRotateY);
    }

    void LateUpdate()
    {

    }

    public void ResetCamera()
    {
        _targetX = _cameraBeginTransform.Rotation.eulerAngles.y;
        _targetY = _cameraBeginTransform.Rotation.eulerAngles.x;
        _targetDistance = _cameraBeginTransform.Distance;
    }

    /// <summary>
    /// 响应鼠标所有操作
    /// In reaction to all mouse controll
    /// </summary>
    private void AllMouseReaction(Transform pTarget, bool pIsRotateY)
    {
        
        if (_inspectFurniture)
        {
            Debug.Log("在观看模式");
            if (_isInspectClickSthElse)
            {
                if (Vector3.Distance(Input.mousePosition, MouseClickDownPosition) > 2f)
                {
                    Debug.Log("在观看模式下移开鼠标了");
                    _isInspectClickSthElse = false;
                }
            }
            if (Input.GetMouseButtonDown((int)MouseButton.LEFT_MOUSE_BUTTON))
            {
                Debug.Log("在观看模式下按下鼠标");
                MouseClickDownPosition = Input.mousePosition;
                _isInspectClickSthElse = true;
            }
            if (Input.GetMouseButtonUp((int)MouseButton.LEFT_MOUSE_BUTTON))
            {
                Debug.Log("在观看模式下松开鼠标");
                if (_isInspectClickSthElse)
                {
                    Debug.Log("在观看模式下松开鼠标");
                    _isInspectClickSthElse = false;
                    //Debug.Log("点击了该物品");
                    TargetTransform = _targetBeginTransform;
                    _targetX = _cameraLastTransform.Rotation.eulerAngles.y;
                    _targetY = _cameraLastTransform.Rotation.eulerAngles.x;
                    _targetDistance = _cameraLastTransform.Distance;
                    _inspectFurniture = false;
                }
            }
        }
        else
        {
            Debug.Log("在非观看模式");
            if (_isClickSthDown)
            {
                if (Vector3.Distance(Input.mousePosition, MouseClickDownPosition) > 2f)
                {
                    Debug.Log("在非观看模式下移开鼠标了");
                    _isClickSthDown = false;
                }
            }
            if (IsHitGameObjectByTag(ColorController.TARGET_TAG))
            {
                if (Input.GetMouseButtonDown((int)MouseButton.LEFT_MOUSE_BUTTON))
                {
                    Debug.Log("在非观看模式下按下鼠标");
                    MouseClickDownPosition = Input.mousePosition;
                    _isClickSthDown = true;
                }
                if (Input.GetMouseButtonUp((int)MouseButton.LEFT_MOUSE_BUTTON))
                {
                    Debug.Log("在非观看模式下松开鼠标");
                    if (_isClickSthDown)
                    {
                        Debug.Log("在非观看模式下松开鼠标时符合条件");
                        _isClickSthDown = false;
                        //Debug.Log("点击了该物品");
                        _cameraLastTransform = new StaticTransform(MainCamera.transform.position, MainCamera.transform.rotation, Distance);
                        TargetTransform = _hitInfo.transform;
                        _targetX = FocusTargetX;
                        _targetY = FocusTargetY;
                        _targetDistance = FocusDistance;
                        _inspectFurniture = true;
                        _isInspectClickSthElse = false;
                    }
                }
            }
        }
        OnMouseDrag(pTarget, pIsRotateY);
    }




    private void OnMouseDrag(Transform pTarget, bool pIsRotateY)
    {
        if (pTarget == null) return;
        #region 鼠标滚轮 MouseScrollWheel
        float tScroll = Input.GetAxis("Mouse ScrollWheel");
        if (tScroll > 0.0f) _targetDistance -= ZoomSpeed;
        else if (tScroll < 0.0f)
            _targetDistance += ZoomSpeed;
        _targetDistance = Mathf.Clamp(_targetDistance, MinDistance, MaxDistance);
        #endregion

        #region 鼠标左键 MouseLeftButton
        if (Input.GetMouseButton((int)MouseButton.LEFT_MOUSE_BUTTON))
        {
            Debug.Log("触发了GetMouseButton");
            _targetX += Input.GetAxis("Mouse X") * XSpeed * 0.02f;
            if (pIsRotateY)
            {
                _targetY -= Input.GetAxis("Mouse Y") * YSpeed * 0.02f;
                _targetY = ClampAngle(_targetY, YMinLimit, YMaxLimit);
            }
        }
        _x = Mathf.SmoothDampAngle(_x, _targetX, ref _xVelocity, 0.3f);
        if (pIsRotateY) _y = Mathf.SmoothDampAngle(_y, _targetY, ref _yVelocity, 0.3f);
        else _y = _targetY;
        Quaternion tRotation = Quaternion.Euler(_y, _x, 0);//为什么是反的请看上面解释
        Distance = Mathf.SmoothDamp(Distance, _targetDistance, ref _zoomVelocity, 0.5f);
        Vector3 position = tRotation * new Vector3(0.0f, 0.0f, -Distance) + pTarget.position + TargetOffset;//重点公式
        #endregion
        MainCamera.transform.rotation = tRotation;
        MainCamera.transform.position = position;
    }

    /// <summary>
    /// 在最小和最大数之间，并返回满足条件的值
    /// </summary>
    /// <returns></returns>
    private float ClampAngle(float pAngle, float pMin, float pMax)
    {
        if (pAngle < -360) pAngle += 360;
        if (pAngle > 360) pAngle -= 360;
        return Mathf.Clamp(pAngle, pMin, pMax);
    }

    private bool IsHitGameObjectByTag(string pTag)
    {
        bool tIsHitFurniture = false;
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        _hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out _hitInfo))
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.DrawLine(MainCamera.transform.position, _hitInfo.point, Color.red);
#endif
            if (_hitInfo.collider.gameObject.tag == pTag)
            {
                tIsHitFurniture = true;
            }
        }
        return tIsHitFurniture;
    }
}