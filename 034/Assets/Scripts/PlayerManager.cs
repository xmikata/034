using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.ProBuilder.MeshOperations;

public enum Solid
{
    Cube, Sphere, Triangle
}

public enum Mode
{
    Mode1,Mode2
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager playerManager;

    Camera playerCamera;

    public GameObject currentSolid;
    public LayerMask layerMask;

    [Header("属性设置")]
    public float rotateSpeed = 1;
    public float disappearSize = 0.5f;

    float horizontal;
    float vertical;

    [Header("预制体")]
    public GameObject cubePrefab;
    public GameObject spherePrefab;
    public GameObject trianglePrefab;
    public GameObject connectionPrefab;

    public Dictionary<GameObject, GameObject> morphingSolids;//正在变换中的所有体,<老的gameobject,新的型状>
    private List<GameObject> keysToRemove;//用来删字典的临时list

    [Header("现在的谜题")]
    public PuzzleManager currentPuzzle;
    public List<GameObject> solidsInGame;//谜题中的所有体
    //[Header("连接")]
    //public List<GameObject> connectSolids1;
    //public List<GameObject> connectSolids2;

    [Header("图形变换模式：mode1是两个变，mode2是3个变")]
    public Mode mode;
    public Solid[] mode1 = new Solid[2];
    public Solid[] mode2 = new Solid[3];

    public bool operationLock;//操作锁

    private void Awake()
    {
        playerCamera = Camera.main;
        if (playerManager==null)
        {
            playerManager = this;
        }
        else
        {
            Destroy(this);
        }

        //solidsInGame = new List<GameObject>();
        morphingSolids = new Dictionary<GameObject, GameObject>();
        keysToRemove = new List<GameObject>();

    }

    private void Start()
    {
        GenerateConnection();
    }
    private void Update()
    {
        HandleSolidOutline();
        if (!operationLock)
        {
            HandleChooseSolid();
            InputHandler();
            HandleRotation();
        }
        else
        {
            currentSolid = null;
        }

        if (currentSolid!=null)
        ClickMorphToTargetSolid();
        LockOperation();
        Morph();
        DeleteDictionaryPair();
    }

    private void LateUpdate()
    {
        horizontal = 0;
        vertical = 0;
    }
    public void HandleChooseSolid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray,out hit,100,layerMask))
        {
            currentSolid = hit.collider.gameObject;
        }
        else
        {
            currentSolid = null;
        }
    }
    //选中
    public void HandleSolidOutline()
    {
        if (currentSolid!=null)
        {
            currentSolid.GetComponent<MeshRenderer>().materials[1].SetFloat("_OpenOutline", 1);
        }
    }
    //描边
    public void DeleteDictionaryPair()
    {
        if (keysToRemove.Count>0)
        {
            for (int i = keysToRemove.Count-1; i >= 0; i--)
            {
                morphingSolids.Remove(keysToRemove[i]);
                Destroy(keysToRemove[i]);
                keysToRemove.RemoveAt(i);
            }
        }
    }
    //删除变完的
    public void ClickMorphToTargetSolid()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject currentSolid = this.currentSolid;
            if (mode==Mode.Mode1)
            {
                List<GameObject> solidsNeedToChange = new List<GameObject>();
                solidsNeedToChange.Add(currentSolid);

                for (int i = 0; i < currentPuzzle.connectSolids1.Count; i++)
                {
                    if (currentPuzzle.connectSolids1[i] == currentSolid)
                    {
                        solidsNeedToChange.Add(currentPuzzle.connectSolids2[i]);
                    }
                }

                for (int i = 0; i < currentPuzzle.connectSolids2.Count; i++)
                {
                    if (currentPuzzle.connectSolids2[i] == currentSolid)
                    {
                        solidsNeedToChange.Add(currentPuzzle.connectSolids1[i]);
                    }
                }

                for (int i = 0; i < solidsNeedToChange.Count; i++)
                {
                    Debug.Log(i);
                    Solid nextSolidType = solidsNeedToChange[i].GetComponent<SolidManager>().solid;
                    if (nextSolidType == Solid.Cube)
                    {
                        GameObject newSolid = GameObject.Instantiate(NextSolidNeedToMorph(solidsNeedToChange[i]), solidsNeedToChange[i].transform.position, solidsNeedToChange[i].transform.rotation, currentPuzzle.transform);
                        Color newColor = newSolid.GetComponent<MeshRenderer>().materials[0].color;
                        newColor = new Color(newColor.r, newColor.g, newColor.b, 0);

                        morphingSolids.Add(solidsNeedToChange[i], newSolid);
                        newSolid.GetComponent<MeshRenderer>().materials[0].color = newColor;
                        newSolid.transform.localScale = new Vector3(disappearSize, disappearSize, disappearSize);
                        solidsInGame.Remove(solidsNeedToChange[i]);

                        #region 改变连接
                        for (int j = 0; j < currentPuzzle.connectSolids1.Count; j++)
                        {
                            if (currentPuzzle.connectSolids1[j] == solidsNeedToChange[i])
                            {
                                currentPuzzle.connectSolids1[j] = newSolid;
                                continue;
                            }
                        }

                        for (int j = 0; j < currentPuzzle.connectSolids2.Count; j++)
                        {
                            if (currentPuzzle.connectSolids2[j] == solidsNeedToChange[i])
                            {
                                currentPuzzle.connectSolids2[j] = newSolid;
                                continue;
                            }
                        }
                        #endregion
                    }
                    else if (nextSolidType == Solid.Sphere)
                    {
                        GameObject newSolid = GameObject.Instantiate(NextSolidNeedToMorph(solidsNeedToChange[i]), solidsNeedToChange[i].transform.position, solidsNeedToChange[i].transform.rotation, currentPuzzle.transform);
                        Color newColor = newSolid.GetComponent<MeshRenderer>().materials[0].color;
                        newColor = new Color(newColor.r, newColor.g, newColor.b, 0);

                        morphingSolids.Add(solidsNeedToChange[i], newSolid);
                        newSolid.GetComponent<MeshRenderer>().materials[0].color = newColor;
                        newSolid.transform.localScale = new Vector3(disappearSize, disappearSize, disappearSize);
                        solidsInGame.Remove(solidsNeedToChange[i]);

                        #region 改变连接
                        for (int j = 0; j < currentPuzzle.connectSolids1.Count; j++)
                        {
                            if (currentPuzzle.connectSolids1[j] == solidsNeedToChange[i])
                            {
                                currentPuzzle.connectSolids1[j] = newSolid;
                                continue;
                            }
                        }

                        for (int j = 0; j < currentPuzzle.connectSolids2.Count; j++)
                        {
                            if (currentPuzzle.connectSolids2[j] == solidsNeedToChange[i])
                            {
                                currentPuzzle.connectSolids2[j] = newSolid;
                                continue;
                            }
                        }
                        #endregion

                    }
                }
            }
            else if(mode == Mode.Mode2)
            {
                
            }
        }
    }
    public GameObject NextSolidNeedToMorph(GameObject solid)
    {
        SolidManager solidManager =solid.GetComponent<SolidManager>();
        int nextNum = 0;Solid nextSolid=Solid.Cube;
        if (mode==Mode.Mode1)
        {
            for (int i = 0; i < mode1.Length; i++)
            {
                if (solidManager.solid == mode1[i])
                {
                    nextNum = i + 1;
                }
                if (nextNum > mode1.Length-1)
                {
                    nextNum = 0;
                }
            }
            nextSolid = mode1[nextNum];
        }
        else if(mode == Mode.Mode2)
        {
            for (int i = 0; i < mode2.Length; i++)
            {
                if (solidManager.solid == mode1[i])
                {
                    nextNum = i + 1;
                }
                if (nextNum > mode1.Length-1)
                {
                    nextNum = 0;
                }
            }
            nextSolid = mode1[nextNum];
        }
        switch (nextSolid)
        {
            case Solid.Cube:
                return cubePrefab;
            case Solid.Sphere:
                return spherePrefab;
            case Solid.Triangle:
                return trianglePrefab;
        }
        return cubePrefab;
    }
    public void Morph()
    {
        foreach (KeyValuePair<GameObject, GameObject> pair in morphingSolids)
        {
            MeshRenderer meshRenderer1 = pair.Key.GetComponent<MeshRenderer>();
            MeshRenderer meshRenderer2 = pair.Value.GetComponent<MeshRenderer>();
            Color currentColor1 = meshRenderer1.materials[0].color;//要变透明的
            Color currentColor2 = meshRenderer2.materials[0].color;//要变不透明的
            Vector3 currentScale1 = pair.Key.transform.localScale;
            Vector3 currentScale2 = pair.Value.transform.localScale;

            #region 修变形的时候圆有透明bug的
            if (currentColor1.a<=0.1f)
            {
                //meshRenderer1.enabled = false;
            }
            #endregion

            #region 变形结束
            if ((currentColor1.a >= 0 && currentColor1.a <= 0.01)||
                (currentColor2.a >= 0.99 && currentColor1.a <= 1))
            {
                meshRenderer2.materials[0].color = new Color(meshRenderer2.materials[0].color.r, meshRenderer2.materials[0].color.g, meshRenderer2.materials[0].color.b, 1);
                pair.Value.transform.localScale = new Vector3(1, 1, 1);
                keysToRemove.Add(pair.Key);
                return;
            }
            #endregion

            currentColor1.a = Mathf.Lerp(currentColor1.a, 0, Time.deltaTime *5);
            currentScale1 = Vector3.Lerp(currentScale1, new Vector3(disappearSize, disappearSize, disappearSize), Time.deltaTime * 5);
            currentColor2.a = Mathf.Lerp(currentColor2.a, 1, Time.deltaTime *5);
            currentScale2 = Vector3.Lerp(currentScale2, new Vector3(1, 1, 1), Time.deltaTime * 5);
            meshRenderer1.materials[0].color = currentColor1;
            pair.Key.transform.localScale=currentScale1;
            meshRenderer2.materials[0].color = currentColor2;
            pair.Value.transform.localScale = currentScale2;
        }


    }
    //型状变换
    public void InputHandler()
    {
        if (Input.GetMouseButton(0))
        {
            horizontal = Input.GetAxis("Mouse X");
            vertical = Input.GetAxis("Mouse Y");
        }
    }
    //鼠标输入

    public void HandleRotation()
    {
        if (currentPuzzle!=null)
        {
            //currentPuzzle.transform.Rotate(-vertical * rotateSpeed, horizontal * rotateSpeed, 0, Space.Self);
            Vector3 cameraRight = playerCamera.transform.right; // 相机水平轴（你屏幕的左右方向）

            // 步骤3：水平旋转（绕原点0,0,0，贴合视角左右转）
            // Vector3.up = 世界Y轴（稳定，推荐）；也可替换为mainCamera.transform.up（完全贴合相机上下）
            currentPuzzle.transform.RotateAround(currentPuzzle.transform.position, Vector3.down, horizontal);

            // 步骤4：垂直旋转（绕原点0,0,0，贴合视角上下转）
            // 加负号是让“鼠标下拖=物体向下转”（贴合直觉，可按需去掉）
            currentPuzzle.transform.RotateAround(currentPuzzle.transform.position, -cameraRight, -vertical);
        }
    }
    public void LockOperation()
    {
        if (morphingSolids.Count > 0)
        {
            operationLock = true;
        }
        else
        {
            operationLock = false;
        }
    }

    public void InitializeConnectedPair()//添加键,每一关开始时调用
    {
        //for (int i = 0; i < solidsInGame.Count; i++)
        //{
        //    Debug.Log(solidsInGame[i].name);
        //    Connect connect = solidsInGame[i].GetComponent<Connect>();
        //    Debug.Log(connect.connectedSolids.Count);

        //    for (int j = 0; j < connect.connectedSolids.Count; j++)
        //    {
        //        Debug.Log(j);
        //        GameObject connectedGameObject = connect.connectedSolids[j];
        //        GameObject valueGameObject;
        //        bool next = false;
        //        bool a = false, b = false;
        //        if (connectedPair.TryGetValue(solidsInGame[i], out valueGameObject))//如果能得到key是这个原子
        //        {
        //            if (valueGameObject == connectedGameObject)//如果已经有这一对了
        //            {
        //                a = true;
        //            }
        //        }
        //        else if (connectedPair.TryGetValue(solidsInGame[i], out valueGameObject))///如果能得到key是这个原子的第j个连接
        //        {
        //            if (valueGameObject == solidsInGame[i])//如果已经有着一对了
        //            {
        //                b = true;
        //            }
        //        }
        //        if (a || b)
        //        {
        //            next = true;
        //        }

        //        if (next)
        //        {
        //            continue;
        //        }
        //        else//添加新的一对
        //        {
        //            connectedPair.Add(solidsInGame[i], connect.connectedSolids[j]);
        //        }
        //    }
        //}
        //Debug.Log("字典里添加了" + connectedPair.Count.ToString() + "对");
    }

    public void GenerateConnection()
    {
        if (currentPuzzle.connectSolids1.Count== currentPuzzle.connectSolids2.Count)
        {
            for (int i = 0; i < currentPuzzle.connectSolids1.Count; i++)
            {
                Vector3 a = new Vector3(), b = new Vector3() ;
                a = currentPuzzle.connectSolids1[i].transform.position; b= currentPuzzle.connectSolids2[i].transform.position;

                Vector3 intermediatePoint = (a + b) / 2;
                float distance = Vector3.Distance(a, b);
                GameObject connection = GameObject.Instantiate(connectionPrefab, intermediatePoint, Quaternion.identity,currentPuzzle.transform);
                Quaternion direction = Quaternion.FromToRotation(Vector3.up,a-connection.transform.position);
                connection.transform.rotation = direction;
                connection.transform.localScale = new Vector3(1, distance, 1);
            }
        }
        else
        {
            Debug.LogError("傻逼你没连完");
        }
    }

}
