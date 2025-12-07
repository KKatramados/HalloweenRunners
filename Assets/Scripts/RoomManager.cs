using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public enum DoorDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [System.Serializable]
    public class RoomDoor
    {
        public int roomNumber;
        public GameObject doorObject;
        public int enemiesRemaining;
        public DoorDirection direction = DoorDirection.Up;
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
            StartCoroutine(OpenDoorAnimation(door));
        }
    }

    System.Collections.IEnumerator OpenDoorAnimation(RoomDoor door)
    {
        float duration = 1f;
        float elapsed = 0f;
        float distance = 3f;
        Vector3 startPos = door.doorObject.transform.position;

        // Determine movement direction based on door direction
        Vector3 movementVector = Vector3.zero;
        switch (door.direction)
        {
            case DoorDirection.Up:
                movementVector = Vector3.up * distance;
                break;
            case DoorDirection.Down:
                movementVector = Vector3.down * distance;
                break;
            case DoorDirection.Left:
                movementVector = Vector3.left * distance;
                break;
            case DoorDirection.Right:
                movementVector = Vector3.right * distance;
                break;
        }

        Vector3 endPos = startPos + movementVector;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            door.doorObject.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        // Disable collider after opening
        Collider2D doorCollider = door.doorObject.GetComponent<Collider2D>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }
}
