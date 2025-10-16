using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision){
        if(collision.transform.CompareTag("Enemy")){
            Debug.Log("MEngenani Enemy");
        }
    }
}
