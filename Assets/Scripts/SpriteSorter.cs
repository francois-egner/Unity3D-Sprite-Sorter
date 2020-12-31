﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSorter : MonoBehaviour{
    
    /// <summary>
    /// List of sprites to be sorted each frame (most commonly moving objects)
    /// </summary>
    [SerializeField] List<Tuple<SpriteRenderer, float>> dynamicRenderers = new List<Tuple<SpriteRenderer, float>>();
    
    /// <summary>
    /// Accuracy of sorting
    /// </summary>
    
    [Tooltip("Accuracy of sorting. Increment this value if two objects close to each other are not sorted correctly.")]
    [SerializeField] int accuracy = 2;
    /// <summary>
    /// Destroy SortPoint after its offset to parent was calculated?
    /// </summary>
    [Tooltip("If true, child object which represents the sorting point will be destroyed after sorting once. Set to true if you run into memory issues regarding to many GameObjects!")]
    [SerializeField] bool destroySortPoint = false;
    
    [Tooltip("Tag of the children object which represents the sorting point of the parent object")]
    [SerializeField] String sortPointTag = "SortPoint";
    
    /// <summary>
    /// Sort static SceneObjects (props) at the start of the game/scene
    /// </summary>
    void Awake() {
        sortStaticSceneObject();
    }

    /// <summary>
    /// Register new renderer. Registered renderers will be sorted every frame, even if their parent object is not moving. (Improvement coming soon)
    /// </summary>
    /// <param name="renderer">Renderer to be sorted every frame</param>
    public void registerRenderer(SpriteRenderer renderer){
        if (renderer){
            GameObject rendererParent = renderer.gameObject;
            float yOffset = getSortYOffset(rendererParent);
            
            dynamicRenderers.Add(new Tuple<SpriteRenderer, float>(renderer, yOffset));
        }
    }

    /// <summary>
    /// Get y-offset of sorting point and actual GameObject. 
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    private float getSortYOffset(GameObject parent){
        foreach (Transform t  in parent.transform){
            if(t.tag.Equals(sortPointTag)){
                float offset = t.localPosition.y * parent.transform.localScale.y;
                if (destroySortPoint) {
                    Destroy(t.gameObject);
                }
                return offset;
            }
        }
        return float.NaN;
    }

    /// <summary>
    /// Set sorting order for given SpriteRenderer of a newly instantiated static GameObject.
    /// </summary>
    /// <param name="renderer">SpriteRenderer to be sorted once</param>
    public void sortOnce(SpriteRenderer renderer){
        if (renderer) {
            GameObject rendererParent = renderer.gameObject;
            float sortOffset = getSortYOffset(rendererParent);
            renderer.sortingOrder = (int)((rendererParent.transform.position.y + sortOffset) * -(Math.Pow(10, accuracy)));
        }
    }
    
    /// <summary>
    /// Sorts SpriteRenderers of all moving/dynamic GameObjects. This will be called every frame
    /// </summary>
    void sortDynamicSprites(){
        for (int index = 0; index < dynamicRenderers.Count; index++){
            if (dynamicRenderers[index].Item1){ //Item 1 = SpriteRenderer, Item2 = sortPoint offset
                GameObject rendererParent = dynamicRenderers[index].Item1.gameObject;   //Get sortPoint offset from list tuple
                dynamicRenderers[index].Item1.sortingOrder = (int)((rendererParent.transform.position.y + dynamicRenderers[index].Item2) * -(Math.Pow(10, accuracy))); //Calculate new sortOrder
            }
            else {
                dynamicRenderers.RemoveAt(index);  //Remove renderer from sorting list
            }
        }
    }
    
    /// <summary>
    /// Checks existence of a sorting point
    /// </summary>
    /// <param name="gObject">SpriteRenderers GameObject which must have a sorting point as a child GameObject</param>
    /// <returns></returns>
    bool checkSortPointExists(GameObject gObject) {
        foreach (Transform t in gObject.transform) {
            if (t.tag.Equals(sortPointTag)) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Search for props with an existing sort point and sort their SpriteRennderer
    /// </summary>
    void sortStaticSceneObject() {
        GameObject[] props = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject prop in props) {
            if (prop.GetComponent<SpriteRenderer>() && checkSortPointExists(prop) ) {
                SpriteRenderer renderer = prop.GetComponent<SpriteRenderer>();
                float sortOffset = getSortYOffset(prop);
                renderer.sortingOrder = (int)((prop.transform.position.y + sortOffset) * -(Math.Pow(10, accuracy)));
            }
        }
    }

    void Update(){
        sortDynamicSprites();
    }
}
