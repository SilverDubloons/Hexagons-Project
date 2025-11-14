using UnityEngine;

public class VertexListPrinter : MonoBehaviour
{
    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter != null)
        {
            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = mesh.vertices;
            Transform meshTransform = transform; // Get the mesh's transform

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 localVertex = vertices[i];
                Vector3 worldVertex = meshTransform.TransformPoint(localVertex);
                Debug.Log("Vertex " + i + " (World Space): " + worldVertex);
            }
        }
        else
        {
            Debug.LogError("No MeshFilter component found.");
        }
    }
	
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.B))
		{
			MeshFilter meshFilter = GetComponent<MeshFilter>();

			if (meshFilter != null)
			{
				Mesh mesh = meshFilter.mesh;
				Vector3[] vertices = mesh.vertices;
				Transform meshTransform = transform; // Get the mesh's transform

				for (int i = 0; i < vertices.Length; i++)
				{
					Vector3 localVertex = vertices[i];
					Vector3 worldVertex = meshTransform.TransformPoint(localVertex);
					Debug.Log("Vertex " + i + " (World Space): " + worldVertex);
				}
			}
			else
			{
				Debug.LogError("No MeshFilter component found.");
			}
		}
	}
}



