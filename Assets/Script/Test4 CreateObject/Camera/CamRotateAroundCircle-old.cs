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

    //����ϵ��  
    private float distance = 0.0f;
    //���һ����ƶ��ٶ�  
    private float xSpeed = 250.0f;
    private float ySpeed = 120.0f;
    //��������ϵ��  
    private float yMinLimit = -360;
    private float yMaxLimit = 360;
    //����ͷ��λ��  
    private float x = 0.0f;
    private float y = 0.0f;
    //��¼��һ���ֻ�����λ���ж��û�������Ŵ�����С����
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;
    //���ڰ󶨲��������
    private Transform target;

    public bool canChangeView;
    public static CamRotateAroundCircleold  _instance;

    private Vector3 scToWorPos;//��Ļʵʱλ��

    public bool canControl = true;     //�Ƿ���Կ��ƽ���
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

    //�������λ
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
    //��ת��Ļ
    if (Input.GetMouseButton(0) && Input.touchCount == 0)
    {
        //���ݴ��������X��Yλ��  
        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
    }

    //ƽ����Ļ
    ScreenPosControl();

    //�������������
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
        //�����������λ��
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
    //��������հ״�ʱ��������Ļ�ƶ���
    if (Input.touchCount == 1)
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            ////���ݴ��������X��Yλ��  
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

        }
        if (target)
        {
            //�����������λ��
            y = ClampAngle(y, yMinLimit, yMaxLimit);
            var rotation = Quaternion.Euler(y, x, 0);

            transform.rotation = rotation;

        }
    }
    else if (Input.touchCount == 2)     //�жϴ�������Ϊ��㴥��  
    {
        //ǰ��ֻ��ָ�������Ͷ�Ϊ�ƶ����� 
        if (Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(1).phase == TouchPhase.Began)
        {
            tempPosition1 = Input.GetTouch(0).position;
            tempPosition2 = Input.GetTouch(1).position;
            oldPosition1=tempPosition1;
            oldPosition2=tempPosition2;
        }
        if (Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            //�������ǰ���㴥�����λ��  
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

                //����������Ϊ�Ŵ󣬷��ؼ�Ϊ��С  
                if (isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
                {
                    //�Ŵ�ϵ������3�Ժ���������Ŵ�  
                    //����������Ǹ�������Ŀ�е�ģ�Ͷ����ڵģ���ҿ����Լ������޸�  
                    if (distance > 3.0f)
                    {
                        distance -= 0.2f;
                    }
                }
                else
                {
                    //��Сϴ������18.5�����������С  
                    //����������Ǹ�������Ŀ�е�ģ�Ͷ����ڵģ���ҿ����Լ������޸�  
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
                    //�����������λ��
                    y = ClampAngle(y, yMinLimit, yMaxLimit);
                    var rotationl = Quaternion.Euler(y, x, 0);

                    var position = rotationl * new Vector3(0.0f, 0.0f, -distance) * 0.5f + target.position;

                    //transform.rotation = rotation;
                    transform.position = position;
                }
                #endregion
            }
            //������һ�δ������λ�ã����ڶԱ�  
            oldPosition1 = tempPosition1;
            oldPosition2 = tempPosition2;
        }
        else if ((Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Stationary) || (Input.GetTouch(0).phase == TouchPhase.Stationary && Input.GetTouch(1).phase == TouchPhase.Moved))
        {
            //�������ǰ���㴥�����λ��  
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

            //�������ǰ���㴥�����λ��  
            var tempPosition1 = Input.GetTouch(0).position;
            var tempPosition2 = Input.GetTouch(1).position;
            //����������Ϊ�Ŵ󣬷��ؼ�Ϊ��С  
            if (isMove(oldPosition1, oldPosition2, tempPosition1, tempPosition2) == false)
            {
                if (isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
                {
                    //�Ŵ�ϵ������3�Ժ���������Ŵ�  
                    //����������Ǹ�������Ŀ�е�ģ�Ͷ����ڵģ���ҿ����Լ������޸�  
                    if (distance > 3.0f)
                    {
                        distance -= 0.2f;
                    }
                }
                else
                {
                    //��Сϴ������18.5�����������С  
                    //����������Ǹ�������Ŀ�е�ģ�Ͷ����ڵģ���ҿ����Լ������޸�  
                    if (distance < 80)
                    {
                        distance += 0.2f;
                    }
                }
            }
           
            //������һ�δ������λ�ã����ڶԱ�  
            oldPosition1 = tempPosition1;
            oldPosition2 = tempPosition2;
        }
    }

   
}

#region old Touch
//public void TouchOperationOld()
//{
//        //����������Ļ��ת
//        //�жϴ�������Ϊ���㴥�� 
//        if (canChangeView == true)
//        {
  
//            //��������հ״�ʱ��������Ļ�ƶ���
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
//                        //���ݴ��������X��Yλ��  
//                        x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
//                        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

//                    }
//                }
//            }
//         }

//        //�жϴ�������Ϊ��㴥��  
//        if (Input.touchCount == 2)
//        {
//            #region ����
//            //ǰ��ֻ��ָ�������Ͷ�Ϊ�ƶ�����  
//            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
//            {
//                //�������ǰ���㴥�����λ��  
//                var tempPosition1 = Input.GetTouch(0).position;
//                var tempPosition2 = Input.GetTouch(1).position;
//                //����������Ϊ�Ŵ󣬷��ؼ�Ϊ��С  
//                if (isEnlarge(oldPosition1, oldPosition2, tempPosition1, tempPosition2))
//                {
//                    //�Ŵ�ϵ������3�Ժ���������Ŵ�  
//                    //����������Ǹ�������Ŀ�е�ģ�Ͷ����ڵģ���ҿ����Լ������޸�  
//                    if (distance > 3.0f)
//                    {
//                        distance -= 0.2f;
//                    }
//                }
//                else
//                {
//                    //��Сϴ������18.5�����������С  
//                    //����������Ǹ�������Ŀ�е�ģ�Ͷ����ڵģ���ҿ����Լ������޸�  
//                    if (distance < 80)
//                    {
//                        distance += 0.2f;
//                    }
//                }
//                //������һ�δ������λ�ã����ڶԱ�  
//                oldPosition1 = tempPosition1;
//                oldPosition2 = tempPosition2;
//            }
//            #endregion

//        }
//    }


////����������Ļ�ƶ�
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

    //����������Ϊ�Ŵ󣬷��ؼ�Ϊ��С
    bool isEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        //����������һ�δ��������λ���뱾�δ��������λ�ü�����û�������
        var leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        var leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));

        if (leng1 < leng2)
        {

            //�Ŵ�����
            return true;
        }
        else
        {
            //��С����
            return false;
        }
    }

    

    //ͨ������ƶ���Ļλ��
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


    //Update����һ�����ý����Ժ����������������������λ��
    void LateUpdate()
    {
        // targetΪ���ǰ󶨵����������������ת�Ĳ�����
        if (target && canChangeView == true)
        {
            //�����������λ��
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