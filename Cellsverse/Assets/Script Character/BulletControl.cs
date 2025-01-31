using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletControl : MonoBehaviour, IPunInstantiateMagicCallback{
    public float bulletDamage;
    public GameObject hitEffect;
    public AudioClip explosionSound;
    healthBarControl HBControl;
    PhotonView PV;
    public GameObject destructable;
    DesroyBlock DB;
    void Start(){
        PV = GetComponent<PhotonView>();
        HBControl = GetComponent<healthBarControl>();
        try{
            destructable = GameObject.Find("HeartLogic").GetComponent<HeartLogic>().map;
        }
        catch{
            Debug.Log("No suck object");
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (PV.IsMine)
        {
            PV.RPC("enemyBulletCollision", RpcTarget.OthersBuffered);

            GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            Destroy(effect, 0.3f);
            if (collision.gameObject.name == "destructable(Clone)")
            {
                DB = destructable.GetComponent<DesroyBlock>();
                DB.DestroyBlock(collision.contacts);
                PhotonNetwork.Destroy(gameObject);
            }
            else if (collision.gameObject.name != "Immue(Clone)" && collision.gameObject.name != "Bacteria(Clone)"){
                PhotonNetwork.Destroy(gameObject);
            }

            else
            {
                int viewID = collision.gameObject.GetComponent<PhotonView>().ViewID;
                PV.RPC("enemyDamaged", RpcTarget.Others, bulletDamage, viewID);
                PhotonNetwork.Destroy(gameObject);
                
            }


        }

    }

    [PunRPC]
    void enemyDamaged(float bulletDamage, int viewID)
    {
        var player = PhotonView.Find(viewID).gameObject;
        player.GetComponent<healthBarControl>().currentHP -= bulletDamage * player.GetComponent<healthBarControl>().defense;
    }


    [PunRPC]
    void enemyBulletCollision() //explosion effect
    {
        GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        Destroy(effect, 0.3f);
    }

    // [PunRPC]
    // void DestoryStuff(int viewID){
    //     var bullet = PhotonView.Find(viewID).gameObject;
    //     if (bullet.GetComponent<PhotonView>().IsMine)
    //     {
    //         Debug.Log("Cry Ar");
    //         PhotonNetwork.Destroy(bullet);
    //     }
        
    // }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        bulletDamage = (float)instantiationData[0];
    }

}