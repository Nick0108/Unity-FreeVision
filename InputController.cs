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

    private StaticTransform _cameraBeginTransform;
    private StaticTransform _cameraLastTransform;

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
    //[SerializeField]
    private float _x = 0.0f;
    //[SerializeField]
    private float _y = 0.0f;
    //[SerializeField]
    private float _targetX = 0f;
    //[SerializeField]
    private float _targetY = 0f;
    //[SerializeField]
    private float _targetDistance = 0f;
    //[SerializeField]
    private float _xVelocity = 1f;
    //[SerializeField]
    private float _yVelocity = 1f;
    //SerializeField]
    private float _zoomVelocity = 1f;

    private bool _resetingCamera = false;
    private bool _isHitFurniture = false;
    private bool _inspectFurniture = false;

    RaycastHit _hitInfo;
    RaycastHit _ClickhitInfo;

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
        public StaticTransform(Vector3 pPosition, Quaternion pRotation)
        {
            _position = pPosition;
            _rotation = pRotation;
        }
    }


    // Use this for initialization
    void Start()
    {
        if(MainCamera == null)
        {
            MainCamera = Camera.main;
        }

        _cameraBeginTransform = new StaticTransform(MainCamera.transform.position, MainCamera.transform.rotation);

        var angles = MainCamera.transform.eulerAngles;
        //初始化用于计算rotation的两个值，但是由于后面用Quaternion.Euler(_y, _x, 0)时是反的，所以这里也是反的
        _targetY = _y = angles.x;
        _targetX = _x = ClampAngle(angles.y, YMinLimit, YMaxLimit);
        TargetOffset.y = MainCamera.transform.position.y;
        //_targetDistance = Distance;
        _targetDistance = Mathf.Clamp(Vector3.Distance(MainCamera.transform.position, TargetTransform.position), MinDistance, MaxDistance); 
        Distance = Mathf.Clamp(_targetDistance,MinDistance,MaxDistance);
    }

    // Update is called once per frame
    void Update()
    {
        ResetCameraTransform(ref _resetingCamera);

        if (_inspectFurniture)
        {
            MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, new Vector3(_ClickhitInfo.transform.position.x, _ClickhitInfo.transform.position.y, _ClickhitInfo.transform.position.z - 15), 0.1f);
            MainCamera.transform.LookAt(_ClickhitInfo.transform.position);
            if (MainCamera.transform.position == _ClickhitInfo.transform.position)
            {
                _inspectFurniture = false;
            }
        }

        OnMouseController(TargetTransform, AllowRotateY);


        _isHitFurniture = false;
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        _hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out _hitInfo))
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.DrawLine(MainCamera.transform.position, _hitInfo.point, Color.red);
#endif
            if (_hitInfo.collider.gameObject.tag == ColorController.TARGET_TAG)
            {
                //_isHitFurniture = true;
            }
        }
    }

    void LateUpdate()
    {

    }

    public void ResetCamera()
    {
        _resetingCamera = true;
    }

    /// <summary>
    /// 响应鼠标所有操作
    /// In reaction to all mouse controll
    /// </summary>
    private void OnMouseController(Transform pTarget, bool pIsRotateY)
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
            _resetingCamera = false;
            _inspectFurniture = false;
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

        #region 处理摄像头复位问题 Deal with reseting camera
        if (_resetingCamera)
        {
            tRotation = _cameraBeginTransform.Rotation;
            position = _cameraBeginTransform.Position;
            return;
        }
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

    /// <summary>
    /// 摄像机复位
    /// </summary>
    /// <param name="pNeedCamera">是否需要复位</param>
    private void ResetCameraTransform(ref bool pNeedCamera)
    {
        if (pNeedCamera)
        {

            MainCamera.transform.position = Vector3.Slerp(MainCamera.transform.position, _cameraBeginTransform.Position, 0.1f);
            MainCamera.transform.rotation = Quaternion.Slerp(MainCamera.transform.rotation, _cameraBeginTransform.Rotation, 0.1f);
           
            if (MainCamera.transform.position == _cameraBeginTransform.Position)
            {
                pNeedCamera = false;
            }
        }
    }
}