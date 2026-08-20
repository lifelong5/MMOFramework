using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTest : MonoBehaviour
{
    private List<Node> path;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray,out RaycastHit hit))
            {
                Vector3 targetPos = new Vector3(hit.point.x,0.5f, hit.point.z);
                path = AStarPathFinder.Instance.FindPath(transform.position, targetPos);
                StartCoroutine(MoveAlongPath());
            }
        }
    }
    private IEnumerator MoveAlongPath()
    {
        if(path == null || path.Count == 0)
        {
            yield break;
        }
        for(int i = 0; i < path.Count; i++)
        {
            Debug.Log("i" + i);
            Vector3 targetPosition = GridManager.Instance.GetWorldPosition(path[i].x, path[i].y);
            transform.position = targetPosition;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
