using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public float moveSpeed = 5f; // Tốc độ di chuyển, có thể điều chỉnh từ Inspector
    private Vector3 initialPosition;

    public GameObject prefabB; // Prefab cho GameObject B
    public float forceMagnitude = 1f; // Độ mạnh của lực tác động
    private List<GameObject> spawnedObjects = new List<GameObject>(); // Danh sách các GameObject B


    // MSSV để tính toán độ cao Y
    public int MSSV = 21120187;
    private float maxHeight;

    private bool isRotating = false;
    public float rotationSpeed = 200f; // Tốc độ xoay

    private bool isObjectsBRotating = false;

    private bool isMovingUpDown = false;
    private float initialY;
    private float movementRange = 2f; // Khoảng cách di chuyển lên xuống
    private float upDownSpeed;

    private Renderer objectRenderer; // Renderer của GameObject A

    public int Points { get; private set; } = 0; // Thuộc tính Points

    void Start()
    {
        // Tạo vị trí ngẫu nhiên cho GameObject A
        float randomY = Random.Range(10f, 20f);
        transform.position = new Vector3(0f, randomY, 0f);
        initialPosition = transform.position;

        maxHeight = 10f + (MSSV % 10); // Tính độ cao tối đa dựa trên MSSV

        upDownSpeed = 2f + (MSSV % 10); // Tốc độ di chuyển dựa trên MSSV
        initialY = transform.position.y;

        objectRenderer = GetComponent<Renderer>(); // Lấy render của object A
    }

    void Update()
    {
        // Di chuyển theo các phím mũi tên hoặc WASD
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D hoặc mũi tên trái/phải
        float verticalInput = Input.GetAxis("Vertical");     // W/S hoặc mũi tên lên/xuống

        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        // Phát sinh GameObject B khi nhấn Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnObjectB();
        }

        // Xóa GameObject B khi nhấn Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(DestroyRandomObjects());
        }

        // Phím R để xoay GameObject A
        if (Input.GetKeyDown(KeyCode.R))
        {
            isRotating = !isRotating;
        }

        // Thực hiện xoay nếu đang trong trạng thái xoay
        if (isRotating)
        {
            transform.Rotate(Vector3.left * rotationSpeed * Time.deltaTime, Space.Self);
        }

        // Phím T để xoay các GameObject B
        if (Input.GetKeyDown(KeyCode.T))
        {

            if (spawnedObjects.Count < 1) return;

            isObjectsBRotating = !isObjectsBRotating;
            if (isObjectsBRotating)
            {
                // Thay đổi độ cao Y của các GameObject B
                foreach (GameObject obj in spawnedObjects)
                {
                    if (obj != null)
                    {

                        Rigidbody rb = obj.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = true;
                        }


                        Vector3 pos = obj.transform.position;
                        pos.y += Random.Range(1f, 3f);
                        obj.transform.position = pos;

                    }
                }
            }
            else
            {
                // Tắt chế độ isKinematic khi ngừng xoay để bật lại vật lý
                foreach (GameObject obj in spawnedObjects)
                {
                    if (obj != null)
                    {

                        Rigidbody rb = obj.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.isKinematic = false;
                        }
                    }
                }


            }


        }


        // Xoay các GameObject B nếu đang trong trạng thái xoay
        if (isObjectsBRotating)
        {
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                {
                    obj.transform.Rotate(Vector3.left * rotationSpeed * Time.deltaTime, Space.Self);
                }
            }
        }

        Debug.Log("Trạng thái xoay của object B: " + isObjectsBRotating);
    
        //Debug.Log("Số lượng object B: " + spawnedObjects.Count);

        // Tắt xoay khi không còn GameObject B
        if (spawnedObjects.Count < 1)
        {
            isObjectsBRotating = false;
        }


        // Phím M để bắt đầu/dừng di chuyển lên xuống
        if (Input.GetKeyDown(KeyCode.M))
        {
            isMovingUpDown = !isMovingUpDown;
            if (isMovingUpDown)
            {
                initialY = transform.position.y;
            }
        }

        // Thực hiện di chuyển lên xuống
        if (isMovingUpDown)
        {
            float newY = initialY + Mathf.Sin(Time.time * upDownSpeed) * movementRange;
            Vector3 newPosition = transform.position;
            newPosition.y = newY;
            transform.position = newPosition;
        }

        // Phím C để đổi màu các GameObject B
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeColorOfObjectsB();
        }
    }


    void OnMouseEnter()
    {
        // Đổi màu GameObject A khi di chuyển chuột vào
        objectRenderer.material.color = Color.red; // Màu tùy chọn
    }

    void OnMouseExit()
    {
        // Đổi màu GameObject A thành màu trắng khi di chuyển chuột ra
        objectRenderer.material.color = Color.white;
    }


    void ChangeColorOfObjectsB()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Renderer objRenderer = obj.GetComponent<Renderer>();
                if (objRenderer != null)
                {
                    objRenderer.material.color = new Color(
                        Random.Range(0f, 1f),
                        Random.Range(0f, 1f),
                        Random.Range(0f, 1f)
                    );
                }
            }
        }
    }

    void SpawnObjectB()
    {
        // Tạo vị trí ngẫu nhiên
        float randomX = Random.Range(-10f, 10f);
        float randomY = Random.Range(10f, maxHeight);
        float randomZ = Random.Range(-10f, 10f);
        Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

        // Tạo đối tượng mới
        GameObject newObject = Instantiate(prefabB, spawnPosition, Quaternion.identity);

        // Thêm Rigidbody nếu chưa có
        Rigidbody rb = newObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = newObject.AddComponent<Rigidbody>();
        }

        // Thêm lực ngẫu nhiên
        Vector3 randomForce = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized * forceMagnitude;

        rb.AddForce(randomForce, ForceMode.Impulse);

        // Thêm vào danh sách để quản lý
        spawnedObjects.Add(newObject);

        // Thêm script để bắt sự kiện bấm chuột vào GameObject B
        newObject.AddComponent<ClickableObject>().gameController = this;
    }

    IEnumerator DestroyRandomObjects()
    {
        yield return new WaitForSeconds(2f); // Đợi 2 giây

        while (spawnedObjects.Count > 0)
        {
            int randomIndex = Random.Range(0, spawnedObjects.Count);
            GameObject objectToDestroy = spawnedObjects[randomIndex];
            spawnedObjects.RemoveAt(randomIndex);

            if (objectToDestroy != null)
            {
                Destroy(objectToDestroy);
            }

            yield return new WaitForSeconds(0.1f); // Đợi 0.2 giây giữa mỗi lần xóa
        }
    }


    void OnGUI()
    {
        // GUI style with font size and color
        GUIStyle style = new GUIStyle();
        style.fontSize = 50;
        style.normal.textColor = Color.white;
        // Set points label
        GUI.Label(new Rect(10, 10, 100, 20), "Points: " + Points, style);

    }

    public void IncrementPoints()
    {
        Points++;
    }

    
}


public class ClickableObject : MonoBehaviour
{
    public GameController gameController;

    void OnMouseDown()
    {
        // Hủy GameObject B khi bấm chuột vào
        Destroy(gameObject);
        // Tăng điểm số Points của GameObject A lên 1
        gameController.IncrementPoints();
    }
}