using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Net;
using ThreeDNet;
using ThreeDNet.Client;

namespace ThreeDNet.Engine
{
    public class Player : MonoBehaviour
    {
        private static Player instance;

        public static Player getInstance()
        {
            if (instance == null)
                instance = new Player();
            return instance;
        }

        public GameObject PlayerObject;
        public GameObject PlayerCamera;
        Rigidbody RigidbodyComp;

        private List<bool> keys = new List<bool> { false, false, false, false, false };

        public Vector2Int NetCoords;

        World world;

        public float Speed;
        public float MouseSensitivity;

        float rotX = 0;
        float rotY = 0;

        bool isGrounded = false;

        Client.Client client = Client.Client.getInstance();

        // Use this for initialization
        void Start()
        {
            Debug.Log(Utils.AddrToDim(IPAddress.Parse("127.0.0.1")));
            Debug.Log(Utils.AddrToDim(IPAddress.Parse("79.164.55.74")));

            this.world = World.getInstance();
            this.world.ChangeActiveScene(NetCoords);

            RigidbodyComp = PlayerObject.GetComponent<Rigidbody>();

            //Debug.Log(Encoding.Default.GetString(Client.Client.Send("3dnet://127.0.0.1:53534/main.3dml")));
            //PlayerObject.transform.position = new Vector3(NetCoords.x * 20, 0, NetCoords.y * 20);
            //Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        void Update(){
            PlayerInput();
            Look();
        }

        void FixedUpdate()
        {
            Move();
        }
        
        void Awake()
        {
            UnityThread.initUnityThread();
        }

        void OnGUI(){
            GUI.Label(new Rect(10, 10, 150, 20), PlayerObject.transform.position.ToString());
            GUI.Label(new Rect(10, 25, 150, 20), NetCoords.ToString());
            GUI.Label(new Rect(10, 40, 250, 20), "DimToAddr: "+Utils.DimToAddr(NetCoords).ToString());
            GUI.Label(new Rect(10, 55, 250, 20), "AddrToDim(DimToAddr): "+Utils.AddrToDim(Utils.DimToAddr(NetCoords)).ToString());
            GUI.Label(new Rect(10, 70, 150, 20), Input.mousePosition.ToString());
            GUI.Label(new Rect(10, 85, 150, 20), PlayerObject.transform.eulerAngles.ToString());
            GUI.Label(new Rect(10, 100, 150, 20), Mathf.RoundToInt(1f / Time.unscaledDeltaTime).ToString());
            GUI.Label(new Rect(10, 115, 150, 20), rotX.ToString());
            GUI.Label(new Rect(10, 130, 150, 20), isGrounded.ToString());
        }

        void OnCollisionStay(Collision collisionInfo) {
            if(!isGrounded && RigidbodyComp.velocity.y == 0) { isGrounded = true; }
        }

        void PlayerInput()
        {
            if (Input.GetKeyDown(KeyCode.W))
                keys[0] = true;
            if (Input.GetKeyDown(KeyCode.A))
                keys[1] = true;
            if (Input.GetKeyDown(KeyCode.S))
                keys[2] = true;
            if (Input.GetKeyDown(KeyCode.D))
                keys[3] = true;

            if (Input.GetKeyDown(KeyCode.Space))
                keys[4] = true;

            if (Input.GetKeyUp(KeyCode.W))
                keys[0] = false;
            if (Input.GetKeyUp(KeyCode.A))
                keys[1] = false;
            if (Input.GetKeyUp(KeyCode.S))
                keys[2] = false;
            if (Input.GetKeyUp(KeyCode.D))
                keys[3] = false;

            if (Input.GetKeyUp(KeyCode.Space))
                keys[4] = false;

        }

        void Move(){
            //PlayerObject.transform.eulerAngles = new Vector3(0, rotX);
            this.RigidbodyComp.MoveRotation(Quaternion.Euler(0, rotX, 0));

            if (PlayerObject.transform.position.y <= -100)
                PlayerObject.transform.position = new Vector3(0.5f, 1.2f, 0.5f);

            if (keys[4] && isGrounded)
            {
                isGrounded = false;
                RigidbodyComp.AddForce(Vector3.up*4f, ForceMode.Impulse);
            }

            // float xrot = PlayerObject.transform.eulerAngles.x;
            // PlayerObject.transform.eulerAngles = new Vector3(0, PlayerObject.transform.eulerAngles.y, 0);

            if (keys[0] == true)
                //PlayerObject.transform.Translate(Vector3.forward * Time.deltaTime * Speed);
                //RigidbodyComp.velocity = new Vector3(8, RigidbodyComp.velocity.y, 8);
                RigidbodyComp.MovePosition(RigidbodyComp.position + 
                    PlayerObject.transform.TransformDirection(Vector3.forward) * Time.deltaTime * Speed);
            if (keys[2] == true)
                RigidbodyComp.MovePosition(RigidbodyComp.position + 
                    PlayerObject.transform.TransformDirection(Vector3.back) * Time.deltaTime * Speed);
            if (keys[1] == true)
                RigidbodyComp.MovePosition(RigidbodyComp.position + 
                    PlayerObject.transform.TransformDirection(Vector3.left) * Time.deltaTime * Speed);
            if (keys[3] == true)
                RigidbodyComp.MovePosition(RigidbodyComp.position + 
                    PlayerObject.transform.TransformDirection(Vector3.right) * Time.deltaTime * Speed);

            //PlayerObject.transform.eulerAngles = new Vector3(xrot, PlayerObject.transform.eulerAngles.y, 0);

            if ((NetCoords.x <= 0) && (PlayerObject.transform.position.x <= 0))
            {
                PlayerObject.transform.position = new Vector3(0, PlayerObject.transform.position.y, PlayerObject.transform.position.z);
            }
            if ((NetCoords.x >= 65536) && (PlayerObject.transform.position.x >= 20))
            {
                PlayerObject.transform.position = new Vector3(20, PlayerObject.transform.position.y, PlayerObject.transform.position.z);
            }

            if ((NetCoords.y <= 0) && (PlayerObject.transform.position.z <= 0))
            {
                PlayerObject.transform.position = new Vector3(PlayerObject.transform.position.x, PlayerObject.transform.position.y, 0);
            }
            if ((NetCoords.y >= 65536) && (PlayerObject.transform.position.z >= 20))
            {
                PlayerObject.transform.position = new Vector3(PlayerObject.transform.position.x, PlayerObject.transform.position.y, 20);
            }

            // Vector3 position = PlayerObject.transform.position;
            // position[1] = 1; // the Y value
            // PlayerObject.transform.position = position;

            if ((PlayerObject.transform.position.x < 0) || (PlayerObject.transform.position.x > 20) ||
                (PlayerObject.transform.position.z < 0) || (PlayerObject.transform.position.z > 20))
            {
                Vector3 pos = PlayerObject.transform.position;
                if (PlayerObject.transform.position.x > 20)
                {
                    NetCoords.x += 1;
                    pos.x = pos.x - 20 + 0.1f;
                }
                else if (PlayerObject.transform.position.x < 0)
                {
                    NetCoords.x -= 1;
                    pos.x = 20 - pos.x - 0.1f;
                }
                if (PlayerObject.transform.position.z > 20)
                {
                    NetCoords.y += 1;
                    pos.z = pos.z - 20 + 0.1f;
                }
                else if (PlayerObject.transform.position.z < 0)
                {
                    NetCoords.y -= 1;
                    pos.z = 20 - pos.z - 0.1f;
                }

                //Utils.("Current chunk IP: %s\n", Utils.DimToAddr(NetCoords).ToString());

                PlayerObject.transform.position = pos;

                world.ChangeActiveScene(NetCoords);
            }
        }

        void Look()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                if (Cursor.lockState == CursorLockMode.None)
                    Cursor.lockState = CursorLockMode.Locked;
                else
                    Cursor.lockState = CursorLockMode.None;
                
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // Get the mouse delta. This is not in the range -1...1
                rotX += MouseSensitivity * Input.GetAxis("Mouse X");
                rotY += MouseSensitivity * Input.GetAxis("Mouse Y");

                rotX = Mathf.Repeat(rotX, 360f);
                rotY = Mathf.Clamp(rotY, -90f, 90f);

                PlayerCamera.transform.eulerAngles = new Vector3(-rotY, rotX, 0);
            }
        }

    }
}
