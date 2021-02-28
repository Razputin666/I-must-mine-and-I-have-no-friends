using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct ShadowTile
{
    public Vector2Int position;

    public float clarity;
}
public class ShadowRendererCompute : MonoBehaviour
{

    //private ShadowTile[] data;
    //private List<GameObject> objects;

    //public RenderTexture shadowMap;
    [SerializeField] private ComputeShader computeShader;
    //[SerializeField] private Texture2D meme;
    [SerializeField] private RawImage shadowImage;
    //[SerializeField] private Mesh mesh;
    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        CreateShadows();
    //    }
    //}

    private void Start()
    {

    }

    //private void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    if (shadowMap == null)
    //    {
    //        //shadowMap = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 1);
    //        shadowMap = new RenderTexture(Worldgeneration.Instance.GetWorldWidth, Worldgeneration.Instance.GetWorldHeight, 1);
    //        shadowMap.enableRandomWrite = true;
    //        shadowMap.Create();

    //    }

    //    int claritySize = sizeof(float) * 1;
    //    int positionSize = sizeof(float) * 2;
    //    int totalSize = claritySize + positionSize;
    //    ComputeBuffer shadowBuffer = new ComputeBuffer(TileMapManager.Instance.shadowData.Length, totalSize);
    //    shadowBuffer.SetData(TileMapManager.Instance.shadowData);


    //    computeShader.SetBuffer(0, "litTiles", shadowBuffer);
    //    computeShader.SetTexture(0, "Result", shadowMap);
    //    computeShader.Dispatch(0, shadowMap.width / 8, shadowMap.height / 8, 1);

    //    //shadowBuffer.GetData(TileMapManager.Instance.shadowData);
    //    //Camera.current.backgroundColor = new Color(0, 0, 0, 0);

    //    Graphics.Blit(shadowMap, destination);
    //    shadowBuffer.Dispose();
    //}



}
