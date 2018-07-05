using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

    public Transform TargetObject;
    public Transform BaseObject;
    public Camera MainCamera;

    public int ROTATE_PARA = 15;

    private int LEFT_MOUSE_NULL = 0;
    private int LEFT_MOUSE_DOWN = 1;
    private int LEFT_MOUSE_RELEASE = 2;

    private int LeftMouseState = 0;
    private Vector3 CameraBeginPosition;
    private Quaternion CameraBeginRotation;
    private Vector3 CameraLatestPosition;
    private Vector3 CameraLastPosition;
    private Vector3 MouseLatestPosition;
    private Vector3 MouseMovePosition;
    private float MouseMoveX = 0;
    private float MouseMoveY = 0;

    private bool _resetingCamera = false;
    private bool _isHitFurniture = false;
    private bool _inspectFurniture = false;

    RaycastHit _hitInfo;
    RaycastHit _ClickhitInfo;

    private float _mouseX = 0;
    private float _mouseY = 0;

    //private Quaternion _tempCameraRotation;

    private const float SPEED_X = 240f;
    private const float SPEED_Y = 120f;

    private const float MIN_LIMIT_Y = -90;
    private const float MAX_LIMIT_Y = 90;

    private const float DAMPING = 10f;

    public const float DISTANCE = 5F;

    private enum MouseButton
    {
        LEFT_MOUSE_BUTTON = 0,
        RIGHT_MOUSE_BUTTON = 1,
        MIDDLE_MOUSE_BUTTON = 2
    }


    // Use this for initialization
    void Start () {
        CameraBeginPosition = BaseObject.position;
        CameraBeginRotation = BaseObject.rotation;

        //初始化旋转角度
        _mouseX = BaseObject.eulerAngles.x;
        _mouseY = BaseObject.eulerAngles.y;
    }
	
	// Update is called once per frame
	void Update () {
        if (_resetingCamera)
        {
            BaseObject.position = Vector3.Lerp(BaseObject.position, CameraBeginPosition, 0.1f);
            BaseObject.rotation = Quaternion.Lerp(BaseObject.rotation, CameraBeginRotation, 0.1f);
            if (BaseObject.position == CameraBeginPosition)
            {
                _resetingCamera = false;
            }
        }

        if (_inspectFurniture)
        {
            BaseObject.position = Vector3.Lerp(BaseObject.position, new Vector3(_ClickhitInfo.transform.position.x, _ClickhitInfo.transform.position.y, _ClickhitInfo.transform.position.z-15), 0.1f);
            BaseObject.LookAt(_ClickhitInfo.transform.position);
            if (BaseObject.position == _ClickhitInfo.transform.position)
            {
                _inspectFurniture = false;
            }
        }

        _isHitFurniture = false;
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        _hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out _hitInfo))
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.DrawLine(MainCamera.transform.position, _hitInfo.point, Color.red);
#endif
            if(_hitInfo.collider.gameObject.tag == ColorController.TARGET_TAG)
            {
                _isHitFurniture = true;
            }
        }

        CheckMouseLeftClick(true);

        //if (LeftMouseState == LEFT_MOUSE_DOWN)
        //{
        //    MouseMovePosition = Input.mousePosition;
        //    MainCamera.transform.RotateAround(TargetObject.position, Vector3.up, (MouseMovePosition.x - MouseLatestPosition.x) * Time.deltaTime * ROTATE_PARA);
        //    if(Vector3.Angle(Vector3.up, CameraLatestPosition - TargetObject.position)<5.0f)
        //    {
        //        if (MouseMovePosition.y - MouseLatestPosition.y > 0)
        //        {
        //            MainCamera.transform.RotateAround(TargetObject.position, Vector3.Cross(Vector3.up, CameraLatestPosition - TargetObject.position), (MouseMovePosition.y - MouseLatestPosition.y) * Time.deltaTime * ROTATE_PARA);
        //        }
                
        //    }
        //    else if(MainCamera.transform.position.y < 1)
        //    {
        //        if(MouseMovePosition.y - MouseLatestPosition.y < 0)
        //        {
        //            MainCamera.transform.RotateAround(TargetObject.position, Vector3.Cross(Vector3.up, CameraLatestPosition - TargetObject.position), (MouseMovePosition.y - MouseLatestPosition.y) * Time.deltaTime * ROTATE_PARA);
        //        }
        //    }
        //    else
        //    {
        //        MainCamera.transform.RotateAround(TargetObject.position, Vector3.Cross(Vector3.up, CameraLatestPosition - TargetObject.position), (MouseMovePosition.y - MouseLatestPosition.y) * Time.deltaTime * ROTATE_PARA);
        //    }
        //    MainCamera.transform.rotation = Quaternion.Euler(MainCamera.transform.rotation.eulerAngles.x, MainCamera.transform.rotation.eulerAngles.y, 0);
        //}

        //if (Input.GetKeyUp(KeyCode.Mouse0))
        //{
        //    LeftMouseState = LEFT_MOUSE_RELEASE;
        //    //MouseBeginPosition = new Vector3(0,0,0);
        //    MouseLatestPosition = new Vector3(0, 0, 0);
        //    CameraLatestPosition = new Vector3(0, 0, 0);
        //}

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (MainCamera.fieldOfView <= 100)
                MainCamera.fieldOfView += 2;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (MainCamera.fieldOfView > 40)
                MainCamera.fieldOfView -= 2;
        }
    }

    void LateUpdate()
    {
        if(LeftMouseState == LEFT_MOUSE_DOWN)
        {
            MouseLatestPosition = Input.mousePosition;
            CameraLatestPosition = MainCamera.transform.position;
        }
    }

    public void ResetCamera()
    {
        _resetingCamera = true;
        //MainCamera.transform.SetPositionAndRotation(CameraBeginPosition, CameraBeginRotation);
        Debug.Log("ResetCamera");
        Debug.Log("CameraBeginTransform.position = " + CameraBeginPosition);
        Debug.Log("CameraBeginTransform.rotation = " + CameraBeginRotation);
    }

    /// <summary>
    /// 检查鼠标左键是否被按下
    /// check left key of mouse has been Click down 
    /// </summary>
    private void CheckMouseLeftClick(bool pIsNeedDamping = true)
    {
        if (Input.GetMouseButton((int)MouseButton.LEFT_MOUSE_BUTTON))
        {
            //_resetingCamera = false;
            //_inspectFurniture = false;
            //if (_isHitFurniture)
            //{
            //    CameraLastPosition = MainCamera.transform.position;
            //    _ClickhitInfo = _hitInfo;
            //    _inspectFurniture = true;
            //}
            //else
            //{
            //获取鼠标输入
                //Debug.Log(Input.GetAxis("Mouse X"));
                //Debug.Log(Input.GetAxis("Mouse Y"));
                _mouseX += (Mathf.Abs(Input.GetAxis("Mouse X"))>0.2? Input.GetAxis("Mouse X"):0) * SPEED_X * 0.02f;
                _mouseY -= (Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.2 ? Input.GetAxis("Mouse Y") : 0) * SPEED_Y * 0.02f;
            //范围限制
            _mouseY = ClampAngle(_mouseY, MIN_LIMIT_Y, MAX_LIMIT_Y);
            //计算旋转
            Quaternion tCameraRotation = Quaternion.AngleAxis(_mouseY, Vector3.right) * Quaternion.AngleAxis(_mouseX, Vector3.up);
            //Quaternion tCameraRotation = Quaternion.Euler(_mouseY, _mouseX, 0);
            //根据是否插值采取不同的角度计算方式
            if (pIsNeedDamping)
                {
                BaseObject.rotation = Quaternion.Lerp(BaseObject.rotation, tCameraRotation, Time.deltaTime * DAMPING);
                }
                else
                {
                BaseObject.rotation = tCameraRotation;
                }

                //重新计算位置
                Vector3 tPosition = tCameraRotation * new Vector3(0.0F, 0.0F, -DISTANCE) + TargetObject.position;
                //设置相机的位置
                if (pIsNeedDamping)
                {
                BaseObject.position = Vector3.Lerp(transform.position, tPosition, Time.deltaTime * DAMPING);
                }
                else
                {
                BaseObject.position = tPosition;
                }

            //}
        }
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

}
