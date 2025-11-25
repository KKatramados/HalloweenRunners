using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [System.Serializable]
    public class RoomDoor
    {
        public int roomNumber;
        public GameObject doorObject;
        public int enemiesRemaining;
    }

    public RoomDoor[] doors;

    public void RegisterEnemy(int roomNumber, Enemy enemy)
    {
        foreach (var door in doors)
        {
            if (door.roomNumber == roomNumber)
            {
                door.enemiesRemaining++;
                break;
            }
        }
    }

    public void RegisterEnemy(int roomNumber, SkeletonBoss enemy)
    {
        foreach (var door in doors)
        {
            if (door.roomNumber == roomNumber)
            {
                door.enemiesRemaining++;
                break;
            }
        }
    }

    public void EnemyDied(int roomNumber)
    {
        foreach (var door in doors)
        {
            if (door.roomNumber == roomNumber)
            {
                door.enemiesRemaining--;

                if (door.enemiesRemaining <= 0)
                {
                    OpenDoor(door);
                }
                break;
            }
        }
    }

    void OpenDoor(RoomDoor door)
    {
        if (door.doorObject != null)
        {
            // AUDIO: Play door open sound
            if (AudioManager.instance != null)
                AudioManager.instance.PlayDoorOpen();

            // Animate door opening
            StartCoroutine(OpenDoorAnimation(door.doorObject));
        }
    }

    System.Collections.IEnumerator OpenDoorAnimation(GameObject door)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = door.transform.position;
        Vector3 endPos = startPos + Vector3.up * 3f; // Move door up

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            door.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        // Disable collider after opening
        Collider2D doorCollider = door.GetComponent<Collider2D>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }
}
