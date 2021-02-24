using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    public GameObject camPrefab;
    public Texture2D noiseTexture;

    [HideInInspector]
    public Camera renderCam;
    private Camera mainCamera;
    private Player player;

    [HideInInspector]
    public MeshRenderer screen;
    private Material screenMaterial;
    private RenderTexture screenTexture;

    [HideInInspector]
    public bool isPlayerInPortal = false;
    [HideInInspector]
    public float oldDotProduct;

    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;
    private Matrix4x4 defaultProjectionMatrix;

    void Awake(){
        player = FindObjectOfType<Player>();
        mainCamera = player.GetComponentInChildren<Camera>();
        screen = GetComponent<MeshRenderer>();

        renderCam = Instantiate(camPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Camera>();
        screenMaterial = new Material(Shader.Find("Unlit/PortalShader"));
        screenTexture = new RenderTexture(Screen.width, Screen.height, 24);
    }

    void Start(){
        defaultProjectionMatrix = linkedPortal.renderCam.projectionMatrix;

        screenMaterial.SetTexture("_MainTex", screenTexture);
        screenMaterial.SetTexture("_SecondTex", noiseTexture);
        linkedPortal.renderCam.targetTexture = screenTexture;
        screen.material = screenMaterial;
    }

    void Update(){
        TeleportPlayer();
    }

    void OnTriggerEnter(Collider other){
        if (other.tag == "Player")
            isPlayerInPortal = true;
    }
    
    void OnTriggerExit(Collider other){
        if (other.tag == "Player")
            isPlayerInPortal = false;
    }

    public void Render(){
        AdjustCameraClipPlain();
        UpdateRenderCam();
        linkedPortal.renderCam.Render();
    }

    // update the linked portal renderCam position and retation relative to the player
    void UpdateRenderCam(){
        Vector3 positionOffset = mainCamera.transform.position - transform.position;
        float angularDiff = Quaternion.Angle(transform.rotation, linkedPortal.transform.rotation);
        Quaternion rotationDiff = Quaternion.AngleAxis(angularDiff, Vector3.up);
        Vector3 newCameraRotation = rotationDiff * mainCamera.transform.forward;

        linkedPortal.renderCam.transform.position = linkedPortal.transform.position + positionOffset;
        linkedPortal.renderCam.transform.rotation = Quaternion.LookRotation(newCameraRotation, Vector3.up);
    }

    // check if the player need to be teleported and teleport him if needed
    void TeleportPlayer(){
        Vector3 offsetFromPortal = player.transform.position - transform.position;
        float actualDotProduct = Vector3.Dot(offsetFromPortal, transform.forward);
        if(isPlayerInPortal && ((actualDotProduct < 0) != (oldDotProduct < 0))){
            float rotationDiff = -Quaternion.Angle(transform.rotation, linkedPortal.transform.rotation);
            Vector3 positionDiff = Quaternion.Euler(0f, rotationDiff, 0f) * offsetFromPortal;
            player.transform.Rotate(Vector3.up, rotationDiff);
            player.Teleport(linkedPortal.transform.position + positionDiff);

            isPlayerInPortal = false;
            linkedPortal.isPlayerInPortal = true;
            Vector3 offsetFromLinkedPortal = player.transform.position - linkedPortal.transform.position;
            linkedPortal.oldDotProduct = Vector3.Dot(offsetFromLinkedPortal, transform.forward);
        }
        oldDotProduct = actualDotProduct;
    }

    // adjust the camera clip plain via camera's projection matrix
    void AdjustCameraClipPlain(){
        Transform clipPlane = transform;
        Camera cam = linkedPortal.renderCam;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - cam.transform.position));

        Vector3 camSpacePos = cam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = cam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;
        
        if(Mathf.Abs(camSpaceDst) > nearClipLimit){
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
            cam.projectionMatrix = cam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }else{
            cam.projectionMatrix = defaultProjectionMatrix;
        }
    }
}
