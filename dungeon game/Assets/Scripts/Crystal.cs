using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The breakable crystal
public class Crystal : MonoBehaviour
{
    public enum size{
        Small,
        Medium,
        Big
    }

    public size crystalSize;

    //Materials to replace the normal ones with.
    [Header("Materials")]
    public Material topCracked;
    public Material sideCracked;

    int crystals;
    int hit = 0;

    //Set how many crystals it should give when broken
    void Start()
    {
        switch(crystalSize){
            case size.Small:
            crystals = Random.Range(1,5);
            break;
            case size.Medium:
            crystals = Random.Range(6,15);
            break;
            case size.Big:
            crystals = Random.Range(16,30);
            break;
        }
    }

    //Decreases the size of the crystal for funky effect
    IEnumerator Resize(){
        while(true){
                transform.localScale = transform.localScale * .95f;
                if(transform.localScale.x <= 0.0035) break;
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }
    
    //Gives ~50% of children a rigidbody and boxcollider so they break
    IEnumerator Break(){
        foreach(Transform child in transform){
            if(Random.Range(0,2) == 0){
                child.gameObject.AddComponent<Rigidbody>();
                var collider = child.gameObject.AddComponent<BoxCollider>();
            }else{
                Destroy(child.gameObject);
            }
        }
        GetComponent<BoxCollider>().enabled = false;
        yield return new WaitForSeconds(1);
        StartCoroutine(Resize());
    }

    //replaces all materials with cracked version
    void CrackTime(){
        foreach(MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>()){
            if(renderer.material.name == "Top (Instance)"){
                renderer.material = topCracked;
            }else{
                renderer.material = sideCracked;
            }
        }
    }

    //cracks/breaks the crystal depending on how many hits & what size
    public void Hit(){
        hit += 1;
        switch(crystalSize){
            case size.Small:
                if(hit == 1){
                    CrackTime();
                    GameObject.FindObjectOfType<Game>().AddCrystals(crystals);
                    StartCoroutine(Break());
                }
            break;
            case size.Medium:
                if(hit == 1){
                    CrackTime();
                }else if(hit == 2){
                    GameObject.FindObjectOfType<Game>().AddCrystals(crystals);
                    StartCoroutine(Break());
                }
            break;
            case size.Big:
                if(hit == 2){
                    CrackTime();
                }else if(hit == 4){
                    GameObject.FindObjectOfType<Game>().AddCrystals(crystals);
                    StartCoroutine(Break());
                }
            break;

        }
    }

}
