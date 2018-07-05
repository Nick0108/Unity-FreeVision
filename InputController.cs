using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public Transform TargetObject;
    public Camera MainCamera;

    public int ROTATE_PARA = 15;

    private int LEFT_MOUSE_NULL = 0;
    private int LEFT_MOUSE_DOWN = 1;
    private int LEFT_MOUSE_RELEASE = 2;

    //private Transform tempCameraTrans;

    private int LeftMouseState = 0;
    private Vector3 CameraBeginPosition;
    private Quaternion CameraBeginRotation;
    private Vector3 CameraLatestPosition;
    private Vector3 CameraLastPosition;
    private Vector3 MouseLatestPosition;
    private Vector3 MouseMovePosition;
    private float MouseMoveX = 0;
    private float MouseMoveY = 0;

    private float _acumulateY = 0;
    public float MAX_Y = 15f;
    public float Min_y = -15f;

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


    // Use this for initialization
    void Start()
    {
        //tempCameraTrans = MainCamera.transform.Find("tempCameraTrans");
        //if(tempCameraTrans == null)
        //{
        //    tempCameraTrans = Instantiate(new GameObject(), MainCamera.transform.position, MainCamera.transform.rotation).transform;
        //    tempCameraTrans.name = "tempCameraTrans";
        //}
        CameraBeginPosition = MainCamera.transform.position;
        CameraBeginRotation = MainCamera.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (_resetingCamera)
        {
            MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, CameraBeginPosition, 0.1f);
            MainCamera.transform.rotation = Quaternion.Lerp(MainCamera.transform.rotation, CameraBeginRotation, 0.1f);
            if (MainCamera.transform.position == CameraBeginPosition)
            {
                _resetingCamera = false;
            }
        }

        if (_inspectFurniture)
        {
            MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, new Vector3(_ClickhitInfo.transform.position.x, _ClickhitInfo.transform.position.y, _ClickhitInfo.transform.position.z - 15), 0.1f);
            MainCamera.transform.LookAt(_ClickhitInfo.transform.position);
            if (MainCamera.transform.position == _ClickhitInfo.transform.position)
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
            if (_hitInfo.collider.gameObject.tag == ColorController.TARGET_TAG)
            {
                //_isHitFurniture = true;
            }
        }

        

        CheckLeftMouseClick();
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
        //if (LeftMouseState == LEFT_MOUSE_DOWN)
        //{
            MouseLatestPosition = Input.mousePosition;
            CameraLatestPosition = MainCamera.transform.position;
        //}
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
    private void CheckLeftMouseClick()
    {
        if (Input.GetMouseButton((int)MouseButton.LEFT_MOUSE_BUTTON))
        {
            _resetingCamera = false;
            _inspectFurniture = false;
            if (_isHitFurniture)
            {
                CameraLastPosition = MainCamera.transform.position;
                _ClickhitInfo = _hitInfo;
                _inspectFurniture = true;
            }
            else
            {
                //LeftMouseState = LEFT_MOUSE_DOWN;
                //MouseLatestPosition = Input.mousePosition;
                //CameraLatestPosition = MainCamera.transform.position;
                //tempCameraTrans.RotateAround(TargetObject.position, Vector3.Cross(Vector3.up, CameraLatestPosition - TargetObject.position), Input.GetAxis("Mouse Y") * ROTATE_PARA);
                
                _acumulateY += Input.GetAxis("Mouse Y");
                Debug.Log(_acumulateY);
                if (_acumulateY > MAX_Y || _acumulateY < Min_y)
                {
                    return;
                }
                if (Vector3.Angle(Vector3.up, MainCamera.transform.position - TargetObject.position) < 5.0f || _acumulateY > MAX_Y)
                {
                    if (Input.GetAxis("Mouse Y") > 0)
                    {
                        MainCamera.transform.RotateAround(TargetObject.position, Vector3.Cross(Vector3.up, CameraLatestPosition - TargetObject.position), Input.GetAxis("Mouse Y") * ROTATE_PARA);
                    }
                }
                else if(MainCamera.transform.position.y < 1)
                {
                    if (Input.GetAxis("Mouse Y") < 0)
                    {
                        MainCamera.transform.RotateAround(TargetObject.position, Vector3.Cross(Vector3.up, CameraLatestPosition - TargetObject.position), Input.GetAxis("Mouse Y") * ROTATE_PARA);
                    }
                }
                else
                {
                    MainCamera.transform.RotateAround(TargetObject.position, Vector3.Cross(Vector3.up, CameraLatestPosition - TargetObject.position), Input.GetAxis("Mouse Y") * ROTATE_PARA);
                }
                MainCamera.transform.rotation = Quaternion.Euler(MainCamera.transform.rotation.eulerAngles.x, MainCamera.transform.rotation.eulerAngles.y, 0);
                MainCamera.transform.RotateAround(TargetObject.position, Vector3.up, Input.GetAxis("Mouse X") * ROTATE_PARA);
            }
        }

        //if (LeftMouseState == LEFT_MOUSE_DOWN)
        //{
        //    MouseMovePosition = Input.mousePosition;
        //    MainCamera.transform.RotateAround(TargetObject.position, Vector3.up, (MouseMovePosition.x - MouseLatestPosition.x) * Time.deltaTime * ROTATE_PARA);
        //    if (Vector3.Angle(Vector3.up, CameraLatestPosition - TargetObject.position) < 5.0f)
        //    {
        //        if (MouseMovePosition.y - MouseLatestPosition.y > 0)
        //        {
        //            MainCamera.transform.RotateAround(TargetObject.position, Vector3.Cross(Vector3.up, CameraLatestPosition - TargetObject.position), (MouseMovePosition.y - MouseLatestPosition.y) * Time.deltaTime * ROTATE_PARA);
        //        }

        //    }
        //    else if (MainCamera.transform.position.y < 1)
        //    {
        //        if (MouseMovePosition.y - MouseLatestPosition.y < 0)
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
    }
}