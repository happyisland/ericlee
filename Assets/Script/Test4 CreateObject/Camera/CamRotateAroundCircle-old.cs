using UnityEngine;
using System.Collections;
using Game.Scene;

//delegate void CameraController();
public class CamRotateAroundCircleold : MonoBehaviour
{
    private Vector3 oriPos;
    private Vector3 oriAng;
    private float oriSize;

    private Vector3 oriPosTarget;

    //缩放系数  
    private float distance = 0.0f;
    //左右滑动移动速度  
    private float xSpeed = 250.0f;
    private float ySpeed = 120.0f;
    //缩放限制系数  
    private float yMinLimit = -360;
    private float yMaxLimit = 360;
    //摄像头的位置  
    private float x = 0.0f;
    private float y = 0.0f;
    //记录上一次手机触摸位置判断用户是在左放大还是缩小手势
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;
    //用于绑定参照物对象
    private Transform target;

    public bool canChangeView;
    public static CamRotateAroundCircleold  _instance;

    private Vector3 scToWorPos;//屏幕实时位置

    public bool canControl = true;     //是否可以控制界面
    private bool canMove = false;

    public Vector3 centerPos;
    public GameObject center;
    public Vector3 camEgler;

    float xTemp;
    float yTemp;

    float defaultField;

    //GameObject cameramr;
    void Awake()
    {

        centerPos = new Vector3(0, 1.0f, -0.68f);
        Vector3 rotatTemp = new Vector3(0, 90.0f, 0);
        Quaternion tempquat = Quaternion.Euler(rotatTemp);
        this.transform.rotation = tempquat;

        center = Resources.Load("Prefab/Test4/ScensePrefab/Center") as GameObject;
        center = GameObject.Instantiate(center, centerPos, Quaternion.identity) as GameObject;
        center.GetComponentInChildren<MeshRenderer>().enabled = false;
        center.name = "Center";
        //center.transform.position = new Vector3(-6.413f, 1.336f, -0.287f);
        
        if (RobotMgr.Instance.newRobot == false)
        {
            center.transform.position = centerPos;
        }
        else
        {
            center.transform.position = new Vector3(-0.46f, 0, -2.68f);
        }

         defaultField = Camera.main.fieldOfView;

         
    }
    //UILabel testlabel;
    void Start()
    {

       // testlabel = GameObject.Find("MainUIRoot/ModelDetails/TopLeft/TestLabel").GetComponent<UILabel>();
        target = center.transform;

        distance = 10.0f;
        canChangeView = true;
        _instance = this;

        

        this.camera.fieldOfView = 60;

        oriPos = this.transform.position;
        oriAng = this.transform.eulerAngles;
        oriSize = this.camera.fieldOfView;

        oriPosTarget = target.transform.position;
        oriPosTarget = target.transform.position;

        camEgler = this.camera.transform.localEulerAngles;
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        xTemp = x;
        yTemp = y;

        if (SceneMgr.GetCurrentSceneType() == SceneType.Assemble)
        {
            AddEvent(JMSimulatorOnly.Instance.btns["Refresh"]);
            JMSimulatorOnly.Instance.EnterSceneHide();
        }
    }

    public void AddEvent(GameObject btn)
    {
        if (SceneMgr.GetCurrentSceneType() == SceneType.Assemble)
        {

            UIEventListener.Get(btn).onClick += ResetCam;
        }
    }

    void Update()
    {
        //if (canControl)
        //{
            CameraControl();
        //}
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    Application.Quit();
        //}
    }

    /// <summary>
    /// reset
    /// </summary>
    bool isResetOdder = false;
    public void ResetOriState()
    {
        this.transform.position = oriPos;
        this.transform.eulerAngles = oriAng;
        this.camera.fieldOfView = oriSize;

        target.transform.position = oriPosTarget;
        target.transform.position = oriPosTarget;
        
        this.camera.fieldOfView = 60;

        y = camEgler.x;
        x = camEgler.y;
       
    }

    public float minfov = 15f;
    public float maxfov = 120f;
    public float sensitivity = 1f;

    public void CameraControl()
    {
        if (null != UICamera.hoveredObject)
        {
            if (UICamera.hoveredObject.name != "CameraMR")
            {
                return;
            }
        }
    #if UNITY_EDITOR
        MouseOperation();
    #endif

    #if UNITY_IPHONE 
        TouchOperation();
    #endif
    #if UNITY_ANDROID
        TouchOperation();
    #endif

   }

    //让相机复位
    public void ResetCam(GameObject go)
    {
        center.transform.position = centerPos;
        x=xTemp;
        y=yTemp;
        Camera.main.fieldOfView=defaultField;
        distance = 10.0f;
    }

public void MouseOperation()
{
    scToWorPos = camera.ScreenToViewportPoint(Input.mousePosition);
    //旋转屏幕
    if (Input.GetMouseButton(0) && Input.touchCount == 0)
    {
        //根据触摸点计算X与Y位置  
        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
    }

    //平移屏幕
    ScreenPosControl();

    //鼠标滚轴控制缩放
    if (Input.GetAxis("Mouse ScrollWheel") != 0)
    {

        float fov = Camera.main.fieldOfView;
        fov += Input.GetAxis("Mouse ScrollWheel") * sensitivity*10;
        fov = Mathf.Clamp(fov, minfov, maxfov);
        Camera.main.fieldOfView = fov;
       // testlabel.text = "view:" + fov;
    }

    if (target)
    {
        //重置摄像机的位置
        y = ClampAngle(y, yMinLimit, yMaxLimit);
        var rotation = Quaternion.Euler(y, x, 0);

        var position = rotation * new Vector3(0.0f, 0.0f, -distance) * 0.5f + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }
}
Vector2 tempPosition1;
Vector2 tempPosition2;
public void TouchOperation()
{
    //当鼠标点击空白处时，控制屏幕移动，
    if (Input.touchCount == 1)
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            ////根据触摸点计算X与Y位置  
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

        }
        if (target)
        {
            //重置摄像机的位置
            y = ClampAngle(y, yMinLimit, yMaxLimit);
            var rotation = Quaternion.Euler(y, x, 0);

            transform.rotation = rotation;

        }
    }
    else if (Input.touchCount == 2)     //判断触摸数量为多点触摸  
    {
        //前两只手指触摸类型都为移动触摸 
        if (Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(1).phase == TouchPhase.Began)
        {
            tempPosition1 = Input.GetTouch(0).position;
            tempPosition2 = Input.GetTouch(1).position;
            oldPosition1=tempPosition1;
            oldPosition2=tempPosition2;
        }
        if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            //计算出当前两点触摸点的位置  
            tempPosition1 = Input.GetTouch(0).position;
            tempPosition2 = Input.GetTouch(1).position;

            var leng1 = Mathf.Sqrt((oldPosition1.x - oldPosition2.x) * (oldPosition1.x - oldPosition2.x) + (oldPosition1.y - oldPosition2.y) * (oldPosition1.y - oldPosition2.y));
            var leng2 = Mathf.Sqrt((tempPosition1.x - tempPosition2.x) * (tempPosition1.x - tempPosition2.x) + (tempPosition1.y - tempPosition2.y) * (tempPosition1.y - tempPosition2.y));

            float distanceTwo = leng1 - leng2;
            if(Mathf.Abs(distanceTwo)>20.0f)
            {
                //float fov = Camera.main.fieldOfView;
                //fov += distanceTwo * sensitivity*0.2f;
                //fov = Mathf.Clamp(fov, minfov, maxfov);
                //Camera.main.fieldOfView = fov;

                //函数返回真为放大，返回假为缩小  
                if (isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
                {
                    //放大系数超过3以后不允许继续放大  
                    //这里的数据是根据我项目中的模型而调节的，大家可以自己任意修改  
                    if (distance > 3.0f)
                    {
                        distance -= 0.2f;
                    }
                }
                else
                {
                    //缩小洗漱返回18.5后不允许继续缩小  
                    //这里的数据是根据我项目中的模型而调节的，大家可以自己任意修改  
                    if (distance < 80)
                    {
                        distance += 0.2f;
                    }
                }
            }
            else
            {
                #region test
                float delta_x = Input.GetAxis("Mouse X") * 0.02f;
                float delta_y = Input.GetAxis("Mouse Y") * 0.02f;
                Quaternion rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
                if ((-270 < y && y < -90) || (90 < y && y < 270))
                {
                    transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + transform.localPosition;
                    target.transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + target.transform.localPosition;
                }
                else
                {
                    transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + transform.localPosition;
                    target.transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + target.transform.localPosition;
                }

                if (target)
                {
                    //重置摄像机的位置
                    y = ClampAngle(y, yMinLimit, yMaxLimit);
                    var rotationl = Quaternion.Euler(y, x, 0);

                    var position = rotationl * new Vector3(0.0f, 0.0f, -distance) * 0.5f + target.position;

                    //transform.rotation = rotation;
                    transform.position = position;
                }
                #endregion
            }
            //备份上一次触摸点的位置，用于对比  
            oldPosition1 = tempPosition1;
            oldPosition2 = tempPosition2;
        }
        else if ((Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Stationary) || (Input.GetTouch(0).phase == TouchPhase.Stationary && Input.GetTouch(1).phase == TouchPhase.Moved))
        {
            //计算出当前两点触摸点的位置  
            //tempPosition1 = Input.GetTouch(0).position;
            //tempPosition2 = Input.GetTouch(1).position;

            //var leng1 = Mathf.Sqrt((oldPosition1.x - oldPosition2.x) * (oldPosition1.x - oldPosition2.x) + (oldPosition1.y - oldPosition2.y) * (oldPosition1.y - oldPosition2.y));
            //var leng2 = Mathf.Sqrt((tempPosition1.x - tempPosition2.x) * (tempPosition1.x - tempPosition2.x) + (tempPosition1.y - tempPosition2.y) * (tempPosition1.y - tempPosition2.y));

            //float distanceTwo = leng1 - leng2;
            //if (Mathf.Abs(distanceTwo) > 2.0f)
            //{
            //    //float fov = Camera.main.fieldOfView;
            //    //fov += distanceTwo * sensitivity * 0.2f;
            //    //fov = Mathf.Clamp(fov, minfov, maxfov);
            //    //Camera.main.fieldOfView = fov;
            //   // testlabel.text = "view:" + fov;
            //}

            //计算出当前两点触摸点的位置  
            var tempPosition1 = Input.GetTouch(0).position;
            var tempPosition2 = Input.GetTouch(1).position;
            //函数返回真为放大，返回假为缩小  
            if (isMove(oldPosition1, oldPosition2, tempPosition1, tempPosition2) == false)
            {
                if (isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
                {
                    //放大系数超过3以后不允许继续放大  
                    //这里的数据是根据我项目中的模型而调节的，大家可以自己任意修改  
                    if (distance > 3.0f)
                    {
                        distance -= 0.2f;
                    }
                }
                else
                {
                    //缩小洗漱返回18.5后不允许继续缩小  
                    //这里的数据是根据我项目中的模型而调节的，大家可以自己任意修改  
                    if (distance < 80)
                    {
                        distance += 0.2f;
                    }
                }
            }
           
            //备份上一次触摸点的位置，用于对比  
            oldPosition1 = tempPosition1;
            oldPosition2 = tempPosition2;
        }
    }

   
}

#region old Touch
//public void TouchOperationOld()
//{
//        //触摸控制屏幕旋转
//        //判断触摸数量为单点触摸 
//        if (canChangeView == true)
//        {
  
//            //当鼠标点击空白处时，控制屏幕移动，
//            if (Input.touchCount == 1)
//            {
//                if (canMove)
//                {
//                    cameramr.GetComponent<UISprite>().spriteName = "Move";
//                    ScreenPosTouch();
//                }
//                else
//                {
//                    cameramr.GetComponent<UISprite>().spriteName = "Rotate";
//                    if (Input.GetTouch(0).phase == TouchPhase.Moved)
//                    {
//                        //根据触摸点计算X与Y位置  
//                        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
//                        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

//                    }
//                }
//            }
//         }

//        //判断触摸数量为多点触摸  
//        if (Input.touchCount == 2)
//        {
//            #region 缩放
//            //前两只手指触摸类型都为移动触摸  
//            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
//            {
//                //计算出当前两点触摸点的位置  
//                var tempPosition1 = Input.GetTouch(0).position;
//                var tempPosition2 = Input.GetTouch(1).position;
//                //函数返回真为放大，返回假为缩小  
//                if (isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
//                {
//                    //放大系数超过3以后不允许继续放大  
//                    //这里的数据是根据我项目中的模型而调节的，大家可以自己任意修改  
//                    if (distance > 3.0f)
//                    {
//                        distance -= 0.2f;
//                    }
//                }
//                else
//                {
//                    //缩小洗漱返回18.5后不允许继续缩小  
//                    //这里的数据是根据我项目中的模型而调节的，大家可以自己任意修改  
//                    if (distance < 80)
//                    {
//                        distance += 0.2f;
//                    }
//                }
//                //备份上一次触摸点的位置，用于对比  
//                oldPosition1 = tempPosition1;
//                oldPosition2 = tempPosition2;
//            }
//            #endregion

//        }
//    }


////触屏控制屏幕移动
//public void ScreenPosTouch()
//{
//    #region touch control screenpos
//    if (Input.touchCount == 1)
//    {
//        scToWorPos = camera.ScreenToViewportPoint(Input.GetTouch(0).position);
//        if (Input.GetTouch(0).phase == TouchPhase.Moved)
//        {
//            float delta_x = Input.GetAxis("Mouse X") * 0.02f;
//            float delta_y = Input.GetAxis("Mouse Y") * 0.02f;
//            Quaternion rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
//            if ((-270 < y && y < -90) || (90 < y && y < 270))
//            {
//                transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + transform.localPosition;
//                target.transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + target.transform.localPosition;
//            }
//            else
//            {
//                transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + transform.localPosition;
//                target.transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + target.transform.localPosition;
//            }
//        }
//    }
//    #endregion
//}

//    public void CameraMRControl(GameObject tempgo)
//    {
//       // Debug.Log("dfdfd");
//        canMove = !canMove;
//    }
#endregion

    bool isMove(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        var lengdis = Mathf.Abs(leng1-leng2);
        if(lengdis <=10.0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //函数返回真为放大，返回假为缩小
    bool isEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        //函数传入上一次触摸两点的位置与本次触摸两点的位置计算出用户的手势
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));

        if (leng1 < leng2)
        {

            //放大手势
            return true;
        }
        else
        {
            //缩小手势
            return false;
        }
    }

    

    //通过鼠标移动屏幕位置
    public void ScreenPosControl()
    {
        #region mouse Control screenPos
        scToWorPos = camera.ScreenToViewportPoint(Input.mousePosition);

     
        if (Input.GetMouseButton(1))
        {
        
            float delta_x = Input.GetAxis("Mouse X") * 0.02f;
            float delta_y = Input.GetAxis("Mouse Y") * 0.02f;
            Quaternion rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
            if((-270<y&&y<-90)||(90<y&&y<270))
            {
              transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + transform.localPosition;
              target.transform.localPosition = rotation * new Vector3(delta_x, delta_y, 0) + target.transform.localPosition;
            }
            else
            {
                transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + transform.localPosition;
                target.transform.localPosition = rotation * new Vector3(-delta_x, -delta_y, 0) + target.transform.localPosition;
            }
        }
        #endregion
    }


    //Update方法一旦调用结束以后进入这里算出重置摄像机的位置
    void LateUpdate()
    {
        // target为我们绑定的物体变量，缩放旋转的参照物
        if (target && canChangeView == true)
        {
            //重置摄像机的位置
            y = ClampAngle(y, yMinLimit, yMaxLimit);
            var rotation = Quaternion.Euler(y, x, 0);

            var position = rotation * new Vector3(0.0f, 0.0f, -distance) * 0.3f + target.position;

           // transform.rotation = rotation;
            transform.position = position;
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 10;
        if (angle > 360)
            angle -= 10;
        return Mathf.Clamp(angle, min, max);
    }

}