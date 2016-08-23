using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Kvant
{
    public class WigTemplateEditor
    {
        static Object[] SelectedMeshes {
            get { return Selection.GetFiltered(typeof(Mesh), SelectionMode.Deep); }
        }

        static string NewFileName(Mesh mesh, string postfix)
        {
            var dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mesh));
            var filePath = Path.Combine(dirPath, "Wig " + postfix + ".asset");
            return AssetDatabase.GenerateUniqueAssetPath(filePath);
        }

        [MenuItem("Assets/Kvant/Wig/Convert To Template", true)]
        static bool ValidateConvertToTemplate()
        {
            return SelectedMeshes.Length > 0;
        }

        [MenuItem("Assets/Kvant/Wig/Convert To Template")]
        static void ConvertToTemplate()
        {
            foreach (Mesh mesh in SelectedMeshes)
            {
                var foundation = CreateFoundation(mesh);
                var template = CreateTemplate(foundation.width, 32);
                AssetDatabase.CreateAsset(foundation, NewFileName(mesh, "Foundation"));
                AssetDatabase.CreateAsset(template, NewFileName(mesh, "Template"));
            }
        }

        static Texture2D CreateFoundation(Mesh source)
        {
            var inVertices = source.vertices;
            var inNormals = source.normals;

            var outVertices = new List<Vector3>();
            var outNormals = new List<Vector3>();

            for (var i = 0; i < inVertices.Length; i++)
            {
                if (!outVertices.Any(_ => _ == inVertices[i]))
                {
                    outVertices.Add(inVertices[i]);
                    outNormals.Add(inNormals[i]);
                }
            }

            var tex = new Texture2D(outVertices.Count, 2, TextureFormat.RGBAFloat, false);

            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (var i = 0; i < outVertices.Count; i++)
            {
                var v = outVertices[i];
                var n = outNormals[i];
                tex.SetPixel(i, 0, new Color(v.x, v.y, v.z, 1));
                tex.SetPixel(i, 1, new Color(n.x, n.y, n.z, 0));
            }

            tex.Apply(false, true);

            return tex;
        }

        static Mesh CreateTemplate(int vcount, int length)
        {
            var vertices = new List<Vector3>(vcount * length);
            var indices = new List<int>(vcount * (length - 1) * 2);

            for (var i1 = 0; i1 < vcount; i1++)
            {
                var u = (float)i1 / vcount;

                for (var i2 = 0; i2 < length; i2++)
                {
                    var v = (float)i2 / length;
                    vertices.Add(new Vector3(u, v, 0));
                }

                for (var i2 = 0; i2 < length - 1; i2++)
                {
                    var i = i1 * length + i2;
                    indices.Add(i);
                    indices.Add(i + 1);
                }
            }

            var mesh = new Mesh();
            mesh.name = "Wig Template";
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);

            mesh.Optimize();
            mesh.UploadMeshData(true);

            return mesh;
        }
    }
}
