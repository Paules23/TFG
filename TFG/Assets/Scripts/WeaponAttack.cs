//using UnityEngine;
//using System.Collections;

//public class WeaponAttack : MonoBehaviour
//{
//    public Transform pivot;  // Punto de giro de la espada
//    public Transform weapon; // Referencia a la espada (rectángulo)
//    public float attackSpeed = 0.2f; // Velocidad del ataque
//    public float attackAngle = 30f; // Ángulo del golpe
//    public float attackDistance = 0.3f; // Distancia de desplazamiento en el arco
//    public LayerMask destructibleLayer; // Capa de objetos destructibles

//    private Quaternion originalRotation;
//    private Vector3 originalPosition;
//    private bool isAttacking = false;

//    private void Start()
//    {
//        originalRotation = pivot.localRotation;
//        originalPosition = weapon.localPosition; // Guardamos la posición original de la espada
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
//        {
//            StartCoroutine(Attack());
//        }
//    }

//    private IEnumerator Attack()
//    {
//        isAttacking = true;

//        float elapsedTime = 0f;
//        Quaternion targetRotation = Quaternion.Euler(0f, 0f, -attackAngle);
//        Vector3 targetPosition = originalPosition + new Vector3(0f, -attackDistance, 0f); // Mover la espada en arco

//        // Movimiento de ataque (rotación + desplazamiento)
//        while (elapsedTime < attackSpeed)
//        {
//            pivot.localRotation = Quaternion.Lerp(originalRotation, targetRotation, elapsedTime / attackSpeed);
//            weapon.localPosition = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / attackSpeed);
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }

//        // Detectar objetos destructibles
//        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(weapon.position, weapon.GetComponent<BoxCollider2D>().size, 0f, destructibleLayer);
//        foreach (Collider2D hit in hitObjects)
//        {
//            if (hit.GetComponent<DestructibleObject>() != null)
//            {
//                hit.GetComponent<DestructibleObject>().DestroyObject();
//            }
//        }

//        yield return new WaitForSeconds(0.1f);

//        // Regreso a posición original
//        elapsedTime = 0f;
//        while (elapsedTime < attackSpeed)
//        {
//            pivot.localRotation = Quaternion.Lerp(targetRotation, originalRotation, elapsedTime / attackSpeed);
//            weapon.localPosition = Vector3.Lerp(targetPosition, originalPosition, elapsedTime / attackSpeed);
//            elapsedTime += Time.deltaTime;
//            yield return null;
//        }

//        isAttacking = false;
//    }
//}
