//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Rendering;
//using Unity.Mathematics;
//using Unity.Transforms;

//public struct SpriteSheetAnimation_Data : IComponentData
//{
//    public int currentFrame;
//    public int frameCount;
//    public float frameTimer;
//    public float frameTimerMax;

//    public Vector4 uv;
//    public Matrix4x4 matrix;
//}


//public class SpriteSheetAnimation_Animate : SystemBase
//{
//    //public float deltaTime = 0.01f;
//    //public struct Job : IJobForEach<SpriteSheetAnimation_Data, Translation>
//    //{
//    //    public float deltaTime;

//    //    public void Execute()
//    //    {

//    //        s
//    //    }

//    //}

//    protected override void OnUpdate()
//    {
//        Entities.ForEach((ref SpriteSheetAnimation_Data spriteSheetAnimationData, ref Translation translation) =>
//        {
//            float deltaTime = UnityEngine.Time.deltaTime;
//            spriteSheetAnimationData.frameTimer += deltaTime;
//            while (spriteSheetAnimationData.frameTimer >= spriteSheetAnimationData.frameTimerMax)
//            {
//                spriteSheetAnimationData.frameTimer -= spriteSheetAnimationData.frameTimerMax;
//                spriteSheetAnimationData.currentFrame = (spriteSheetAnimationData.currentFrame + 1) % spriteSheetAnimationData.frameCount;

//                float uvWidth = 1f / spriteSheetAnimationData.frameCount;
//                float uvHeight = 1f;
//                float uvOffsetX = uvWidth * spriteSheetAnimationData.currentFrame;
//                float uvOffsetY = 0f;
//                spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);

//                float3 position = translation.Value;
//                position.z = position.y * .01f;
//                spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
//            }
//        }).Run();
//    }
//}

//public struct SpriteData : IComponentData
//{
//    public Vector4 uv;
//    public Matrix4x4 matrix;
//}

//public class SpriteRenderer : ComponentSystem
//{
//    private struct RenderData
//    {
//        public Entity entity;
//        public float3 position;
//        public Matrix4x4 matrix;
//        public Vector4 uv;
//    }

//    private class CullAndSortNativeQueueJob : SystemBase
//    {
//        public float yTop_1; //Top most cull position
//        public float yTop_2; //Second slice from top
//        public float yBottom; //Botom most cull position

//        public NativeQueue<RenderData>.ParallelWriter nativeQueue_1;
//        public NativeQueue<RenderData>.ParallelWriter nativeQueue_2;

//        //public void Execute(Entity entity, int index, ref Translation translation, ref SpriteSheetAnimation_Data spriteData)
//        //{
//        //    float positionX = translation.Value.x;
//        //    float positionY = translation.Value.y;
//        //    if (positionY > yBottom && positionY < yTop_1)
//        //    {
//        //        //Valid position
//        //        RenderData renderData = new RenderData
//        //        {
//        //            entity = entity,
//        //            position = translation.Value,
//        //            matrix = spriteData.matrix,
//        //            uv = spriteData.uv
//        //        };

//        //        if (positionY < yTop_2)
//        //            nativeQueue_2.Enqueue(renderData);
//        //        else
//        //            nativeQueue_1.Enqueue(renderData);
//        //    }
//        //}
        
//        protected override void OnUpdate()
//        {
//            Entities.ForEach((Entity entity, ref Translation translation, ref SpriteSheetAnimation_Data spriteData) =>
//            {
//                float positionX = translation.Value.x;
//                float positionY = translation.Value.y;
//                if (positionY > yBottom && positionY < yTop_1)
//                {
//                    //Valid position
//                    RenderData renderData = new RenderData
//                    {
//                        entity = entity,
//                        position = translation.Value,
//                        matrix = spriteData.matrix,
//                        uv = spriteData.uv
//                    };

//                    if (positionY < yTop_2)
//                        nativeQueue_2.Enqueue(renderData);
//                    else
//                        nativeQueue_1.Enqueue(renderData);
//                }
//            }).WithoutBurst().Run();

//        }
//    }

//    private struct NativeQueueToArrayJob : IJob
//    {
//        public NativeQueue<RenderData> nativeQueue;
//        public NativeArray<RenderData> nativeArray;

//        public void Execute()
//        {
//            int index = 0;
//            RenderData renderData;
//            while (nativeQueue.TryDequeue(out renderData))
//            {
//                nativeArray[index] = renderData;
//                index++;
//            }
//        }
//    }

//    private struct SortByPositionJob : IJob
//    {
//        public NativeArray<RenderData> sortArray;

//        public void Execute()
//        {
//            for (int i = 0; i < sortArray.Length; i++)
//            {
//                for (int j = i + 1; j < sortArray.Length; j++)
//                {
//                    if(sortArray[i].position.y < sortArray[j].position.y)
//                    {
//                        RenderData tmp = sortArray[i];
//                        sortArray[i] = sortArray[j];
//                        sortArray[j] = tmp;
//                    }
//                }
//            }
//            //public PositionComparer comparer;
//            //public NativeArray<RenderData> sortArray;
//            //sortArray.Sort(comparer);
//        }
//    }

//    private struct FillArraysParallelJob : IJobParallelFor
//    {
//        [ReadOnly] public NativeArray<RenderData> nativeArray;
//        [NativeDisableParallelForRestriction] public NativeArray<Matrix4x4> matrixArray;
//        [NativeDisableParallelForRestriction] public NativeArray<Vector4> uvArray;
//        public int startingIndex;

//        public void Execute(int index)
//        {
//            RenderData entityPositionWithIndex = nativeArray[index];
//            matrixArray[startingIndex + index] = entityPositionWithIndex.matrix;
//            uvArray[startingIndex + index] = entityPositionWithIndex.uv;
//        }
//    }

//    private struct ClearQueueJob : IJob
//    {
//        public NativeQueue<RenderData> nativeQueue;
//        public void Execute()
//        {
//            nativeQueue.Clear();
//        }
//    }

//    private const int DRAW_MESH_INSTANCED_SLICE_COUNT = 1023;
//    private Matrix4x4[] matrixInstancedArray;
//    private Vector4[] uvInstancedArray;
//    private MaterialPropertyBlock materialPropertyBlock;
//    private Mesh mesh;
//    private Material material;
//    private int shaderMainTexUVid;

//    private void InitDrawMeshInstacedSlicedData()
//    {
//        if (matrixInstancedArray != null)
//            return; //Already initialized
//        matrixInstancedArray = new Matrix4x4[DRAW_MESH_INSTANCED_SLICE_COUNT];
//        uvInstancedArray = new Vector4[DRAW_MESH_INSTANCED_SLICE_COUNT];
//        materialPropertyBlock = new MaterialPropertyBlock();
//        shaderMainTexUVid = Shader.PropertyToID("_MainTex_UV");
//        //mesh = GameManager.GetInstance().quadMesh;
//        //material = GameManager.GetInstance().walkingSpriteSheetMaterial;
//    }

//    private const int POSITION_SLICES = 20;
//    private NativeQueue<RenderData>[] nativeQueueArray;
//    private NativeArray<JobHandle> jobHandleArray;
//    private NativeArray<RenderData>[] nativeArrayArray;
//    //private PositionComparer positionComparer;

//    protected override void OnCreate()
//    {
//        base.OnCreate();

//        nativeQueueArray = new NativeQueue<RenderData>[POSITION_SLICES];

//        for (int i = 0; i < POSITION_SLICES; i++)
//        {
//            NativeQueue<RenderData> nativeQueue = new NativeQueue<RenderData>(Allocator.Persistent);
//            nativeQueueArray[i] = nativeQueue;
//        }
//        jobHandleArray = new NativeArray<JobHandle>(POSITION_SLICES, Allocator.Persistent);

//        nativeArrayArray = new NativeArray<RenderData>[POSITION_SLICES];

//        //positionComparer = new PositionComparer();
//    }

//    protected override void OnDestroy()
//    {
//        base.OnDestroy();

//        jobHandleArray.Dispose();
//    }

//    protected override void OnUpdate()
//    {
//       for(int i = 0; i < POSITION_SLICES; i++)
//        {
//            ClearQueueJob clearQueueJob = new ClearQueueJob
//            {
//                nativeQueue = nativeQueueArray[i]
//            };
//            jobHandleArray[i] = clearQueueJob.Schedule();
//        }

//        JobHandle.CompleteAll(jobHandleArray);


//        NativeQueue<RenderData> nativeQueue_1 = new NativeQueue<RenderData>(Allocator.TempJob);
//        NativeQueue<RenderData> nativeQueue_2 = new NativeQueue<RenderData>(Allocator.TempJob);

//        Camera camera = Camera.main;
//        float cameraWidth = camera.aspect * camera.orthographicSize;
//        float3 cameraPosition = camera.transform.position;
//        float marginX = cameraWidth / 10f;
//        float xMin = cameraPosition.x - cameraWidth - marginX;
//        float xMax = cameraPosition.x + cameraWidth + marginX;
//        float cameraSliceSize = camera.orthographicSize * 2f / POSITION_SLICES;
//        float yBottom = cameraPosition.y - camera.orthographicSize; //Bottom Cull position
//        float yTop_1 = cameraPosition.y + camera.orthographicSize; // Top most Cull position

//        float yTop_2 = cameraPosition.y + 0f;

//        //float marginY = camera.orthograpichsSize / 10f;
//        //yTop_1 += marginY;
//        //yBottom -= marginY;

//        CullAndSortNativeQueueJob cullAndSortNativeQueueJob = new CullAndSortNativeQueueJob
//        {
//            //xMin = xMin;
//            //xMax = xMax;
//            yBottom = yBottom,
//            yTop_1 = yTop_1,
//            yTop_2 = yTop_2,

//            nativeQueue_1 = nativeQueue_1.AsParallelWriter(),
//            nativeQueue_2 = nativeQueue_2.AsParallelWriter(),
//        };
//        //JobHandle cullAndSort = cullAndSortNativeQueueJob..Schedule();
//        //cullAndSort.Complete();

//        int visibleEntityTotal = 0;
//        for(int i = 0; i < POSITION_SLICES; i++)
//        {
//            visibleEntityTotal += nativeQueueArray[i].Count;
//        }

//        for (int i = 0; i < POSITION_SLICES; i++)
//        {
//            NativeArray<RenderData> nativeArray = new NativeArray<RenderData>(nativeQueueArray[i].Count, Allocator.TempJob);
//            nativeArrayArray[i] = nativeArray;
//        }

//        for (int i = 0; i < POSITION_SLICES; i++)
//        {
//            NativeQueueToArrayJob nativeQueueToArrayJob = new NativeQueueToArrayJob
//            {
//                nativeQueue = nativeQueueArray[i],
//                nativeArray = nativeArrayArray[i],
//            };
//            jobHandleArray[i] = nativeQueueToArrayJob.Schedule();
//        }

//        //Sort by position
//        for (int i = 0; i < POSITION_SLICES; i++)
//        {
//            SortByPositionJob sortByPositionJob = new SortByPositionJob
//            {
//                sortArray = nativeArrayArray[i],
//                //comparer = positionComparer,
//            };
//        }

//        JobHandle.CompleteAll(jobHandleArray);

//        // Fill up individual Arrays
//        NativeArray<Matrix4x4> matrixArray = new NativeArray<Matrix4x4>(visibleEntityTotal, Allocator.TempJob);
//        NativeArray<Vector4> uvArray = new NativeArray<Vector4>(visibleEntityTotal, Allocator.TempJob);

//        int startingIndex = 0;

//        for(int i = 0; i < POSITION_SLICES; i++)
//        {
//            //if( i!= 4) continue
//            FillArraysParallelJob fillArraysParallelJob = new FillArraysParallelJob
//            {
//                nativeArray = nativeArrayArray[i],
//                matrixArray = matrixArray,
//                uvArray = uvArray,
//                startingIndex = startingIndex
//            };
//            startingIndex += nativeArrayArray[i].Length;
//            jobHandleArray[i] = fillArraysParallelJob.Schedule(nativeArrayArray[i].Length, 10);
//        }

//        JobHandle.CompleteAll(jobHandleArray);

//        for(int i = 0; i < POSITION_SLICES; i++)
//        {
//            nativeArrayArray[i].Dispose();
//        }

//        //Slice Array and Draw
//        InitDrawMeshInstacedSlicedData();
//        for(int i = 0; i < visibleEntityTotal; i+= DRAW_MESH_INSTANCED_SLICE_COUNT)
//        {
//            int sliceSize = math.min(visibleEntityTotal - i, DRAW_MESH_INSTANCED_SLICE_COUNT);

//            NativeArray<Matrix4x4>.Copy(matrixArray, i, matrixInstancedArray, 0, sliceSize);
//            NativeArray<Vector4>.Copy(uvArray, i, uvInstancedArray, 0, sliceSize);

//            materialPropertyBlock.SetVectorArray(shaderMainTexUVid, uvInstancedArray);

//            Graphics.DrawMeshInstanced(mesh, 0, material, matrixInstancedArray, sliceSize, materialPropertyBlock);
//        }

//        ////Convert Queues into Arrays for sorting
//        //NativeArray<RenderData> nativeArray_1 = new NativeArray<RenderData>(nativeQueue_1.Count, Allocator.TempJob);
//        //NativeArray<RenderData> nativeArray_2 = new NativeArray<RenderData>(nativeQueue_2.Count, Allocator.TempJob);

//        //NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(2, Allocator.TempJob);

//        //NativeQueueToArrayJob nativeQueueToArrayJob_1 = new NativeQueueToArrayJob
//        //{
//        //    nativeQueue = nativeQueue_1,
//        //    nativeArray = nativeArray_1,
//        //};

//        //jobHandleArray[0] = nativeQueueToArrayJob_1.Schedule();

//        //NativeQueueToArrayJob nativeQueueToArrayJob_2 = new NativeQueueToArrayJob
//        //{
//        //    nativeQueue = nativeQueue_2,
//        //    nativeArray = nativeArray_2,
//        //};

//        //jobHandleArray[1] = nativeQueueToArrayJob_1.Schedule();
//    }
//}